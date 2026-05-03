using FluentValidation.Internal;
using System.Linq.Expressions;
using System.Reflection;

namespace GMForce.Bricks.Initialization.Http;
internal static class CamelCaseResolver
{
    internal static string ResolvePropertyName(Type _, MemberInfo memberInfo, LambdaExpression expression)
    {
        var propertyName = DefaultPropertyNameResolver(memberInfo, expression);
        return ToCamelCase(propertyName);
    }

    private static string DefaultPropertyNameResolver(MemberInfo memberInfo, LambdaExpression expression)
    {
        if (expression == null)
        {
            return ResultFrom(memberInfo);
        }

        var chain = PropertyChain.FromExpression(expression);
        return chain.Count > 0 ? chain.ToString() : ResultFrom(memberInfo);
    }

    private static string ToCamelCase(string original) =>
        string.IsNullOrEmpty(original) ? string.Empty : char.ToLowerInvariant(original[0]) + original.Substring(1);

    private static string ResultFrom(MemberInfo memberInfo) => memberInfo != null ? memberInfo.Name : string.Empty;
}
