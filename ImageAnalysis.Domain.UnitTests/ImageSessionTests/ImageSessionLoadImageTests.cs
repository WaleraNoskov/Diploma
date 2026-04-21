using FluentAssertions;
using FluentAssertions.Execution;
using ImageAnalysis.Domain.Entities;
using ImageAnalysis.Domain.Events;
using ImageAnalysis.Domain.UnitTests.Infrastructure;

namespace ImageAnalysis.Domain.UnitTests.ImageSessionTests;

public sealed class ImageSessionLoadImageTests
{
    [Fact]
    public void LoadImage_ValidData_SetsHasImageToTrue()
    {
        var session = ImageSession.Create();
 
        session.LoadImage(ImageDataMother.Default());
 
        session.HasImage.Should().BeTrue();
    }
 
    [Fact]
    public void LoadImage_SetsOriginalAndCurrentImage()
    {
        var session = ImageSession.Create();
        var image   = ImageDataMother.Default();
 
        session.LoadImage(image);
 
        using var _ = new AssertionScope();
        session.OriginalImage.Should().BeSameAs(image);
        session.CurrentImage.Should().BeSameAs(image);
    }
 
    [Fact]
    public void LoadImage_RaisesImageLoadedEvent()
    {
        var session = ImageSession.Create();
        var image   = ImageDataMother.Default();
 
        session.LoadImage(image);
 
        var evt = session.ShouldHaveSingleEvent<ImageLoadedEvent>();
        using var _ = new AssertionScope();
        evt.SessionId.Should().Be(session.Id);
        evt.Dimensions.Should().Be(image.Dimensions);
        evt.Format.Should().Be(image.Format);
    }
 
    [Fact]
    public void LoadImage_ClearsPreviousContoursMeasurementsAndHistory()
    {
        // Pre-populate a session, then reload
        var session = new ImageSessionBuilder()
            .WithDetectedContours(ContourPointsMother.SmallSquare())
            .Build();
 
        session.TakeMeasurement(PixelPointMother.At(10, 10), PixelPointMother.At(50, 50));
 
        // Act
        session.LoadImage(ImageDataMother.Default());
 
        using var _ = new AssertionScope();
        session.Contours.Should().BeEmpty();
        session.Measurements.Should().BeEmpty();
        session.History.CanUndo.Should().BeFalse();
    }
 
    [Fact]
    public void LoadImage_Null_ThrowsArgumentNullException()
    {
        var session = ImageSession.Create();
 
        var act = () => session.LoadImage(null!);
 
        act.Should().Throw<ArgumentNullException>();
    }
}