using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Diploma.Model;
using Diploma.Mvvm;
using ImageAnalysis.Application.Dtos;

namespace Diploma.ViewModel;

public sealed class ImageCanvasViewModel : BaseViewModel
{
    // -------------------------------------------------------------------------
    // Backing fields
    // -------------------------------------------------------------------------

    private BitmapSource? _imageSource;
    private double _zoomLevel = 1.0;
    private double _panOffsetX;
    private double _panOffsetY;
    private InteractionMode _interactionMode = InteractionMode.View;
    private DraftOverlayItem? _draftOverlay;
    private Point? _measurementFirstPoint;
    private Point _cursorPosition;
    private bool _hasImage;
    private string? _statusText;

    // Pan drag state — not properties, just internal tracking
    private bool _isPanning;
    private Point _panStartMouse;
    private double _panStartOffsetX;
    private double _panStartOffsetY;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public ImageCanvasViewModel()
    {
        ZoomInCommand = new RelayCommand(_ => ZoomIn(), _ => HasImage);
        ZoomOutCommand = new RelayCommand(_ => ZoomOut(), _ => HasImage && ZoomLevel > MinZoom);
        ResetZoomCommand = new RelayCommand(_ => ResetView(), _ => HasImage);

        RemoveRoiCommand = new RelayCommand(id => RemoveRoi((Guid)id), _ => HasImage);
        RemoveMeasurementCommand = new RelayCommand(id => RemoveMeasurement((Guid)id), _ => HasImage);
        SelectContourCommand = new RelayCommand(id => SelectContour((Guid)id), _ => HasImage);
    }

    // -------------------------------------------------------------------------
    // Image
    // -------------------------------------------------------------------------

    /// <summary>Currently displayed bitmap. Null means no image is loaded.</summary>
    public BitmapSource? ImageSource
    {
        get => _imageSource;
        set
        {
            SetField(ref _imageSource, value);
            HasImage = value is not null;
            if (value is not null) ResetView();
        }
    }

    public bool HasImage
    {
        get => _hasImage;
        private set => SetField(ref _hasImage, value);
    }

    // -------------------------------------------------------------------------
    // Zoom / Pan
    // -------------------------------------------------------------------------

    public const double MinZoom = 0.05;
    public const double MaxZoom = 20.0;
    public const double ZoomStep = 0.15;

    public double ZoomLevel
    {
        get => _zoomLevel;
        private set
        {
            var clamped = Math.Clamp(value, MinZoom, MaxZoom);
            SetField(ref _zoomLevel, clamped);
            OnPropertyChanged(nameof(ZoomPercent));
        }
    }

    public string ZoomPercent => $"{ZoomLevel * 100:F0}%";

    public double PanOffsetX
    {
        get => _panOffsetX;
        private set => SetField(ref _panOffsetX, value);
    }

    public double PanOffsetY
    {
        get => _panOffsetY;
        private set => SetField(ref _panOffsetY, value);
    }

    // -------------------------------------------------------------------------
    // Interaction mode
    // -------------------------------------------------------------------------

    public InteractionMode InteractionMode
    {
        get => _interactionMode;
        set
        {
            SetField(ref _interactionMode, value);

            // Cancel any in-progress draft when switching modes
            DraftOverlay = null;
            _measurementFirstPoint = null;
            StatusText = ModeHint(value);
        }
    }

    // -------------------------------------------------------------------------
    // Status
    // -------------------------------------------------------------------------

    public Point CursorPosition
    {
        get => _cursorPosition;
        private set
        {
            SetField(ref _cursorPosition, value);
            OnPropertyChanged(nameof(CursorPositionText));
        }
    }

    public string CursorPositionText =>
        $"X: {(int)CursorPosition.X}  Y: {(int)CursorPosition.Y}";

    public string? StatusText
    {
        get => _statusText;
        private set => SetField(ref _statusText, value);
    }

    // -------------------------------------------------------------------------
    // Overlays
    // -------------------------------------------------------------------------

    public ObservableCollection<RoiOverlayItem> RoiOverlays { get; } = [];
    public ObservableCollection<MeasurementOverlayItem> MeasurementOverlays { get; } = [];
    public ObservableCollection<ContourOverlayItem> ContourOverlays { get; } = [];

    /// <summary>Rubber-band shape being drawn; null when not in a draw operation.</summary>
    public DraftOverlayItem? DraftOverlay
    {
        get => _draftOverlay;
        private set => SetField(ref _draftOverlay, value);
    }

    // -------------------------------------------------------------------------
    // Commands
    // -------------------------------------------------------------------------

    public ICommand ZoomInCommand { get; }
    public ICommand ZoomOutCommand { get; }
    public ICommand ResetZoomCommand { get; }
    public ICommand RemoveRoiCommand { get; }
    public ICommand RemoveMeasurementCommand { get; }
    public ICommand SelectContourCommand { get; }

    // -------------------------------------------------------------------------
    // Events raised to parent ViewModel
    // -------------------------------------------------------------------------

    /// <summary>Raised when the user finishes drawing a new ROI.</summary>
    public event Action<RoiBoundsDto>? RoiDrawn;

    /// <summary>Raised when the user places both measurement endpoints.</summary>
    public event Action<PixelPointDto, PixelPointDto>? MeasurementPointsPlaced;

    /// <summary>Raised when the user requests removal of an overlay item.</summary>
    public event Action<Guid>? RoiRemovalRequested;

    public event Action<Guid>? MeasurementRemovalRequested;
    public event Action<Guid>? ContourSelectionRequested;

    // -------------------------------------------------------------------------
    // Mouse event handlers — called from View code-behind
    // (minimal code-behind: just translates WPF events to ViewModel calls)
    // -------------------------------------------------------------------------

    public void OnMouseWheel(Point position, double delta)
    {
        if (!HasImage) return;

        // Zoom toward the cursor position
        var zoomFactor = delta > 0 ? 1.0 + ZoomStep : 1.0 - ZoomStep;
        var newZoom = Math.Clamp(ZoomLevel * zoomFactor, MinZoom, MaxZoom);
        var scaleDelta = newZoom / ZoomLevel;

        PanOffsetX = position.X - scaleDelta * (position.X - PanOffsetX);
        PanOffsetY = position.Y - scaleDelta * (position.Y - PanOffsetY);
        ZoomLevel = newZoom;
    }

    public void OnMouseMove(Point canvasPosition)
    {
        var imagePoint = CanvasToImage(canvasPosition);
        CursorPosition = imagePoint;

        if (_interactionMode == InteractionMode.View && _isPanning)
        {
            PanOffsetX = _panStartOffsetX + (canvasPosition.X - _panStartMouse.X);
            PanOffsetY = _panStartOffsetY + (canvasPosition.Y - _panStartMouse.Y);
            return;
        }

        // Update rubber-band draft
        if (DraftOverlay is not null)
            DraftOverlay.EndPoint = imagePoint;
    }

    public void OnMouseDown(Point canvasPosition, MouseButton button)
    {
        if (!HasImage) return;

        var imagePoint = CanvasToImage(canvasPosition);

        switch (_interactionMode)
        {
            case InteractionMode.View when button == MouseButton.Left:
                _isPanning = true;
                _panStartMouse = canvasPosition;
                _panStartOffsetX = PanOffsetX;
                _panStartOffsetY = PanOffsetY;
                break;

            case InteractionMode.RoiSelection when button == MouseButton.Left:
                DraftOverlay = new DraftOverlayItem
                {
                    StartPoint = imagePoint,
                    EndPoint = imagePoint,
                    IsRectangle = true
                };
                break;

            case InteractionMode.Measurement when button == MouseButton.Left:
                if (_measurementFirstPoint is null)
                {
                    // First click — start the line draft
                    _measurementFirstPoint = imagePoint;
                    DraftOverlay = new DraftOverlayItem
                    {
                        StartPoint = imagePoint,
                        EndPoint = imagePoint,
                        IsRectangle = false
                    };
                }
                else
                {
                    // Second click — finalise measurement
                    var from = _measurementFirstPoint.Value;
                    MeasurementPointsPlaced?.Invoke(
                        new PixelPointDto((int)from.X, (int)from.Y),
                        new PixelPointDto((int)imagePoint.X, (int)imagePoint.Y));

                    _measurementFirstPoint = null;
                    DraftOverlay = null;
                }

                break;
        }
    }

    public void OnMouseUp(Point canvasPosition, MouseButton button)
    {
        if (button == MouseButton.Left && _isPanning)
        {
            _isPanning = false;
            return;
        }

        if (_interactionMode == InteractionMode.RoiSelection &&
            button == MouseButton.Left &&
            DraftOverlay is not null)
        {
            var rect = DraftOverlay.ToRect;

            // Ignore tiny accidental drags
            if (rect.Width > 4 && rect.Height > 4)
            {
                RoiDrawn?.Invoke(new RoiBoundsDto(
                    TopLeft: new PixelPointDto((int)rect.X, (int)rect.Y),
                    Width: (int)rect.Width,
                    Height: (int)rect.Height,
                    Area: (int)(rect.Width * rect.Height),
                    Center: new PixelPointDto((int)rect.X + (int)rect.Width / 2, (int)rect.Y + (int)rect.Height / 2),
                    BottomRight: new PixelPointDto((int)(rect.X + rect.Width), (int)(rect.Y + rect.Height))));
            }

            DraftOverlay = null;
        }
    }

    // -------------------------------------------------------------------------
    // Overlay mutation (called from MainViewModel after server round-trip)
    // -------------------------------------------------------------------------

    public void LoadImage(BitmapSource source)
    {
        ImageSource = source;
        ClearAllOverlays();
    }

    public void ApplyRoi(RegionOfInterestDto dto)
    {
        var existing = RoiOverlays.FirstOrDefault(r => r.Id == dto.Id);
        if (existing is not null)
        {
            existing.IsActive = dto.IsActive;
            return;
        }

        RoiOverlays.Add(new RoiOverlayItem
        {
            Id = dto.Id,
            TopLeft = new Point(dto.Bounds.TopLeft.X, dto.Bounds.TopLeft.Y),
            Width = dto.Bounds.Width,
            Height = dto.Bounds.Height,
            Label = dto.Label,
            IsActive = dto.IsActive
        });
    }

    public void RemoveRoiOverlay(Guid id)
    {
        var item = RoiOverlays.FirstOrDefault(r => r.Id == id);
        if (item is not null) RoiOverlays.Remove(item);
    }

    public void ApplyMeasurement(MeasurementDto dto)
    {
        if (MeasurementOverlays.Any(m => m.Id == dto.Id)) return;

        MeasurementOverlays.Add(new MeasurementOverlayItem
        {
            Id = dto.Id,
            From = new Point(dto.From.X, dto.From.Y),
            To = new Point(dto.To.X, dto.To.Y),
            DistancePixels = dto.DistancePixels,
            Label = dto.Label
        });
    }

    public void RemoveMeasurementOverlay(Guid id)
    {
        var item = MeasurementOverlays.FirstOrDefault(m => m.Id == id);
        if (item is not null) MeasurementOverlays.Remove(item);
    }

    public void ApplyContours(IEnumerable<ContourDto> contours)
    {
        ContourOverlays.Clear();
        foreach (var dto in contours)
        {
            ContourOverlays.Add(new ContourOverlayItem
            {
                Id = dto.Id,
                Points = dto.Points.Select(p => new Point(p.X, p.Y)).ToList(),
                IsSelected = dto.IsSelected
            });
        }
    }

    public void UpdateContourSelection(Guid selectedId)
    {
        foreach (var overlay in ContourOverlays)
            overlay.IsSelected = overlay.Id == selectedId;
    }

    public void ClearAllOverlays()
    {
        RoiOverlays.Clear();
        MeasurementOverlays.Clear();
        ContourOverlays.Clear();
        DraftOverlay = null;
        _measurementFirstPoint = null;
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private void ZoomIn() => ZoomLevel += ZoomStep;
    private void ZoomOut() => ZoomLevel -= ZoomStep;

    private void ResetView()
    {
        ZoomLevel = 1.0;
        PanOffsetX = 0;
        PanOffsetY = 0;
    }

    private void RemoveRoi(Guid id)
    {
        RemoveRoiOverlay(id);
        RoiRemovalRequested?.Invoke(id);
    }

    private void RemoveMeasurement(Guid id)
    {
        RemoveMeasurementOverlay(id);
        MeasurementRemovalRequested?.Invoke(id);
    }

    private void SelectContour(Guid id)
    {
        UpdateContourSelection(id);
        ContourSelectionRequested?.Invoke(id);
    }

    /// <summary>Converts a point in canvas (screen) space to image pixel space.</summary>
    private Point CanvasToImage(Point canvas) =>
        new((canvas.X - PanOffsetX) / ZoomLevel,
            (canvas.Y - PanOffsetY) / ZoomLevel);

    private static string ModeHint(InteractionMode mode) => mode switch
    {
        InteractionMode.View => "Режим просмотра: прокрутка для масштаба, ЛКМ для перемещения",
        InteractionMode.RoiSelection => "Режим ROI: нарисуйте прямоугольник на изображении",
        InteractionMode.Measurement => "Режим измерения: кликните первую и вторую точку",
        _ => string.Empty
    };

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            (ZoomInCommand as IDisposable)?.Dispose();
            (ZoomOutCommand as IDisposable)?.Dispose();
            (ResetZoomCommand as IDisposable)?.Dispose();
            (RemoveRoiCommand as IDisposable)?.Dispose();
            (RemoveMeasurementCommand as IDisposable)?.Dispose();
            (SelectContourCommand as IDisposable)?.Dispose();
        }

        base.Dispose(disposing);
    }
}