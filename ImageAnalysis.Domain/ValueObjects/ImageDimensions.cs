namespace ImageAnalysis.Domain.ValueObjects;

/// <summary>
/// Размеры изображения. Валидируются при создании.
/// </summary>
public sealed record ImageDimensions
{
    public int Width { get; }
    public int Height { get; }
    public int TotalPixels => Width * Height;
 
    public ImageDimensions(int width, int height)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), "Ширина должна быть положительной.");
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "Высота должна быть положительной.");
        Width = width;
        Height = height;
    }
 
    public bool Contains(PixelPoint point) =>
        point.X >= 0 && point.X < Width &&
        point.Y >= 0 && point.Y < Height;
 
    public override string ToString() => $"{Width}x{Height}";
}