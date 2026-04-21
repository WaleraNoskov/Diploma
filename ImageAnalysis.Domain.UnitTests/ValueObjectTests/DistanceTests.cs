using FluentAssertions;
using ImageAnalysis.Domain.UnitTests.Infrastructure;
using ImageAnalysis.Domain.ValueObjects;

namespace ImageAnalysis.Domain.UnitTests.ValueObjectTests;

public sealed class DistanceTests
{
    [Fact]
    public void Between_KnownPoints_ComputesCorrectEuclideanDistance()
    {
        // 3-4-5 Pythagorean triple
        var from = PixelPointMother.At(0, 0);
        var to = PixelPointMother.At(3, 4);

        var distance = Distance.Between(from, to);

        distance.Pixels.Should().BeApproximately(5.0, precision: 1e-10);
    }

    [Fact]
    public void Between_IdenticalPoints_ReturnsZeroDistance()
    {
        var point = PixelPointMother.At(50, 50);

        Distance.Between(point, point).Pixels.Should().Be(0d);
    }

    [Fact]
    public void Between_IsSymmetric()
    {
        var a = PixelPointMother.At(10, 20);
        var b = PixelPointMother.At(40, 60);

        Distance.Between(a, b).Pixels.Should()
            .BeApproximately(Distance.Between(b, a).Pixels, precision: 1e-10);
    }

    [Fact]
    public void ToString_FormatsToTwoDecimalPlaces()
    {
        var d = Distance.Between(PixelPointMother.At(0, 0), PixelPointMother.At(1, 0));

        d.ToString().Should().Be("1.00 px");
    }
}