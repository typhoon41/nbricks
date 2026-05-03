using GMForce.NDDD.Contracts;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GMForce.Bricks.Initialization.Filters;

public class TransactionFilter(Func<IUnitOfWork> unitOfWork) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var resultContext = await next();

        var statusCode = context.HttpContext.Response.StatusCode;
        var okStatusCode = statusCode is >= 200 and < 300;

        if (resultContext.Exception == null && okStatusCode && context.ModelState.IsValid)
        {
            _ = await unitOfWork().SaveChangesAsync();
        }
    }
}
