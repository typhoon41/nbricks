using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using GMForce.NDDD.Contracts;
using GMForce.NDDD.Persistance;
using Microsoft.EntityFrameworkCore;

namespace GMForce.Bricks.Persistence;

public abstract class BaseDbContext : DbContext
{
    private readonly IDispatchEvents _eventsDispatcher;
    private readonly IStoreEvents _eventsStore;

    protected BaseDbContext([NotNull] IDispatchEvents eventsDispatcher,
        [NotNull] IStoreEvents eventsStore, DbContextOptions options) : base(options)
    {
        _eventsDispatcher = eventsDispatcher ?? throw new ArgumentNullException(nameof(eventsDispatcher));
        _eventsStore = eventsStore ?? throw new ArgumentNullException(nameof(eventsStore));
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = GatherDomainEvents();
        var result = await base.SaveChangesAsync(cancellationToken);

        foreach (var @event in domainEvents)
        {
            await Dispatch(@event);
        }

        // Publish integration events that have nothing to do with entity modifications.
        await Publish();
        return result;
    }

    private async Task Dispatch(IDomainEvent @event) => await _eventsDispatcher.Dispatch(@event);

    private async Task Publish() => await _eventsStore.Publish();

    private IEnumerable<IDomainEvent> GatherDomainEvents()
    {
        var entries = ChangeTracker.Entries<EntityDto>()
            .Where(x => x.Entity.DomainEvents.Any())
            .ToList();
        var domainEvents = entries.SelectMany(x => x.Entity.DomainEvents)
            .ToList();
        entries.ToList()
        .ForEach(entry => entry.Entity.ClearDomainEvents());

        return domainEvents;
    }

    protected abstract Assembly CurrentAssembly { get; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        _ = modelBuilder.ApplyConfigurationsFromAssembly(CurrentAssembly);
        RegisterEntities(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    private void RegisterEntities(ModelBuilder modelBuilder)
    {
        var entityMethod = modelBuilder.GetEntityMethod();
        var entityTypes = CurrentAssembly.GetTypes()
            .Where(x => x.IsSubclassOf(typeof(EntityDto)) && !x.IsAbstract);
        foreach (var type in entityTypes)
        {
            _ = entityMethod.MakeGenericMethod(type).Invoke(modelBuilder, []);
        }
    }
}
