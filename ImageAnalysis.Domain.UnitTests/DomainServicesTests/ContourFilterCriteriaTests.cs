using FluentAssertions;
using ImageAnalysis.Domain.UnitTests.Infrastructure;
using ImageAnalysis.Domain.ValueObjects;

namespace ImageAnalysis.Domain.UnitTests.DomainServicesTests;

public sealed class ContourFilterCriteriaTests
{
    [Fact]
    public void Matches_NoCriteria_AlwaysReturnsTrue()
    {
        var filter = new ContourFilterCriteria();
        var session = new ImageSessionBuilder()
            .WithDetectedContours(ContourPointsMother.SmallSquare())
            .Build();

        var contour = session.Contours.First();

        filter.Matches(contour).Should().BeTrue();
    }

    [Fact]
    public void Matches_ContourAreaBelowMinArea_ReturnsFalse()
    {
        var filter = new ContourFilterCriteria { MinArea = 500 };
        var session = new ImageSessionBuilder()
            .WithDetectedContours(ContourPointsMother.SmallSquare()) // area = 100
            .Build();

        filter.Matches(session.Contours.First()).Should().BeFalse();
    }

    [Fact]
    public void Matches_ContourAreaAboveMaxArea_ReturnsFalse()
    {
        var filter = new ContourFilterCriteria { MaxArea = 50 };
        var session = new ImageSessionBuilder()
            .WithDetectedContours(ContourPointsMother.SmallSquare()) // area = 100
            .Build();

        filter.Matches(session.Contours.First()).Should().BeFalse();
    }

    [Fact]
    public void Matches_ContourWithinAllCriteria_ReturnsTrue()
    {
        var filter = new ContourFilterCriteria { MinArea = 50, MaxArea = 200 };
        var session = new ImageSessionBuilder()
            .WithDetectedContours(ContourPointsMother.SmallSquare()) // area = 100
            .Build();

        filter.Matches(session.Contours.First()).Should().BeTrue();
    }

    [Fact]
    public void Matches_ContourBelowMinPerimeter_ReturnsFalse()
    {
        var filter = new ContourFilterCriteria { MinPerimeter = 1000 };
        var session = new ImageSessionBuilder()
            .WithDetectedContours(ContourPointsMother.SmallSquare()) // perimeter = 40
            .Build();

        filter.Matches(session.Contours.First()).Should().BeFalse();
    }
}