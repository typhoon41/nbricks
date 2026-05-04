using FakeItEasy;
using GMForce.Bricks.Initialization.Filters;
using GMForce.Bricks.Tests.Arrangement.Builders;
using GMForce.NDDD.Contracts;
using Microsoft.AspNetCore.Mvc.Filters;
using NUnit.Framework;

namespace GMForce.Bricks.Tests.Initialization.Filters;

internal sealed class TransactionFilterFixture
{
    private IUnitOfWork _unitOfWork = null!;
    private TransactionFilter _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = A.Fake<IUnitOfWork>();
        _sut = new TransactionFilter(() => _unitOfWork);
    }

    [Test]
    public async Task SuccessResultCallsSaveChanges()
    {
        var context = new ActionExecutingContextBuilder().Build();
        Task<ActionExecutedContext> next()
        {
            return Task.FromResult(new ActionExecutedContextBuilder().Build());
        }

        await _sut.OnActionExecutionAsync(context, next);

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task ExceptionOnResultDoesNotSave()
    {
        var context = new ActionExecutingContextBuilder().Build();
        Task<ActionExecutedContext> next()
        {
            return Task.FromResult(
            new ActionExecutedContextBuilder().WithException(new InvalidOperationException()).Build());
        }

        await _sut.OnActionExecutionAsync(context, next);

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Test]
    public async Task FailureStatusCodeDoesNotSave()
    {
        var context = new ActionExecutingContextBuilder().Build();
        Task<ActionExecutedContext> next()
        {
            return Task.FromResult(
            new ActionExecutedContextBuilder().WithStatusCode(400).Build());
        }

        await _sut.OnActionExecutionAsync(context, next);

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Test]
    public async Task InvalidModelStateDoesNotSave()
    {
        var context = new ActionExecutingContextBuilder().WithInvalidModelState().Build();
        Task<ActionExecutedContext> next()
        {
            return Task.FromResult(new ActionExecutedContextBuilder().Build());
        }

        await _sut.OnActionExecutionAsync(context, next);

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
    }
}
