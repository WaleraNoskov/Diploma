namespace ImageAnalysis.Application.Dtos;

public sealed record ImageSessionDto(
    Guid Id,
    bool HasImage,
    Guid? CurrentImageId,
    Guid? OriginalImageId,
    ImageDimensionsDto? Dimensions,
    int Channels,
    int ChannelSize,
    int Stride,
    int ContourCount,
    int MeasurementCount,
    int RegionCount,
    bool CanUndo,
    bool CanRedo,
    IReadOnlyList<OperationHistoryItemDto> OperationHistory,
    DateTime CreatedAt,
    DateTime? LastModifiedAt);