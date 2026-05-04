using GMForce.NDDD.Persistance;

namespace GMForce.Bricks.Tests.Arrangement.Stubs;

internal sealed record TestEntity : EntityDto
{
    internal string Name { get; set; } = string.Empty;
}
