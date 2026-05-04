using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;

namespace GMForce.Bricks.Tests.Arrangement.Builders;

internal sealed class ActionExecutingContextBuilder
{
    private readonly ModelStateDictionary _modelState = new();
    private readonly DefaultHttpContext _httpContext = new();

    internal ActionExecutingContextBuilder WithInvalidModelState()
    {
        _modelState.AddModelError("key", "error");
        return this;
    }

    internal ActionExecutingContextBuilder WithHeader(string key, string value)
    {
        _httpContext.Request.Headers[key] = value;
        return this;
    }

    internal ActionExecutingContext Build()
    {
        var actionContext = new ActionContext(_httpContext, new RouteData(), new ActionDescriptor(), _modelState);
        return new ActionExecutingContext(actionContext, [], new Dictionary<string, object?>(), null!);
    }
}
