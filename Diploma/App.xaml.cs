using System.Windows;
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
        services.AddImageProcessingApplication(typeof(DependencyInjection).Assembly, typeof(LoadImageCommand).Assembly);

        
        services.AddSingleton<MainWindow>();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        var builder = Host.CreateApplicationBuilder();
        ConfigureServices(builder.Services);
        _host = builder.Build();

        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
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