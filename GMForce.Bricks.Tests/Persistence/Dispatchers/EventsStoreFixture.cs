using FakeItEasy;
using GMForce.Bricks.Persistence.Dispatchers;
using GMForce.NDDD.Contracts;
using NUnit.Framework;
using Shouldly;

namespace GMForce.Bricks.Tests.Persistence.Dispatchers;

internal sealed class EventsStoreFixture
{
    private IDispatchEvents _eventsDispatcher = null!;
    private EventsStore _eventStore = null!;

    [SetUp]
    public void SetUp()
    {
        _eventsDispatcher = A.Fake<IDispatchEvents>();
        _eventStore = new EventsStore(_eventsDispatcher);
    }

    [Test]
    public void NullDispatcherThrows()
    {
        static void act()
        {
            new EventsStore(null!);
        }

        Should.Throw<ArgumentNullException>(act);
    }

    [Test]
    public async Task AddThenPublishDispatchesAllEventsInOrder()
    {
        var event1 = A.Fake<IDomainEvent>();
        var event2 = A.Fake<IDomainEvent>();
        _eventStore.Add(event1);
        _eventStore.Add(event2);

        await _eventStore.Publish();

        A.CallTo(() => _eventsDispatcher.Dispatch(event1)).MustHaveHappenedOnceExactly()
            .Then(A.CallTo(() => _eventsDispatcher.Dispatch(event2)).MustHaveHappenedOnceExactly());
    }

    [Test]
    public async Task PublishWhenEmptyMakesNoDispatchCalls()
    {
        await _eventStore.Publish();

        A.CallTo(() => _eventsDispatcher.Dispatch(A<IDomainEvent>._)).MustNotHaveHappened();
    }
}
