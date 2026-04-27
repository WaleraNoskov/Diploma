using System.Windows;

namespace Diploma.Model;

/// <summary>
/// Line segment with an endpoint marker and a distance label.
/// </summary>
public sealed class MeasurementOverlayItem : OverlayItem
{
    public Point From { get; set; }
    public Point To { get; set; }
    public double DistancePixels { get; set; }
    public string? Label { get; set; }

    /// <summary>Midpoint for label placement.</summary>
    public Point LabelPosition => new(
        (From.X + To.X) / 2,
        (From.Y + To.Y) / 2 - 12); // offset above the line

    public string DisplayText =>
        string.IsNullOrWhiteSpace(Label)
            ? $"{DistancePixels:F1} px"
            : $"{Label}: {DistancePixels:F1} px";
}