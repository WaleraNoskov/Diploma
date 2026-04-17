using ImageAnalysis.Domain.Base;
using ImageAnalysis.Domain.ValueObjects;

namespace ImageAnalysis.Domain.Entities;

/// <summary>
/// Контур объекта на изображении.
/// Управляется только через ImageSession (агрегат).
/// </summary>
public sealed class Contour : Entity<Guid>
{
    public ContourPoints Points { get; }
    public bool IsSelected { get; private set; }
 
    // Вычисляемые характеристики (делегируют в Value Object)
    public double Area => Points.Area();
    public double Perimeter => Points.Perimeter();
    public PixelPoint Centroid => Points.Centroid();
 
    internal Contour(ContourPoints points)
    {
        Id = Guid.NewGuid();
        Points = points;
    }
 
    internal void Select() => IsSelected = true;
    internal void Deselect() => IsSelected = false;
}