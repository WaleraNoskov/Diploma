using FluentAssertions;
using FluentAssertions.Execution;
using ImageAnalysis.Domain.Entities;
using ImageAnalysis.Domain.Entities.ProcessingOperations;
using ImageAnalysis.Domain.Events;
using ImageAnalysis.Domain.UnitTests.Infrastructure;

namespace ImageAnalysis.Domain.UnitTests.ImageSessionTests;

public sealed class ImageSessionResetTests
{
    [Fact]
    public void Reset_WithoutImage_ThrowsInvalidOperationException()
    {
        var session = ImageSession.Create();

        var act = () => session.Reset();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reset_RestoresCurrentImageToOriginal()
    {
        var session = new ImageSessionBuilder().Build();
        var original = session.OriginalImage;

        session.ApplyOperation(new GrayscaleOperation(), ImageDataMother.WithDimensions(200, 200));
        session.Reset();

        session.CurrentImage.Should().BeSameAs(original);
    }

    [Fact]
    public void Reset_ClearsHistory()
    {
        var session = new ImageSessionBuilder().Build();
        session.ApplyOperation(new GrayscaleOperation(), ImageDataMother.Default());

        session.Reset();

        session.History.CanUndo.Should().BeFalse();
    }

    [Fact]
    public void Reset_ClearsContoursMeasurementsAndRegions()
    {
        var session = new ImageSessionBuilder()
            .WithDetectedContours(ContourPointsMother.SmallSquare())
            .Build();

        session.TakeMeasurement(PixelPointMother.At(10, 10), PixelPointMother.At(50, 50));
        session.SelectRoi(RoiBoundsMother.Small());

        session.Reset();

        using var _ = new AssertionScope();
        session.Contours.Should().BeEmpty();
        session.Measurements.Should().BeEmpty();
        session.Regions.Should().BeEmpty();
    }

    [Fact]
    public void Reset_RaisesSessionResetEvent()
    {
        var session = new ImageSessionBuilder().Build();

        session.Reset();

        session.ShouldHaveSingleEvent<SessionResetEvent>();
    }
}