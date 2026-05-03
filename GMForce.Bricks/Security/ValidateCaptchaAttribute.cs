using GMForce.Bricks.Initialization.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GMForce.Bricks.Security;

public sealed class ValidateCaptchaAttribute : ActionFilterAttribute
{
    private const string CaptchaHeader = "Captcha";
    private const string CaptchaAction = "CaptchaAction";
    private const double Threshold = 0.25;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly CaptchaSettings _settings;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ValidateCaptchaAttribute> _logger;

    public ValidateCaptchaAttribute(IHttpClientFactory httpClientFactory, CaptchaSettings settings, IWebHostEnvironment environment,
        ILogger<ValidateCaptchaAttribute> logger)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(logger);
        _httpClientFactory = httpClientFactory;
        _settings = settings;
        _environment = environment;
        _logger = logger;
    }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        if (!_environment.IsProduction())
        {
            _ = await next();
            return;
        }

        var captchaHeader = context.HttpContext.Request.Headers[CaptchaHeader];
        var captchaAction = context.HttpContext.Request.Headers[CaptchaAction];

        if (!captchaHeader.Any() || !captchaAction.Any())
        {
            _logger.LogDebug("Missing captcha headers. Returning status code 400...");
            context.Result = new BadRequestResult();
            return;
        }

        var url = GetUrlFrom(_settings);
        _logger.LogDebug("Sending captcha validation to {Url}", GetBaseUrlFrom(_settings));
        using var httpClient = _httpClientFactory.CreateClient();
        var input = new Request()
        {
            Event = new RequestBody
            {
                ExpectedAction = captchaAction.First()!,
                SiteKey = _settings.ClientKey,
                Token = captchaHeader.First()!
            }
        };
        var result = await httpClient.PostWithResponseAsync<Request, Response>(url, input);

        if (result.Invalid(Threshold))
        {
            _logger.LogWarning("Captcha validation failed due to low result score of {Score}. Reasons: {Reasons}", result.RiskAnalysis.Score, result.InvalidReason);
            context.Result = new BadRequestResult();
            return;
        }

        _logger.LogDebug("Captcha validation was successful with score: {Score}", result.RiskAnalysis.Score);
        _ = await next();
    }

    private static string GetBaseUrlFrom(CaptchaSettings settings) =>
        $"https://recaptchaenterprise.googleapis.com/v1/projects/{settings.ProjectName}/assessments";

    private static string GetUrlFrom(CaptchaSettings settings) =>
        $"{GetBaseUrlFrom(settings)}?key={settings.ApiKey}";
}
