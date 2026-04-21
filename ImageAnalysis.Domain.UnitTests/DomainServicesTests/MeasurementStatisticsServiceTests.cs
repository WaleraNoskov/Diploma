using FluentAssertions;
using FluentAssertions.Execution;
using ImageAnalysis.Domain.Services;
using ImageAnalysis.Domain.UnitTests.Infrastructure;

namespace ImageAnalysis.Domain.UnitTests.DomainServicesTests;

public sealed class MeasurementStatisticsServiceTests
{
    [Fact]
    public void Calculate_EmptyCollection_ThrowsInvalidOperationException()
    {
        var act = () => MeasurementStatisticsService.Calculate([]);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Нет измерений*");
    }

    [Fact]
    public void Calculate_SingleMeasurement_HasZeroStdDev()
    {
        var session = new ImageSessionBuilder().Build();
        var measurement = session.TakeMeasurement(PixelPointMother.At(0, 0), PixelPointMother.At(3, 4));

        var stats = MeasurementStatisticsService.Calculate([measurement]);

        using var _ = new AssertionScope();
        stats.Count.Should().Be(1);
        stats.StdDev.Should().BeApproximately(0.0, precision: 1e-10);
        stats.Min.Should().BeApproximately(stats.Max, precision: 1e-10);
    }

    [Fact]
    public void Calculate_KnownMeasurements_ReturnsCorrectMinMaxAverage()
    {
        // Create three measurements with pixel distances 3-4-5, 0-5, 0-10
        // i.e. distances 5, 5, 10 → min=5, max=10, avg=6.666...
        var session = new ImageSessionBuilder().Build();
        var m1 = session.TakeMeasurement(PixelPointMother.At(0, 0), PixelPointMother.At(3, 4)); // = 5
        var m2 = session.TakeMeasurement(PixelPointMother.At(0, 0), PixelPointMother.At(0, 5)); // = 5
        var m3 = session.TakeMeasurement(PixelPointMother.At(0, 0), PixelPointMother.At(0, 10)); // = 10

        var stats = MeasurementStatisticsService.Calculate([m1, m2, m3]);

        using var _ = new AssertionScope();
        stats.Min.Should().BeApproximately(5.0, precision: 1e-10);
        stats.Max.Should().BeApproximately(10.0, precision: 1e-10);
        stats.Average.Should().BeApproximately(20.0 / 3.0, precision: 1e-10);
        stats.Count.Should().Be(3);
    }

    [Fact]
    public void Calculate_IdenticalMeasurements_StandardDeviationIsZero()
    {
        var session = new ImageSessionBuilder().Build();
        var m1 = session.TakeMeasurement(PixelPointMother.At(0, 0), PixelPointMother.At(0, 10));
        var m2 = session.TakeMeasurement(PixelPointMother.At(10, 0), PixelPointMother.At(10, 10));

        var stats = MeasurementStatisticsService.Calculate([m1, m2]);

        stats.StdDev.Should().BeApproximately(0.0, precision: 1e-10);
    }

    [Fact]
    public void Calculate_Statistics_Count_MatchesMeasurementCount()
    {
        var session = new ImageSessionBuilder().Build();
        var measurements = Enumerable.Range(1, 5)
            .Select(i => session.TakeMeasurement(
                PixelPointMother.At(0, 0),
                PixelPointMother.At(i * 10, 0)))
            .ToList();

        var stats = MeasurementStatisticsService.Calculate(measurements);

        stats.Count.Should().Be(5);
    }
}