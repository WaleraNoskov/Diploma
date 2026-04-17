using ImageAnalysis.Domain.Base;

namespace ImageAnalysis.Domain.Events;

public sealed record ContourSelectedEvent(
    Guid SessionId,
    Guid ContourId,
    double Area) : DomainEvent;