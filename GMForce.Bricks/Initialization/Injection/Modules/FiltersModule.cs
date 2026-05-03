using System.Diagnostics.CodeAnalysis;
using Autofac;
using GMForce.Bricks.Initialization.Filters;
using GMForce.Bricks.Initialization.Injection;

namespace GMForce.Bricks.Initialization.Injection.Modules;

[ExcludeFromCodeCoverage]
public class FiltersModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterSelfLifetimeScope<RollbackTransactionFilter>();
        builder.RegisterSelfLifetimeScope<TransactionFilter>();
    }
}
