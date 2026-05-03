using System.Diagnostics.CodeAnalysis;
using FluentValidation;
using GMForce.Bricks.Configuration;
using GMForce.Bricks.Initialization.Http;
using GMForce.Bricks.Logging.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GMForce.Bricks.Initialization.Exceptions;
internal static class HandlingExtensions
{
    internal static void UseExceptionsHandler(this IApplicationBuilder builder) => _ = builder.UseExceptionHandler(new ExceptionHandlerOptions
    {
        AllowStatusCode404Response = true,
        ExceptionHandler = HandleExceptionFrom
    });

    private static async Task HandleExceptionFrom(HttpContext context)
    {
        var logger = context.Resolve<ILogger<Exception>>();
        var exception = context.Features.Get<IExceptionHandlerPathFeature>()!.Error;
        switch (exception)
        {
            case ValidationException validationException:
                logger.LogDebug(exception, "Validation exception");
                await HandleValidationExceptionFrom(context, validationException);
                return;
            default:
                logger.LogError(exception, "Unexpected error");
                await HandleUnexpectedErrorFrom(context, exception);
                break;
        }
    }

    private static async Task HandleValidationExceptionFrom(HttpContext context,
        ValidationException validationException)
    {
        var validationResult = context.InvalidResponseFrom(validationException);
        var jsonResult = new JsonResult(validationResult)
        {
            ContentType = "application/problem+json",
            StatusCode = StatusCodes.Status400BadRequest
        };
        await ExecuteResult(context, jsonResult);
    }

    [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract")]
    private static async Task HandleUnexpectedErrorFrom(HttpContext context, Exception exception)
    {
        var accept = context.Request.GetTypedHeaders().Accept;
        if (accept != null && accept.All(header => header.MediaType != "application/json"))
        {
            // server does not accept Json, leaving to default MVC error page handler.
            return;
        }

        var problemDetails = GetProblemDetails(context, exception);
        var jsonResult = new JsonResult(problemDetails)
        {
            ContentType = "application/problem+json",
            StatusCode = 500
        };

        var environment = context.RequestServices.GetService<IHostEnvironment>();
        if (environment?.IsDevelopment() == false)
        {
            await NotifyDeveloperAbout(context, exception);
        }

        await ExecuteResult(context, jsonResult);
    }

    private static async Task NotifyDeveloperAbout(HttpContext context, Exception exception)
    {
        var emailService = context.Resolve<ISendEmail>();
        var developer = context.Resolve<DeveloperContact>();
        await emailService.Send(developer.Email, "Unhandled exception occurred!", $"{exception.Message}: {exception.StackTrace}");
    }

    private static async Task ExecuteResult(HttpContext context, IActionResult actionResult)
    {
        var routeData = context.GetRouteData();
        var actionDescriptor = new ActionDescriptor();
        var actionContext = new ActionContext(context, routeData, actionDescriptor);
        await actionResult.ExecuteResultAsync(actionContext);
    }

    private static ProblemDetails GetProblemDetails(HttpContext context, Exception exception)
    {
        var problemDetails = new ProblemDetails
        {
            Title = "An unexpected error occurred!",
            Instance = context.Request.Path,
            Status = StatusCodes.Status500InternalServerError,
            Detail = string.Empty
        };

        return problemDetails;
    }
}
