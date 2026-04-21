using ImageAnalysis.Domain.ValueObjects;

namespace ImageAnalysis.Domain.UnitTests.Infrastructure;

/// <summary>
/// Factory for <see cref="PixelPoint"/> test values.
/// </summary>
public class PixelPointMother
{
    public static PixelPoint Origin()              => new(0, 0);
    public static PixelPoint At(int x, int y)     => new(x, y);
    public static PixelPoint Center()             => new(400, 300);
    public static PixelPoint TopRight()           => new(799, 0);
    public static PixelPoint BottomLeft()         => new(0, 599);
}