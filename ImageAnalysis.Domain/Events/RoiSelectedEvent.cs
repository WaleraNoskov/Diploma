using ImageAnalysis.Domain.Base;
using ImageAnalysis.Domain.ValueObjects;

namespace ImageAnalysis.Domain.Events;

public sealed record RoiSelectedEvent(
    Guid SessionId,
    Guid RoiId,
    RoiBounds Bounds) : DomainEvent;