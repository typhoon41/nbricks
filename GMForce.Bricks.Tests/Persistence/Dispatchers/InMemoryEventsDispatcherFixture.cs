using FakeItEasy;
using GMForce.Bricks.Persistence.Dispatchers;
using GMForce.Bricks.Persistence.Dispatchers.Storage;
using GMForce.NDDD.Contracts;
using NUnit.Framework;

namespace GMForce.Bricks.Tests.Persistence.Dispatchers;

internal sealed class InMemoryEventsDispatcherFixture
{
    private IHoldDispatcherStorage _storage = null!;
    private InMemoryEventsDispatcher _dispatcher = null!;

    [SetUp]
    public void SetUp()
    {
        _storage = A.Fake<IHoldDispatcherStorage>();
        _dispatcher = new InMemoryEventsDispatcher(_storage);
    }

    [Test]
    public async Task DispatchEnqueuesToStorage()
    {
        var domainEvent = A.Fake<IDomainEvent>();

        await _dispatcher.Dispatch(domainEvent);

        A.CallTo(() => _storage.EnqueueAsync(domainEvent)).MustHaveHappenedOnceExactly();
    }
}
