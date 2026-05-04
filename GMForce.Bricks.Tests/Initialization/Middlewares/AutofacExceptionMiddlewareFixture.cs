using Autofac.Core.Resolving.Pipeline;
using GMForce.Bricks.Initialization.Middlewares;
using NUnit.Framework;
using Shouldly;

namespace GMForce.Bricks.Tests.Initialization.Middlewares;

internal sealed class AutofacExceptionMiddlewareFixture
{
    private AutofacExceptionMiddleware _sut = null!;

    [SetUp]
    public void SetUp() => _sut = new AutofacExceptionMiddleware();

    [Test]
    public void PhaseIsActivation() => _sut.Phase.ShouldBe(PipelinePhase.Activation);

    [Test]
    public void ExecuteNoExceptionCallsNext()
    {
        var wasCalled = false;
        void next(ResolveRequestContext _) => wasCalled = true;

        _sut.Execute(null!, next);

        wasCalled.ShouldBeTrue();
    }

    [Test]
    public void ExecuteExceptionThrownRethrows()
    {
        var exception = new InvalidOperationException("test");
        void next(ResolveRequestContext _) => throw exception;

        void act() => _sut.Execute(null!, next);

        Should.Throw<InvalidOperationException>(act).Message.ShouldBe("test");
    }
}
