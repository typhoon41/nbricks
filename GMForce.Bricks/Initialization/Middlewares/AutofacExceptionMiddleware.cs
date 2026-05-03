using Autofac;
using Autofac.Core.Resolving.Pipeline;
using Microsoft.Extensions.Logging;

namespace GMForce.Bricks.Initialization.Middlewares;
internal class AutofacExceptionMiddleware : IResolveMiddleware
{
    public PipelinePhase Phase => PipelinePhase.Activation;

    public void Execute(ResolveRequestContext context, Action<ResolveRequestContext> next)
    {
        ArgumentNullException.ThrowIfNull(next);

        try
        {
            next.Invoke(context);
        }
        catch (Exception exception)
        {
            var logger = context.Resolve<ILogger<AutofacExceptionMiddleware>>();
            logger.LogError(exception, "An error occurred while executing Autofac middleware");
            throw;
        }
    }
}
