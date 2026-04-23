using ImageAnalysis.Application.Services;
using ImageAnalysis.Domain.Base;
using ImageAnalysis.Infrastructure.Contracts;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ImageAnalysis.Infrastructure.Services;

/// <summary>
/// Dispatches domain events collected on an aggregate via MediatR's
/// <see cref="IPublisher"/>. Each <see cref="DomainEvent"/> becomes a
/// MediatR notification and is dispatched to all registered
/// <see cref="INotificationHandler{TNotification}"/> instances.
///
/// Events are published sequentially (not in parallel) to preserve ordering.
/// </summary>
public sealed class MediatRDomainEventPublisher(
    IPublisher publisher,
    ILogger<MediatRDomainEventPublisher> logger)
    : IDomainEventPublisher
{
    public async Task PublishAndClearAsync<TId>(
        AggregateRoot<TId> aggregate,
        CancellationToken ct = default)
    {
        var events = aggregate.DomainEvents.ToList();
        aggregate.ClearDomainEvents();

        foreach (var domainEvent in events)
        {
            logger.LogDebug(
                "Publishing domain event {EventType} ({EventId})",
                domainEvent.GetType().Name,
                domainEvent.EventId);

            // Wrap the DomainEvent in a MediatR notification
            await publisher.Publish(
                new DomainEventNotification(domainEvent),
                ct);
        }
    }
}