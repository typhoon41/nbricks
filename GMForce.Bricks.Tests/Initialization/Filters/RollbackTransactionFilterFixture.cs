using FakeItEasy;
using GMForce.Bricks.Initialization.Filters;
using GMForce.Bricks.Tests.Arrangement.Builders;
using GMForce.NDDD.Contracts;
using NUnit.Framework;
using Shouldly;

namespace GMForce.Bricks.Tests.Initialization.Filters;

internal sealed class RollbackTransactionFilterFixture
{
    private IUnitOfWork _unitOfWork = null!;
    private RollbackTransactionFilter _filter = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = A.Fake<IUnitOfWork>();
        _filter = new RollbackTransactionFilter(() => _unitOfWork);
    }

    [Test]
    public void OnActionExecutingNullContextThrows()
    {
        void act() => _filter.OnActionExecuting(null!);
        Should.Throw<ArgumentNullException>(act);
    }

    [Test]
    public void OnActionExecutedNullContextThrows()
    {
        void act() => _filter.OnActionExecuted(null!);
        Should.Throw<ArgumentNullException>(act);
    }

    [Test]
    public void OnActionExecutingValidModelDoesNotCancelSaving()
    {
        var context = new ActionExecutingContextBuilder().Build();

        _filter.OnActionExecuting(context);

        A.CallTo(() => _unitOfWork.CancelSaving()).MustNotHaveHappened();
    }

    [Test]
    public void OnActionExecutingInvalidModelCancelsSaving()
    {
        var context = new ActionExecutingContextBuilder().WithInvalidModelState().Build();

        _filter.OnActionExecuting(context);

        A.CallTo(() => _unitOfWork.CancelSaving()).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void OnActionExecutedInvalidModelCancelsSaving()
    {
        var context = new ActionExecutedContextBuilder().WithInvalidModelState().Build();

        _filter.OnActionExecuted(context);

        A.CallTo(() => _unitOfWork.CancelSaving()).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void OnActionExecutedValidModelDoesNotCancelSaving()
    {
        var context = new ActionExecutedContextBuilder().Build();

        _filter.OnActionExecuted(context);

        A.CallTo(() => _unitOfWork.CancelSaving()).MustNotHaveHappened();
    }
}
