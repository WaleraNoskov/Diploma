using FluentAssertions;
using ImageAnalysis.Domain.Entities;
using ImageAnalysis.Domain.Entities.ProcessingOperations;
using ImageAnalysis.Domain.Events;
using ImageAnalysis.Domain.UnitTests.Infrastructure;

namespace ImageAnalysis.Domain.UnitTests.ImageSessionTests;

/// <summary>
/// High-value integration tests that verify the full state-machine of the aggregate
/// by asserting on the sequence of events produced by a real workflow.
/// </summary>
public sealed class ImageSessionEventSequenceTests
{
    [Fact]
    public void FullWorkflow_LoadApplyUndoEvents_AreRaisedInExpectedOrder()
    {
        var session = ImageSession.Create();

        session.LoadImage(ImageDataMother.Default());
        session.ApplyOperation(new GrayscaleOperation(), ImageDataMother.Default());
        session.UndoLastOperation(ImageDataMother.Default());

        session.ShouldHaveRaisedEventsInOrder(
            typeof(ImageLoadedEvent),
            typeof(OperationAppliedEvent),
            typeof(OperationUndoneEvent));
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllEvents()
    {
        var session = ImageSession.Create();
        session.LoadImage(ImageDataMother.Default());
        session.DomainEvents.Should().NotBeEmpty();

        session.ClearDomainEvents();

        session.DomainEvents.Should().BeEmpty();
    }
}