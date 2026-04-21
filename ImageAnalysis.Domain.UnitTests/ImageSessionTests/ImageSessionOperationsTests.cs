using FluentAssertions;
using ImageAnalysis.Domain.Entities;
using ImageAnalysis.Domain.Entities.ProcessingOperations;
using ImageAnalysis.Domain.Events;
using ImageAnalysis.Domain.UnitTests.Infrastructure;

namespace ImageAnalysis.Domain.UnitTests.ImageSessionTests;

public sealed class ImageSessionOperationsTests
{
    [Fact]
    public void ApplyOperation_WithoutImage_ThrowsInvalidOperationException()
    {
        var session = ImageSession.Create();
 
        var act = () => session.ApplyOperation(new GrayscaleOperation(), ImageDataMother.Default());
 
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*не загружено*");
    }
 
    [Fact]
    public void ApplyOperation_PushesOperationToHistory()
    {
        var session   = new ImageSessionBuilder().Build();
        var operation = new GrayscaleOperation();
        var result    = ImageDataMother.Default();
 
        session.ApplyOperation(operation, result);
 
        session.History.CanUndo.Should().BeTrue();
    }
 
    [Fact]
    public void ApplyOperation_UpdatesCurrentImage()
    {
        var session   = new ImageSessionBuilder().Build();
        var newImage  = ImageDataMother.WithDimensions(200, 200);
 
        session.ApplyOperation(new GrayscaleOperation(), newImage);
 
        session.CurrentImage.Should().BeSameAs(newImage);
    }
 
    [Fact]
    public void ApplyOperation_InvalidatesDetectedContours()
    {
        var session = new ImageSessionBuilder()
            .WithDetectedContours(ContourPointsMother.SmallSquare())
            .Build();
 
        // Sanity check: contours existed before
        session.Contours.Should().NotBeEmpty();
 
        session.ApplyOperation(new GrayscaleOperation(), ImageDataMother.Default());
 
        session.Contours.Should().BeEmpty(
            because: "any operation on the image invalidates previously detected contours");
    }
 
    [Fact]
    public void ApplyOperation_RaisesOperationAppliedEvent()
    {
        var session   = new ImageSessionBuilder().Build();
        var operation = new BrightnessOperation(50);
 
        session.ApplyOperation(operation, ImageDataMother.Default());
 
        var evt = session.ShouldHaveSingleEvent<OperationAppliedEvent>();
        evt.OperationType.Should().Be(operation.OperationType);
    }
 
    [Fact]
    public void UndoLastOperation_WithoutHistory_ThrowsInvalidOperationException()
    {
        var session = new ImageSessionBuilder().Build();
 
        var act = () => session.UndoLastOperation(ImageDataMother.Default());
 
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*отмены*");
    }
 
    [Fact]
    public void UndoLastOperation_RestoresPreviousImage()
    {
        var session      = new ImageSessionBuilder().Build();
        var beforeImage  = session.CurrentImage!;
        var afterImage   = ImageDataMother.WithDimensions(200, 200);
 
        session.ApplyOperation(new GrayscaleOperation(), afterImage);
        session.ClearDomainEvents();
 
        session.UndoLastOperation(beforeImage);
 
        session.CurrentImage.Should().BeSameAs(beforeImage);
    }
 
    [Fact]
    public void UndoLastOperation_RaisesOperationUndoneEvent()
    {
        var session = new ImageSessionBuilder().Build();
        session.ApplyOperation(new GrayscaleOperation(), ImageDataMother.Default());
        session.ClearDomainEvents();
 
        session.UndoLastOperation(ImageDataMother.Default());
 
        session.ShouldHaveSingleEvent<OperationUndoneEvent>();
    }
 
    [Fact]
    public void UndoLastOperation_WithoutImage_ThrowsInvalidOperationException()
    {
        var session = ImageSession.Create();
 
        var act = () => session.UndoLastOperation(ImageDataMother.Default());
 
        act.Should().Throw<InvalidOperationException>();
    }
}