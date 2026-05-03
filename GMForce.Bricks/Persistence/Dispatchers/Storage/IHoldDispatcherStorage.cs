using GMForce.NDDD.Contracts;

namespace GMForce.Bricks.Persistence.Dispatchers.Storage;

public interface IHoldDispatcherStorage
{
    Task EnqueueAsync(IDomainEvent message);
    Task<IDomainEvent> DequeueAsync();
    IAsyncEnumerable<IDomainEvent> ReadAllAsync(CancellationToken cancellationToken = default);
}
