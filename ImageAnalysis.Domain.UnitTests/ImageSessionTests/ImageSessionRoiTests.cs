using FluentAssertions;
using FluentAssertions.Execution;
using ImageAnalysis.Domain.Entities;
using ImageAnalysis.Domain.Events;
using ImageAnalysis.Domain.UnitTests.Infrastructure;

namespace ImageAnalysis.Domain.UnitTests.ImageSessionTests;

public sealed class ImageSessionRoiTests
{
    [Fact]
    public void SelectRoi_WithoutImage_ThrowsInvalidOperationException()
    {
        var session = ImageSession.Create();

        var act = () => session.SelectRoi(RoiBoundsMother.Small());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SelectRoi_ValidBounds_AddsRoiToRegions()
    {
        var session = new ImageSessionBuilder().Build();

        session.SelectRoi(RoiBoundsMother.Small());

        session.Regions.Should().HaveCount(1);
    }

    [Fact]
    public void SelectRoi_NewRoi_IsActiveRoi()
    {
        var session = new ImageSessionBuilder().Build();

        var roi = session.SelectRoi(RoiBoundsMother.Small());

        session.ActiveRoi.Should().BeSameAs(roi);
    }

    [Fact]
    public void SelectRoi_WhenPreviousRoiExists_DeactivatesPreviousRoi()
    {
        var session = new ImageSessionBuilder().Build();

        var first = session.SelectRoi(RoiBoundsMother.Small());
        session.ClearDomainEvents();

        session.SelectRoi(RoiBoundsMother.Medium());

        first.IsActive.Should().BeFalse(
            because: "only one ROI should be active at a time");
    }

    [Fact]
    public void SelectRoi_BoundsOutsideImage_ThrowsArgumentOutOfRangeException()
    {
        // Small image: 100×100; ROI goes outside
        var session = new ImageSessionBuilder().WithSmallImage().Build();

        var act = () => session.SelectRoi(RoiBoundsMother.OutsideSmallImage());

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SelectRoi_RaisesRoiSelectedEvent()
    {
        var session = new ImageSessionBuilder().Build();
        var bounds = RoiBoundsMother.Small();

        var roi = session.SelectRoi(bounds);

        var evt = session.ShouldHaveSingleEvent<RoiSelectedEvent>();
        using var _ = new AssertionScope();
        evt.RoiId.Should().Be(roi.Id);
        evt.Bounds.Should().Be(bounds);
    }

    [Fact]
    public void RemoveRoi_ExistingId_RemovesIt()
    {
        var session = new ImageSessionBuilder().Build();
        var roi = session.SelectRoi(RoiBoundsMother.Small());
        session.ClearDomainEvents();

        session.RemoveRoi(roi.Id);

        session.Regions.Should().BeEmpty();
    }

    [Fact]
    public void RemoveRoi_ActiveRoi_SetsActiveRoiToNull()
    {
        var session = new ImageSessionBuilder().Build();
        var roi = session.SelectRoi(RoiBoundsMother.Small());
        session.ClearDomainEvents();

        session.RemoveRoi(roi.Id);

        session.ActiveRoi.Should().BeNull();
    }

    [Fact]
    public void RemoveRoi_NonActiveRoi_DoesNotClearActiveRoi()
    {
        var session = new ImageSessionBuilder().Build();
        var first = session.SelectRoi(RoiBoundsMother.Small());
        var second = session.SelectRoi(RoiBoundsMother.Medium());
        session.ClearDomainEvents();

        // Remove the non-active (first) ROI
        session.RemoveRoi(first.Id);

        session.ActiveRoi.Should().BeSameAs(second,
            because: "removing a non-active ROI should not change the active ROI");
    }

    [Fact]
    public void RemoveRoi_UnknownId_ThrowsInvalidOperationException()
    {
        var session = new ImageSessionBuilder().Build();

        var act = () => session.RemoveRoi(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RemoveRoi_RaisesRoiRemovedEvent()
    {
        var session = new ImageSessionBuilder().Build();
        var roi = session.SelectRoi(RoiBoundsMother.Small());
        session.ClearDomainEvents();

        session.RemoveRoi(roi.Id);

        var evt = session.ShouldHaveSingleEvent<RoiRemovedEvent>();
        evt.RoiId.Should().Be(roi.Id);
    }

    [Fact]
    public void ActivateRoi_SwitchesActiveRoi()
    {
        var session = new ImageSessionBuilder().Build();
        var first = session.SelectRoi(RoiBoundsMother.Small());
        var second = session.SelectRoi(RoiBoundsMother.Medium());

        session.ActivateRoi(first.Id);

        using var _ = new AssertionScope();
        session.ActiveRoi.Should().BeSameAs(first);
        second.IsActive.Should().BeFalse();
    }
}