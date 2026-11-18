using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using Backend.Content;
using Backend.Data;
using Jint;
using Microsoft.EntityFrameworkCore;

namespace Backend.Prerender;

// Prerendering (SSR) af siden.
// Frontenden bygges ogsaa som et "server-bundle" (frontend: vite build --ssr src/entry-server.tsx
// -> /app/ssr/entry-server.js). Det er den samme React-app som i browseren. Her koeres bundlet i
// Jint (en JavaScript-motor skrevet i .NET, saa der ikke skal koere Node ved siden af), med de
// aktuelle tekster og billeder fra databasen. Resultatet flettes ind i wwwroot/index.html sammen
// med dataene (window.__INITIAL_DATA__), og browseren "hydrerer" den faerdige HTML.
// HTML'en caches og genbruges, indtil teksterne/billederne (eller de byggede filer) aendrer sig,
// saa en rettelse i admin ses med det samme uden nyt deploy.
// Siderne: forsiden ("/"), et produkt ("/produkter/<slug>"), cookiepolitik ("/cookiepolitik"),
// kontakt ("/kontakt"), tak ("/tak") og "siden findes ikke" (404).
// Hver side faar sin egen titel, beskrivelse, canonical, Open Graph og strukturerede data i <head>.
public sealed record RenderedPage(string Html, int StatusCode, string? RedirectTo = null);

public sealed partial class PrerenderService
{
    private const string HtmlMarker = "<!--app-html-->";
    private const string DataMarker = "<!--app-data-->";
    private const string JsonLdMarker = "<!--app-jsonld-->";

    // Ingen egentlig browser paa serveren: React's server-renderer refererer et par browser-API'er,
    // som renderToString ikke bruger. De faar tomme stand-ins her.
    private const string Polyfills = """
        var process = { env: { NODE_ENV: "production" } };
        var self = globalThis;
        var setTimeout = function () { return 0; }, clearTimeout = function () {};
        var setImmediate = function () { return 0; }, clearImmediate = function () {};
        var queueMicrotask = function () {};
        var MessageChannel = function () { this.port1 = {}; this.port2 = { postMessage: function () {} }; };
        var TextEncoder = function () {};
        TextEncoder.prototype.encode = function (s) {
          var b = unescape(encodeURIComponent(String(s === undefined ? "" : s)));
          var a = new Uint8Array(b.length);
          for (var i = 0; i < b.length; i++) a[i] = b.charCodeAt(i);
          return a;
        };
        """;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    // Til tekst i HTML-attributter (<meta content="...">): escaper " < > & men lader æ, ø, å staa
    private static readonly HtmlEncoder Html = HtmlEncoder.Create(UnicodeRanges.All);

    private const int MaxCachedPages = 100;

    private readonly IWebHostEnvironment _env;
    private readonly ILogger<PrerenderService> _logger;
    private readonly string _bundlePath;
    private readonly string _siteUrl;
    private readonly string _gtmId;
    private readonly string[] _verifications;
    private readonly object _lock = new();

    private Engine? _engine;
    private string? _template;
    private DateTime _templateStamp, _bundleStamp;
    private readonly Dictionary<string, (string Key, string Html)> _cache = new(StringComparer.Ordinal);
    private bool _warnedMissing;

    public PrerenderService(IWebHostEnvironment env, IConfiguration config, ILogger<PrerenderService> logger)
    {
        _env = env;
        _logger = logger;
        _bundlePath = config["Prerender:Bundle"] ?? Path.Combine(env.ContentRootPath, "ssr", "entry-server.js");
        var siteUrl = (config["Site:Url"] ?? "").Trim();
        _siteUrl = siteUrl.Length == 0 ? "" : siteUrl.EndsWith('/') ? siteUrl : siteUrl + "/";
        _gtmId = NormalizeGtm(config["Analytics:GtmId"] ?? Environment.GetEnvironmentVariable("GTM_ID"));
        _verifications = (config["Site:GoogleSiteVerification"] ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(v => VerificationRegex().IsMatch(v))
            .ToArray();
    }

    // Tom eller ugyldig = Google Tag Manager indlæses ikke.
    private static string NormalizeGtm(string? raw)
    {
        var id = (raw ?? "").Trim().ToUpperInvariant();
        return GtmRegex().IsMatch(id) ? id : "";
    }

    [GeneratedRegex(@"^GTM-[A-Z0-9]+$")]
    private static partial Regex GtmRegex();

    [GeneratedRegex(@"^[A-Za-z0-9_-]{8,120}$")]
    private static partial Regex VerificationRegex();

    private string Absolute(string relative)
    {
        var path = relative.TrimStart('/');
        if (_siteUrl.Length == 0) return path.Length == 0 ? "/" : "/" + path;
        return path.Length == 0 ? _siteUrl : _siteUrl + path;
    }

    private string WebRoot => _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
    private string TemplatePath => Path.Combine(WebRoot, "index.html");

    // Hele HTML-siden for stien, eller null hvis frontenden ikke er bygget (wwwroot/index.html mangler)
    public async Task<RenderedPage?> RenderPageAsync(DataContext db, string path, CancellationToken ct)
    {
        if (!File.Exists(TemplatePath)) return null;

        var (texts, gallery, services, dataOk) = await LoadDataAsync(db, ct);

        // Gamle adresser /ydelser og /ydelser/<slug> -> /produkter
        var legacy = LegacyServicePathRegex().Match(path);
        if (legacy.Success)
        {
            var oldSlug = legacy.Groups["slug"].Success ? Uri.UnescapeDataString(legacy.Groups["slug"].Value) : "";
            var target = oldSlug.Length == 0 ? "/#produkter" : "/produkter/" + oldSlug;
            return new RenderedPage("", StatusCodes.Status301MovedPermanently, target);
        }

        // Hvilken side? /produkter/<slug> er et produkt; alt andet er forsiden (som hidtil)
        var page = "home";
        ServiceDto? service = null;
        var match = ServicePathRegex().Match(path);
        if (match.Success)
        {
            var slug = Uri.UnescapeDataString(match.Groups["slug"].Value);
            if (slug.Length == 0) return new RenderedPage("", StatusCodes.Status301MovedPermanently, "/#produkter");

            service = services.FirstOrDefault(s => s.Slug == slug);
            var loose = service ?? services.FirstOrDefault(s => string.Equals(s.Slug, slug, StringComparison.OrdinalIgnoreCase));
            // Forkert stavemaade (store bogstaver / skraastreg til sidst): videre til den rigtige adresse
            if (loose != null && (service == null || path != "/produkter/" + loose.Slug))
                return new RenderedPage("", StatusCodes.Status301MovedPermanently, "/produkter/" + loose.Slug);

            page = service != null ? "service" : "notfound";
            path = service != null ? "/produkter/" + service.Slug : path;
        }
        else if (CookiePathRegex().IsMatch(path))
        {
            if (path != "/cookiepolitik")
                return new RenderedPage("", StatusCodes.Status301MovedPermanently, "/cookiepolitik");
            page = "cookie";
            path = "/cookiepolitik";
        }
        else if (KontaktPathRegex().IsMatch(path))
        {
            if (path != "/kontakt")
                return new RenderedPage("", StatusCodes.Status301MovedPermanently, "/kontakt");
            page = "kontakt";
            path = "/kontakt";
        }
        else if (ThanksPathRegex().IsMatch(path))
        {
            if (path != "/tak")
                return new RenderedPage("", StatusCodes.Status301MovedPermanently, "/tak");
            page = "thanks";
            path = "/tak";
        }
        else
        {
            path = "/";
        }

        // Kan databasen ikke laeses, ved vi ikke om ydelsen findes: midlertidig fejl i stedet for 404
        var status = page == "notfound"
            ? (dataOk ? StatusCodes.Status404NotFound : StatusCodes.Status503ServiceUnavailable)
            : StatusCodes.Status200OK;

        var data = dataOk ? new { texts, gallery, services, path } : (object)new { path };
        var dataJson = JsonSerializer.Serialize(data, JsonOptions);
        var textMap = texts.ToDictionary(t => t.Id, t => t.Value);

        lock (_lock)
        {
            ReloadIfChanged();
            if (_cache.TryGetValue(path, out var cached) && cached.Key == dataJson) return new RenderedPage(cached.Html, status);

            string appHtml = "";
            var rendered = false;
            if (_engine != null)
            {
                try
                {
                    _engine.SetValue("__data", dataJson);
                    appHtml = _engine.Evaluate("CustombikeSSR.render(__data)").AsString();
                    rendered = true;
                }
                catch (Exception ex)
                {
                    // Siden virker stadig: browseren renderer den selv ud fra dataene
                    _logger.LogError(ex, "Prerender fejlede - sender siden uden forudrenderet HTML");
                }
            }

            var head = page switch
            {
                "service" => ServiceHead(_template!, service!, textMap),
                "cookie" => CookieHead(_template!, textMap),
                "kontakt" => KontaktHead(_template!, textMap),
                "thanks" => ThanksHead(_template!, textMap),
                "notfound" => NotFoundHead(_template!, textMap),
                _ => ApplyHomeHead(_template!),
            };

            // JSON fra System.Text.Json escaper < > & osv., saa den er sikker inde i <script>
            var html = head
                .Replace(HtmlMarker, appHtml)
                .Replace(DataMarker, $"<script>window.__INITIAL_DATA__={dataJson}</script>")
                .Replace(JsonLdMarker, textMap.Count > 0 && page is not "notfound" and not "cookie" and not "thanks" ? BuildJsonLd(textMap, services, service) : "");

            if (rendered && dataOk)
            {
                if (_cache.Count >= MaxCachedPages) _cache.Clear();
                _cache[path] = (dataJson, html);
            }
            return new RenderedPage(html, status);
        }
    }

    // /produkter, /produkter/ og /produkter/<slug>(/)
    [GeneratedRegex(@"^/produkter(?:/(?<slug>[^/]*))?/?$", RegexOptions.IgnoreCase)]
    private static partial Regex ServicePathRegex();

    // Gamle adresser fra før produkterne hed /produkter
    [GeneratedRegex(@"^/ydelser(?:/(?<slug>[^/]*))?/?$", RegexOptions.IgnoreCase)]
    private static partial Regex LegacyServicePathRegex();

    [GeneratedRegex(@"^/cookiepolitik/?$", RegexOptions.IgnoreCase)]
    private static partial Regex CookiePathRegex();

    [GeneratedRegex(@"^/kontakt/?$", RegexOptions.IgnoreCase)]
    private static partial Regex KontaktPathRegex();

    [GeneratedRegex(@"^/tak/?$", RegexOptions.IgnoreCase)]
    private static partial Regex ThanksPathRegex();

    private sealed record TextRow(string Id, string Value, string? Description);
    private sealed record GalleryRow(int Id, string Kind, string? Url, string? YouTubeId, string? PosterUrl, string? Hash);

    // Samme data som GET /api/texts, GET /api/gallery og GET /api/services (kun laesning)
    private async Task<(List<TextRow> Texts, List<GalleryRow> Gallery, List<ServiceDto> Services, bool Ok)> LoadDataAsync(DataContext db, CancellationToken ct)
    {
        try
        {
            var texts = await db.SiteTexts
                .AsNoTracking()
                .OrderBy(t => t.Id)
                .Select(t => new TextRow(t.Id, t.Value, t.Description))
                .ToListAsync(ct);

            var images = await db.GalleryImages
                .AsNoTracking()
                .OrderBy(i => i.SortOrder)
                .ThenBy(i => i.Id)
                .ToListAsync(ct);

            var folder = Path.Combine(WebRoot, "gallery");
            var gallery = images
                .Where(i => i.FileName != null ? File.Exists(Path.Combine(folder, i.FileName)) : i.YouTubeId != null)
                .Select(i => new GalleryRow(
                    i.Id,
                    i.Kind,
                    i.FileName == null ? null : $"/gallery/{Uri.EscapeDataString(i.FileName)}",
                    i.YouTubeId,
                    i.PosterFileName == null ? null : MediaFiles.PosterUrlFor(i.PosterFileName),
                    i.ContentHash))
                .ToList();

            var services = await ServiceData.ListAsync(db, ct);

            return (texts, gallery, services, true);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Prerender: kunne ikke hente tekster/billeder/ydelser - bruger standardtekster");
            return (new(), new(), new(), false);
        }
    }

    // --- <head> pr. side ---

    private string ServiceHead(string template, ServiceDto service, IReadOnlyDictionary<string, string> texts)
    {
        string Get(string id) => texts.TryGetValue(id, out var v) ? v.Trim() : "";
        var company = Get("company.name");
        var city = CityFromAddress(Get("company.address"));
        var title = service.Title + (city != "" ? $" i {city}" : "") + (company != "" ? $" | {company}" : "");
        var description = Describe(service.ShortText != "" ? service.ShortText : service.Body, 160);
        var url = Absolute("produkter/" + service.Slug);
        var image = service.CoverUrl != null ? Absolute(service.CoverUrl) : null;

        var html = SetHead(template, title, description, url);
        if (image != null)
        {
            html = ReplaceMeta(html, "property", "og:image", image);
            html = ReplaceMeta(html, "name", "twitter:image", image);
            html = ReplaceMeta(html, "property", "og:image:alt", service.Title);
            html = ReplaceMeta(html, "name", "twitter:image:alt", service.Title);
            // Billedets maal kendes ikke - de faste maal for logo-billedet fjernes
            html = Regex.Replace(html, @"\s*<meta\s+property=""og:image:(?:width|height)""\s+content=""[^""]*""\s*/?>", "");
        }
        return html;
    }

    private string KontaktHead(string template, IReadOnlyDictionary<string, string> texts)
    {
        string Get(string id) => texts.TryGetValue(id, out var v) ? v.Trim() : "";
        var company = Get("company.name");
        var heading = Get("contact.formTitle") is { Length: > 0 } t ? t : "Kontakt";
        var title = heading + (company != "" ? $" | {company}" : "");
        var description = Describe(Get("contact.formIntro"), 160);
        if (description == "") description = "Skriv til Custombike og fortæl, hvad du har brug for. Vi vender tilbage.";
        return SetHead(template, title, description, Absolute("kontakt"));
    }

    private static string ThanksHead(string template, IReadOnlyDictionary<string, string> texts)
    {
        string Get(string id) => texts.TryGetValue(id, out var v) ? v.Trim() : "";
        var company = Get("company.name");
        var heading = Get("contact.thanksTitle") is { Length: > 0 } t ? t : "Tak for din henvendelse";
        var html = Regex.Replace(template, @"<title>[\s\S]*?</title>",
            _ => $"<title>{Html.Encode(heading + (company != "" ? $" | {company}" : ""))}</title>");
        html = Regex.Replace(html, @"\s*<link\s+rel=""canonical""\s+href=""[^""]*""\s*/?>", "");
        return html.Replace("</title>", "</title>\n    <meta name=\"robots\" content=\"noindex\" />");
    }

    private string CookieHead(string template, IReadOnlyDictionary<string, string> texts)
    {
        string Get(string id) => texts.TryGetValue(id, out var v) ? v.Trim() : "";
        var company = Get("company.name");
        var heading = Get("cookie.title") is { Length: > 0 } t ? t : "Cookiepolitik";
        var title = heading + (company != "" ? $" | {company}" : "");
        var description = Describe(Get("cookie.body"), 160);
        if (description == "") description = "Sådan bruger Custombike cookies, og hvordan du giver eller trækker dit samtykke tilbage.";
        return SetHead(template, title, description, Absolute("cookiepolitik"));
    }

    private static string NotFoundHead(string template, IReadOnlyDictionary<string, string> texts)
    {
        string Get(string id) => texts.TryGetValue(id, out var v) ? v.Trim() : "";
        var company = Get("company.name");
        var heading = Get("notFound.title") is { Length: > 0 } t ? t : "Siden findes ikke";
        var html = Regex.Replace(template, @"<title>[\s\S]*?</title>",
            _ => $"<title>{Html.Encode(heading + (company != "" ? $" | {company}" : ""))}</title>");
        html = Regex.Replace(html, @"\s*<link\s+rel=""canonical""\s+href=""[^""]*""\s*/?>", "");
        return html.Replace("</title>", "</title>\n    <meta name=\"robots\" content=\"noindex\" />");
    }

    private static string SetHead(string html, string title, string description, string url)
    {
        html = Regex.Replace(html, @"<title>[\s\S]*?</title>", _ => $"<title>{Html.Encode(title)}</title>");
        html = ReplaceMeta(html, "name", "description", description);
        html = Regex.Replace(html, @"<link\s+rel=""canonical""\s+href=""[^""]*""\s*/?>", _ => $"<link rel=\"canonical\" href=\"{Html.Encode(url)}\" />");
        html = ReplaceMeta(html, "property", "og:title", title);
        html = ReplaceMeta(html, "property", "og:description", description);
        html = ReplaceMeta(html, "property", "og:url", url);
        html = ReplaceMeta(html, "name", "twitter:title", title);
        html = ReplaceMeta(html, "name", "twitter:description", description);
        return html;
    }

    private static string ReplaceMeta(string html, string attr, string key, string value) =>
        Regex.Replace(html,
            $@"<meta\s+{attr}=""{Regex.Escape(key)}""\s+content=""[^""]*""\s*/?>",
            _ => $"<meta {attr}=\"{key}\" content=\"{Html.Encode(value)}\" />");

    // Tekst til meta-beskrivelse: uden ## Overskrift ##-markeringer, paa een linje, afkortet ved et ord
    private static string Describe(string text, int max)
    {
        var plain = Regex.Replace(text, @"^[ \t]*##[ \t]*(.*?)[ \t]*(?:##)?[ \t]*$",
            m => m.Groups[1].Value is { Length: > 0 } h ? (".!?:".Contains(h[^1]) ? h : h + ".") : "",
            RegexOptions.Multiline);
        plain = Regex.Replace(plain, @"\s+", " ").Trim();
        if (plain.Length <= max) return plain;
        var cut = plain.LastIndexOf(' ', max - 1);
        return plain[..(cut > max / 2 ? cut : max - 1)].TrimEnd(',', '.', ';', ':', '–', '-', ' ') + "…";
    }

    // "Sviegade 2B, 1., 6760 Ribe" -> "Ribe"
    private static string CityFromAddress(string address)
    {
        var lastComma = address.LastIndexOf(',');
        var m = Regex.Match(lastComma > 0 ? address[(lastComma + 1)..].Trim() : "", @"^(\d{4})\s+(.+)$");
        return m.Success ? m.Groups[2].Value : "";
    }

    // Strukturerede data til Google (schema.org), bygget ud fra de redigerbare tekster,
    // saa navn, telefon, mail, adresse og CVR altid svarer til det, der staar paa siden.
    private string BuildJsonLd(IReadOnlyDictionary<string, string> texts, IReadOnlyList<ServiceDto> services, ServiceDto? current)
    {
        string Get(string id) => texts.TryGetValue(id, out var v) ? v.Trim() : "";

        // "Sviegade 2B, 1., 6760 Ribe" -> gade = alt foer sidste komma; "6760 Ribe" -> postnr. + by
        var address = Get("company.address");
        var lastComma = address.LastIndexOf(',');
        var street = lastComma > 0 ? address[..lastComma].Trim() : address;
        var cityMatch = Regex.Match(lastComma > 0 ? address[(lastComma + 1)..].Trim() : "", @"^(\d{4})\s+(.+)$");
        var city = cityMatch.Success ? cityMatch.Groups[2].Value : "";

        var postal = new Dictionary<string, object?>
        {
            ["@type"] = "PostalAddress",
            ["streetAddress"] = cityMatch.Success ? street : address,
            ["postalCode"] = cityMatch.Success ? cityMatch.Groups[1].Value : null,
            ["addressLocality"] = cityMatch.Success ? city : null,
            ["addressCountry"] = "DK",
        };

        var cvr = Regex.Replace(Get("company.cvr"), @"\D", "");
        var ld = new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "Store",
            ["@id"] = Absolute("") + "#virksomhed",
            ["name"] = Get("company.name"),
            ["legalName"] = Get("company.legalName"),
            ["url"] = Absolute(""),
            ["logo"] = Absolute("apple-touch-icon.png"),
            ["image"] = Absolute("og-image.png"),
            ["telephone"] = Regex.Replace(Get("contact.phone"), @"[^\d+]", ""),
            ["email"] = Get("contact.email"),
            ["address"] = address == "" ? null : Compact(postal),
            ["vatID"] = cvr == "" ? null : "DK" + cvr,
            ["areaServed"] = city == "" ? null : new Dictionary<string, object?> { ["@type"] = "City", ["name"] = city },
            // Ydelserne som et katalog med links til deres egne sider
            ["hasOfferCatalog"] = services.Count == 0 ? null : new Dictionary<string, object?>
            {
                ["@type"] = "OfferCatalog",
                ["name"] = Get("services.title") is { Length: > 0 } catalog ? catalog : "Produkter",
                ["itemListElement"] = services.Select(s => new Dictionary<string, object?>
                {
                    ["@type"] = "Offer",
                    ["itemOffered"] = new Dictionary<string, object?>
                    {
                        ["@type"] = "Product",
                        ["@id"] = Absolute("produkter/" + s.Slug) + "#produkt",
                        ["name"] = s.Title,
                        ["url"] = Absolute("produkter/" + s.Slug),
                    },
                }).ToList(),
            },
        };

        // System.Text.Json escaper < > & osv., saa teksterne ikke kan bryde ud af <script>
        var scripts = $"<script type=\"application/ld+json\">{JsonSerializer.Serialize(Compact(ld))}</script>";

        // Paa en ydelses side: selve ydelsen (knyttet til virksomheden via @id) og en broedkrumme
        if (current != null)
        {
            var url = Absolute("produkter/" + current.Slug);
            var service = new Dictionary<string, object?>
            {
                ["@context"] = "https://schema.org",
                ["@type"] = "Product",
                ["@id"] = url + "#produkt",
                ["name"] = current.Title,
                ["serviceType"] = current.Title,
                ["description"] = Describe(current.ShortText != "" ? current.ShortText : current.Body, 300),
                ["url"] = url,
                ["image"] = current.CoverUrl == null ? null : Absolute(current.CoverUrl),
                ["provider"] = new Dictionary<string, object?> { ["@id"] = Absolute("") + "#virksomhed" },
                ["areaServed"] = city == "" ? null : new Dictionary<string, object?> { ["@type"] = "City", ["name"] = city },
            };
            var crumbs = new Dictionary<string, object?>
            {
                ["@context"] = "https://schema.org",
                ["@type"] = "BreadcrumbList",
                ["itemListElement"] = new object[]
                {
                    new Dictionary<string, object?> { ["@type"] = "ListItem", ["position"] = 1, ["name"] = Get("company.name") is { Length: > 0 } n ? n : "Forside", ["item"] = Absolute("") },
                    new Dictionary<string, object?> { ["@type"] = "ListItem", ["position"] = 2, ["name"] = Get("services.title") is { Length: > 0 } y ? y : "Produkter", ["item"] = Absolute("") + "#produkter" },
                    new Dictionary<string, object?> { ["@type"] = "ListItem", ["position"] = 3, ["name"] = current.Title, ["item"] = url },
                },
            };
            scripts += $"<script type=\"application/ld+json\">{JsonSerializer.Serialize(Compact(service))}</script>"
                + $"<script type=\"application/ld+json\">{JsonSerializer.Serialize(crumbs)}</script>";
        }
        return scripts;
    }

    // Udelader tomme felter
    private static Dictionary<string, object?> Compact(Dictionary<string, object?> d) =>
        d.Where(kv => kv.Value is not null && !(kv.Value is string s && s == ""))
         .ToDictionary(kv => kv.Key, kv => kv.Value);

    // Forsidens titel og beskrivelse står i index.html. Her gøres adresser absolutte, når Site:Url er sat.
    private string ApplyHomeHead(string html)
    {
        if (_siteUrl.Length == 0) return html;
        html = Regex.Replace(html, @"<link\s+rel=""canonical""\s+href=""[^""]*""\s*/?>",
            _ => $"<link rel=\"canonical\" href=\"{Html.Encode(_siteUrl)}\" />");
        html = ReplaceMeta(html, "property", "og:url", _siteUrl);
        html = ReplaceMeta(html, "property", "og:image", Absolute("og-image.png"));
        html = ReplaceMeta(html, "name", "twitter:image", Absolute("og-image.png"));
        return html;
    }

    // Google Tag Manager og Search Console sættes kun ind, når de er konfigureret. Tomt = ingen Google-tags.
    private string DecorateTemplate(string html)
    {
        var verify = string.Concat(_verifications.Select(v =>
            $"\n    <meta name=\"google-site-verification\" content=\"{Html.Encode(v)}\" />"));
        var gtmHead = "";
        var gtmBody = "";
        if (_gtmId.Length > 0)
        {
            gtmHead =
                "\n    <!-- Google Tag Manager -->\n    <script>(function(w,d,s,l,i){w[l]=w[l]||[];w[l].push({\"gtm.start\":\n" +
                "    new Date().getTime(),event:\"gtm.js\"});var f=d.getElementsByTagName(s)[0],\n" +
                "    j=d.createElement(s),dl=l!=\"dataLayer\"?\"&l=\"+l:\"\";j.async=true;j.src=\n" +
                "    \"https://www.googletagmanager.com/gtm.js?id=\"+i+dl;f.parentNode.insertBefore(j,f);\n" +
                "    })(window,document,\"script\",\"dataLayer\",\"" + _gtmId + "\");</script>\n    <!-- End Google Tag Manager -->";
            gtmBody =
                "\n    <!-- Google Tag Manager (noscript) -->\n    <noscript><iframe src=\"https://www.googletagmanager.com/ns.html?id=" +
                _gtmId + "\"\n    height=\"0\" width=\"0\" style=\"display:none;visibility:hidden\" title=\"Google Tag Manager\"></iframe></noscript>\n    <!-- End Google Tag Manager (noscript) -->";
        }
        return html
            .Replace("<!--site-verification-->", verify)
            .Replace("<!--gtm-head-->", gtmHead)
            .Replace("<!--gtm-body-->", gtmBody);
    }

    // Indlaeser index.html og server-bundlet igen, hvis filerne er blevet aendret (nyt build)
    private void ReloadIfChanged()
    {
        var templateStamp = File.GetLastWriteTimeUtc(TemplatePath);
        if (_template == null || templateStamp != _templateStamp)
        {
            _template = DecorateTemplate(File.ReadAllText(TemplatePath));
            _templateStamp = templateStamp;
            _cache.Clear();
        }

        if (!File.Exists(_bundlePath))
        {
            if (!_warnedMissing)
                _logger.LogWarning("Prerender: {Bundle} findes ikke - siden sendes uden forudrenderet HTML", _bundlePath);
            _warnedMissing = true;
            _engine = null;
            return;
        }

        _warnedMissing = false;
        var bundleStamp = File.GetLastWriteTimeUtc(_bundlePath);
        if (_engine != null && bundleStamp == _bundleStamp) return;

        try
        {
            var engine = new Engine(o => o.LimitRecursion(5000).TimeoutInterval(TimeSpan.FromSeconds(10)));
            engine.Execute(Polyfills);
            engine.Execute(File.ReadAllText(_bundlePath));
            _engine = engine;
            _logger.LogInformation("Prerender: indlaeste {Bundle}", _bundlePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Prerender: kunne ikke indlaese {Bundle}", _bundlePath);
            _engine = null;
        }
        _bundleStamp = bundleStamp;
        _cache.Clear();
    }
}
