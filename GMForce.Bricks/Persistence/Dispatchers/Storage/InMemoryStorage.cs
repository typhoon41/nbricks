using System.Threading.Channels;
using GMForce.NDDD.Contracts;

namespace GMForce.Bricks.Persistence.Dispatchers.Storage;

public class InMemoryStorage : IHoldDispatcherStorage
{
    private readonly Channel<IDomainEvent> _channel;

    public InMemoryStorage()
    {
        _channel = Channel.CreateUnbounded<IDomainEvent>();
    }

    public async Task EnqueueAsync(IDomainEvent message) => await _channel.Writer.WriteAsync(message);

    public async Task<IDomainEvent> DequeueAsync() => await _channel.Reader.ReadAsync();

    public IAsyncEnumerable<IDomainEvent> ReadAllAsync(CancellationToken cancellationToken = default) => _channel.Reader.ReadAllAsync(cancellationToken);
}
