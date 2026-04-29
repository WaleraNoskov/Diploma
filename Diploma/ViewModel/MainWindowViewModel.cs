using Diploma.Mvvm;

namespace Diploma.ViewModel;

public class MainWindowViewModel(
    ImageViewerViewModel imageViewerViewModel,
    ProjectManagementViewModel projectManagementViewModel,
    FiltersViewModel filtersViewModel) 
    : BaseViewModel
{
    public ImageViewerViewModel ImageViewerViewModel { get; } = imageViewerViewModel;
    public ProjectManagementViewModel ProjectManagementViewModel { get; } = projectManagementViewModel;
    public FiltersViewModel FiltersViewModel { get; } = filtersViewModel;
}