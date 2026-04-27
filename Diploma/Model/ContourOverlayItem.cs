using System.Windows;
using System.Windows.Media;

namespace Diploma.Model;

/// <summary>
/// Polygon outline around a detected contour.
/// </summary>
public sealed class ContourOverlayItem : OverlayItem
{
    public IReadOnlyList<Point> Points { get; init; } = [];
    public bool IsSelected { get; set; }

    /// <summary>
    /// Selected contours are rendered in green; unselected in semi-transparent cyan.
    /// </summary>
    public Brush Stroke => IsSelected
        ? new SolidColorBrush(Color.FromRgb(0x00, 0xE6, 0x76)) // green
        : new SolidColorBrush(Color.FromArgb(0xCC, 0x00, 0xBC, 0xFF)); // cyan-alpha

    public double StrokeThickness => IsSelected ? 2.5 : 1.5;

    /// <summary>Converts to WPF PointCollection for Polygon binding.</summary>
    public PointCollection ToPointCollection()
    {
        var pc = new PointCollection();
        foreach (var p in Points) pc.Add(p);
        return pc;
    }
}