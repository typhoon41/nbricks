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

    protected override void OnApplicationRun()
    {
        if (Builder is null)
        {
            throw new InvalidOperationException("The WebApplicationBuilder is not initialized.");
        }

        _ = Builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        _ = Builder.Host.ConfigureContainer(_withGivenConfiguration);
        _configureServices();
        Builder.UseAspireServiceDiscovery();

        var application = Builder.Build();
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
            configureContainer(Builder ?? throw new InvalidOperationException("The WebApplicationBuilder is not initialized."), containerBuilder);
        };
        return this;
    }

    public ApiRunner ConfigureServices(Action<WebApplicationBuilder> configureServices)
    {
        _configureServices = () =>
        {
            if (Builder is null)
            {
                throw new InvalidOperationException("The WebApplicationBuilder is not initialized.");
            }

            configureServices(Builder);
            Builder.Services.AddHttpsSecurity();
            _ = Builder.Services.AddHttpContextAccessor();
            _ = Builder.Services.AddRouting(options => options.LowercaseUrls = true);
            // Must be PostConfigure due to: https://github.com/aspnet/Mvc/issues/7858
            _ = Builder.Services.PostConfigure<ApiBehaviorOptions>(options => options.FluentValidationBehavior());
        };
        return this;
    }

    public ApiRunner ConfigureApplication(Action<WebApplicationBuilder, WebApplication> configureApplication,
        Action<WebApplicationBuilder, WebApplication>? preconfigureApplication = null)
    {
        _configure = (application) =>
        {
            if (Builder is null)
            {
                throw new InvalidOperationException("The WebApplicationBuilder is not initialized.");
            }

            preconfigureApplication?.Invoke(Builder, application);
            _ = application.UseHttpsRedirection();
            application.UseExceptionsHandler();
            _ = application.UseRouting();
            _ = application.MapControllers();
            configureApplication(Builder, application);
            application.ConfigureFluentValidation();
        };

        return this;
    }
}
