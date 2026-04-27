using Diploma.Mvvm;
using ImageAnalysis.Application.Dtos;

namespace Diploma.ViewModel;

public sealed class StatusBarViewModel : BaseViewModel
{
    private string _message       = "Откройте изображение для начала работы";
    private string _sessionInfo   = string.Empty;
    private bool   _hasOperations;
 
    public string Message
    {
        get => _message;
        private set => SetField(ref _message, value);
    }
 
    public string SessionInfo
    {
        get => _sessionInfo;
        private set => SetField(ref _sessionInfo, value);
    }
 
    public bool HasOperations
    {
        get => _hasOperations;
        private set => SetField(ref _hasOperations, value);
    }
 
    public void SetMessage(string message) => Message = message;
 
    public void Update(ImageSessionDto session)
    {
        SessionInfo = session.Dimensions is not null
            ? $"{session.Dimensions.Width}×{session.Dimensions.Height} | " +
              $"Контуры: {session.ContourCount} | " +
              $"Измерения: {session.MeasurementCount} | " +
              $"ROI: {session.RegionCount}"
            : string.Empty;
 
        HasOperations = session.CanUndo;
    }
}
