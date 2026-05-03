using FluentValidation;
using GMForce.Bricks.Initialization.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;

namespace GMForce.Bricks.Initialization.Http;
internal static class ValidationExtensions
{
    internal static void ConfigureFluentValidation(this IApplicationBuilder _) => ValidatorOptions.Global.PropertyNameResolver = CamelCaseResolver.ResolvePropertyName;

    internal static void FluentValidationBehavior(this ApiBehaviorOptions options) => options.InvalidModelStateResponseFactory = context => context.HttpContext.InvalidResponseFrom(context.ModelState);
}
