using FluentAssertions;
using ImageAnalysis.Domain.Entities;
using ImageAnalysis.Domain.Events;
using ImageAnalysis.Domain.UnitTests.Infrastructure;

namespace ImageAnalysis.Domain.UnitTests.ImageSessionTests;

public sealed class ImageSessionMeasurementsTests
{
    [Fact]
    public void TakeMeasurement_WithoutImage_ThrowsInvalidOperationException()
    {
        var session = ImageSession.Create();

        var act = () => session.TakeMeasurement(PixelPointMother.At(0, 0), PixelPointMother.At(1, 1));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void TakeMeasurement_ValidPoints_AddsMeasurement()
    {
        var session = new ImageSessionBuilder().Build();

        session.TakeMeasurement(PixelPointMother.At(10, 10), PixelPointMother.At(50, 50));

        session.Measurements.Should().HaveCount(1);
    }

    [Fact]
    public void TakeMeasurement_ReturnsMeasurementWithCorrectDistance()
    {
        var session = new ImageSessionBuilder().Build();
        var from = PixelPointMother.At(0, 0);
        var to = PixelPointMother.At(3, 4);

        var measurement = session.TakeMeasurement(from, to);

        measurement.Distance.Pixels.Should().BeApproximately(5.0, precision: 1e-10);
    }

    [Fact]
    public void TakeMeasurement_SamePoints_ThrowsInvalidOperationException()
    {
        var session = new ImageSessionBuilder().Build();
        var point = PixelPointMother.At(50, 50);

        var act = () => session.TakeMeasurement(point, point);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*совпадать*");
    }

    [Fact]
    public void TakeMeasurement_PointOutsideImage_ThrowsArgumentOutOfRangeException()
    {
        var session = new ImageSessionBuilder().Build();
        var inside = PixelPointMother.At(10, 10);
        var outsideImage = PixelPointMother.At(9999, 9999);

        var act = () => session.TakeMeasurement(inside, outsideImage);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void TakeMeasurement_WithLabel_StoresLabel()
    {
        var session = new ImageSessionBuilder().Build();
        const string Label = "Crack width";

        var measurement = session.TakeMeasurement(
            PixelPointMother.At(10, 10),
            PixelPointMother.At(50, 50),
            Label);

        measurement.Label.Should().Be(Label);
    }

    [Fact]
    public void TakeMeasurement_RaisesMeasurementTakenEvent()
    {
        var session = new ImageSessionBuilder().Build();

        var measurement = session.TakeMeasurement(
            PixelPointMother.At(10, 10), PixelPointMother.At(50, 50));

        var evt = session.ShouldHaveSingleEvent<MeasurementTakenEvent>();
        evt.MeasurementId.Should().Be(measurement.Id);
    }

    [Fact]
    public void RemoveMeasurement_ExistingId_RemovesIt()
    {
        var session = new ImageSessionBuilder().Build();
        var measurement = session.TakeMeasurement(PixelPointMother.At(10, 10), PixelPointMother.At(50, 50));
        session.ClearDomainEvents();

        session.RemoveMeasurement(measurement.Id);

        session.Measurements.Should().BeEmpty();
    }

    [Fact]
    public void RemoveMeasurement_UnknownId_ThrowsInvalidOperationException()
    {
        var session = new ImageSessionBuilder().Build();

        var act = () => session.RemoveMeasurement(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RemoveMeasurement_RaisesMeasurementRemovedEvent()
    {
        var session = new ImageSessionBuilder().Build();
        var measurement = session.TakeMeasurement(PixelPointMother.At(10, 10), PixelPointMother.At(50, 50));
        session.ClearDomainEvents();

        session.RemoveMeasurement(measurement.Id);

        var evt = session.ShouldHaveSingleEvent<MeasurementRemovedEvent>();
        evt.MeasurementId.Should().Be(measurement.Id);
    }

    [Fact]
    public void MultipleMeasurements_AllStoredIndependently()
    {
        var session = new ImageSessionBuilder().Build();

        session.TakeMeasurement(PixelPointMother.At(0, 0), PixelPointMother.At(10, 0));
        session.TakeMeasurement(PixelPointMother.At(10, 10), PixelPointMother.At(20, 10));
        session.TakeMeasurement(PixelPointMother.At(20, 20), PixelPointMother.At(30, 20));

        session.Measurements.Should().HaveCount(3);
    }
}