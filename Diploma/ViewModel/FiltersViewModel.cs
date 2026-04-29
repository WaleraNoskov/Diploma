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
    private Guid? _currentSessionId;

    public FiltersViewModel(IMediator mediator, IDialogService dialogService)
    {
        _mediator = mediator;
        _dialogService = dialogService;

        ApplyFilterAsyncCommand =
            new AsyncRelayCommand(OnApplyFilterAsyncCommandExecuted, CanApplyFilterAsyncCommandExecute);

        WeakReferenceMessenger.Default.Register<NewSessionOpened>(this, OnNewSessionOpened);
    }

    /// <summary> 
    /// Gets or sets selected filter.
    /// </summary>
    public OperationType SelectedFilter
    {
        get;
        set => SetField(ref field, value);
    }

    /// <summary> 
    /// Gets that view model is busy. 
    /// </summary>
    public bool IsBusy
    {
        get;
        set => SetField(ref field, value);
    }

    #region ApplyFilterAsyncCommand

    public IAsyncCommand ApplyFilterAsyncCommand { get; set; }

    private async Task OnApplyFilterAsyncCommandExecuted(object parameter)
    {
        if (IsBusy || !_currentSessionId.HasValue || parameter is not OperationPayload operationPayload)
            return;

        IsBusy = true;

        var result = await _mediator.Send(new ApplyOperationCommand(_currentSessionId.Value, operationPayload));
        if (result.IsFailure)
            _dialogService.ShowError(result.Error.Code);

        IsBusy = false;
    }

    private bool CanApplyFilterAsyncCommandExecute(object parameter) => SelectedFilter != OperationType.None && 
                                                                        !IsBusy &&
                                                                        _currentSessionId.HasValue;

    #endregion

    private async void OnNewSessionOpened(object recipient, NewSessionOpened message)
    {
        try
        {
            _currentSessionId = message.Value;
            SelectedFilter = OperationType.None;
        }
        catch (Exception e)
        {
            // ignored
        }
    }
}