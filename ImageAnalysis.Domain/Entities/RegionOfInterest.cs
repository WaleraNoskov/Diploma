using ImageAnalysis.Domain.Base;
using ImageAnalysis.Domain.ValueObjects;

namespace ImageAnalysis.Domain.Entities;

/// <summary>
/// Область интереса (ROI) — прямоугольная зона для локального анализа.
/// </summary>
public sealed class RegionOfInterest : Entity<Guid>
{
    public RoiBounds Bounds { get; private set; }
    public string? Label { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; }
 
    internal RegionOfInterest(RoiBounds bounds, string? label = null)
    {
        Id = Guid.NewGuid();
        Bounds = bounds;
        Label = label;
        CreatedAt = DateTime.UtcNow;
    }
 
    internal void Resize(RoiBounds newBounds) => Bounds = newBounds;
    internal void Activate() => IsActive = true;
    internal void Deactivate() => IsActive = false;
    internal void Rename(string label) => Label = label;
}