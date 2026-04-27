using System.Windows;
using System.Windows.Media;

namespace Diploma.Model;

/// <summary>
/// Rectangular region-of-interest drawn as a dashed border rectangle.
/// </summary>
public sealed class RoiOverlayItem : OverlayItem
{
    /// <summary>Top-left corner in image pixels.</summary>
    public Point TopLeft { get; set; }

    public double Width { get; set; }
    public double Height { get; set; }
    public string? Label { get; set; }
    public bool IsActive { get; set; }

    /// <summary>Stroke color: active ROI is highlighted in a distinct colour.</summary>
    public Brush Stroke => IsActive
        ? new SolidColorBrush(Color.FromRgb(0xFF, 0xA5, 0x00)) // amber
        : new SolidColorBrush(Color.FromRgb(0x00, 0xBC, 0xFF)); // sky-blue

    public Rect ToRect() => new(TopLeft, new Size(Width, Height));
}