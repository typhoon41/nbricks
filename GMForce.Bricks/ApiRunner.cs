using Autofac;
using Autofac.Extensions.DependencyInjection;
using GMForce.Bricks.Initialization;
using GMForce.Bricks.Initialization.Exceptions;
using GMForce.Bricks.Initialization.Http;
using GMForce.Bricks.Initialization.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GMForce.Bricks;

public class ApiRunner : ApplicationRunner
{
    private Action<HostBuilderContext, ContainerBuilder> _withGivenConfiguration = (context, builder) => { };
    private Action _configureServices = () => { };
    private Action<WebApplication> _configure = (application) => { };

    private WebApplicationBuilder RequiredBuilder =>
        Builder ?? throw new InvalidOperationException("The WebApplicationBuilder is not initialized.");

    protected override void OnApplicationRun()
    {
        var builder = RequiredBuilder;
        _ = builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        _ = builder.Host.ConfigureContainer(_withGivenConfiguration);
        _configureServices();
        builder.UseAspireServiceDiscovery();

        var application = builder.Build();
        _ = application.MapDefaultEndpoints();

        _configure(application);

        application.Run();
    }

    public ApiRunner ConfigureContainer(Action<WebApplicationBuilder, ContainerBuilder> configureContainer)
    {
        _withGivenConfiguration = (host, containerBuilder) =>
        {
            containerBuilder.ComponentRegistryBuilder.Registered += (sender, args) =>
                args.ComponentRegistration.PipelineBuilding += (pipelineSender, pipelineArgs) =>
                    pipelineArgs.Use(new AutofacExceptionMiddleware());
            configureContainer(RequiredBuilder, containerBuilder);
        };
        return this;
    }

    public ApiRunner ConfigureServices(Action<WebApplicationBuilder> configureServices)
    {
        _configureServices = () =>
        {
            var builder = RequiredBuilder;
            configureServices(builder);
            builder.Services.AddHttpsSecurity();
            _ = builder.Services.AddHttpContextAccessor();
            _ = builder.Services.AddRouting(options => options.LowercaseUrls = true);
            // Must be PostConfigure due to: https://github.com/aspnet/Mvc/issues/7858
            _ = builder.Services.PostConfigure<ApiBehaviorOptions>(options => options.FluentValidationBehavior());
        };
        return this;
    }

    public ApiRunner ConfigureApplication(Action<WebApplicationBuilder, WebApplication> configureApplication,
        Action<WebApplicationBuilder, WebApplication>? preconfigureApplication = null)
    {
        _configure = (application) =>
        {
            var builder = RequiredBuilder;
            preconfigureApplication?.Invoke(builder, application);
            _ = application.UseHttpsRedirection();
            application.UseExceptionsHandler();
            _ = application.UseRouting();
            _ = application.MapControllers();
            configureApplication(builder, application);
            application.ConfigureFluentValidation();
        };

        return this;
    }
}
