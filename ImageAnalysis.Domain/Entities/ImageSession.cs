using ImageAnalysis.Domain.Base;
using ImageAnalysis.Domain.Entities.ProcessingOperations;
using ImageAnalysis.Domain.Events;
using ImageAnalysis.Domain.ValueObjects;

namespace ImageAnalysis.Domain.Entities;

/// <summary>
/// Сессия обработки изображения — корень агрегата.
/// 
/// Все изменения состояния проходят ТОЛЬКО через методы этого класса.
/// Публикует доменные события после каждого изменения состояния.
/// Инварианты:
///   - Нельзя применять операции без загруженного изображения.
///   - Нельзя выбрать контур вне обнаруженного набора.
///   - Нельзя создать измерение за пределами изображения.
///   - Только один ROI может быть Active в один момент времени.
/// </summary>
public sealed class ImageSession : AggregateRoot<Guid>
{
    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------
 
    private ImageData? _originalImage;
    private ImageData? _currentImage;
 
    private readonly List<Contour> _contours = [];
    private Contour? _selectedContour;
 
    private readonly List<Measurement> _measurements = [];
 
    private readonly List<RegionOfInterest> _regions = [];
    private RegionOfInterest? _activeRoi;
 
    public OperationHistory History { get; } = new();
 
    // -------------------------------------------------------------------------
    // Read-only projections
    // -------------------------------------------------------------------------
 
    public bool HasImage => _originalImage is not null;
    public ImageData? CurrentImage => _currentImage;
    public ImageData? OriginalImage => _originalImage;
 
    public IReadOnlyCollection<Contour> Contours => _contours.AsReadOnly();
    public Contour? SelectedContour => _selectedContour;
 
    public IReadOnlyCollection<Measurement> Measurements => _measurements.AsReadOnly();
 
    public IReadOnlyCollection<RegionOfInterest> Regions => _regions.AsReadOnly();
    public RegionOfInterest? ActiveRoi => _activeRoi;
 
    public DateTime CreatedAt { get; }
    public DateTime? LastModifiedAt { get; private set; }
 
    // -------------------------------------------------------------------------
    // Constructor — Factory method recommended; ctor protected for ORM
    // -------------------------------------------------------------------------

    private ImageSession()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }
 
    public static ImageSession Create()
    {
        return new ImageSession();
    }
 
    // -------------------------------------------------------------------------
    // Image Management
    // -------------------------------------------------------------------------
 
    /// <summary>Загрузить исходное изображение в сессию.</summary>
    public void LoadImage(ImageData imageData)
    {
        ArgumentNullException.ThrowIfNull(imageData);
 
        _originalImage = imageData;
        _currentImage = imageData;
        _contours.Clear();
        _measurements.Clear();
        _regions.Clear();
        _selectedContour = null;
        _activeRoi = null;
        History.Clear();
        Touch();
 
        Raise(new ImageLoadedEvent(Id, imageData.Dimensions, imageData.Format));
    }
 
    /// <summary>Сбросить к исходному изображению, очистить всю историю.</summary>
    public void Reset()
    {
        EnsureImageLoaded();
        _currentImage = _originalImage;
        _contours.Clear();
        _measurements.Clear();
        _regions.Clear();
        _selectedContour = null;
        _activeRoi = null;
        History.Clear();
        Touch();
 
        Raise(new SessionResetEvent(Id));
    }
 
    // -------------------------------------------------------------------------
    // Operations
    // -------------------------------------------------------------------------
 
    /// <summary>Применить операцию обработки к текущему изображению.</summary>
    public void ApplyOperation(ProcessingOperation operation, ImageData resultImage)
    {
        EnsureImageLoaded();
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(resultImage);
 
        History.Push(operation);
        _currentImage = resultImage;
 
        // Контуры инвалидируются при изменении изображения
        _contours.Clear();
        _selectedContour = null;
        Touch();
 
        Raise(new OperationAppliedEvent(Id, operation.Id, operation.OperationType));
    }
 
    /// <summary>Отменить последнюю операцию (Undo).</summary>
    public void UndoLastOperation(ImageData previousImage)
    {
        EnsureImageLoaded();
        if (!History.CanUndo)
            throw new InvalidOperationException("Нет операций для отмены.");
 
        var reverted = History.PopForUndo()!;
        _currentImage = previousImage;
        _contours.Clear();
        _selectedContour = null;
        Touch();
 
        Raise(new OperationUndoneEvent(Id, reverted.Id, reverted.OperationType));
    }
 
    // -------------------------------------------------------------------------
    // Contours
    // -------------------------------------------------------------------------
 
    /// <summary>
    /// Зафиксировать результат поиска контуров.
    /// Вызывается после выполнения алгоритма поиска (из Application layer).
    /// </summary>
    public void SetDetectedContours(IEnumerable<ContourPoints> contourPoints, ContourFilterCriteria? filter = null)
    {
        EnsureImageLoaded();
        _contours.Clear();
        _selectedContour = null;
 
        var all = contourPoints.Select(p => new Contour(p));
 
        // Фильтрация применяется внутри агрегата — бизнес-правило домена
        var filtered = filter is null
            ? all
            : all.Where(c => filter.Matches(c));
 
        _contours.AddRange(filtered);
        Touch();
 
        Raise(new ContoursDetectedEvent(Id, _contours.Count));
    }
 
    /// <summary>Выбрать контур для дальнейшего анализа.</summary>
    public void SelectContour(Guid contourId)
    {
        var contour = FindContourOrThrow(contourId);
 
        _selectedContour?.Deselect();
        contour.Select();
        _selectedContour = contour;
        Touch();
 
        Raise(new ContourSelectedEvent(Id, contour.Id, contour.Area));
    }
 
    /// <summary>Снять выделение с текущего контура.</summary>
    public void DeselectContour()
    {
        if (_selectedContour is null) return;
 
        var id = _selectedContour.Id;
        _selectedContour.Deselect();
        _selectedContour = null;
        Touch();
 
        Raise(new ContourDeselectedEvent(Id, id));
    }
 
    // -------------------------------------------------------------------------
    // Measurements
    // -------------------------------------------------------------------------
 
    /// <summary>Провести измерение расстояния между двумя точками.</summary>
    public Measurement TakeMeasurement(PixelPoint from, PixelPoint to, string? label = null)
    {
        EnsureImageLoaded();
        EnsurePointInImage(from);
        EnsurePointInImage(to);
 
        if (from == to)
            throw new InvalidOperationException("Точки измерения не могут совпадать.");
 
        var measurement = new Measurement(from, to, label);
        _measurements.Add(measurement);
        Touch();
 
        Raise(new MeasurementTakenEvent(Id, measurement.Id, measurement.Distance));
        return measurement;
    }
 
    /// <summary>Удалить измерение по идентификатору.</summary>
    public void RemoveMeasurement(Guid measurementId)
    {
        var m = _measurements.FirstOrDefault(m => m.Id == measurementId)
            ?? throw new InvalidOperationException($"Измерение {measurementId} не найдено.");
 
        _measurements.Remove(m);
        Touch();
 
        Raise(new MeasurementRemovedEvent(Id, measurementId));
    }
 
    // -------------------------------------------------------------------------
    // Regions of Interest
    // -------------------------------------------------------------------------
 
    /// <summary>Выделить новую область интереса.</summary>
    public RegionOfInterest SelectRoi(RoiBounds bounds, string? label = null)
    {
        EnsureImageLoaded();
        EnsureRoiBoundsInImage(bounds);
 
        // Деактивировать предыдущий активный ROI
        _activeRoi?.Deactivate();
 
        var roi = new RegionOfInterest(bounds, label);
        roi.Activate();
        _regions.Add(roi);
        _activeRoi = roi;
        Touch();
 
        Raise(new RoiSelectedEvent(Id, roi.Id, bounds));
        return roi;
    }
 
    /// <summary>Удалить ROI.</summary>
    public void RemoveRoi(Guid roiId)
    {
        var roi = _regions.FirstOrDefault(r => r.Id == roiId)
            ?? throw new InvalidOperationException($"ROI {roiId} не найден.");
 
        if (_activeRoi?.Id == roiId)
            _activeRoi = null;
 
        _regions.Remove(roi);
        Touch();
 
        Raise(new RoiRemovedEvent(Id, roiId));
    }
 
    /// <summary>Сделать ROI активным.</summary>
    public void ActivateRoi(Guid roiId)
    {
        var roi = FindRoiOrThrow(roiId);
        _activeRoi?.Deactivate();
        roi.Activate();
        _activeRoi = roi;
        Touch();
    }
 
    // -------------------------------------------------------------------------
    // Guards (инварианты агрегата)
    // -------------------------------------------------------------------------
 
    private void EnsureImageLoaded()
    {
        if (!HasImage)
            throw new InvalidOperationException("Изображение не загружено в сессию.");
    }
 
    private void EnsurePointInImage(PixelPoint point)
    {
        if (_originalImage is null || !_originalImage.Dimensions.Contains(point))
            throw new ArgumentOutOfRangeException(nameof(point),
                $"Точка {point} находится за пределами изображения {_originalImage?.Dimensions}.");
    }
 
    private void EnsureRoiBoundsInImage(RoiBounds bounds)
    {
        EnsureImageLoaded();
        var dim = _originalImage!.Dimensions;
        if (!dim.Contains(bounds.TopLeft) || !dim.Contains(bounds.BottomRight))
            throw new ArgumentOutOfRangeException(nameof(bounds),
                $"ROI {bounds} выходит за пределы изображения {dim}.");
    }
 
    private Contour FindContourOrThrow(Guid contourId) =>
        _contours.FirstOrDefault(c => c.Id == contourId)
        ?? throw new InvalidOperationException($"Контур {contourId} не найден в текущей сессии.");
 
    private RegionOfInterest FindRoiOrThrow(Guid roiId) =>
        _regions.FirstOrDefault(r => r.Id == roiId)
        ?? throw new InvalidOperationException($"ROI {roiId} не найден в текущей сессии.");
 
    private void Touch() => LastModifiedAt = DateTime.UtcNow;
}