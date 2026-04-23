using ImageAnalysis.Infrastructure.Contracts;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ImageAnalysis.Infrastructure.Handlers;

/// <summary>
/// Logs every domain event for diagnostics.
/// In production you'd register specific handlers per event type.
/// </summary>
public sealed class DomainEventLoggingHandler(
    ILogger<DomainEventLoggingHandler> logger)
    : INotificationHandler<DomainEventNotification>
{
    public Task Handle(DomainEventNotification notification, CancellationToken ct)
    {
        var evt = notification.Event;
        logger.LogInformation(
            "[DomainEvent] {EventType} at {OccurredAt} (id={EventId})",
            evt.GetType().Name,
            evt.OccurredAt,
            evt.EventId);
        return Task.CompletedTask;
    }
}
