using System.Text.RegularExpressions;
using Backend.Analytics;
using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

// Indbygget, cookiefri besoegsstatistik.
//  POST /api/analytics/event  - offentlig: frontenden melder sidevisninger og klik (svarer altid 204)
//  GET  /api/analytics/stats  - kun admin: tal for i dag / 7 dage / 30 dage / i aar
// Der gemmes aldrig IP-adresser, kun en daglig skiftende hash (se Analytics/VisitorHasher.cs).
[ApiController]
[Route("api/analytics")]
public partial class AnalyticsController : ControllerBase
{
    private static readonly HashSet<string> SimpleTargets = new(StringComparer.Ordinal) { "phone", "email", "contact", "facebook" };

    // Egne domaener taeller ikke som henvisning (det er bare intern navigation)
    private static readonly HashSet<string> OwnHosts = new(StringComparer.OrdinalIgnoreCase)
    {
        "carltonpaataget.dk", "www.carltonpaataget.dk", "carlton-paa-taget.fly.dev", "localhost", "127.0.0.1",
    };

    private readonly DataContext _db;
    private readonly VisitorHasher _hasher;
    private readonly GeoLookup _geo;

    public AnalyticsController(DataContext db, VisitorHasher hasher, GeoLookup geo)
    {
        _db = db;
        _hasher = hasher;
        _geo = geo;
    }

    public sealed class EventDto
    {
        public string? Type { get; set; }
        public string? Path { get; set; }
        public string? Target { get; set; }
        public string? Referrer { get; set; }
    }

    [HttpPost("event")]
    [AllowAnonymous]               // undtaget fra AdminOnlyWritesFilter; begraenses af rate limiteren (POST /api)
    [RequestSizeLimit(4096)]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Event([FromBody] EventDto? dto, CancellationToken ct)
    {
        // Ugyldige, bot- og admin-haendelser ignoreres stille (altid 204, saa intet kan udledes)
        if (dto == null) return NoContent();
        if (User.Identity?.IsAuthenticated == true && User.IsInRole("Admin")) return NoContent();

        var userAgent = Request.Headers.UserAgent.ToString();
        if (IsBot(userAgent)) return NoContent();

        var type = dto.Type;
        string? target = null;
        if (type == "click")
        {
            target = dto.Target;
            if (target == null || !(SimpleTargets.Contains(target) || TargetRegex().IsMatch(target))) return NoContent();
            // Ydelseskort: kun ydelser, der findes (så der ikke kan fyldes vilkårlige mål i statistikken)
            if (target.StartsWith("service:", StringComparison.Ordinal))
            {
                var slug = target["service:".Length..];
                if (!await _db.Services.AnyAsync(s => s.Slug == slug, ct)) return NoContent();
            }
        }
        else if (type != "pageview")
        {
            return NoContent();
        }

        var ip = ClientIp(HttpContext);
        var now = DateTime.UtcNow;
        var (country, city) = _geo.Lookup(ip);

        _db.AnalyticsEvents.Add(new AnalyticsEvent
        {
            CreatedAt = now,
            Type = type,
            Path = CleanPath(dto.Path),
            Target = target,
            VisitorHash = await _hasher.HashAsync(_db, ip, userAgent.Length > 512 ? userAgent[..512] : userAgent, now, ct),
            Referrer = type == "pageview" ? ReferrerDomain(dto.Referrer, Request.Host.Host) : null,
            Device = MobileRegex().IsMatch(userAgent) ? "mobile" : "desktop",
            Country = country,
            City = city,
        });
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("stats")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Stats([FromQuery] string? period, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var today = DanishTime.Today(now);
        var (key, fromDay) = period switch
        {
            "7d" => ("7d", today.AddDays(-6)),
            "30d" => ("30d", today.AddDays(-29)),
            "year" => ("year", new DateOnly(today.Year, 1, 1)),
            _ => ("today", today),
        };
        var from = DanishTime.MidnightUtc(fromDay);

        var events = _db.AnalyticsEvents.AsNoTracking().Where(e => e.CreatedAt >= from);
        var views = events.Where(e => e.Type == "pageview");
        var clicks = events.Where(e => e.Type == "click");

        // Hashen skifter hver dag, saa "unikke besoegende" for en periode = summen af hver dags unikke
        var visitors = await events.Select(e => e.VisitorHash).Distinct().CountAsync(ct);
        var pageviews = await views.CountAsync(ct);

        var clickRows = await clicks.GroupBy(e => e.Target)
            .Select(g => new { target = g.Key, count = g.Count() })
            .OrderByDescending(x => x.count).Take(30).ToListAsync(ct);

        // Kilder samles i SQL til fire grupper (det raa domaene gemmes stadig):
        // google = google.* (og underdomaener), facebook = facebook.com/fb.com (m., l., lm. osv.), direct = intet, other = resten
        var sourceRows = await views.GroupBy(e =>
                e.Referrer == null ? "direct"
                : e.Referrer.StartsWith("google.") || e.Referrer.Contains(".google.") ? "google"
                : e.Referrer == "facebook.com" || e.Referrer.EndsWith(".facebook.com")
                  || e.Referrer == "fb.com" || e.Referrer.EndsWith(".fb.com") ? "facebook"
                : "other")
            .Select(g => new { source = g.Key, visitors = g.Select(x => x.VisitorHash).Distinct().Count() })
            .ToListAsync(ct);
        int Source(string s) => sourceRows.FirstOrDefault(r => r.source == s)?.visitors ?? 0;

        var countries = await events.Where(e => e.Country != null).GroupBy(e => e.Country)
            .Select(g => new { code = g.Key, visitors = g.Select(x => x.VisitorHash).Distinct().Count() })
            .OrderByDescending(x => x.visitors).Take(10).ToListAsync(ct);
        var cities = await events.Where(e => e.City != null).GroupBy(e => new { e.City, e.Country })
            .Select(g => new { city = g.Key.City, country = g.Key.Country, visitors = g.Select(x => x.VisitorHash).Distinct().Count() })
            .OrderByDescending(x => x.visitors).Take(10).ToListAsync(ct);

        var devices = await events.GroupBy(e => e.Device)
            .Select(g => new { device = g.Key, visitors = g.Select(x => x.VisitorHash).Distinct().Count() })
            .ToListAsync(ct);

        var pages = await views.GroupBy(e => e.Path)
            .Select(g => new { path = g.Key, pageviews = g.Count() })
            .OrderByDescending(x => x.pageviews).Take(10).ToListAsync(ct);

        return Ok(new
        {
            period = key,
            from = fromDay.ToString("yyyy-MM-dd"),
            to = today.ToString("yyyy-MM-dd"),
            uniqueVisitors = visitors,
            pageviews,
            totalClicks = clickRows.Sum(c => c.count),
            clicks = clickRows,
            sources = new { google = Source("google"), facebook = Source("facebook"), direct = Source("direct"), other = Source("other") },
            countries,
            cities,
            mobile = devices.FirstOrDefault(d => d.device == "mobile")?.visitors ?? 0,
            desktop = devices.FirstOrDefault(d => d.device == "desktop")?.visitors ?? 0,
            pages,
            geoAvailable = _geo.Available,
        });
    }

    // --- Hjaelpere ---

    // Fly-proxyen saetter Fly-Client-IP; ellers forbindelsens adresse. Bruges kun til hash og geo-opslag.
    private static string ClientIp(HttpContext ctx)
    {
        var flyIp = ctx.Request.Headers["Fly-Client-IP"].ToString().Trim();
        return flyIp.Length > 0 ? flyIp : ctx.Connection.RemoteIpAddress?.ToString() ?? "";
    }

    private static string CleanPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || path[0] != '/') return "/";
        var cut = path.IndexOfAny(['?', '#']);
        if (cut >= 0) path = path[..cut];
        path = path.Trim();
        if (path.Length == 0) return "/";
        return path.Length > 200 ? path[..200] : path;
    }

    private static string? ReferrerDomain(string? referrer, string requestHost)
    {
        if (string.IsNullOrWhiteSpace(referrer) || !Uri.TryCreate(referrer.Trim(), UriKind.Absolute, out var uri)) return null;
        var host = uri.Host.ToLowerInvariant().TrimEnd('.');
        if (host.Length == 0 || OwnHosts.Contains(host) || host.Equals(requestHost, StringComparison.OrdinalIgnoreCase)) return null;
        if (host.StartsWith("www.")) host = host[4..];
        return host.Length > 100 ? host[..100] : host;
    }

    private static bool IsBot(string userAgent) =>
        string.IsNullOrWhiteSpace(userAgent) || userAgent.Length < 20 || BotRegex().IsMatch(userAgent);

    // gallery:<id> eller service:<slug> (Target-kolonnen er højst 64 tegn: "service:" + 56)
    [GeneratedRegex(@"^(gallery:\d{1,9}|service:[a-z0-9-]{1,56})$")]
    private static partial Regex TargetRegex();

    [GeneratedRegex(@"Mobi|Android|iPhone|iPad|iPod", RegexOptions.IgnoreCase)]
    private static partial Regex MobileRegex();

    [GeneratedRegex(@"bot\b|bot/|crawl|spider|slurp|archiver|facebookexternalhit|facebookcatalog|embedly|preview|whatsapp|telegram|discord|skype|headless|phantomjs|selenium|puppeteer|playwright|lighthouse|pagespeed|gtmetrix|curl|wget|python|aiohttp|httpclient|okhttp|go-http|java/|libwww|axios|node-fetch|undici|postman|insomnia|uptime|pingdom|statuscake|monitor|scanner|validator", RegexOptions.IgnoreCase)]
    private static partial Regex BotRegex();
}
