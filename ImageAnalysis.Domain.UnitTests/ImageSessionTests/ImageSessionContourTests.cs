using FluentAssertions;
using ImageAnalysis.Domain.Entities;
using ImageAnalysis.Domain.Events;
using ImageAnalysis.Domain.UnitTests.Infrastructure;
using ImageAnalysis.Domain.ValueObjects;

namespace ImageAnalysis.Domain.UnitTests.ImageSessionTests;

public sealed class ImageSessionContoursTests
{
    [Fact]
    public void SetDetectedContours_WithoutImage_ThrowsInvalidOperationException()
    {
        var session = ImageSession.Create();
 
        var act = () => session.SetDetectedContours([ContourPointsMother.SmallSquare()]);
 
        act.Should().Throw<InvalidOperationException>();
    }
 
    [Fact]
    public void SetDetectedContours_PopulatesContourCollection()
    {
        var session = new ImageSessionBuilder().Build();
 
        session.SetDetectedContours([
            ContourPointsMother.SmallSquare(),
            ContourPointsMother.LargeRectangle()
        ]);
 
        session.Contours.Should().HaveCount(2);
    }
 
    [Fact]
    public void SetDetectedContours_RaisesContoursDetectedEvent()
    {
        var session = new ImageSessionBuilder().Build();
 
        session.SetDetectedContours([
            ContourPointsMother.SmallSquare(),
            ContourPointsMother.Triangle()
        ]);
 
        var evt = session.ShouldHaveSingleEvent<ContoursDetectedEvent>();
        evt.ContourCount.Should().Be(2);
    }
 
    [Fact]
    public void SetDetectedContours_ReplacesExistingContours()
    {
        var session = new ImageSessionBuilder()
            .WithDetectedContours(ContourPointsMother.SmallSquare(), ContourPointsMother.Triangle())
            .Build();
 
        session.SetDetectedContours([ContourPointsMother.LargeRectangle()]);
 
        session.Contours.Should().HaveCount(1,
            because: "SetDetectedContours must replace, not append");
    }
 
    [Fact]
    public void SetDetectedContours_WithMinAreaFilter_ExcludesSmallContours()
    {
        var session = new ImageSessionBuilder().Build();
        var filter  = new ContourFilterCriteria { MinArea = 500 };
        // SmallSquare area = 100 → excluded; LargeRectangle area = 5000 → included
 
        session.SetDetectedContours(
            [ContourPointsMother.SmallSquare(), ContourPointsMother.LargeRectangle()],
            filter);
 
        session.Contours.Should().HaveCount(1);
        session.Contours.Single().Area.Should().BeGreaterThan(500);
    }
 
    [Fact]
    public void SelectContour_Unknown_ThrowsInvalidOperationException()
    {
        var session = new ImageSessionBuilder()
            .WithDetectedContours(ContourPointsMother.SmallSquare())
            .Build();
 
        var act = () => session.SelectContour(Guid.NewGuid());
 
        act.Should().Throw<InvalidOperationException>();
    }
 
    [Fact]
    public void SelectContour_KnownContour_SetsSelectedContour()
    {
        var session = new ImageSessionBuilder()
            .WithDetectedContours(ContourPointsMother.SmallSquare())
            .Build();
 
        var target = session.Contours.First();
        session.SelectContour(target.Id);
 
        session.SelectedContour.Should().BeSameAs(target);
    }
 
    [Fact]
    public void SelectContour_WhenAnotherAlreadySelected_DeselectsPrevious()
    {
        var session = new ImageSessionBuilder()
            .WithDetectedContours(ContourPointsMother.SmallSquare(), ContourPointsMother.Triangle())
            .Build();
 
        var first  = session.Contours.First();
        var second = session.Contours.Last();
 
        session.SelectContour(first.Id);
        session.ClearDomainEvents();
 
        session.SelectContour(second.Id);
 
        first.IsSelected.Should().BeFalse(
            because: "selecting a new contour must deselect the previous one");
    }
 
    [Fact]
    public void SelectContour_RaisesContourSelectedEvent()
    {
        var session = new ImageSessionBuilder()
            .WithDetectedContours(ContourPointsMother.SmallSquare())
            .Build();
 
        var contour = session.Contours.First();
        session.SelectContour(contour.Id);
 
        var evt = session.ShouldHaveSingleEvent<ContourSelectedEvent>();
        evt.ContourId.Should().Be(contour.Id);
    }
 
    [Fact]
    public void DeselectContour_WhenNoneSelected_DoesNotRaiseEvent()
    {
        var session = new ImageSessionBuilder()
            .WithDetectedContours(ContourPointsMother.SmallSquare())
            .Build();
 
        session.DeselectContour();
 
        session.ShouldHaveNoEvents();
    }
 
    [Fact]
    public void DeselectContour_WhenContourSelected_RaisesContourDeselectedEvent()
    {
        var session = new ImageSessionBuilder()
            .WithDetectedContours(ContourPointsMother.SmallSquare())
            .Build();
 
        var contour = session.Contours.First();
        session.SelectContour(contour.Id);
        session.ClearDomainEvents();
 
        session.DeselectContour();
 
        var evt = session.ShouldHaveSingleEvent<ContourDeselectedEvent>();
        evt.ContourId.Should().Be(contour.Id);
    }
 
    [Fact]
    public void DeselectContour_SetsSelectedContourToNull()
    {
        var session = new ImageSessionBuilder()
            .WithDetectedContours(ContourPointsMother.SmallSquare())
            .Build();
 
        session.SelectContour(session.Contours.First().Id);
        session.DeselectContour();
 
        session.SelectedContour.Should().BeNull();
    }
}