using Microsoft.AspNetCore.Http;

namespace GMForce.Bricks.Initialization.Middlewares;

public class LocalizationMiddleware(RequestDelegate next, CultureResolver cultureResolver)
{
    private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));
    private readonly CultureResolver _cultureResolver = cultureResolver ?? throw new ArgumentNullException(nameof(cultureResolver));

    public async Task InvokeAsync(HttpContext context)
    {
        var culture = GetRequestCultureFrom(context);
        _cultureResolver.SetCulture(culture);

        await _next(context);
    }

    private static string GetRequestCultureFrom(HttpContext context)
    {
        const string languageHeaderKey = "Accept-Language";
        var languageHeader = context.Request.Headers[languageHeaderKey];
        return languageHeader.SingleOrDefault() ?? string.Empty;
    }
}
