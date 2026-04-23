using ImageAnalysis.Domain.Base;
using MediatR;

namespace ImageAnalysis.Infrastructure.Contracts;

/// <summary>
/// MediatR notification wrapper around a <see cref="DomainEvent"/>.
/// Handlers subscribe to specific event types by pattern-matching on
/// <see cref="DomainEventNotification.Event"/>.
/// </summary>
public sealed record DomainEventNotification(DomainEvent Event) : INotification;
