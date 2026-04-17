using ImageAnalysis.Domain.Entities;

namespace ImageAnalysis.Domain.Services;

/// <summary>
/// Доменный сервис: вычисление статистики по набору измерений.
/// Логика не принадлежит ни одной сущности — вынесена в сервис.
/// </summary>
public static class MeasurementStatisticsService
{
    public sealed record Statistics(
        double Min,
        double Max,
        double Average,
        double StdDev,
        int Count);
 
    public static Statistics Calculate(IReadOnlyCollection<Measurement> measurements)
    {
        if (measurements.Count == 0)
            throw new InvalidOperationException("Нет измерений для вычисления статистики.");
 
        var values = measurements.Select(m => m.Distance.Pixels).ToList();
        var avg = values.Average();
        var variance = values.Average(v => Math.Pow(v - avg, 2));
 
        return new Statistics(
            Min: values.Min(),
            Max: values.Max(),
            Average: avg,
            StdDev: Math.Sqrt(variance),
            Count: values.Count);
    }
}