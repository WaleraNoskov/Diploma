using System.Windows;
using Diploma.Mvvm;
using Diploma.Services;
using Diploma.ViewModel;
using ImageAnalysis.Application.Commands.LoadImage;
using ImageAnalysis.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Diploma;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private IHost? _host;

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddImageProcessingApplication(typeof(DependencyInjection).Assembly, 
            typeof(LoadImageCommand).Assembly,
            typeof(DomainEventHandler).Assembly);

        services.AddTransient<ImageViewerViewModel>();
        services.AddTransient<ProjectManagementViewModel>();
        services.AddTransient<FiltersViewModel>();
        
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainWindow>();
        
        services.AddSingleton<IDialogService, DialogService>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        var builder = Host.CreateApplicationBuilder();
        ConfigureServices(builder.Services);
        _host = builder.Build();

        await _host.StartAsync();

        var mainWindowViewModel = _host.Services.GetRequiredService<MainWindowViewModel>();
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.DataContext = mainWindowViewModel;
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            using var host = _host;
            await _host.StopAsync();
        }

        base.OnExit(e);
    }
}