using System.Windows;
using System.Windows.Controls;
using Diploma.Model;
using Diploma.ViewModel;
using ImageAnalysis.Application.Commands;
using ImageAnalysis.Domain.Entities.ProcessingOperations;

namespace Diploma.View;

public partial class FiltersControl : UserControl
{
    private FiltersViewModel? _viewModel;

    public FiltersControl()
    {
        InitializeComponent();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        if (_viewModel is null)
            return;

        OperationPayload? payload = _viewModel.SelectedFilter switch
        {
            OperationType.Threshold when ThresholdingTypeCombobox.SelectedValue is ThresholdingMode mode =>
                new OperationPayload.Thresholding((byte)ThresholdingFactorSlider.Value, mode),
            OperationType.Brightness => new OperationPayload.Brightness((int)BrightnessDeltaSlider.Value),
            OperationType.Contrast => new OperationPayload.Contrast((int)ContrastFactorSlider.Value),
            OperationType.GaussianBlur =>
                new OperationPayload.GaussianBlur((int)GaussianKernelSizeSlider.Value, GaussianSigmaSlider.Value),
            OperationType.Grayscale => new OperationPayload.Grayscale(),
            OperationType.Median => new OperationPayload.MedianFilter((int)MedianKernelSizeSlider.Value),
            _ => null
        };

        if (_viewModel.ApplyFilterAsyncCommand.CanExecute(payload))
            _viewModel.ApplyFilterAsyncCommand.Execute(payload);
    }

    private void FiltersControl_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not FiltersViewModel filtersViewModel)
            return;

        _viewModel = filtersViewModel;
    }
    
    
}