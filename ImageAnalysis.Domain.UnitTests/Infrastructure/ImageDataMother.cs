using ImageAnalysis.Domain.ValueObjects;

namespace ImageAnalysis.Domain.UnitTests.Infrastructure;

/// <summary>
/// Factory for <see cref="ImageData"/> test values.
/// </summary>
public class ImageDataMother
{
    public static ImageData Default() =>
        new(imageId: Guid.NewGuid(),
            dimensions: new ImageDimensions(TestConstants.DefaultImageWidth, TestConstants.DefaultImageHeight),
            format: TestConstants.DefaultFormat);

    public static ImageData WithDimensions(int width, int height) =>
        new(imageId: Guid.NewGuid(),
            dimensions: new ImageDimensions(width, height),
            format: TestConstants.DefaultFormat);

    public static ImageData Small() => WithDimensions(100, 100);
}