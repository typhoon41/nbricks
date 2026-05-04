using System.Net;
using System.Text;
using System.Text.Json;
using FakeItEasy;
using GMForce.Bricks.Security;
using GMForce.Bricks.Tests.Arrangement;
using GMForce.Bricks.Tests.Arrangement.Builders;
using GMForce.Bricks.Tests.Arrangement.Codebooks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;

namespace GMForce.Bricks.Tests.Security;

internal sealed class ValidateCaptchaAttributeFixture
{
    private IHttpClientFactory _factory = null!;
    private CaptchaSettings _settings = null!;
    private ILogger<ValidateCaptchaAttribute> _logger = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = A.Fake<IHttpClientFactory>();
        _settings = new CaptchaSettings { ClientKey = "key", ApiKey = "api", ProjectName = "proj" };
        _logger = A.Fake<ILogger<ValidateCaptchaAttribute>>();
    }

    [Test]
    public void NullFactoryThrows()
    {
        void act()
        {
            new ValidateCaptchaAttribute(null!, _settings, ProductionEnvironment(), _logger);
        }

        Should.Throw<ArgumentNullException>(act);
    }

    [Test]
    public void NullSettingsThrows()
    {
        void act()
        {
            new ValidateCaptchaAttribute(_factory, null!, ProductionEnvironment(), _logger);
        }

        Should.Throw<ArgumentNullException>(act);
    }

    [Test]
    public void NullEnvironmentThrows()
    {
        void act()
        {
            new ValidateCaptchaAttribute(_factory, _settings, null!, _logger);
        }

        Should.Throw<ArgumentNullException>(act);
    }

    [Test]
    public void NullLoggerThrows()
    {
        void act()
        {
            new ValidateCaptchaAttribute(_factory, _settings, ProductionEnvironment(), null!);
        }

        Should.Throw<ArgumentNullException>(act);
    }

    [Test]
    public async Task NonProductionEnvironmentSkipsValidation()
    {
        var sut = new ValidateCaptchaAttribute(_factory, _settings, DevelopmentEnvironment(), _logger);
        var nextCalled = false;
        Task<ActionExecutedContext> next()
        {
            nextCalled = true;
            return Task.FromResult(new ActionExecutedContextBuilder().Build());
        }

        await sut.OnActionExecutionAsync(new ActionExecutingContextBuilder().Build(), next);

        nextCalled.ShouldBeTrue();
        A.CallTo(() => _factory.CreateClient(A<string>._)).MustNotHaveHappened();
    }

    [Test]
    public async Task MissingCaptchaHeaderReturns400()
    {
        var sut = new ValidateCaptchaAttribute(_factory, _settings, ProductionEnvironment(), _logger);
        var context = new ActionExecutingContextBuilder().Build();
        static Task<ActionExecutedContext> next()
        {
            return Task.FromResult(new ActionExecutedContextBuilder().Build());
        }

        await sut.OnActionExecutionAsync(context, next);

        context.Result.ShouldBeOfType<BadRequestResult>();
    }

    [Test]
    public async Task MissingActionHeaderReturns400()
    {
        var sut = new ValidateCaptchaAttribute(_factory, _settings, ProductionEnvironment(), _logger);
        var context = new ActionExecutingContextBuilder().WithHeader("Captcha", "token").Build();
        static Task<ActionExecutedContext> next()
        {
            return Task.FromResult(new ActionExecutedContextBuilder().Build());
        }

        await sut.OnActionExecutionAsync(context, next);

        context.Result.ShouldBeOfType<BadRequestResult>();
    }

    [Test]
    public async Task LowScoreReturns400()
    {
        using var client = ClientReturning(CaptchaResponse(valid: false, score: Scores.BelowThreshold));
        A.CallTo(() => _factory.CreateClient(A<string>._)).Returns(client);
        var sut = new ValidateCaptchaAttribute(_factory, _settings, ProductionEnvironment(), _logger);
        var context = new ActionExecutingContextBuilder()
            .WithHeader("Captcha", "token")
            .WithHeader("CaptchaAction", "action")
            .Build();
        Task<ActionExecutedContext> next()
        {
            return Task.FromResult(new ActionExecutedContextBuilder().Build());
        }

        await sut.OnActionExecutionAsync(context, next);

        context.Result.ShouldBeOfType<BadRequestResult>();
    }

    [Test]
    public async Task ValidScoreCallsNext()
    {
        using var client = ClientReturning(CaptchaResponse(valid: true, score: Scores.AboveThreshold));
        A.CallTo(() => _factory.CreateClient(A<string>._)).Returns(client);
        var sut = new ValidateCaptchaAttribute(_factory, _settings, ProductionEnvironment(), _logger);
        var context = new ActionExecutingContextBuilder()
            .WithHeader("Captcha", "token")
            .WithHeader("CaptchaAction", "action")
            .Build();
        var nextCalled = false;
        Task<ActionExecutedContext> next()
        {
            nextCalled = true;
            return Task.FromResult(new ActionExecutedContextBuilder().Build());
        }

        await sut.OnActionExecutionAsync(context, next);

        nextCalled.ShouldBeTrue();
    }

    private static IWebHostEnvironment ProductionEnvironment()
    {
        var env = A.Fake<IWebHostEnvironment>();
        A.CallTo(() => env.EnvironmentName).Returns(Environments.Production);
        return env;
    }

    private static IWebHostEnvironment DevelopmentEnvironment()
    {
        var env = A.Fake<IWebHostEnvironment>();
        A.CallTo(() => env.EnvironmentName).Returns(Environments.Development);
        return env;
    }

    private static HttpClient ClientReturning(string json)
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        return new HttpClient(new FakeMessageHandler(response));
    }

    private static string CaptchaResponse(bool valid, double score)
    {
        var payload = new
        {
            tokenProperties = new { valid, invalidReason = string.Empty },
            riskAnalysis = new { score, reasons = Array.Empty<string>() }
        };
        return JsonSerializer.Serialize(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    }
}
