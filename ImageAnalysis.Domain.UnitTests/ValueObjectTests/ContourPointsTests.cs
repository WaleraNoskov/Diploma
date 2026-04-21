using FluentAssertions;
using FluentAssertions.Execution;
using ImageAnalysis.Domain.UnitTests.Infrastructure;
using ImageAnalysis.Domain.ValueObjects;

namespace ImageAnalysis.Domain.UnitTests.ValueObjectTests;

public sealed class ContourPointsTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Constructor_FewerThanThreePoints_ThrowsArgumentException(int count)
    {
        var points = Enumerable.Range(0, count)
            .Select(i => PixelPointMother.At(i, 0));

        var act = () => new ContourPoints(points);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*не менее трёх*");
    }

    [Fact]
    public void Constructor_ThreeOrMorePoints_SetsPointsCorrectly()
    {
        var points = new[] { new PixelPoint(0, 0), new PixelPoint(1, 0), new PixelPoint(0, 1) };

        var contour = new ContourPoints(points);

        contour.Points.Should().HaveCount(3);
    }

    [Fact]
    public void Area_UnitSquare_ReturnsCorrectArea()
    {
        // 10×10 square → area = 100
        var contour = ContourPointsMother.SmallSquare();

        contour.Area().Should().BeApproximately(100.0, precision: 1e-10);
    }

    [Fact]
    public void Area_RightTriangle_ReturnsHalfBaseTimesHeight()
    {
        // legs 3 and 4 → area = 6
        var contour = ContourPointsMother.Triangle();

        contour.Area().Should().BeApproximately(6.0, precision: 1e-10);
    }

    [Fact]
    public void Area_IsNonNegativeRegardlessOfPointOrder()
    {
        // Clockwise and counter-clockwise should yield the same area magnitude
        var ccw = new ContourPoints([new(0, 0), new(10, 0), new(10, 10), new(0, 10)]);
        var cw = new ContourPoints([new(0, 0), new(0, 10), new(10, 10), new(10, 0)]);

        ccw.Area().Should().BeApproximately(cw.Area(), precision: 1e-10);
    }

    [Fact]
    public void Perimeter_Square_IsCorrect()
    {
        // 10×10 square → perimeter = 40
        var contour = ContourPointsMother.SmallSquare();

        contour.Perimeter().Should().BeApproximately(40.0, precision: 1e-10);
    }

    [Fact]
    public void Centroid_AxisAlignedSquare_IsGeometricCenter()
    {
        var contour = ContourPointsMother.SmallSquare();

        var centroid = contour.Centroid();

        using var _ = new AssertionScope();
        centroid.X.Should().Be(5);
        centroid.Y.Should().Be(5);
    }
}