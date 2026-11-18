using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Backend.Security;

// Sikkerhedsnet: alle requests der ændrer noget (POST/PUT/PATCH/DELETE) kræver rollen Admin,
// også hvis man glemmer [Authorize] på et nyt endpoint. Brug [AllowAnonymous] for undtagelser.
public class AdminOnlyWritesFilter : IAuthorizationFilter
{
    private static readonly HashSet<string> SafeMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "GET", "HEAD", "OPTIONS"
    };

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (SafeMethods.Contains(context.HttpContext.Request.Method)) return;
        if (context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any()) return;

        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated == true && user.IsInRole("Admin")) return;

        // Ingen Admin-rolle behandles som ikke logget ind
        context.Result = new UnauthorizedResult();
    }
}
