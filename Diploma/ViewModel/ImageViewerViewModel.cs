using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
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
using AsyncRelayCommand = Diploma.Mvvm.AsyncRelayCommand;
using RelayCommand = Diploma.Mvvm.RelayCommand;

namespace Diploma.ViewModel;

public class ImageViewerViewModel : BaseViewModel
{
    private readonly IMediator _mediator;
    private readonly IDialogService _dialogService;
    private PixelPoint? _firstPoint;
    private Guid? _currentSessionId;
    private Matrix _matrix = Matrix.Identity;

    public ImageViewerViewModel(IMediator mediator, IDialogService dialogService)
    {
        _mediator = mediator;
        _dialogService = dialogService;

        WeakReferenceMessenger.Default.Register<NewSessionOpened>(this, OnNewSessionOpened);
        WeakReferenceMessenger.Default.Register<NewSessionNotification>(this, OnNewSessionNotification);
    }

    /// <summary> 
    /// Gets current image source .
    /// </summary>
    public ImageSource? ImageSource
    {
        get;
        private set => SetField(ref field, value);
    }

    public ObservableCollection<MeasurementDto> Measurements { get; } = new();

    /// <summary> 
    /// Gets current zoom. 
    /// </summary>
    public double Zoom
    {
        get;
        set => SetField(ref field, value);
    } = 1;

    /// <summary> 
    /// Gets the pixel point. 
    /// </summary>
    public PixelPoint Offset
    {
        get;
        set => SetField(ref field, value);
    } = new(0, 0);

    public double Scale
    {
        get;
        set => SetField(ref field, value);
    } = 1.0;

    public MatrixTransform PanTransform { get; } = new();

    public MatrixTransform Transform { get; } = new();

    public void ZoomAt(Point cursor, double scale)
    {
        var m = _matrix;

        m.Translate(-cursor.X, -cursor.Y);
        m.Scale(scale, scale);
        m.Translate(cursor.X, cursor.Y);

        _matrix = m;
        Transform.Matrix = _matrix;
    }

    public Point ScreenToImage(Point p)
    {
        var inv = _matrix;
        inv.Invert();
        return inv.Transform(p);
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
            if (message.Value.Event is not
                (OperationAppliedEvent
                or OperationUndoneEvent
                or SessionResetEvent
                or MeasurementTakenEvent
                or MeasurementRemovedEvent))
                return;

            await Reload();
            await ResetImage();
        }
        catch (Exception e)
        {
            _dialogService.ShowError(e.Message);
        }
    }
}