using FluentValidation;
using FluentValidation.Results;
using GMForce.Bricks.Initialization.Http;
using GMForce.Bricks.Tests.Arrangement.Stubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;

namespace GMForce.Bricks.Tests.Initialization.Http;

internal sealed class ContextExtensionsFixture
{
    private DefaultHttpContext _httpContext = null!;

    [SetUp]
    public void SetUp()
    {
        var services = new ServiceCollection();
        services.AddSingleton<TestService>();
        _httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };
    }

    [Test]
    public void ResolveRegisteredServiceReturnsIt()
    {
        var result = _httpContext.Resolve<TestService>();

        result.ShouldNotBeNull();
    }

    [Test]
    public void ResolveMissingServiceThrows()
    {
        void act() => _httpContext.Resolve<string>();

        Should.Throw<InvalidOperationException>(act);
    }

    [Test]
    public void InvalidResponseFromModelStateBuildsCorrectResult()
    {
        var modelState = new Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary();
        modelState.AddModelError("name", "Name is required");

        var result = _httpContext.InvalidResponseFrom(modelState);

        var badRequest = result.ShouldBeOfType<BadRequestObjectResult>();
        var problem = badRequest.Value.ShouldBeOfType<ValidationProblemDetails>();
        problem.Errors.ShouldContainKey("name");
    }

    [Test]
    public void InvalidResponseFromValidationExceptionCopiesErrors()
    {
        var failures = new[] { new ValidationFailure("email", "Email is required") };
        var exception = new ValidationException(failures);

        var result = _httpContext.InvalidResponseFrom(exception);

        result.Errors.ShouldContainKey("email");
        result.Errors["email"].ShouldContain("Email is required");
    }
}
