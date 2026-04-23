using ImageAnalysis.Domain.Base;

namespace ImageAnalysis.Application.Services;

/// <summary>
/// Dispatches collected domain events after a successful aggregate mutation.
/// Implemented with MediatR's IPublisher in Infrastructure.
///
/// Kept as a separate abstraction so Application layer doesn't depend
/// directly on MediatR — only on this contract.
/// </summary>
public interface IDomainEventPublisher
{
    /// <summary>
    /// Publishes all pending events from <paramref name="aggregate"/>, then
    /// clears the event collection.
    /// </summary>
    Task PublishAndClearAsync<TId>(AggregateRoot<TId> aggregate, CancellationToken ct = default);
}

