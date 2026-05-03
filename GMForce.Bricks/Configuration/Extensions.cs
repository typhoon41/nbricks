using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace GMForce.Bricks.Configuration;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static T ResolveFrom<T>(this IConfiguration configuration, string sectionName)
    {
        var sectionSettings = configuration.GetSection(sectionName).Get<T>() ??
            throw new InvalidOperationException($"Section is missing from configuration. Section Name: {sectionName}");

        return sectionSettings;
    }
}
