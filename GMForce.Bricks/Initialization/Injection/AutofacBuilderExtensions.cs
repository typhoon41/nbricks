using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using Autofac;
using Autofac.Builder;
using Microsoft.AspNetCore.Http;

namespace GMForce.Bricks.Initialization.Injection;

[ExcludeFromCodeCoverage]
public static class AutofacBuilderExtensions
{
    public static IRegistrationBuilder<ClaimsPrincipal, SimpleActivatorData, SingleRegistrationStyle>
        RegisterPrincipal(this ContainerBuilder builder) => builder.Register(GetPrincipal).As<IPrincipal>().InstancePerLifetimeScope();

    private static ClaimsPrincipal GetPrincipal(IComponentContext componentContext)
        => componentContext.Resolve<IHttpContextAccessor>().HttpContext?.User!;

    public static void AncestorRegistration<TDerived, TBase>(this ContainerBuilder builder) where TDerived : TBase where TBase : class =>
        builder.RegisterType<TDerived>()
               .As<TBase>()
               .InstancePerLifetimeScope();

    public static void DefaultInterfaceRegistration<T>(this ContainerBuilder builder) where T : class =>
        builder.RegisterType<T>()
               .AsImplementedInterfaces()
               .InstancePerLifetimeScope();

    public static void RegisterSelfLifetimeScope<T>(this ContainerBuilder builder) where T : class =>
        builder.RegisterType<T>()
            .AsSelf()
            .InstancePerLifetimeScope();

    public static void SingleInterfaceRegistration<T>(this ContainerBuilder builder) where T : class =>
        builder.RegisterType<T>()
           .AsImplementedInterfaces()
           .SingleInstance();

    public static void SingleSelfRegistration<T>(this ContainerBuilder builder,
        Func<IComponentContext, T> definition) where T : class =>
        builder.Register(definition)
               .AsSelf()
               .SingleInstance();

    public static void SingleSelfRegistration<T>(this ContainerBuilder builder) where T : class =>
        builder.RegisterType<T>()
            .AsSelf()
            .SingleInstance();

    public static void RegisterAllImplementationsIn(this ContainerBuilder builder, Assembly assembly, Type interfaceType) =>
         builder.RegisterAssemblyTypes(assembly)
                .Where(type => ImplementsInterface(interfaceType, type))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

    private static bool ImplementsInterface(Type interfaceType, Type concreteType)
    {
        bool IsGeneric(Type type)
        {
            return interfaceType.IsGenericTypeDefinition && type.IsGenericType;
        }

        Type ExtractReal(Type type)
        {
            return IsGeneric(type) ? type.GetGenericTypeDefinition() : type;
        }

        return concreteType.GetInterfaces()
                           .Any(type => ExtractReal(type) == interfaceType);
    }
}
