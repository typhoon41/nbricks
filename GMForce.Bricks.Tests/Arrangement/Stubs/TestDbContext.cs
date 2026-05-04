using System.Reflection;
using GMForce.Bricks.Persistence;
using GMForce.NDDD.Contracts;
using GMForce.NDDD.Persistance;
using Microsoft.EntityFrameworkCore;

namespace GMForce.Bricks.Tests.Arrangement.Stubs;

internal sealed class TestDbContext(IDispatchEvents eventsDispatcher, IStoreEvents eventsStore, DbContextOptions<TestDbContext> options)
    : BaseDbContext(eventsDispatcher, eventsStore, options)
{
    internal DbSet<TestEntity> Entities { get; set; } = null!;

    protected override Assembly CurrentAssembly => typeof(TestDbContext).Assembly;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<TestEntity>().Ignore(nameof(EntityDto.DomainEvents));
    }
}
