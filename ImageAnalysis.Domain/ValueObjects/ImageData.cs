namespace ImageAnalysis.Domain.ValueObjects;

/// <summary>
/// Бинарные данные изображения + метаданные.
/// Оборачивает raw-байты — не позволяет передавать пустой массив.
/// </summary>
public sealed class ImageData
{
    public byte[] Bytes { get; }
    public ImageDimensions Dimensions { get; }
    public string Format { get; }   // "PNG", "JPEG", "BMP", ...
 
    public ImageData(byte[] bytes, ImageDimensions dimensions, string format)
    {
        if (bytes is null || bytes.Length == 0)
            throw new ArgumentException("Данные изображения не могут быть пустыми.", nameof(bytes));
 
        Bytes = bytes;
        Dimensions = dimensions;
        Format = format;
    }
}