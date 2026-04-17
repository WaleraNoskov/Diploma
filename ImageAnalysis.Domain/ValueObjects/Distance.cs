namespace ImageAnalysis.Domain.ValueObjects;

/// <summary>
/// Расстояние между двумя точками измерения (в пикселях).
/// Инкапсулирует вычисление, не позволяет создать отрицательное расстояние.
/// </summary>
public sealed record Distance
{
    public double Pixels { get; }
 
    private Distance(double pixels) => Pixels = pixels;
 
    public static Distance Between(PixelPoint from, PixelPoint to)
    {
        var value = from.DistanceTo(to);
        return new Distance(value);
    }
 
    public override string ToString() => $"{Pixels:F2} px";
}