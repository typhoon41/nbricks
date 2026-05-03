using GMForce.NDDD.Contracts;

namespace GMForce.Bricks.Persistence.Dispatchers;

public class EventsStore(IDispatchEvents eventsDispatcher) : IStoreEvents
{
    private readonly IDispatchEvents _eventsDispatcher = eventsDispatcher ?? throw new ArgumentNullException(nameof(eventsDispatcher));
    private readonly Queue<IDomainEvent> _events = new();

    public void Add(IDomainEvent domainEvent) => _events.Enqueue(domainEvent);

    public async Task Publish()
    {
        while (_events.Count > 0)
        {
            var nextEvent = _events.Dequeue();
            await _eventsDispatcher.Dispatch(nextEvent);
        }
    }
}
