using FakeItEasy;
using GMForce.Bricks.Tests.Arrangement.Stubs;
using GMForce.NDDD.Contracts;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Shouldly;

namespace GMForce.Bricks.Tests.Persistence;

internal sealed class BaseDbContextFixture : IDisposable
{
    private IDispatchEvents _eventsDispatcher = null!;
    private IStoreEvents _eventsStore = null!;
    private TestDbContext _context = null!;

    [SetUp]
    public void SetUp()
    {
        _eventsDispatcher = A.Fake<IDispatchEvents>();
        _eventsStore = A.Fake<IStoreEvents>();
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;
        _context = new TestDbContext(_eventsDispatcher, _eventsStore, options);
    }

    [TearDown]
    public void TearDown() => Dispose();

    public void Dispose() => _context?.Dispose();

    [Test]
    public void NullDispatcherThrows()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase("x").Options;

        void act()
        {
            new TestDbContext(null!, _eventsStore, options);
        }

        Should.Throw<ArgumentNullException>(act);
    }

    [Test]
    public void NullStoreThrows()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>().UseInMemoryDatabase("x").Options;

        void act()
        {
            new TestDbContext(_eventsDispatcher, null!, options);
        }

        Should.Throw<ArgumentNullException>(act);
    }

    [Test]
    public async Task SaveChangesAsyncEntityWithDomainEventDispatchesIt()
    {
        var domainEvent = new TestDomainEvent();
        var entity = new TestEntity { Id = Guid.NewGuid() };
        entity.AddDomainEvent(domainEvent);
        _context.Entities.Add(entity);

        await _context.SaveChangesAsync();

        A.CallTo(() => _eventsDispatcher.Dispatch(domainEvent)).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task SaveChangesAsyncClearsDomainEventsAfterDispatch()
    {
        var entity = new TestEntity { Id = Guid.NewGuid() };
        entity.AddDomainEvent(new TestDomainEvent());
        _context.Entities.Add(entity);

        await _context.SaveChangesAsync();

        entity.DomainEvents.ShouldBeEmpty();
    }

    [Test]
    public async Task SaveChangesAsyncAlwaysPublishesIntegrationEvents()
    {
        _context.Entities.Add(new TestEntity { Id = Guid.NewGuid() });

        await _context.SaveChangesAsync();

        A.CallTo(() => _eventsStore.Publish()).MustHaveHappenedOnceExactly();
    }
}
