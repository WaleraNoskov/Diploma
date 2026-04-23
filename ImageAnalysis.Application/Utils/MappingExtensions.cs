using ImageAnalysis.Application.Dtos;
using ImageAnalysis.Domain.Entities;
using ImageAnalysis.Domain.Entities.ProcessingOperations;
using ImageAnalysis.Domain.ValueObjects;

namespace ImageAnalysis.Application.Utils;

public static class MappingExtensions
{
    // ---- Domain → DTO ----

    public static ImageSessionDto ToDto(this Domain.Entities.ImageSession session) =>
        new(
            Id: session.Id,
            HasImage: session.HasImage,
            CurrentImageId: session.CurrentImage?.ImageId,
            OriginalImageId: session.OriginalImage?.ImageId,
            Dimensions: session.CurrentImage?.Dimensions.ToDto(),
            ContourCount: session.Contours.Count,
            MeasurementCount: session.Measurements.Count,
            RegionCount: session.Regions.Count,
            CanUndo: session.History.CanUndo,
            CanRedo: session.History.CanRedo,
            OperationHistory: session.History.Applied
                .Select(o => o.ToDto())
                .ToList(),
            CreatedAt: session.CreatedAt,
            LastModifiedAt: session.LastModifiedAt);

    public static ImageDimensionsDto ToDto(this ImageDimensions d) =>
        new(d.Width, d.Height, d.TotalPixels);

    public static OperationHistoryItemDto ToDto(this ProcessingOperation op) =>
        new(op.Id, op.OperationType, op.Describe(), op.AppliedAt);

    public static ContourDto ToDto(this Contour c) =>
        new(
            Id: c.Id,
            Area: c.Area,
            Perimeter: c.Perimeter,
            Centroid: c.Centroid.ToDto(),
            IsSelected: c.IsSelected,
            Points: c.Points.Points.Select(p => p.ToDto()).ToList());

    public static MeasurementDto ToDto(this Measurement m) =>
        new(
            Id: m.Id,
            From: m.From.ToDto(),
            To: m.To.ToDto(),
            DistancePixels: m.Distance.Pixels,
            Label: m.Label,
            CreatedAt: m.CreatedAt);

    public static RegionOfInterestDto ToDto(this RegionOfInterest r) =>
        new(
            Id: r.Id,
            Bounds: r.Bounds.ToDto(),
            Label: r.Label,
            IsActive: r.IsActive,
            CreatedAt: r.CreatedAt);

    public static RoiBoundsDto ToDto(this RoiBounds b) =>
        new(
            TopLeft: b.TopLeft.ToDto(),
            Width: b.Width,
            Height: b.Height,
            Area: b.Area,
            Center: b.Center.ToDto(),
            BottomRight: b.BottomRight.ToDto());

    public static PixelPointDto ToDto(this PixelPoint p) => new(p.X, p.Y);

    // ---- DTO → Domain ----

    public static PixelPoint ToDomain(this PixelPointDto dto) => new(dto.X, dto.Y);

    public static RoiBounds ToDomain(this RoiBoundsDto dto) =>
        new(dto.TopLeft.ToDomain(), dto.Width, dto.Height);
}