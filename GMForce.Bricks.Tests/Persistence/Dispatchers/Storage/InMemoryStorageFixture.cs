using FakeItEasy;
using GMForce.Bricks.Persistence.Dispatchers.Storage;
using GMForce.NDDD.Contracts;
using NUnit.Framework;
using Shouldly;

namespace GMForce.Bricks.Tests.Persistence.Dispatchers.Storage;

internal sealed class InMemoryStorageFixture
{
    private InMemoryStorage _sut = null!;

    [SetUp]
    public void SetUp() => _sut = new InMemoryStorage();

    [Test]
    public async Task EnqueueThenDequeueSingleEventFIFO()
    {
        var domainEvent = A.Fake<IDomainEvent>();
        await _sut.EnqueueAsync(domainEvent);

        var result = await _sut.DequeueAsync();

        result.ShouldBe(domainEvent);
    }

    [Test]
    public async Task EnqueueThenDequeueMultipleEventsFIFO()
    {
        var event1 = A.Fake<IDomainEvent>();
        var event2 = A.Fake<IDomainEvent>();
        await _sut.EnqueueAsync(event1);
        await _sut.EnqueueAsync(event2);

        var first = await _sut.DequeueAsync();
        var second = await _sut.DequeueAsync();

        first.ShouldBe(event1);
        second.ShouldBe(event2);
    }

    [Test]
    public async Task ReadAllAsyncYieldsAllEnqueuedItems()
    {
        var event1 = A.Fake<IDomainEvent>();
        var event2 = A.Fake<IDomainEvent>();
        await _sut.EnqueueAsync(event1);
        await _sut.EnqueueAsync(event2);

        var items = new List<IDomainEvent>();
        using var cts = new CancellationTokenSource();
        try
        {
            await foreach (var item in _sut.ReadAllAsync(cts.Token))
            {
                items.Add(item);
                if (items.Count == 2)
                {
                    await cts.CancelAsync();
                }
            }
        }
        catch (OperationCanceledException) { }

        items.ShouldBe([event1, event2]);
    }
}
