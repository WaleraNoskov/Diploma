using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Messaging;
using Diploma.Contracts.Events;
using Diploma.Mvvm;
using ImageAnalysis.Application.Commands;
using ImageAnalysis.Application.Dtos;
using ImageAnalysis.Application.Queries;
using MediatR;

namespace Diploma.ViewModel;

public class OperationHistoryViewModel : BaseViewModel
{
    private readonly IMediator _mediator;
    private readonly IDialogService _dialogService;
    
    private Guid? _currentSessionId;
    
    public OperationHistoryViewModel(IMediator mediator, IDialogService dialogService)
    {
        _mediator = mediator;
        _dialogService = dialogService;
        
        UndoAsyncCommand = new AsyncRelayCommand(OnUndoAsyncCommandExecuted, CanUndoAsyncCommandExecute);

        WeakReferenceMessenger.Default.Register<NewSessionOpened>(this, OnNewSessionOpened);
        WeakReferenceMessenger.Default.Register<NewSessionNotification>(this, OnNewSessionNotification);
    }

    public ObservableCollection<OperationHistoryItemDto> History { get; } = [];

    #region UndoAsyncCommand

    public IAsyncCommand UndoAsyncCommand { get; set; }

    private async Task OnUndoAsyncCommandExecuted(object parameter)
    {
        if (!_currentSessionId.HasValue || History.Count == 0)
            return;
        
        var result = await _mediator.Send(new UndoOperationCommand(_currentSessionId.Value));
        if(result.IsFailure)
            _dialogService.ShowError(result.Error.Code);
    }

    private bool CanUndoAsyncCommandExecute(object parameter) => _currentSessionId.HasValue && History.Count > 0;

    #endregion
    
    private void OnNewSessionOpened(object recipient, NewSessionOpened message)
    {
        _currentSessionId = message.Value;
        History.Clear();
    }

    private async void OnNewSessionNotification(object recipient, NewSessionNotification message)
    {
        if (_currentSessionId == null)
            return;
        
        History.Clear();

        try
        {
            var result = await _mediator.Send(new GetOperationHistoryQuery(_currentSessionId.Value));
            
            if(result.IsFailure)
                _dialogService.ShowError(result.Error.Code);

            foreach (var operationHistoryItemDto in result.Value)
                History.Add(operationHistoryItemDto);
        }
        catch (Exception ex)
        {
            _dialogService.ShowError(ex.Message);
        }
    }
}