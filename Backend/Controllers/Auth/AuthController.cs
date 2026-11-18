using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;

    public AuthController(IConfiguration config, IWebHostEnvironment env)
    {
        _config = config;
        _env = env;
    }

    // GET /api/auth/login?returnUrl=http://localhost:5173/
    // Sender brugeren til Google og bagefter tilbage til frontenden.
    [HttpGet("login")]
    public IActionResult Login([FromQuery] string? returnUrl)
    {
        var frontendUrl = (_config["Frontend:Url"] ?? "http://localhost:5173").TrimEnd('/');
        // I drift serverer backend selv frontenden, saa login maa vende tilbage til den adresse,
        // det blev startet fra (fly.dev eller carltonpaataget.dk). Lokalt er det Vite (Frontend:Url).
        // Andre adresser afvises stadig (ingen open redirect).
        var requestOrigin = $"{Request.Scheme}://{Request.Host}";
        var allowed = IsFrontendUrl(returnUrl, frontendUrl) || IsFrontendUrl(returnUrl, requestOrigin);
        var fallback = _env.IsDevelopment() ? frontendUrl + "/" : requestOrigin + "/";
        var target = allowed ? returnUrl! : fallback;

        // select_account: Google viser altid kontovalg, saa man ikke logges stille ind
        // med den Google-konto, der tilfaeldigvis er logget ind i browseren.
        var properties = new GoogleChallengeProperties { RedirectUri = target, Prompt = "select_account" };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    // GET /api/auth/me -> hvem er logget ind
    [HttpGet("me")]
    public IActionResult Me()
    {
        // Ingen Admin-rolle = samme som ikke logget ind
        if (User.Identity?.IsAuthenticated != true || !User.IsInRole("Admin"))
        {
            return Ok(new { isAuthenticated = false, isAdmin = false });
        }

        return Ok(new
        {
            isAuthenticated = true,
            isAdmin = true,
            email = User.FindFirstValue(ClaimTypes.Email),
            name = User.FindFirstValue(ClaimTypes.Name)
        });
    }

    // POST /api/auth/logout
    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    // Forhindrer "open redirect": vi sender kun tilbage til vores egen frontend
    private static bool IsFrontendUrl(string? url, string frontendUrl)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;
        if (!Uri.TryCreate(url, UriKind.Absolute, out var target)) return false;
        if (!Uri.TryCreate(frontendUrl, UriKind.Absolute, out var frontend)) return false;

        return target.Scheme == frontend.Scheme
            && string.Equals(target.Host, frontend.Host, StringComparison.OrdinalIgnoreCase)
            && target.Port == frontend.Port;
    }
}
