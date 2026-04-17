using ImageAnalysis.Domain.Base;

namespace ImageAnalysis.Domain.Events;

public sealed record SessionResetEvent(
    Guid SessionId) : DomainEvent;