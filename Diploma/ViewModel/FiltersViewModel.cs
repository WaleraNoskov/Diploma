using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using Diploma.Contracts.Events;
using Diploma.Model;
using Diploma.Mvvm;
using ImageAnalysis.Application.Commands;
using ImageAnalysis.Domain.Entities.ProcessingOperations;
using MediatR;

namespace Diploma.ViewModel;

public class FiltersViewModel : BaseViewModel
{
    private readonly IMediator _mediator;
    private readonly IDialogService _dialogService;
    private OperationPayload? _currentOperationPayload;
    private Guid _currentSessionId;

    public FiltersViewModel(IMediator mediator, IDialogService dialogService)
    {
        _mediator = mediator;
        _dialogService = dialogService;

        SelectFilterCommand = new RelayCommand(OnSelectFilterCommandExecuted, CanSelectFilterCommandExecute);
        ApplyFilterAsyncCommand =
            new AsyncRelayCommand(OnApplyFilterAsyncCommandExecuted, CanApplyFilterAsyncCommandExecute);

        WeakReferenceMessenger.Default.Register<NewSessionOpened>(this, OnNewSessionOpened);
    }

    /// <summary> 
    /// Gets or sets current filter. 
    /// </summary>
    public OperationType SelectedFilter => _currentOperationPayload switch
    {
        OperationPayload.Brightness => OperationType.Brightness,
        OperationPayload.Contrast => OperationType.Contrast,
        OperationPayload.GaussianBlur => OperationType.GaussianBlur,
        OperationPayload.Grayscale => OperationType.Grayscale,
        OperationPayload.MedianFilter => OperationType.Median,
        OperationPayload.Thresholding => OperationType.Threshold,
        _ => OperationType.None
    };

    #region IsBusy : bool

    private bool _isBusy;

    /// <summary> 
    /// Gets that view model is busy. 
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        set => SetField(ref _isBusy, value);
    }

    #endregion IsBusy
    
    #region SelectFilter

    public ICommand SelectFilterCommand { get; set; }

    private void OnSelectFilterCommandExecuted(object parameter)
    {
        if (parameter is not OperationPayload operationPayload || IsBusy)
            return;

        _currentOperationPayload = operationPayload;
        OnPropertyChanged(nameof(SelectedFilter));
    }

    private bool CanSelectFilterCommandExecute(object parameter) => !IsBusy;

    #endregion

    #region ApplyFilterAsyncCommand

    public IAsyncCommand ApplyFilterAsyncCommand { get; set; }

    private async Task OnApplyFilterAsyncCommandExecuted(object parameter)
    {
        if (_currentOperationPayload is null || IsBusy)
            return;

        IsBusy = true;
        
        var result = await _mediator.Send(new ApplyOperationCommand(_currentSessionId, _currentOperationPayload));
        if (result.IsFailure)
            _dialogService.ShowError(result.Error.Code);
        
        IsBusy = false;
    }

    private bool CanApplyFilterAsyncCommandExecute(object parameter) => _currentOperationPayload is not null && !IsBusy;

    #endregion

    private async void OnNewSessionOpened(object recipient, NewSessionOpened message)
    {
        try
        {
            _currentSessionId = message.Value;
            
            _currentOperationPayload = null;
            OnPropertyChanged(nameof(SelectedFilter));
        }
        catch (Exception e)
        {
            // ignored
        }
    }
}