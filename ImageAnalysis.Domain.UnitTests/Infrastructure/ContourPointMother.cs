using ImageAnalysis.Domain.ValueObjects;

namespace ImageAnalysis.Domain.UnitTests.Infrastructure;

/// <summary>
/// Factory for <see cref="ContourPoints"/> test values.
/// All shapes are axis-aligned for deterministic area/perimeter calculations.
/// </summary>
internal static class ContourPointsMother
{
    /// <summary>10×10 square with origin at (0,0). Area = 100, Perimeter = 40.</summary>
    public static ContourPoints SmallSquare() =>
        new([new(0, 0), new(10, 0), new(10, 10), new(0, 10)]);
 
    /// <summary>100×50 rectangle. Area = 5000, Perimeter = 300.</summary>
    public static ContourPoints LargeRectangle() =>
        new([new(0, 0), new(100, 0), new(100, 50), new(0, 50)]);
 
    /// <summary>Right triangle with legs 3 and 4. Area = 6.</summary>
    public static ContourPoints Triangle() =>
        new([new(0, 0), new(3, 0), new(0, 4)]);
 
    /// <summary>Minimal 3-point contour for guard tests.</summary>
    public static ContourPoints Minimal() =>
        new([new(0, 0), new(1, 0), new(0, 1)]);
}