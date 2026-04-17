namespace ImageAnalysis.Domain.ValueObjects;

/// <summary>
/// Набор точек, образующих один контур.
/// Полиморфное поведение (площадь, периметр) внутри Value Object.
/// </summary>
public sealed record ContourPoints
{
    public IReadOnlyList<PixelPoint> Points { get; }
 
    public ContourPoints(IEnumerable<PixelPoint> points)
    {
        var list = points.ToList();
        if (list.Count < 3)
            throw new ArgumentException("Контур должен содержать не менее трёх точек.", nameof(points));
        Points = list.AsReadOnly();
    }
 
    /// <summary>Приближённый периметр по длинам сегментов.</summary>
    public double Perimeter()
    {
        double total = 0;
        for (var i = 0; i < Points.Count; i++)
            total += Points[i].DistanceTo(Points[(i + 1) % Points.Count]);
        return total;
    }
 
    /// <summary>Площадь по формуле Гаусса (Shoelace formula).</summary>
    public double Area()
    {
        double area = 0;
        for (var i = 0; i < Points.Count; i++)
        {
            var j = (i + 1) % Points.Count;
            area += (double)Points[i].X * Points[j].Y;
            area -= (double)Points[j].X * Points[i].Y;
        }
        return Math.Abs(area) / 2.0;
    }
 
    public PixelPoint Centroid()
    {
        var x = (int)Points.Average(p => p.X);
        var y = (int)Points.Average(p => p.Y);
        return new PixelPoint(x, y);
    }
}