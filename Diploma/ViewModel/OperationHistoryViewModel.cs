using System.Collections.ObjectModel;
using Diploma.Mvvm;
using ImageAnalysis.Application.Dtos;

namespace Diploma.ViewModel;

public sealed class OperationHistoryViewModel : BaseViewModel
{
    public ObservableCollection<OperationHistoryItemDto> Items { get; } = [];
 
    public void Refresh(IReadOnlyList<OperationHistoryItemDto> history)
    {
        Items.Clear();
        // Show newest first
        foreach (var item in history.Reverse())
            Items.Add(item);
    }
}
