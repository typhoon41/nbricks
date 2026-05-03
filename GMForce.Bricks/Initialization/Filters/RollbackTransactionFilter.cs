using GMForce.NDDD.Contracts;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GMForce.Bricks.Initialization.Filters;

public class RollbackTransactionFilter(Func<IUnitOfWork> unitOfWork) : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        AbortInvalidSaving(context);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        AbortInvalidSaving(context);
    }

    private void AbortInvalidSaving(FilterContext context)
    {
        if (context.ModelState.IsValid)
        {
            return;
        }

        unitOfWork().CancelSaving();
    }
}
