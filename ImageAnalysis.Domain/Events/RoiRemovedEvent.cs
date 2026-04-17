using ImageAnalysis.Domain.Base;

namespace ImageAnalysis.Domain.Events;

public sealed record RoiRemovedEvent(
    Guid SessionId,
    Guid RoiId) : DomainEvent;