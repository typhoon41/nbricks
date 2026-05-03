using GMForce.NDDD.Contracts;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace GMForce.Bricks.Initialization.Filters;

public class TransactionFilter(Func<IUnitOfWork> unitOfWork) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var resultContext = await next();

        var isFailureResult = resultContext.Result is IStatusCodeActionResult { StatusCode: >= 300 };

        if (resultContext.Exception == null && !isFailureResult && context.ModelState.IsValid)
        {
            _ = await unitOfWork().SaveChangesAsync();
        }
    }
}
