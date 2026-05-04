using FakeItEasy;
using GMForce.Bricks.Initialization.Filters;
using GMForce.Bricks.Tests.Arrangement.Builders;
using GMForce.NDDD.Contracts;
using NUnit.Framework;

namespace GMForce.Bricks.Tests.Initialization.Filters;

internal sealed class RollbackTransactionFilterFixture
{
    private IUnitOfWork _unitOfWork = null!;
    private RollbackTransactionFilter _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = A.Fake<IUnitOfWork>();
        _sut = new RollbackTransactionFilter(() => _unitOfWork);
    }

    [Test]
    public void OnActionExecutingValidModelDoesNotCancelSaving()
    {
        var context = new ActionExecutingContextBuilder().Build();

        _sut.OnActionExecuting(context);

        A.CallTo(() => _unitOfWork.CancelSaving()).MustNotHaveHappened();
    }

    [Test]
    public void OnActionExecutingInvalidModelCancelsSaving()
    {
        var context = new ActionExecutingContextBuilder().WithInvalidModelState().Build();

        _sut.OnActionExecuting(context);

        A.CallTo(() => _unitOfWork.CancelSaving()).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void OnActionExecutedInvalidModelCancelsSaving()
    {
        var context = new ActionExecutedContextBuilder().WithInvalidModelState().Build();

        _sut.OnActionExecuted(context);

        A.CallTo(() => _unitOfWork.CancelSaving()).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void OnActionExecutedValidModelDoesNotCancelSaving()
    {
        var context = new ActionExecutedContextBuilder().Build();

        _sut.OnActionExecuted(context);

        A.CallTo(() => _unitOfWork.CancelSaving()).MustNotHaveHappened();
    }
}
