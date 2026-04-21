using FluentAssertions;
using FluentAssertions.Execution;
using ImageAnalysis.Domain.UnitTests.Infrastructure;
using ImageAnalysis.Domain.ValueObjects;

namespace ImageAnalysis.Domain.UnitTests.ValueObjectTests;

public sealed class PixelPointTests
{
    [Fact]
    public void Origin_ReturnsPointAtZeroZero()
    {
        var origin = PixelPoint.Origin;

        using var _ = new AssertionScope();
        origin.X.Should().Be(0);
        origin.Y.Should().Be(0);
    }

    [Fact]
    public void DistanceTo_SamePoint_ReturnsZero()
    {
        var point = PixelPointMother.At(5, 5);

        var distance = point.DistanceTo(point);

        distance.Should().Be(0d);
    }

    [Theory]
    [InlineData(0, 0, 3, 4, 5.0)] // 3-4-5 right triangle
    [InlineData(0, 0, 0, 10, 10.0)] // vertical segment
    [InlineData(0, 0, 10, 0, 10.0)] // horizontal segment
    public void DistanceTo_KnownPairs_ReturnsExpectedEuclideanDistance(
        int x1, int y1, int x2, int y2, double expected)
    {
        var from = PixelPointMother.At(x1, y1);
        var to = PixelPointMother.At(x2, y2);

        var actual = from.DistanceTo(to);

        actual.Should().BeApproximately(expected, precision: 1e-10);
    }

    [Fact]
    public void DistanceTo_IsSymmetric()
    {
        var a = PixelPointMother.At(0, 0);
        var b = PixelPointMother.At(7, 24);

        a.DistanceTo(b).Should().BeApproximately(b.DistanceTo(a), precision: 1e-10);
    }

    [Fact]
    public void Equality_SameCoordinates_AreEqual()
    {
        var p1 = PixelPointMother.At(10, 20);
        var p2 = PixelPointMother.At(10, 20);

        p1.Should().Be(p2);
    }

    [Fact]
    public void Equality_DifferentCoordinates_AreNotEqual()
    {
        var p1 = PixelPointMother.At(10, 20);
        var p2 = PixelPointMother.At(10, 21);

        p1.Should().NotBe(p2);
    }

    [Fact]
    public void ToString_ReturnsReadableFormat()
    {
        var point = PixelPointMother.At(42, 99);

        point.ToString().Should().Be("(42, 99)");
    }
}