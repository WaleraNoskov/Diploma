using System.Windows;

namespace Diploma.Model;

/// <summary>
/// Transient overlay item created while the user is dragging to define
/// a new ROI or measurement. Replaced by a permanent overlay on mouse-up.
/// </summary>
public sealed class DraftOverlayItem : OverlayItem
{
    public Point StartPoint { get; set; }
    public Point EndPoint { get; set; }

    /// <summary>True when this draft represents an ROI rectangle; false for a line.</summary>
    public bool IsRectangle { get; init; }

    public Rect ToRect => new Rect(
        Math.Min(StartPoint.X, EndPoint.X),
        Math.Min(StartPoint.Y, EndPoint.Y),
        Math.Abs(EndPoint.X - StartPoint.X),
        Math.Abs(EndPoint.Y - StartPoint.Y)
    );
}