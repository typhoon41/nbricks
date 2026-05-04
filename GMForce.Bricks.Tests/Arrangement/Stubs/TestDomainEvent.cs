using GMForce.NDDD.Contracts;

namespace GMForce.Bricks.Tests.Arrangement.Stubs;

internal sealed class TestDomainEvent : IDomainEvent
{
    public string Name => "TestEvent";
    public object Data => new();
}
