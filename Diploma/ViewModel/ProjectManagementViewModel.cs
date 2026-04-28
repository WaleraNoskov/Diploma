using System.IO;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using Diploma.Mvvm;
using ImageAnalysis.Application.Commands.LoadImage;
using ImageAnalysis.Domain.Events;
using ImageAnalysis.Infrastructure.Contracts;
using MediatR;

namespace Diploma.ViewModel;

public class ProjectManagementViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;
    private readonly IMediator _mediator;

    public ProjectManagementViewModel(IDialogService dialogService, IMediator mediator)
    {
        _dialogService = dialogService;
        _mediator = mediator;
        OpenFileCommand = new AsyncRelayCommand(OnOpenFileCommandExecuted, CanOpenFileCommandExecute);
    }


    #region ProjectName : string?

    private string? _projectName;

    /// <summary> 
    /// Get the project name 
    /// </summary>
    public string? ProjectName
    {
        get => _projectName;
        private set => SetField(ref _projectName, value);
    }

    #endregion ProjectName

    #region OpenFile

    public ICommand OpenFileCommand { get; set; }

    private async Task OnOpenFileCommandExecuted(object parameter)
    {
        const string filter =
            "Images|*.png;*.jpg;*.jpeg;*.bmp;*.tiff|PNG|*.png|JPEG|*.jpg;*.jpeg|BMP|*.bmp|TIFF|*.tiff";

        var path = _dialogService.OpenFile("Открыть изображение", filter);
        if (path is null) return;
        
        var bytes = await File.ReadAllBytesAsync(path);
        var format = Path.GetExtension(path).TrimStart('.').ToUpperInvariant();
        if (format == "JPG") format = "JPEG";

        var result = await _mediator.Send(new LoadImageCommand(bytes, format));
        if(result.IsFailure)
            _dialogService.ShowError("Не удалось открыть файл: " + result.Error.Code);
        
        Path.GetFileNameWithoutExtension(path);
        ProjectName = path;
    }

    private bool CanOpenFileCommandExecute(object parameter) => true;

    #endregion
}