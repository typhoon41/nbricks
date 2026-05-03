using GMForce.NDDD.Contracts;

namespace GMForce.Bricks.Persistence.Dispatchers;

public class NoEventsDispatcher : IDispatchEvents
{
    public Task Dispatch(IDomainEvent domainEvent) => Task.CompletedTask;
}
