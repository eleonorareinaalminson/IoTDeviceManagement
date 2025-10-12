using Microsoft.Extensions.Configuration;


namespace HMI.Services;

public class ConfigurationService
{
    public IConfiguration Configuration { get; }

    public ConfigurationService()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        Configuration = builder.Build();
    }

    public string GetServiceBusConnectionString()
        => Configuration["ServiceBus:ConnectionString"] ?? throw new InvalidOperationException("Service Bus connection string not configured");

    public string GetStatusTopic() => Configuration["ServiceBus:StatusTopic"] ?? "device-status";
    public string GetCommandTopic() => Configuration["ServiceBus:CommandTopic"] ?? "device-commands";
    public string GetAlarmQueue() => Configuration["ServiceBus:AlarmQueue"] ?? "device-alarms";
    public string GetRestApiBaseUrl() => Configuration["RestApi:BaseUrl"] ?? "http://localhost:5000";
}