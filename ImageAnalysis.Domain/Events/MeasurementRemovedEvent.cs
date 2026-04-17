using ImageAnalysis.Domain.Base;

namespace ImageAnalysis.Domain.Events;

public sealed record MeasurementRemovedEvent(
    Guid SessionId,
    Guid MeasurementId) : DomainEvent;