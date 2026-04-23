using ImageAnalysis.Domain.Events;
using ImageAnalysis.Infrastructure.Contracts;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ImageAnalysis.Infrastructure.Handlers;

/// <summary>
/// Example: react to <see cref="ContoursDetectedEvent"/> —
/// could trigger UI refresh via a Prism EventAggregator in the WPF layer.
/// </summary>
public sealed class ContoursDetectedNotificationHandler(
    ILogger<ContoursDetectedNotificationHandler> logger)
    : INotificationHandler<DomainEventNotification>
{
    public Task Handle(DomainEventNotification notification, CancellationToken ct)
    {
        if (notification.Event is not ContoursDetectedEvent evt)
            return Task.CompletedTask;
 
        logger.LogInformation(
            "Session {SessionId}: {Count} contours detected",
            evt.SessionId,
            evt.ContourCount);
 
        // TODO: publish to Prism IEventAggregator for WPF ViewModels
        return Task.CompletedTask;
    }
}
