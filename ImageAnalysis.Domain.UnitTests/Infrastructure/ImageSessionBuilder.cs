using ImageAnalysis.Domain.Entities;
using ImageAnalysis.Domain.ValueObjects;

namespace ImageAnalysis.Domain.UnitTests.Infrastructure;

/// <summary>
/// Fluent builder for <see cref="ImageSession"/> pre-loaded with common state.
/// Eliminates repetitive Arrange boilerplate in aggregate tests.
/// </summary>
internal sealed class ImageSessionBuilder
{
    private ImageData _image = ImageDataMother.Default();
    private List<ContourPoints> _contours = [];
    private ContourFilterCriteria? _filter;

    public ImageSessionBuilder WithImage(ImageData image)
    {
        _image = image;
        return this;
    }

    public ImageSessionBuilder WithSmallImage()
        => WithImage(ImageDataMother.Small());

    public ImageSessionBuilder WithDetectedContours(params ContourPoints[] contours)
    {
        _contours = [..contours];
        return this;
    }

    public ImageSessionBuilder WithContourFilter(ContourFilterCriteria filter)
    {
        _filter = filter;
        return this;
    }

    public ImageSession Build()
    {
        var session = ImageSession.Create();
        session.LoadImage(_image);
        session.ClearDomainEvents(); // isolate: only events after build matter

        if (_contours.Count > 0)
        {
            session.SetDetectedContours(_contours, _filter);
            session.ClearDomainEvents();
        }

        return session;
    }
}