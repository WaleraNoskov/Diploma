using ImageAnalysis.Domain.Entities;
using ImageAnalysis.Domain.ValueObjects;

namespace ImageAnalysis.Domain.UnitTests.EntityBuilders;

public class ContourBuilder
{
    private IEnumerable<ContourPoints> _points =
        [new([new PixelPoint(0, 0), new PixelPoint(0, 1), new PixelPoint(1, 0)])];
    
    private bool _isSelected;

    public ContourBuilder WithPoints(IEnumerable<ContourPoints> points)
    {
        _points = points;
        return this;
    }

    public ContourBuilder WithIsSelected(bool isSelected)
    {
        _isSelected = isSelected;
        return this;
    }

    public Contour Build()
    {
        var image = new ImageData([0], new ImageDimensions(1, 1), "tiff");
        var session = ImageSession.Create();
        session.LoadImage(image);
        session.SetDetectedContours(_points);

        var contourId = session.Contours.First().Id;
        session.SelectContour(contourId);

        var contour = session.SelectedContour!;
        
        if(!_isSelected)
            session.DeselectContour();

        return contour;
    }
}