using FakeItEasy;
using GMForce.Bricks.Initialization.Filters;
using GMForce.Bricks.Tests.Arrangement.Builders;
using GMForce.NDDD.Contracts;
using Microsoft.AspNetCore.Mvc.Filters;
using NUnit.Framework;
using Shouldly;

namespace GMForce.Bricks.Tests.Initialization.Filters;

internal sealed class TransactionFilterFixture
{
    private IUnitOfWork _unitOfWork = null!;
    private TransactionFilter _filter = null!;

    [SetUp]
    public void SetUp()
    {
        _unitOfWork = A.Fake<IUnitOfWork>();
        _filter = new TransactionFilter(() => _unitOfWork);
    }

    [Test]
    public async Task NullContextThrows()
    {
        async Task act() => await _filter.OnActionExecutionAsync(null!, DefaultNext);
        await Should.ThrowAsync<ArgumentNullException>(act);
    }

    [Test]
    public async Task NullNextThrows()
    {
        async Task act() => await _filter.OnActionExecutionAsync(new ActionExecutingContextBuilder().Build(), null!);
        await Should.ThrowAsync<ArgumentNullException>(act);
    }

    [Test]
    public async Task SuccessResultCallsSaveChanges()
    {
        var context = new ActionExecutingContextBuilder().Build();

        await _filter.OnActionExecutionAsync(context, DefaultNext);

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

        await _filter.OnActionExecutionAsync(context, next);

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

        await _filter.OnActionExecutionAsync(context, next);

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
    }

    [Test]
    public async Task InvalidModelStateDoesNotSave()
    {
        var context = new ActionExecutingContextBuilder().WithInvalidModelState().Build();

        await _filter.OnActionExecutionAsync(context, DefaultNext);

        A.CallTo(() => _unitOfWork.SaveChangesAsync(A<CancellationToken>._)).MustNotHaveHappened();
    }

    private static Task<ActionExecutedContext> DefaultNext()
        => Task.FromResult(new ActionExecutedContextBuilder().Build());
}
