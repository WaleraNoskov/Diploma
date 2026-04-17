using ImageAnalysis.Domain.Base;

namespace ImageAnalysis.Domain.Events;

public sealed record OperationUndoneEvent(
    Guid SessionId,
    Guid OperationId,
    string OperationType) : DomainEvent;