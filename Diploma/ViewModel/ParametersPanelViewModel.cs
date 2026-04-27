using System.Windows.Input;
using Diploma.Model;
using Diploma.Mvvm;
using ImageAnalysis.Application.Commands;
using ImageAnalysis.Domain.Entities.ProcessingOperations;
using ImageAnalysis.Domain.ValueObjects;

namespace Diploma.ViewModel;

/// <summary>
/// Manages all parameter controls for processing operations.
/// Thin ViewModel — no async work, no direct service calls.
/// Raises events; MainViewModel subscribes and issues commands.
/// </summary>
public sealed class ParametersPanelViewModel : BaseViewModel
{
    // -------------------------------------------------------------------------
    // Backing fields
    // -------------------------------------------------------------------------

    private SelectedOperation _selectedOperation = SelectedOperation.None;

    // MedianFilter
    private int _medianKernelSize = 3;

    // GaussianBlur
    private int _gaussKernelSize = 5;
    private double _gaussSigma = 1.5;

    // Brightness
    private int _brightnessDelta = 0;

    // Contrast
    private double _contrastFactor = 1.0;

    // Thresholding
    private int _threshold = 128;
    private ThresholdingMode _threshMode = ThresholdingMode.Binary;

    // Contour detection
    private double? _minContourArea = null;
    private double? _maxContourArea = null;
    private double? _minPerimeter = null;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    public ParametersPanelViewModel()
    {
        ApplyCommand = new RelayCommand(
            _ => RaiseOperationRequested(),
            _ => SelectedOperation != SelectedOperation.None);
    }

    // -------------------------------------------------------------------------
    // Operation selector
    // -------------------------------------------------------------------------

    public SelectedOperation SelectedOperation
    {
        get => _selectedOperation;
        set
        {
            SetField(ref _selectedOperation, value);

            OnPropertyChanged(nameof(IsGrayscaleSelected));
            OnPropertyChanged(nameof(IsMedianFilterSelected));
            OnPropertyChanged(nameof(IsGaussianBlurSelected));
            OnPropertyChanged(nameof(IsBrightnessSelected));
            OnPropertyChanged(nameof(IsContrastSelected));
            OnPropertyChanged(nameof(IsThresholdingSelected));
            OnPropertyChanged(nameof(IsContourDetectionSelected));
            OnPropertyChanged(nameof(OperationTitle));
        }
    }

    // Visibility flags — bound to panel section Visibility via BoolToVisibility converter
    public bool IsGrayscaleSelected => SelectedOperation == SelectedOperation.Grayscale;
    public bool IsMedianFilterSelected => SelectedOperation == SelectedOperation.MedianFilter;
    public bool IsGaussianBlurSelected => SelectedOperation == SelectedOperation.GaussianBlur;
    public bool IsBrightnessSelected => SelectedOperation == SelectedOperation.Brightness;
    public bool IsContrastSelected => SelectedOperation == SelectedOperation.Contrast;
    public bool IsThresholdingSelected => SelectedOperation == SelectedOperation.Thresholding;
    public bool IsContourDetectionSelected => SelectedOperation == SelectedOperation.ContourDetection;
    public bool HasParameters => SelectedOperation != SelectedOperation.None;

    public string OperationTitle => SelectedOperation switch
    {
        SelectedOperation.None => "Выберите операцию",
        SelectedOperation.Grayscale => "Оттенки серого",
        SelectedOperation.MedianFilter => "Медианный фильтр",
        SelectedOperation.GaussianBlur => "Гауссово сглаживание",
        SelectedOperation.Brightness => "Яркость",
        SelectedOperation.Contrast => "Контрастность",
        SelectedOperation.Thresholding => "Пороговая обработка",
        SelectedOperation.ContourDetection => "Поиск контуров",
        _ => string.Empty
    };

    // -------------------------------------------------------------------------
    // MedianFilter parameters
    // -------------------------------------------------------------------------

    /// <summary>Kernel size for median filter: 3, 5, 7, 9, 11.</summary>
    public int MedianKernelSize
    {
        get => _medianKernelSize;
        set => SetField(ref _medianKernelSize, value);
    }

    public IReadOnlyList<int> MedianKernelSizes { get; } = [3, 5, 7, 9, 11];

    // -------------------------------------------------------------------------
    // GaussianBlur parameters
    // -------------------------------------------------------------------------

    public int GaussKernelSize
    {
        get => _gaussKernelSize;
        set => SetField(ref _gaussKernelSize, value);
    }

    public double GaussSigma
    {
        get => _gaussSigma;
        set => SetField(ref _gaussSigma, Math.Clamp(value, 0.1, 20.0));
    }

    public IReadOnlyList<int> GaussKernelSizes { get; } = [3, 5, 7, 9, 11, 15, 21];

    // -------------------------------------------------------------------------
    // Brightness parameters
    // -------------------------------------------------------------------------

    /// <summary>Range: -255..+255.</summary>
    public int BrightnessDelta
    {
        get => _brightnessDelta;
        set => SetField(ref _brightnessDelta, Math.Clamp(value, -255, 255));
    }

    public string BrightnessDeltaText => BrightnessDelta >= 0
        ? $"+{BrightnessDelta}"
        : BrightnessDelta.ToString();

    // -------------------------------------------------------------------------
    // Contrast parameters
    // -------------------------------------------------------------------------

    /// <summary>Range: 0.1..5.0.</summary>
    public double ContrastFactor
    {
        get => _contrastFactor;
        set
        {
            SetField(ref _contrastFactor, Math.Clamp(value, 0.1, 5.0));
            OnPropertyChanged(nameof(ContrastFactorText));
        }
    }

    public string ContrastFactorText => $"x{ContrastFactor:F2}";

    // -------------------------------------------------------------------------
    // Thresholding parameters
    // -------------------------------------------------------------------------

    /// <summary>Range: 0..255.</summary>
    public int Threshold
    {
        get => _threshold;
        set => SetField(ref _threshold, Math.Clamp(value, 0, 255));
    }

    public ThresholdingMode ThreshMode
    {
        get => _threshMode;
        set => SetField(ref _threshMode, value);
    }

    public IReadOnlyList<ThresholdingMode> ThresholdingModes { get; } =
        Enum.GetValues<ThresholdingMode>().ToList();

    // -------------------------------------------------------------------------
    // Contour detection parameters (filter criteria)
    // -------------------------------------------------------------------------

    public double? MinContourArea
    {
        get => _minContourArea;
        set => SetField(ref _minContourArea, value);
    }

    public double? MaxContourArea
    {
        get => _maxContourArea;
        set => SetField(ref _maxContourArea, value);
    }

    public double? MinPerimeter
    {
        get => _minPerimeter;
        set => SetField(ref _minPerimeter, value);
    }

    public bool UseAreaFilter
    {
        get => MinContourArea.HasValue || MaxContourArea.HasValue;
        set
        {
            if (!value)
            {
                MinContourArea = null;
                MaxContourArea = null;
            }
        }
    }

    // -------------------------------------------------------------------------
    // Commands
    // -------------------------------------------------------------------------

    public ICommand ApplyCommand { get; }

    // -------------------------------------------------------------------------
    // Events
    // -------------------------------------------------------------------------

    /// <summary>
    /// Raised when the user clicks Apply.
    /// Payload is the typed operation payload ready to send to MediatR.
    /// </summary>
    public event Action<OperationPayload>? OperationRequested;

    /// <summary>
    /// Raised when the user confirms contour detection with optional filter.
    /// </summary>
    public event Action<ContourFilterCriteria?>? ContourDetectionRequested;

    // -------------------------------------------------------------------------
    // Private
    // -------------------------------------------------------------------------

    private void RaiseOperationRequested()
    {
        if (SelectedOperation == SelectedOperation.ContourDetection)
        {
            var filter = BuildFilterCriteria();
            ContourDetectionRequested?.Invoke(filter);
            return;
        }

        var payload = BuildOperationPayload();
        if (payload is not null)
            OperationRequested?.Invoke(payload);
    }

    private OperationPayload? BuildOperationPayload() => SelectedOperation switch
    {
        SelectedOperation.Grayscale => new OperationPayload.Grayscale(),
        SelectedOperation.MedianFilter => new OperationPayload.MedianFilter(MedianKernelSize),
        SelectedOperation.GaussianBlur => new OperationPayload.GaussianBlur(GaussKernelSize, GaussSigma),
        SelectedOperation.Brightness => new OperationPayload.Brightness(BrightnessDelta),
        SelectedOperation.Contrast => new OperationPayload.Contrast(ContrastFactor),
        SelectedOperation.Thresholding => new OperationPayload.Thresholding((byte)Threshold, ThreshMode),
        _ => null
    };

    private ContourFilterCriteria? BuildFilterCriteria()
    {
        if (!MinContourArea.HasValue && !MaxContourArea.HasValue && !MinPerimeter.HasValue)
            return null;

        return new ContourFilterCriteria
        {
            MinArea = MinContourArea,
            MaxArea = MaxContourArea,
            MinPerimeter = MinPerimeter
        };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            (ApplyCommand as IDisposable)?.Dispose();

        base.Dispose(disposing);
    }
}