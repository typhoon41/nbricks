# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

**GMForce.Bricks** is a NuGet utility library for .NET 10 ASP.NET Core applications. It wires up Autofac DI, FluentValidation, EF Core, Serilog, OpenTelemetry, and Swagger into a fluent bootstrap API. It is consumed by other projects — it is not a runnable application itself.

## Build Commands

```bash
# Restore (lock file enforced)
dotnet restore

# Build
dotnet build --configuration Release --no-incremental

# Pack NuGet
dotnet pack --configuration Release -p:Version=<version>
```

`TreatWarningsAsErrors=true` is set globally — the build will fail on any warning.  
Package versions are centrally managed in `Directory.Packages.props`; never add a `Version` attribute to a `<PackageReference>` in the `.csproj`.

## Architecture

### Bootstrap Flow

Consumer apps call `new ApiRunner().RunWith(options)` after chaining three configuration steps:

```
ApiRunner
  .ConfigureContainer(...)    // Autofac ContainerBuilder wiring
  .ConfigureServices(...)     // IServiceCollection registration
  .ConfigureApplication(...)  // IApplicationBuilder middleware pipeline
```

`ApiRunner` extends the abstract `ApplicationRunner`, which owns the `WebApplicationBuilder`, bootstraps Serilog, and calls the template method `OnApplicationRun()`. The Autofac pipeline automatically wraps every component with `AutofacExceptionMiddleware`.

### Assembly Discovery

`AssemblyFinder(projectPrefix)` provides `Assembly.Load("{prefix}.Api")` and `Assembly.Load("{prefix}.Infrastructure")` by convention. Pass it to `MvcExtensions.AddMvc()` and Autofac bulk registration helpers.

### Dependency Injection Helpers (`Initialization/Injection/`)

`AutofacBuilderExtensions` provides shorthand Autofac registration methods:
- `DefaultInterfaceRegistration<T>` — registers as implemented interfaces, `InstancePerLifetimeScope`
- `AncestorRegistration<TDerived, TBase>` — registers derived as base, scoped
- `RegisterAllImplementationsIn(assembly, interfaceType)` — bulk-registers all types implementing an interface from an assembly
- `RegisterPrincipal` — resolves `ClaimsPrincipal` from `IHttpContextAccessor`

Autofac **modules** to load in consumer apps:
- `OrmModule<TDbContext>` — registers EF Core `DbContext` and `IUnitOfWork`
- `FiltersModule` — registers `TransactionFilter` and `RollbackTransactionFilter`

`MvcExtensions.AddMvc(filters, converters, assemblies)` wires FluentValidation auto-validation and the transaction filters globally.

### Persistence (`Persistence/`)

`BaseDbContext` overrides `SaveChangesAsync` to gather domain events from `EntityDto` entities (via `GMForce.NDDD`), dispatch them via `IDispatchEvents`, then publish queued integration events via `IStoreEvents`. Subclasses must implement `CurrentAssembly` — this assembly is scanned for `IEntityTypeConfiguration` implementations and all `EntityDto` subclasses.

`EntityFrameworkUnitOfWork<T>` implements `IUnitOfWork`. `CancelSaving()` suppresses the next `SaveChangesAsync` call.

**Transaction filter pair**: `TransactionFilter` calls `IUnitOfWork.SaveChangesAsync()` after a successful action (2xx, no exception, valid model state). `RollbackTransactionFilter` cancels saving on failure.

**Event infrastructure**:
- `InMemoryEventsDispatcher` enqueues domain events into `IHoldDispatcherStorage`
- `EventsStore` queues integration events and publishes them through `IDispatchEvents`
- `AuditEventsHandler` logs `IAuditUser` events to a dedicated audit log sink

### Logging (`Logging/`)

`SerilogLogger` configures two file sinks with daily rolling:
- Main log — excludes `AuditEventsHandler` source
- Audit log — includes only `AuditEventsHandler` source (file path derived by replacing `.log` with `audit-.log`)

In Development, an additional OpenTelemetry sink is added. Log level is `Debug` in Development, `Warning` in Production.

The log file path must be provided in `appsettings.json` under the key `LogPath`.

### Observability (`Initialization/AspireExtensions.cs`)

`UseAspireServiceDiscovery()` configures:
- OpenTelemetry metrics (ASP.NET Core, HTTP client, runtime) and traces (with health check path filtering)
- Standard resilience handlers on all `HttpClient` instances
- Aspire service discovery on all `HttpClient` instances
- Health check endpoints `/health` and `/alive` (exposed in Development only)

### Security

`ValidateCaptchaAttribute` (action filter) validates Google reCAPTCHA Enterprise tokens from `Captcha` and `CaptchaAction` request headers. Validation is **skipped in non-production** environments. Requires `CaptchaSettings` (with `ClientKey`, `ApiKey`, `ProjectName`) to be registered in DI.

`DeveloperContact` (registered in DI) receives an email notification via `ISendEmail` whenever an unhandled exception occurs in non-development environments.

### Configuration

`configuration.ResolveFrom<T>("SectionName")` — typed config section accessor that throws `InvalidOperationException` if the section is absent.

## Deployment

CI/CD is Azure Pipelines (`Deployment/azure-pipelines.yml`). Merges to `main` trigger a build; NuGet publish requires manual approval. Version is set in the pipeline variable `applicationVersion`. The library is built as a portable `net10.0` package — no runtime identifier is set.
