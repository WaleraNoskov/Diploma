namespace ImageAnalysis.Application.Dtos;

public sealed record RegionOfInterestDto(
    Guid Id,
    RoiBoundsDto Bounds,
    string? Label,
    bool IsActive,
    DateTime CreatedAt);