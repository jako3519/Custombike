using System.Security.Claims;
using Backend.Data;
using Backend.Security;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

DotNetEnv.Env.Load();
// Neon CLI skriver DATABASE_URL til repo-root .env.local — indlæs den oven i Backend/.env
foreach (var envPath in new[]
{
    Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", ".env.local")),
    Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), ".env.local")),
})
{
    if (File.Exists(envPath))
        DotNetEnv.Env.Load(envPath);
}

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
var isDev = builder.Environment.IsDevelopment();

// Fly.io/Railway fortæller hvilken port appen skal lytte på via PORT
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// I drift kommer databasen som DATABASE_URL (postgresql://bruger:kode@host:port/db)
var connectionString = config.GetConnectionString("DefaultConnection");
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrWhiteSpace(databaseUrl))
{
    connectionString = ConnectionStringFromUrl(databaseUrl);
}

// Frontendens adresse – bruges til CORS og til at sende brugeren tilbage efter login
var frontendUrl = (config["Frontend:Url"] ?? "http://localhost:5173").TrimEnd('/');

var googleClientId = config["GoogleOAuth:ClientId"];
var googleClientSecret = config["GoogleOAuth:ClientSecret"];
var googleConfigured = !string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret);

if (!googleConfigured)
    Console.WriteLine("ADVARSEL: GoogleOAuth__ClientId / GoogleOAuth__ClientSecret mangler – siden kører, men login er slået fra.");

// Bag hostingens HTTPS-proxy (Fly.io/Railway): stol på X-Forwarded-Proto, så appen ved at den kører over https
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers(options =>
{
    // Dobbelt sikkerhed: alle POST/PUT/DELETE kræver Admin, også uden [Authorize]
    options.Filters.Add<AdminOnlyWritesFilter>();
});

// --- CORS til Vite-frontend (med cookies) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowViteApp", policy =>
        policy.WithOrigins(frontendUrl)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// --- Database ---
builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(connectionString));

// Noegler til login-cookies gemmes i databasen (ikke i containeren, som nulstilles ved hver genstart)
builder.Services.AddDataProtection()
    .SetApplicationName("carlton-paa-taget")
    .PersistKeysToDbContext<DataContext>();

// --- Besoegsstatistik (cookiefri, kun synlig for admin) - se Controllers/Analytics ---
builder.Services.AddSingleton<Backend.Analytics.GeoLookup>();
builder.Services.AddSingleton<Backend.Analytics.VisitorHasher>();
builder.Services.AddHostedService<Backend.Analytics.AnalyticsCleanupService>();
builder.Services.AddTransient<Backend.Contact.ContactEmailSender>();
builder.Services.AddHostedService<Backend.Contact.ContactCleanupService>();

// --- Prerendering af siden (SSR) - se Prerender/PrerenderService.cs ---
builder.Services.AddSingleton<Backend.Prerender.PrerenderService>();

// --- Login: Google OAuth -> egen HttpOnly-cookie med rollen Admin ---
var authBuilder = builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.Name = "carlton.auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        // Lokalt kører vi http; i drift må cookien kun sendes over https
        options.Cookie.SecurePolicy = isDev ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;

        // API'et skal svare 401/403 i stedet for at redirecte til en login-side
        options.Events.OnRedirectToLogin = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = ctx =>
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };

        // Tjekkes ved hver request: er man ikke (længere) på admin-listen, er man ikke logget ind
        options.Events.OnValidatePrincipal = async ctx =>
        {
            var email = ctx.Principal?.FindFirstValue(ClaimTypes.Email);
            var isAdmin = ctx.Principal?.IsInRole("Admin") == true && AdminWhitelist.Contains(email);

            if (!isAdmin)
            {
                ctx.RejectPrincipal();
                await ctx.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        };
    });

// Google-login slås kun til, når nøglerne findes (ellers fejler alle requests)
if (googleConfigured)
{
    authBuilder.AddGoogle(options =>
    {
        options.ClientId = googleClientId!;
        options.ClientSecret = googleClientSecret!;
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        // Gør at login også virker over http på localhost
        options.CorrelationCookie.SameSite = SameSiteMode.Lax;
        options.CorrelationCookie.SecurePolicy = isDev ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;

        // Login der fejler (fx annulleret hos Google eller et gammelt/ugyldigt login-forsoeg):
        // tilbage til siden med en besked i stedet for en fejlside. Man kan bare proeve igen.
        options.Events.OnRemoteFailure = ctx =>
        {
            var returnUrl = ctx.Properties?.RedirectUri ?? (isDev ? frontendUrl + "/" : "/");
            ctx.HandleResponse();
            ctx.Response.Redirect(QueryHelpers.AddQueryString(returnUrl, "login", "failed"));
            return Task.CompletedTask;
        };

        options.Events.OnTicketReceived = ctx =>
        {
            var email = ctx.Principal?.FindFirstValue(ClaimTypes.Email);
            var returnUrl = ctx.Properties?.RedirectUri ?? frontendUrl;

            // Ikke admin: ingen session, bare tilbage til siden
            if (!AdminWhitelist.Contains(email))
            {
                ctx.HandleResponse();
                ctx.Response.Redirect(QueryHelpers.AddQueryString(returnUrl, "login", "denied"));
                return Task.CompletedTask;
            }

            if (ctx.Principal?.Identity is ClaimsIdentity identity)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
            }
            return Task.CompletedTask;
        };
    });
}

builder.Services.AddAuthorization();

// --- Rate limiting pr. klient-IP (beskytter mod misbrug/overbelastning) ---
// Statiske filer (JS/CSS/billeder) serveres foer limiteren og taeller ikke med.
// Alle oevrige requests: 120/min pr. IP. Login: 10/min. Admin-aendringer (POST/PUT/DELETE/PATCH): 30/min.
// For mange requests -> 429 Too Many Requests.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = (ctx, _) =>
    {
        if (ctx.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            ctx.HttpContext.Response.Headers.RetryAfter = ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();
        return ValueTask.CompletedTask;
    };

    static RateLimitPartition<string> Window(string key, int permits, int minutes = 1) =>
        RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permits,
            Window = TimeSpan.FromMinutes(minutes),
            QueueLimit = 0,
            AutoReplenishment = true,
        });

    // Fly's interne health check (GET /api/texts) kommer ikke gennem Fly-proxyen og har derfor
    // ingen Fly-Client-IP-header - den maa aldrig blive begraenset.
    static bool IsHealthCheck(HttpContext ctx) =>
        HttpMethods.IsGet(ctx.Request.Method)
        && ctx.Request.Path.Equals("/api/texts", StringComparison.OrdinalIgnoreCase)
        && !ctx.Request.Headers.ContainsKey("Fly-Client-IP");

    options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
        // 1) Generel graense for al trafik
        PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
            IsHealthCheck(ctx)
                ? RateLimitPartition.GetNoLimiter("health")
                : Window("all:" + ClientIp(ctx), 120)),
        // 2) Strammere graenser for login og admin-aendringer
        PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
        {
            var path = ctx.Request.Path;
            if (path.StartsWithSegments("/api/auth/login", StringComparison.OrdinalIgnoreCase)
                || path.StartsWithSegments("/api/auth/logout", StringComparison.OrdinalIgnoreCase)
                || path.StartsWithSegments("/signin-google", StringComparison.OrdinalIgnoreCase))
                return Window("auth:" + ClientIp(ctx), 10);

            var method = ctx.Request.Method;
            // Offentlig kontaktformular: strammere loft end øvrige skriv (5 henvendelser pr. 15 min. pr. IP)
            if (HttpMethods.IsPost(method) && path.Equals("/api/contact", StringComparison.OrdinalIgnoreCase))
                return Window("contact:" + ClientIp(ctx), 5, 15);

            var isWrite = HttpMethods.IsPost(method) || HttpMethods.IsPut(method)
                || HttpMethods.IsDelete(method) || HttpMethods.IsPatch(method);
            if (isWrite && path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
                return Window("write:" + ClientIp(ctx), 30);

            return RateLimitPartition.GetNoLimiter("none");
        }));
});

var app = builder.Build();

// Skal ligge først, så resten af pipelinen ser den rigtige scheme (https)
app.UseForwardedHeaders();

// Primaert domaene er carltonpaataget.dk: sidevisninger paa www og fly.dev sendes videre med 301
// (samme sti og query). Kun offentlig trafik gennem Fly-proxyen (har headeren Fly-Client-IP), saa
// Flys interne health checks ikke paavirkes. /signin-google undtages, saa et Google-login der er
// startet paa en anden adresse kan gennemfoeres der (login-cookien hoerer til den adresse).
var canonicalHost = (config["Site:CanonicalHost"] ?? "carltonpaataget.dk").Trim().ToLowerInvariant();
var redirectHosts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    "www." + canonicalHost,
    "carlton-paa-taget.fly.dev",
};
app.Use(async (ctx, next) =>
{
    var req = ctx.Request;
    if (!isDev
        && req.Headers.ContainsKey("Fly-Client-IP")
        && redirectHosts.Contains(req.Host.Host)
        && (HttpMethods.IsGet(req.Method) || HttpMethods.IsHead(req.Method))
        && !req.Path.StartsWithSegments("/signin-google", StringComparison.OrdinalIgnoreCase))
    {
        var target = "https://" + canonicalHost + req.PathBase.ToUriComponent()
            + req.Path.ToUriComponent() + req.QueryString.ToUriComponent();
        ctx.Response.Redirect(target, permanent: true);
        return;
    }
    await next();
});

// Opdater databasen automatisk ved opstart (nye migrationer køres ved deploy)
using (var scope = app.Services.CreateScope())
{
    try
    {
        // Migrationer kræver direkte (ikke-pooler) forbindelse på Neon
        var migrateUrl = Environment.GetEnvironmentVariable("DATABASE_URL_UNPOOLED")
            ?? Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrWhiteSpace(migrateUrl))
        {
            var migrateOptions = new DbContextOptionsBuilder<DataContext>()
                .UseNpgsql(ConnectionStringFromUrl(migrateUrl))
                .Options;
            using var migrateDb = new DataContext(migrateOptions);
            migrateDb.Database.Migrate();
        }
        else
        {
            scope.ServiceProvider.GetRequiredService<DataContext>().Database.Migrate();
        }
    }
    catch (Exception ex) when (isDev)
    {
        Console.WriteLine("ADVARSEL: Kunne ikke migrere databasen — kører den? " + ex.Message);
    }

    // Hashes for billeder/videoer fra før de blev gemt ved upload (så samme billede kun vises én gang)
    try
    {
        var hashed = await Backend.Content.MediaHashes.BackfillAsync(
            scope.ServiceProvider.GetRequiredService<DataContext>(), app.Environment, CancellationToken.None);
        if (hashed > 0) Console.WriteLine($"Medie-hashes udfyldt for {hashed} filer.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("ADVARSEL: Kunne ikke udfylde medie-hashes: " + ex.Message);
    }
}

if (isDev)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

// Vedhæftede billeder fra kontaktformularen må ikke hentes som almindelige statiske filer
app.Use(async (ctx, next) =>
{
    if (ctx.Request.Path.StartsWithSegments("/gallery/contact"))
    {
        ctx.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }
    await next();
});

app.UseStaticFiles();  // frontend + billeder fra wwwroot/gallery

// Efter statiske filer (de taeller ikke med), foer login, saa ogsaa /signin-google begraenses
app.UseRateLimiter();

app.UseRouting();
app.UseCors("AllowViteApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// /admin (ogsaa /Admin): er man allerede admin, gaar man til forsiden i redigeringstilstand.
// Ellers sendes man til login. Afgoeres paa serveren ud fra login-cookien.
app.MapGet("/admin", (HttpContext ctx) =>
{
    if (ctx.User.Identity?.IsAuthenticated == true && ctx.User.IsInRole("Admin"))
        return Results.Redirect("/");
    var home = $"{ctx.Request.Scheme}://{ctx.Request.Host}/";
    return Results.Redirect(QueryHelpers.AddQueryString("/api/auth/login", "returnUrl", home));
});

// sitemap.xml laves ud fra databasen, saa nye ydelser kommer med automatisk
app.MapGet("/sitemap.xml", async (DataContext db, CancellationToken ct) =>
{
    var siteUrl = (config["Site:Url"] ?? "https://carltonpaataget.dk/").TrimEnd('/') + "/";
    var services = await db.Services.AsNoTracking()
        .OrderBy(s => s.SortOrder).ThenBy(s => s.Id)
        .Select(s => new { s.Slug, s.UpdatedAt })
        .ToListAsync(ct);

    var xml = new System.Text.StringBuilder();
    xml.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
    xml.Append("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">\n");
    xml.Append($"  <url>\n    <loc>{System.Security.SecurityElement.Escape(siteUrl)}</loc>\n    <changefreq>monthly</changefreq>\n    <priority>1.0</priority>\n  </url>\n");
    xml.Append($"  <url>\n    <loc>{System.Security.SecurityElement.Escape(siteUrl + "cookiepolitik")}</loc>\n    <changefreq>yearly</changefreq>\n    <priority>0.3</priority>\n  </url>\n");
    xml.Append($"  <url>\n    <loc>{System.Security.SecurityElement.Escape(siteUrl + "kontakt")}</loc>\n    <changefreq>monthly</changefreq>\n    <priority>0.7</priority>\n  </url>\n");
    foreach (var s in services)
    {
        xml.Append($"  <url>\n    <loc>{System.Security.SecurityElement.Escape(siteUrl + "ydelser/" + s.Slug)}</loc>\n");
        xml.Append($"    <lastmod>{s.UpdatedAt.ToUniversalTime():yyyy-MM-dd}</lastmod>\n    <changefreq>monthly</changefreq>\n    <priority>0.8</priority>\n  </url>\n");
    }
    xml.Append("</urlset>\n");
    return Results.Text(xml.ToString(), "application/xml; charset=utf-8");
});

// Alt andet end /api går til frontenden
// Siderne (forsiden, /ydelser/<navn> m.m.) renderes paa serveren med de aktuelle tekster, billeder
// og ydelser, saa HTML'en altid indeholder hele siden. Browseren overtager den bagefter (hydrate).
// En ydelse der ikke findes giver 404 (med en "siden findes ikke"-side).
app.MapFallback(async (HttpContext ctx, Backend.Prerender.PrerenderService prerender, DataContext db) =>
{
    var page = await prerender.RenderPageAsync(db, ctx.Request.Path.Value ?? "/", ctx.RequestAborted);
    if (page is null)
    {
        ctx.Response.StatusCode = StatusCodes.Status404NotFound; // frontenden er ikke bygget ind i wwwroot
        return;
    }
    if (page.RedirectTo is not null)
    {
        // Query (fx ?utm_source=...) foer et evt. #-anker: /?x=1#ydelser
        var hashAt = page.RedirectTo.IndexOf('#');
        var target = hashAt < 0
            ? page.RedirectTo + ctx.Request.QueryString.ToUriComponent()
            : page.RedirectTo[..hashAt] + ctx.Request.QueryString.ToUriComponent() + page.RedirectTo[hashAt..];
        ctx.Response.Redirect(target, permanent: true);
        return;
    }
    ctx.Response.StatusCode = page.StatusCode;
    ctx.Response.ContentType = "text/html; charset=utf-8";
    ctx.Response.Headers.CacheControl = "no-cache";
    await ctx.Response.WriteAsync(page.Html, ctx.RequestAborted);
});

app.Run();


// Klientens IP: Fly-proxyen saetter Fly-Client-IP; ellers forbindelsens adresse
static string ClientIp(HttpContext ctx)
{
    var flyIp = ctx.Request.Headers["Fly-Client-IP"].ToString();
    if (!string.IsNullOrWhiteSpace(flyIp))
        return flyIp.Trim();
    return ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}

// Omsætter postgresql://bruger:kode@host:port/database til Npgsql-format
static string ConnectionStringFromUrl(string url)
{
    var uri = new Uri(url);
    var userInfo = uri.UserInfo.Split(':', 2);

    var csb = new Npgsql.NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Database = uri.AbsolutePath.TrimStart('/'),
        Username = Uri.UnescapeDataString(userInfo[0]),
        Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "",
        SslMode = uri.Host.Contains("neon.tech", StringComparison.OrdinalIgnoreCase)
            ? Npgsql.SslMode.Require
            : Npgsql.SslMode.Prefer
    };

    // Neon m.fl. kræver krypteret forbindelse (?sslmode=require i adressen)
    if (uri.Query.Contains("sslmode=require", StringComparison.OrdinalIgnoreCase))
    {
        csb.SslMode = Npgsql.SslMode.Require;
    }
    return csb.ConnectionString;
}
