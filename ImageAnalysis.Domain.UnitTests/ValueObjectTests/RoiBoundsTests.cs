using FluentAssertions;
using FluentAssertions.Execution;
using ImageAnalysis.Domain.UnitTests.Infrastructure;
using ImageAnalysis.Domain.ValueObjects;

namespace ImageAnalysis.Domain.UnitTests.ValueObjectTests;

public sealed class RoiBoundsTests
{
    [Theory]
    [InlineData(0, 100)]
    [InlineData(-1, 100)]
    [InlineData(100, 0)]
    [InlineData(100, -5)]
    public void Constructor_NonPositiveDimensions_ThrowsArgumentOutOfRangeException(
        int width, int height)
    {
        var act = () => new RoiBounds(PixelPointMother.Origin(), width, height);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_ValidBounds_SetsPropertiesCorrectly()
    {
        var topLeft = PixelPointMother.At(10, 20);
        var bounds = new RoiBounds(topLeft, 100, 80);

        using var _ = new AssertionScope();
        bounds.TopLeft.Should().Be(topLeft);
        bounds.Width.Should().Be(100);
        bounds.Height.Should().Be(80);
    }

    [Fact]
    public void BottomRight_IsComputedFromTopLeftAndSize()
    {
        var bounds = new RoiBounds(PixelPointMother.At(10, 20), 100, 80);

        bounds.BottomRight.Should().Be(PixelPointMother.At(110, 100));
    }

    [Fact]
    public void Area_IsWidthTimesHeight()
    {
        var bounds = new RoiBounds(PixelPointMother.Origin(), 40, 25);

        bounds.Area.Should().Be(1000);
    }

    [Fact]
    public void Center_IsComputedCorrectly()
    {
        var bounds = new RoiBounds(PixelPointMother.At(0, 0), 100, 60);

        bounds.Center.Should().Be(PixelPointMother.At(50, 30));
    }

    [Theory]
    [InlineData(10, 10, true, "corner point is inside")]
    [InlineData(60, 60, true, "interior point")]
    [InlineData(9, 10, false, "just left of left edge")]
    [InlineData(10, 9, false, "just above top edge")]
    public void Contains_VariousPoints_ReturnsExpected(
        int x, int y, bool expected, string scenario)
    {
        // Bounds: topLeft=(10,10), width=100, height=100
        var bounds = new RoiBounds(PixelPointMother.At(10, 10), 100, 100);
        var point = PixelPointMother.At(x, y);

        bounds.Contains(point).Should().Be(expected, because: scenario);
    }

    [Fact]
    public void Intersects_OverlappingBounds_ReturnsTrue()
    {
        var a = new RoiBounds(PixelPointMother.At(0, 0), 100, 100);
        var b = new RoiBounds(PixelPointMother.At(50, 50), 100, 100);

        a.Intersects(b).Should().BeTrue();
    }

    [Fact]
    public void Intersects_AdjacentNonOverlappingBounds_ReturnsFalse()
    {
        var a = new RoiBounds(PixelPointMother.At(0, 0), 100, 100);
        var b = new RoiBounds(PixelPointMother.At(100, 0), 100, 100);

        a.Intersects(b).Should().BeFalse();
    }

    [Fact]
    public void Intersects_IsSymmetric()
    {
        var a = new RoiBounds(PixelPointMother.At(0, 0), 200, 200);
        var b = new RoiBounds(PixelPointMother.At(100, 100), 50, 50);

        a.Intersects(b).Should().Be(b.Intersects(a));
    }
}