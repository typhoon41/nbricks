using FakeItEasy;
using GMForce.Bricks.Persistence.Dispatchers;
using GMForce.NDDD.Contracts;
using NUnit.Framework;
using Shouldly;

namespace GMForce.Bricks.Tests.Persistence.Dispatchers;

internal sealed class NoEventsDispatcherFixture
{
    private NoEventsDispatcher _dispatcher = null!;

    [SetUp]
    public void SetUp() => _dispatcher = new NoEventsDispatcher();

    [Test]
    public async Task DispatchCompletesSuccessfully()
    {
        var domainEvent = A.Fake<IDomainEvent>();

        Task act()
        {
            return _dispatcher.Dispatch(domainEvent);
        }

        await Should.NotThrowAsync(act);
    }
}
