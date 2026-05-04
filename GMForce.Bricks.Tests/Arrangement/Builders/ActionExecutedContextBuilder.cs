using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;

namespace GMForce.Bricks.Tests.Arrangement.Builders;

internal sealed class ActionExecutedContextBuilder
{
    private readonly ModelStateDictionary _modelState = new();
    private Exception? _exception;
    private IActionResult? _result;

    internal ActionExecutedContextBuilder WithInvalidModelState()
    {
        _modelState.AddModelError("key", "error");
        return this;
    }

    internal ActionExecutedContextBuilder WithException(Exception exception)
    {
        _exception = exception;
        return this;
    }

    internal ActionExecutedContextBuilder WithStatusCode(int statusCode)
    {
        _result = new StatusCodeResult(statusCode);
        return this;
    }

    internal ActionExecutedContext Build()
    {
        var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor(), _modelState);
        var context = new ActionExecutedContext(actionContext, [], null!)
        {
            Exception = _exception,
            Result = _result
        };
        return context;
    }
}
