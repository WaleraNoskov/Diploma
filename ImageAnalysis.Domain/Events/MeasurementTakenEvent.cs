using ImageAnalysis.Domain.Base;
using ImageAnalysis.Domain.ValueObjects;

namespace ImageAnalysis.Domain.Events;

public sealed record MeasurementTakenEvent(
    Guid SessionId,
    Guid MeasurementId,
    Distance Distance) : DomainEvent;