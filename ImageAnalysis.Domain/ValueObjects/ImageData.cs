namespace ImageAnalysis.Domain.ValueObjects;

/// <summary>
/// Бинарные данные изображения + метаданные.
/// Оборачивает raw-байты — не позволяет передавать пустой массив.
/// </summary>
public sealed class ImageData
{
    public Guid ImageId { get; }
    public ImageDimensions Dimensions { get; }
    public int Channels { get; }
    public int ChannelSize { get; }
    public int Stride { get; }
    public string Format { get; }   // "PNG", "JPEG", "BMP", ...
 
    public ImageData(Guid imageId,
        ImageDimensions dimensions,
        string format,
        int channels,
        int channelSize,
        int stride)
    {
        if (imageId == Guid.Empty)
            throw new ArgumentException("Данные изображения не могут быть пустыми.", nameof(imageId));
        if (channels < 1)
            throw new ArgumentException("Количество каналов не может быть меньше одного");
 
        ImageId = imageId;
        Dimensions = dimensions;
        Format = format;
        Channels = channels;
        ChannelSize = channelSize;
        Stride = stride;
    }
}