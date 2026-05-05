using System.Globalization;
using GMForce.Bricks.Initialization;
using GMForce.Bricks.Initialization.Middlewares;
using GMForce.Bricks.Tests.Arrangement.Codebooks;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using Shouldly;

namespace GMForce.Bricks.Tests.Initialization.Middlewares;

internal sealed class LocalizationMiddlewareFixture
{
    [Test]
    public void NullNextThrows()
    {
        static void act()
        {
            new LocalizationMiddleware(null!, new CultureResolver([Cultures.Default]));
        }

        Should.Throw<ArgumentNullException>(act);
    }

    [Test]
    public void NullCultureResolverThrows()
    {
        static void act()
        {
            new LocalizationMiddleware(static _ => Task.CompletedTask, null!);
        }

        Should.Throw<ArgumentNullException>(act);
    }

    [Test]
    public async Task AcceptLanguageHeaderPresentSetsCulture()
    {
        var resolver = new CultureResolver([Cultures.Default, Cultures.Secondary]);
        string? capturedCulture = null;
        Task next(HttpContext _)
        {
            capturedCulture = CultureInfo.CurrentCulture.Name;
            return Task.CompletedTask;
        }
        var middleware = new LocalizationMiddleware(next, resolver);
        var context = new DefaultHttpContext();
        context.Request.Headers.AcceptLanguage = Cultures.Secondary;

        await middleware.InvokeAsync(context);

        capturedCulture.ShouldBe(Cultures.Secondary);
    }

    [Test]
    public async Task NoAcceptLanguageHeaderSetsEmptyString()
    {
        var resolver = new CultureResolver([Cultures.Default]);
        string? capturedCulture = null;
        Task next(HttpContext _)
        {
            capturedCulture = CultureInfo.CurrentCulture.Name;
            return Task.CompletedTask;
        }
        var middleware = new LocalizationMiddleware(next, resolver);
        var context = new DefaultHttpContext();

        await middleware.InvokeAsync(context);

        capturedCulture.ShouldBe(Cultures.Default);
    }
}
