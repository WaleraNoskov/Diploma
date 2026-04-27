using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Diploma.Model;
using Diploma.Mvvm;
using ImageAnalysis.Application.Commands;
using ImageAnalysis.Application.Commands.LoadImage;
using ImageAnalysis.Application.Commands.SelectRoi;
using ImageAnalysis.Application.Commands.TakeMeasurement;
using ImageAnalysis.Application.Dtos;
using ImageAnalysis.Application.Queries;
using ImageAnalysis.Application.Services;
using ImageAnalysis.Domain.ValueObjects;
using MediatR;

namespace Diploma.ViewModel;

public sealed class MainViewModel : BaseBusyViewModel
{
    // -------------------------------------------------------------------------
    // Infrastructure
    // -------------------------------------------------------------------------

    private readonly IDialogService _dialogService;
    private readonly IMediator _mediator;

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    private Guid? _currentSessionId;
    private ImageSessionDto? _sessionSnapshot;
    private string? _currentFilePath;
    private string _windowTitle = "Image Inspector";

    // -------------------------------------------------------------------------
    // Child ViewModels (owned, disposed with this VM)
    // -------------------------------------------------------------------------

    public ImageCanvasViewModel Canvas { get; }
    public ParametersPanelViewModel Parameters { get; }
    public StatusBarViewModel StatusBar { get; }
    public OperationHistoryViewModel History { get; }

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public MainViewModel(
        IDialogService dialogService,
        IMediator mediator)
    {
        _dialogService = dialogService;
        _mediator = mediator;

        // Initialise child VMs
        Canvas = new ImageCanvasViewModel();
        Parameters = new ParametersPanelViewModel();
        StatusBar = new StatusBarViewModel();
        History = new OperationHistoryViewModel();

        // Wire Canvas events → this VM
        Canvas.RoiDrawn += OnRoiDrawn;
        Canvas.MeasurementPointsPlaced += OnMeasurementPointsPlaced;
        Canvas.RoiRemovalRequested += OnRoiRemovalRequested;
        Canvas.MeasurementRemovalRequested += OnMeasurementRemovalRequested;
        Canvas.ContourSelectionRequested += OnContourSelectionRequested;

        // Wire Parameters events → this VM
        Parameters.OperationRequested += OnOperationRequested;
        Parameters.ContourDetectionRequested += OnContourDetectionRequested;

        // Build toolbar commands
        InitialiseCommands();
    }

    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    public string WindowTitle
    {
        get => _windowTitle;
        private set => SetField(ref _windowTitle, value);
    }

    public bool HasSession => _currentSessionId.HasValue;

    /// <summary>Currently active interaction mode (bound bidirectionally from toolbar).</summary>
    public InteractionMode ActiveMode
    {
        get => Canvas.InteractionMode;
        set
        {
            Canvas.InteractionMode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsViewMode));
            OnPropertyChanged(nameof(IsRoiMode));
            OnPropertyChanged(nameof(IsMeasureMode));
        }
    }

    public bool IsViewMode => ActiveMode == InteractionMode.View;
    public bool IsRoiMode => ActiveMode == InteractionMode.RoiSelection;
    public bool IsMeasureMode => ActiveMode == InteractionMode.Measurement;

    // -------------------------------------------------------------------------
    // Commands (toolbar)
    // -------------------------------------------------------------------------

    // File
    public ICommand OpenImageCommand { get; private set; }
    public ICommand SaveResultCommand { get; private set; }
    public ICommand ResetSessionCommand { get; private set; }

    // Operation shortcuts (set Parameters.SelectedOperation then show panel)
    public ICommand SelectGrayscaleCommand { get; private set; }
    public ICommand SelectMedianFilterCommand { get; private set; }
    public ICommand SelectGaussianBlurCommand { get; private set; }
    public ICommand SelectBrightnessCommand { get; private set; }
    public ICommand SelectContrastCommand { get; private set; }
    public ICommand SelectThresholdingCommand { get; private set; }

    // Analysis
    public ICommand SelectContourDetectionCommand { get; private set; }
    public ICommand SetRoiModeCommand { get; private set; }
    public ICommand SetMeasureModeCommand { get; private set; }
    public ICommand SetViewModeCommand { get; private set; }

    // Navigation
    public ICommand ZoomInCommand { get; private set; }
    public ICommand ZoomOutCommand { get; private set; }
    public ICommand ResetZoomCommand { get; private set; }

    // History
    public ICommand UndoCommand { get; private set; }

    // -------------------------------------------------------------------------
    // Command initialisation
    // -------------------------------------------------------------------------

    private void InitialiseCommands()
    {
        OpenImageCommand = new AsyncRelayCommand(_ => OpenImageAsync());
        SaveResultCommand = new AsyncRelayCommand(_ => SaveResultAsync(), _ => HasSession);
        ResetSessionCommand = new AsyncRelayCommand(_ => ResetSessionAsync(), _ => HasSession);
        UndoCommand = new AsyncRelayCommand(_ => UndoAsync(),
            _ => HasSession && (_sessionSnapshot?.CanUndo ?? false));

        SelectGrayscaleCommand = new RelayCommand(_ => SelectOperation(SelectedOperation.Grayscale), _ => HasSession);
        SelectMedianFilterCommand =
            new RelayCommand(_ => SelectOperation(SelectedOperation.MedianFilter), _ => HasSession);
        SelectGaussianBlurCommand =
            new RelayCommand(_ => SelectOperation(SelectedOperation.GaussianBlur), _ => HasSession);
        SelectBrightnessCommand = new RelayCommand(_ => SelectOperation(SelectedOperation.Brightness), _ => HasSession);
        SelectContrastCommand = new RelayCommand(_ => SelectOperation(SelectedOperation.Contrast), _ => HasSession);
        SelectThresholdingCommand =
            new RelayCommand(_ => SelectOperation(SelectedOperation.Thresholding), _ => HasSession);

        SelectContourDetectionCommand = new RelayCommand(
            _ => SelectOperation(SelectedOperation.ContourDetection), _ => HasSession);

        SetRoiModeCommand = new RelayCommand(_ => ActiveMode = InteractionMode.RoiSelection, _ => HasSession);
        SetMeasureModeCommand = new RelayCommand(_ => ActiveMode = InteractionMode.Measurement, _ => HasSession);
        SetViewModeCommand = new RelayCommand(_ => ActiveMode = InteractionMode.View);

        // Delegate zoom to Canvas
        ZoomInCommand = Canvas.ZoomInCommand;
        ZoomOutCommand = Canvas.ZoomOutCommand;
        ResetZoomCommand = Canvas.ResetZoomCommand;
    }

    // -------------------------------------------------------------------------
    // File operations
    // -------------------------------------------------------------------------

    private async Task OpenImageAsync()
    {
        const string Filter =
            "Images|*.png;*.jpg;*.jpeg;*.bmp;*.tiff|PNG|*.png|JPEG|*.jpg;*.jpeg|BMP|*.bmp|TIFF|*.tiff";

        var path = _dialogService.OpenFile("Открыть изображение", Filter);
        if (path is null) return;

        await RunBusyAsync(async () =>
        {
            var bytes = await File.ReadAllBytesAsync(path);
            var format = Path.GetExtension(path).TrimStart('.').ToUpperInvariant();
            if (format == "JPG") format = "JPEG";

            var result = await _mediator.Send(new LoadImageCommand(bytes, format));
            if (result.IsFailure)
            {
                _dialogService.ShowError(result.Error.Description);
                return;
            }

            _currentSessionId = result.Value.SessionId;
            _currentFilePath = path;

            // Decode bytes to BitmapSource for display
            var bitmap = DecodeBitmap(bytes);
            Canvas.LoadImage(bitmap);

            UpdateWindowTitle(path);
            await RefreshSessionSnapshotAsync();
        }, "Загрузка изображения...");
    }

    private async Task SaveResultAsync()
    {
        if (!_currentSessionId.HasValue) return;

        const string Filter = "PNG|*.png|BMP|*.bmp";
        var path = _dialogService.SaveFile("Сохранить результат", Filter, "png");
        if (path is null) return;

        // TODO: fetch processed bytes from IImageStorage and write to disk
        // For now: inform user (Infrastructure.OpenCv needed for encoding)
        _dialogService.ShowMessage("Сохранение будет доступно после подключения инфраструктурного слоя.");
    }

    // -------------------------------------------------------------------------
    // Session operations
    // -------------------------------------------------------------------------

    private async Task ResetSessionAsync()
    {
        if (!_currentSessionId.HasValue) return;
        if (!_dialogService.Confirm("Сбросить все изменения и вернуться к исходному изображению?"))
            return;

        await RunBusyAsync(async () =>
        {
            var result = await _mediator.Send(new ResetSessionCommand(_currentSessionId.Value));
            if (result.IsFailure)
            {
                _dialogService.ShowError(result.Error.Description);
                return;
            }

            Canvas.ClearAllOverlays();
            await RefreshSessionSnapshotAsync();
        }, "Сброс сессии...");
    }

    private async Task UndoAsync()
    {
        if (!_currentSessionId.HasValue) return;

        await RunBusyAsync(async () =>
        {
            var result = await _mediator.Send(new UndoOperationCommand(_currentSessionId.Value));
            if (result.IsFailure)
            {
                _dialogService.ShowError(result.Error.Description);
                return;
            }

            Canvas.ContourOverlays.Clear(); // contours invalidated by undo
            await RefreshSessionSnapshotAsync();
        }, "Отмена операции...");
    }

    // -------------------------------------------------------------------------
    // Parameter panel events
    // -------------------------------------------------------------------------

    private void SelectOperation(SelectedOperation op)
    {
        Parameters.SelectedOperation = op;
    }

    private async void OnOperationRequested(OperationPayload payload)
    {
        if (!_currentSessionId.HasValue) return;

        await RunBusyAsync(async () =>
        {
            var result = await _mediator.Send(new ApplyOperationCommand(_currentSessionId.Value, payload));

            if (result.IsFailure)
            {
                _dialogService.ShowError(result.Error.Description);
                return;
            }

            // Refresh display — processed bytes need to be decoded from storage
            // (requires IImageStorage injection; shown as a hook point here)
            await RefreshSessionSnapshotAsync();
            StatusBar.SetMessage($"Операция применена: {payload.GetType().Name}");
        }, "Применение операции...");
    }

    private async void OnContourDetectionRequested(ContourFilterCriteria? filter)
    {
        if (!_currentSessionId.HasValue) return;

        await RunBusyAsync(async () =>
        {
            var result = await _mediator.Send(new DetectContoursCommand(_currentSessionId.Value, filter));
            if (result.IsFailure)
            {
                _dialogService.ShowError(result.Error.Description);
                return;
            }

            Canvas.ApplyContours(result.Value);
            StatusBar.SetMessage($"Найдено контуров: {result.Value.Count}");
            await RefreshSessionSnapshotAsync();
        }, "Поиск контуров...");
    }

    // -------------------------------------------------------------------------
    // Canvas events
    // -------------------------------------------------------------------------

    private async void OnRoiDrawn(RoiBoundsDto bounds)
    {
        if (!_currentSessionId.HasValue) return;

        var result = await _mediator.Send(new SelectRoiCommand(_currentSessionId.Value, bounds));
        if (result.IsFailure)
        {
            _dialogService.ShowError(result.Error.Description);
            return;
        }

        Canvas.ApplyRoi(result.Value);
        ActiveMode = InteractionMode.View; // return to view after drawing
        StatusBar.SetMessage($"ROI выделен: {bounds.Width}×{bounds.Height} px");
    }

    private async void OnMeasurementPointsPlaced(PixelPointDto from, PixelPointDto to)
    {
        if (!_currentSessionId.HasValue) return;

        var result = await _mediator.Send(new TakeMeasurementCommand(_currentSessionId.Value, from, to));
        if (result.IsFailure)
        {
            _dialogService.ShowError(result.Error.Description);
            return;
        }

        Canvas.ApplyMeasurement(result.Value);
        StatusBar.SetMessage($"Расстояние: {result.Value.DistancePixels:F1} px");
    }

    private async void OnRoiRemovalRequested(Guid roiId)
    {
        if (!_currentSessionId.HasValue) return;

        var result = await _mediator.Send(new RemoveRoiCommand(_currentSessionId.Value, roiId));
        if (result.IsFailure) _dialogService.ShowError(result.Error.Description);
    }

    private async void OnMeasurementRemovalRequested(Guid measurementId)
    {
        if (!_currentSessionId.HasValue) return;

        var result = await _mediator.Send(new RemoveMeasurementCommand(_currentSessionId.Value, measurementId));
        if (result.IsFailure) _dialogService.ShowError(result.Error.Description);
    }

    private async void OnContourSelectionRequested(Guid contourId)
    {
        if (!_currentSessionId.HasValue) return;

        var result = await _mediator.Send(new SelectContourCommand(_currentSessionId.Value, contourId));

        if (result.IsFailure) _dialogService.ShowError(result.Error.Description);
        else
            StatusBar.SetMessage(
                $"Контур: площадь={result.Value.Area:F0} px², периметр={result.Value.Perimeter:F1} px");
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private async Task RefreshSessionSnapshotAsync()
    {
        if (!_currentSessionId.HasValue) return;

        var result = await _mediator.Send(new GetSessionByIdQuery(_currentSessionId.Value));

        if (result.IsFailure) return;

        _sessionSnapshot = result.Value;
        History.Refresh(_sessionSnapshot.OperationHistory);
        StatusBar.Update(_sessionSnapshot);
        OnPropertyChanged(nameof(HasSession));
    }

    private static BitmapSource DecodeBitmap(byte[] bytes)
    {
        using var ms = new System.IO.MemoryStream(bytes);
        var decoder = BitmapDecoder.Create(
            ms,
            BitmapCreateOptions.PreservePixelFormat,
            BitmapCacheOption.OnLoad);
        return decoder.Frames[0];
    }

    private void UpdateWindowTitle(string filePath)
    {
        var name = Path.GetFileName(filePath);
        WindowTitle = $"Image Inspector — {name}";
    }

    // -------------------------------------------------------------------------
    // Dispose
    // -------------------------------------------------------------------------

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Unwire events
            Canvas.RoiDrawn -= OnRoiDrawn;
            Canvas.MeasurementPointsPlaced -= OnMeasurementPointsPlaced;
            Canvas.RoiRemovalRequested -= OnRoiRemovalRequested;
            Canvas.MeasurementRemovalRequested -= OnMeasurementRemovalRequested;
            Canvas.ContourSelectionRequested -= OnContourSelectionRequested;
            Parameters.OperationRequested -= OnOperationRequested;
            Parameters.ContourDetectionRequested -= OnContourDetectionRequested;

            // Dispose commands
            (OpenImageCommand as IDisposable)?.Dispose();
            (SaveResultCommand as IDisposable)?.Dispose();
            (ResetSessionCommand as IDisposable)?.Dispose();
            (UndoCommand as IDisposable)?.Dispose();
            (SelectGrayscaleCommand as IDisposable)?.Dispose();
            (SelectMedianFilterCommand as IDisposable)?.Dispose();
            (SelectGaussianBlurCommand as IDisposable)?.Dispose();
            (SelectBrightnessCommand as IDisposable)?.Dispose();
            (SelectContrastCommand as IDisposable)?.Dispose();
            (SelectThresholdingCommand as IDisposable)?.Dispose();
            (SelectContourDetectionCommand as IDisposable)?.Dispose();
            (SetRoiModeCommand as IDisposable)?.Dispose();
            (SetMeasureModeCommand as IDisposable)?.Dispose();
            (SetViewModeCommand as IDisposable)?.Dispose();

            // Dispose child VMs
            Canvas.Dispose();
            Parameters.Dispose();
            StatusBar.Dispose();
            History.Dispose();
        }

        base.Dispose(disposing);
    }
}