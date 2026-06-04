using FitTrack.Models;
using Microsoft.AspNetCore.Identity;

namespace FitTrack.Middleware;

public class ProfileSetupMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly string[] ExemptPrefixes =
    [
        "/profile/setup",
        "/account/logout",
        "/account/login",
        "/account/register",
        "/admin",           // Admin users never need profile setup
        "/home/error",
        "/_",
        "/favicon",
        "/lib/",
        "/css/",
        "/js/"
    ];

    public ProfileSetupMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
            var isExempt = ExemptPrefixes.Any(p => path.StartsWith(p));

            if (!isExempt)
            {
                var user = await userManager.GetUserAsync(context.User);

                if (user is not null && !user.HasCompletedProfile)
                {
                    // Admins are exempt — they can navigate freely without a profile
                    var isAdmin = await userManager.IsInRoleAsync(user, "Admin");
                    if (!isAdmin)
                    {
                        context.Response.Redirect("/Profile/Setup");
                        return;
                    }
                }
            }
        }

        await _next(context);
    }
}
