namespace ImageAnalysis.Application.Dtos;

public sealed record RoiBoundsDto(
    PixelPointDto TopLeft,
    int Width,
    int Height,
    int Area,
    PixelPointDto Center,
    PixelPointDto BottomRight);