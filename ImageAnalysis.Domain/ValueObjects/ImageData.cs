namespace ImageAnalysis.Domain.ValueObjects;

/// <summary>
/// Бинарные данные изображения + метаданные.
/// Оборачивает raw-байты — не позволяет передавать пустой массив.
/// </summary>
public sealed class ImageData
{
    public Guid ImageId { get; }
    public ImageDimensions Dimensions { get; }
    public string Format { get; }   // "PNG", "JPEG", "BMP", ...
 
    public ImageData(Guid imageId, ImageDimensions dimensions, string format)
    {
        if (imageId == Guid.Empty)
            throw new ArgumentException("Данные изображения не могут быть пустыми.", nameof(imageId));
 
        ImageId = imageId;
        Dimensions = dimensions;
        Format = format;
    }
}