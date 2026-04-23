namespace ImageAnalysis.Application.Dtos;

public sealed record MeasurementStatisticsDto(
    double Min,
    double Max,
    double Average,
    double StdDev,
    int Count);