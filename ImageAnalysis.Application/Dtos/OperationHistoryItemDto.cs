namespace ImageAnalysis.Application.Dtos;

public sealed record OperationHistoryItemDto(
    Guid   Id,
    string OperationType,
    string Description,
    DateTime AppliedAt);

