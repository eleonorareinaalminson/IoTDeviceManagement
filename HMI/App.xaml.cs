using HMI.Services;
using HMI.ViewModels;
using HMI.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;


namespace HMI;

public partial class App : Application
{
    public IServiceProvider ServiceProvider { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        ServiceProvider = serviceCollection.BuildServiceProvider();

        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Services
        services.AddSingleton<ConfigurationService>();
        services.AddSingleton<IDeviceService, ServiceBusService>();
        services.AddSingleton<RestApiService>();

        // ViewModels
        services.AddTransient<MainViewModel>();

        // Windows
        services.AddTransient<MainWindow>();
    }
}