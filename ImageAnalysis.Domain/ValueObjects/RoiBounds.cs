namespace ImageAnalysis.Domain.ValueObjects;

/// <summary>
/// Прямоугольные границы области интереса (ROI).
/// Гарантирует корректность геометрии при создании.
/// </summary>
public sealed record RoiBounds
{
    public PixelPoint TopLeft { get; }
    public int Width { get; }
    public int Height { get; }
 
    public PixelPoint BottomRight => new(TopLeft.X + Width, TopLeft.Y + Height);
    public int Area => Width * Height;
    public PixelPoint Center => new(TopLeft.X + Width / 2, TopLeft.Y + Height / 2);
 
    public RoiBounds(PixelPoint topLeft, int width, int height)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), "Ширина ROI должна быть положительной.");
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "Высота ROI должна быть положительной.");
        TopLeft = topLeft;
        Width = width;
        Height = height;
    }
 
    public bool Contains(PixelPoint point) =>
        point.X >= TopLeft.X && point.X <= BottomRight.X &&
        point.Y >= TopLeft.Y && point.Y <= BottomRight.Y;
 
    public bool Intersects(RoiBounds other) =>
        TopLeft.X < other.BottomRight.X && BottomRight.X > other.TopLeft.X &&
        TopLeft.Y < other.BottomRight.Y && BottomRight.Y > other.TopLeft.Y;
 
    public override string ToString() => $"[{TopLeft} {Width}x{Height}]";
}