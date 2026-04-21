using ImageAnalysis.Domain.ValueObjects;

namespace ImageAnalysis.Domain.UnitTests.Infrastructure;

/// <summary>
/// Factory for <see cref="RoiBounds"/> test values.
/// All bounds fit inside <see cref="TestConstants.DefaultImageWidth"/>×<see cref="TestConstants.DefaultImageHeight"/>.
/// </summary>
internal static class RoiBoundsMother
{
    public static RoiBounds Small() => new(new(10, 10), 50, 50);
    public static RoiBounds Medium() => new(new(100, 100), 200, 150);
    public static RoiBounds Large() => new(new(0, 0), 400, 300);

    /// <summary>ROI positioned so it sits completely outside the small 100×100 image.</summary>
    public static RoiBounds OutsideSmallImage() => new(new(50, 50), 200, 200);
}