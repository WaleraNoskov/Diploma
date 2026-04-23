namespace ImageAnalysis.Application.Dtos;

public sealed record MeasurementDto(
    Guid Id,
    PixelPointDto From,
    PixelPointDto To,
    double DistancePixels,
    string? Label,
    DateTime CreatedAt);