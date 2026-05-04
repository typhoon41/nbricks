using FakeItEasy;
using GMForce.Bricks.Persistence.Handlers;
using GMForce.NDDD.Contracts;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;

namespace GMForce.Bricks.Tests.Persistence.Handlers;

internal sealed class AuditEventsHandlerFixture
{
    private ILogger<AuditEventsHandler> _logger = null!;
    private AuditEventsHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _logger = A.Fake<ILogger<AuditEventsHandler>>();
        _sut = new AuditEventsHandler(_logger);
    }

    [Test]
    public async Task HandleAuditUserEventLogsInformation()
    {
        var auditEvent = A.Fake<IDomainEvent>(o => o.Implements<IAuditUser>());
        A.CallTo(() => ((IAuditUser)auditEvent).Report()).Returns("audit log");
        A.CallTo(() => ((IAuditUser)auditEvent).Details()).Returns(["detail1"]);

        await _sut.Handle(auditEvent, CancellationToken.None);

        A.CallTo(_logger).MustHaveHappened();
    }

    [Test]
    public async Task HandleNonAuditEventDoesNotLog()
    {
        var domainEvent = A.Fake<IDomainEvent>();

        await _sut.Handle(domainEvent, CancellationToken.None);

        A.CallTo(_logger).MustNotHaveHappened();
    }

    [Test]
    public async Task HandleReturnsCompletedTask()
    {
        var domainEvent = A.Fake<IDomainEvent>();

        var result = _sut.Handle(domainEvent, CancellationToken.None);

        await result;
        result.IsCompletedSuccessfully.ShouldBeTrue();
    }
}
