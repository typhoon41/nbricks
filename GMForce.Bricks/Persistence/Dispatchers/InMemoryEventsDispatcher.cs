using GMForce.Bricks.Persistence.Dispatchers.Storage;
using GMForce.NDDD.Contracts;

namespace GMForce.Bricks.Persistence.Dispatchers;

public class InMemoryEventsDispatcher(IHoldDispatcherStorage inMemoryStore) : IDispatchEvents
{
    public async Task Dispatch(IDomainEvent domainEvent) => await inMemoryStore.EnqueueAsync(domainEvent);
}
