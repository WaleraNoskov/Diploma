namespace ImageAnalysis.Application.Dtos;

public sealed record ContourDto(
    Guid   Id,
    double Area,
    double Perimeter,
    PixelPointDto Centroid,
    bool   IsSelected,
    IReadOnlyList<PixelPointDto> Points);
