using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace GMForce.Bricks.Initialization.Http;

[ExcludeFromCodeCoverage]
internal static class SecurityExtensions
{
    internal static void AddHttpsSecurity(this IServiceCollection services)
    {
        _ = services.AddHttpsRedirection(options =>
        {
            options.RedirectStatusCode = (int)HttpStatusCode.PermanentRedirect;
            options.HttpsPort = 443;
        });
        _ = services.AddHsts(options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromDays(365);
        });
    }
}
