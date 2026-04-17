using ImageAnalysis.Domain.Base;

namespace ImageAnalysis.Domain.Events;

public sealed record ContourDeselectedEvent(
    Guid SessionId,
    Guid ContourId) : DomainEvent;