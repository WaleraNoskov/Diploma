using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Contracts;
using Diploma.Contracts.Events;
using Diploma.Mvvm;
using ImageAnalysis.Application.Commands.TakeMeasurement;
using ImageAnalysis.Application.Dtos;
using ImageAnalysis.Application.Queries;
using ImageAnalysis.Application.Utils;
using ImageAnalysis.Domain.Entities;
using ImageAnalysis.Domain.Events;
using ImageAnalysis.Domain.ValueObjects;
using MediatR;

namespace Diploma.ViewModel;

public class ImageViewerViewModel : BaseViewModel
{
    private readonly IMediator _mediator;
    private readonly IDialogService _dialogService;
    private PixelPoint? _firstPoint;
    private Guid? _currentSessionId;

    public ImageViewerViewModel(IMediator mediator, IDialogService dialogService)
    {
        _mediator = mediator;
        _dialogService = dialogService;

        WeakReferenceMessenger.Default.Register<NewSessionOpened>(this, OnNewSessionOpened);
        WeakReferenceMessenger.Default.Register<NewSessionNotification>(this, OnNewSessionNotification);
    }

    #region ImageSource : ImageSource

    private ImageSource _imageSource;

    /// <summary> 
    /// Gets current image source .
    /// </summary>
    public ImageSource ImageSource
    {
        get => _imageSource;
        private set => SetField(ref _imageSource, value);
    }

    #endregion ImageSource

    public ObservableCollection<MeasurementDto> Measurements { get; } = new();

    #region Zoom : double

    private double _zoom = 1;

    /// <summary> 
    /// Gets current zoom. 
    /// </summary>
    public double Zoom
    {
        get => _zoom;
        set => SetField(ref _zoom, value);
    }

    #endregion Zoom

    #region Offset : PixelPoint

    private PixelPoint _pixelPoint = new(0, 0);

    /// <summary> 
    /// Gets the pixel point. 
    /// </summary>
    public PixelPoint Offset
    {
        get => _pixelPoint;
        set => SetField(ref _pixelPoint, value);
    }

    #endregion Offset

    public TransformGroup Transform => new TransformGroup
    {
        Children = new TransformCollection
        {
            new ScaleTransform(Zoom, Zoom),
            new TranslateTransform(Offset.X, Offset.Y)
        }
    };

    public ICommand ClickCommand => new RelayCommand<PixelPoint>(async p =>
    {
        if (!_currentSessionId.HasValue)
            return;

        var imgPoint = ScreenToImage(p);

        if (_firstPoint == null)
        {
            _firstPoint = imgPoint;
            return;
        }

        await _mediator.Send(new TakeMeasurementCommand(_currentSessionId.Value,
            _firstPoint.ToDto(),
            imgPoint.ToDto()));

        _firstPoint = null;

        await Reload();
    });

    public ICommand MouseWheelCommand => new RelayCommand<int>(delta =>
    {
        Zoom *= delta > 0 ? 1.1 : 0.9;
        OnPropertyChanged(nameof(Transform));
    });

    public ICommand MouseMoveCommand => new RelayCommand<PixelPoint>(_ => { });

    private PixelPoint ScreenToImage(PixelPoint p)
    {
        return new PixelPoint(
            (int)((p.X - Offset.X) / Zoom),
            (int)((p.Y - Offset.Y) / Zoom));
    }

    private async Task ResetImage()
    {
        if (!_currentSessionId.HasValue) return;

        var getSessionResult = await _mediator.Send(new GetSessionByIdQuery(_currentSessionId.Value));
        if (getSessionResult.IsFailure ||
            getSessionResult.Value.Dimensions is null ||
            !getSessionResult.Value.CurrentImageId.HasValue)
            return;

        var getBytesResult = await _mediator.Send(new GetImageBytesQuery(getSessionResult.Value.CurrentImageId.Value));
        if (getBytesResult.IsFailure) return;

        var channels = getSessionResult.Value.Channels;
        var channelSize = getSessionResult.Value.ChannelSize;
        var pixelFormat = (channels * channelSize) switch
        {
            1 => PixelFormats.Gray8,
            2 => PixelFormats.Gray16,
            3 => PixelFormats.Bgr24,
            4 => PixelFormats.Bgra32,
            _ => throw new NotSupportedException($"Channels: {channels}")
        };

        var width = getSessionResult.Value.Dimensions.Width;
        var height = getSessionResult.Value.Dimensions.Height;

        try
        {
            var bitmap = BitmapSource.Create(
                width,
                height,
                96,
                96,
                pixelFormat,
                null,
                getBytesResult.Value.Bytes,
                getSessionResult.Value.Stride);

            ImageSource = bitmap;
        }
        catch (Exception e)
        {
            _dialogService.ShowError(e.Message);
        }
    }

    private async Task Reload()
    {
        if (!_currentSessionId.HasValue)
            return;

        var result = await _mediator.Send(new GetMeasurementsQuery(_currentSessionId.Value));

        if (result.IsFailure)
            return;

        Measurements.Clear();
        foreach (var m in result.Value)
            Measurements.Add(m);

        OnPropertyChanged(nameof(Measurements));
    }

    private async void OnNewSessionOpened(object recipient, NewSessionOpened message)
    {
        try
        {
            _currentSessionId = message.Value;
            await Reload();
            await ResetImage();
        }
        catch (Exception e)
        {
            // ignored
        }
    }
    
    private async void OnNewSessionNotification(object recipient, NewSessionNotification message)
    {
        try
        {
            if(message.Value.Event is MeasurementTakenEvent or MeasurementRemovedEvent)
            {
                await Reload();
                await ResetImage();
            }
            
            else if (message.Value.Event is OperationAppliedEvent or OperationUndoneEvent or SessionResetEvent)
                await ResetImage();
        }
        catch (Exception e)
        {
            _dialogService.ShowError(e.Message);
        }
    }
}