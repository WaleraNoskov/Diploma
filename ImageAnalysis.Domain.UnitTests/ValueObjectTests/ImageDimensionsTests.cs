using FluentAssertions;
using FluentAssertions.Execution;
using ImageAnalysis.Domain.UnitTests.Infrastructure;
using ImageAnalysis.Domain.ValueObjects;

namespace ImageAnalysis.Domain.UnitTests.ValueObjectTests;

public sealed class ImageDimensionsTests
{
    [Theory]
    [InlineData(0, 100)]
    [InlineData(-1, 100)]
    [InlineData(100, 0)]
    [InlineData(100, -5)]
    public void Constructor_NonPositiveDimensions_ThrowsArgumentOutOfRangeException(
        int width, int height)
    {
        var act = () => new ImageDimensions(width, height);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Constructor_ValidDimensions_SetsPropertiesCorrectly()
    {
        var dims = new ImageDimensions(1920, 1080);

        using var _ = new AssertionScope();
        dims.Width.Should().Be(1920);
        dims.Height.Should().Be(1080);
    }

    [Fact]
    public void TotalPixels_IsWidthTimesHeight()
    {
        var dims = new ImageDimensions(800, 600);

        dims.TotalPixels.Should().Be(480_000);
    }

    [Theory]
    [InlineData(0, 0, true, "origin")]
    [InlineData(799, 599, true, "last valid pixel")]
    [InlineData(800, 0, false, "x equals width")]
    [InlineData(0, 600, false, "y equals height")]
    [InlineData(-1, 0, false, "negative x")]
    [InlineData(0, -1, false, "negative y")]
    public void Contains_VariousPoints_ReturnsExpected(
        int x, int y, bool expected, string scenario)
    {
        var dims = new ImageDimensions(800, 600);
        var point = PixelPointMother.At(x, y);

        dims.Contains(point).Should().Be(expected, because: scenario);
    }

    [Fact]
    public void ToString_ReturnsWidthxHeight()
    {
        new ImageDimensions(1280, 720).ToString().Should().Be("1280x720");
    }
}