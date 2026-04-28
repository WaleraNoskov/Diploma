using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using Diploma.Mvvm;
using ImageAnalysis.Application.Commands.TakeMeasurement;
using ImageAnalysis.Application.Dtos;
using ImageAnalysis.Application.Queries;
using ImageAnalysis.Application.Utils;
using ImageAnalysis.Domain.Entities;
using ImageAnalysis.Domain.ValueObjects;
using MediatR;

namespace Diploma.ViewModel;

public class ImageViewerViewModel : BaseViewModel
{
    private readonly IMediator _mediator;
    private PixelPoint? _firstPoint;
    private Guid? _currentSessionId;

    public ImageViewerViewModel(IMediator mediator, ImageSource imageSource, PixelPoint offset)
    {
        _mediator = mediator;
        ImageSource = imageSource;
        Offset = offset;
    }

    public ImageSource ImageSource { get; set; }
    public ObservableCollection<MeasurementDto> Measurements { get; } = new();
    public double Zoom { get; private set; } = 1.0;
    public PixelPoint Offset { get; private set; }

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
}