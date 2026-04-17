using ImageAnalysis.Domain.Entities;

namespace ImageAnalysis.Domain.ValueObjects;

/// <summary>
/// Критерий фильтрации контуров — доменный объект,
/// инкапсулирующий бизнес-правила отбора.
/// </summary>
public sealed class ContourFilterCriteria
{
    public double? MinArea { get; init; }
    public double? MaxArea { get; init; }
    public double? MinPerimeter { get; init; }
 
    public bool Matches(Contour contour)
    {
        if (MinArea.HasValue && contour.Area < MinArea.Value) return false;
        if (MaxArea.HasValue && contour.Area > MaxArea.Value) return false;
        if (MinPerimeter.HasValue && contour.Perimeter < MinPerimeter.Value) return false;
        return true;
    }
}