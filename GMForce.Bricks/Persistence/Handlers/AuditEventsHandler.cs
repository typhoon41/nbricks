using System.Diagnostics.CodeAnalysis;
using GMForce.NDDD.Contracts;
using Microsoft.Extensions.Logging;

namespace GMForce.Bricks.Persistence.Handlers;

public class AuditEventsHandler(ILogger<AuditEventsHandler> logger) : IHandleDomainEvents
{
    [SuppressMessage("Usage", "CA2254:Template should be a static expression",
        Justification = "Message is a constant one but can come from different sources.")]
    public Task Handle(IDomainEvent notification, CancellationToken cancellationToken)
    {
        if (notification is IAuditUser audit)
        {
            logger.LogInformation(audit.Report(), audit.Details());
        }

        return Task.CompletedTask;
    }
}
