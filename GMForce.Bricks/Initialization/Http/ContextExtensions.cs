using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using ValidationException = FluentValidation.ValidationException;

namespace GMForce.Bricks.Initialization.Http;

[ExcludeFromCodeCoverage]
internal static class ContextExtensions
{
    internal static T Resolve<T>(this HttpContext httpContext) =>
        httpContext.RequestServices.GetService<T>() ?? throw new InvalidOperationException("Trying to get non-existent service!");

    internal static BadRequestObjectResult InvalidResponseFrom(this HttpContext _, ModelStateDictionary modelState)
    {
        var problemDetails = new ValidationProblemDetails(modelState);
        return ToBadRequestResult(problemDetails);
    }

    internal static ValidationProblemDetails InvalidResponseFrom(this HttpContext context, ValidationException validationException)
    {
        ArgumentNullException.ThrowIfNull(validationException);

        var problemDetails = ValidationProblemDetails(context);
        CopyErrorsFrom(problemDetails, validationException.Errors);
        return problemDetails;
    }

    private static ValidationProblemDetails ValidationProblemDetails(HttpContext context) => new()
    {
        Instance = context.Request.Path,
        Status = StatusCodes.Status400BadRequest,
        Type = "https://asp.net/core",
        Detail = "Please refer to the errors property for additional details."
    };

    private static BadRequestObjectResult ToBadRequestResult(ValidationProblemDetails problemDetails) => new(problemDetails)
    {
        ContentTypes = { "application/problem+json", "application/problem+xml" }
    };

    private static void CopyErrorsFrom(ValidationProblemDetails problemDetails, IEnumerable<ValidationFailure> validationErrors)
    {
        foreach (var validationError in validationErrors)
        {
            var key = validationError.PropertyName;
            if (!problemDetails.Errors.TryGetValue(key, out var messages))
            {
                messages = Array.Empty<string>();
            }

            messages = [.. messages, validationError.ErrorMessage];
            problemDetails.Errors[key] = messages;
        }
    }
}
