using FluentAssertions;
using ImageAnalysis.Domain.UnitTests.EntityBuilders;
using ImageAnalysis.Domain.ValueObjects;

namespace ImageAnalysis.Domain.UnitTests.EntitiesTests;

public class ContourTests
{
    [Fact]
    public void Area_CalculatesCorrect()
    {
        //Arrange
        var contourBuilder = new ContourBuilder();
        var points = new ContourPoints([
            new PixelPoint(0, 0),
            new PixelPoint(2, 0),
            new PixelPoint(0, 1),
            new PixelPoint(2, 1)
        ]);
        var contour = contourBuilder.WithPoints([points]).Build();

        //Act
        var area = contour.Area;
        
        //Assert
        area.Should().Be(points.Area());
    }

    [Fact]
    public void Perimeter_CalculatesCorrect()
    {
        //Arrange
        var contourBuilder = new ContourBuilder();
        var points = new ContourPoints([
            new PixelPoint(0, 0),
            new PixelPoint(2, 0),
            new PixelPoint(0, 1),
            new PixelPoint(2, 1)
        ]);
        var contour = contourBuilder.WithPoints([points]).Build();

        //Act
        var perimeter = contour.Perimeter;
        
        //Assert
        perimeter.Should().Be(points.Perimeter());
    }

    [Fact]
    public void Centroid_CalculatesCorrect()
    {
        //Arrange
        var contourBuilder = new ContourBuilder();
        var points = new ContourPoints([
            new PixelPoint(0, 0),
            new PixelPoint(2, 0),
            new PixelPoint(0, 1),
            new PixelPoint(2, 1)
        ]);
        var contour = contourBuilder.WithPoints([points]).Build();

        //Act
        var centroid = contour.Centroid;
        
        //Assert
        centroid.Should().Be(points.Centroid());
    }
    
    [Fact]
    public void Points_ShouldReturnCurrentPoints()
    {
        //Arrange
        var contourBuilder = new ContourBuilder();
        var points = new ContourPoints([
            new PixelPoint(0, 0),
            new PixelPoint(2, 0),
            new PixelPoint(0, 1),
            new PixelPoint(2, 1)
        ]);
        var contour = contourBuilder.WithPoints([points]).Build();
        
        //Act
        var gotPoints = contour.Points;
        
        //Assert
        gotPoints.Should().BeEquivalentTo(points);
    }
}