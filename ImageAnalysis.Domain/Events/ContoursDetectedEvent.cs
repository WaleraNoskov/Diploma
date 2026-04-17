using ImageAnalysis.Domain.Base;

namespace ImageAnalysis.Domain.Events;

public sealed record ContoursDetectedEvent(
    Guid SessionId,
    int ContourCount) : DomainEvent;