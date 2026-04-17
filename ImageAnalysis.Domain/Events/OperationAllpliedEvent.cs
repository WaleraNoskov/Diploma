using ImageAnalysis.Domain.Base;

namespace ImageAnalysis.Domain.Events;

public sealed record OperationAppliedEvent(
    Guid SessionId,
    Guid OperationId,
    string OperationType) : DomainEvent;