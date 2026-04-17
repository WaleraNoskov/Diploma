namespace ImageAnalysis.Domain.ValueObjects;

/// <summary>
/// Точка на изображении в пиксельных координатах.
/// Неизменяема — координаты задаются один раз.
/// </summary>
public sealed record PixelPoint(int X, int Y)
{
    public static PixelPoint Origin => new(0, 0);
 
    public double DistanceTo(PixelPoint other)
    {
        var dx = X - other.X;
        var dy = Y - other.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
 
    public override string ToString() => $"({X}, {Y})";
}