using System.Diagnostics.CodeAnalysis;

namespace GMForce.Bricks.Initialization.Documentation;

[ExcludeFromCodeCoverage]
public record ApiDetails
{
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}
