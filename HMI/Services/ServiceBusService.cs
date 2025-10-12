using Azure.Messaging.ServiceBus;
using HMI.Models;
using Shared.DTOs;
using System.Text.Json;

namespace HMI.Services;
public class ServiceBusService : IDeviceService, IAsyncDisposable
{
    private readonly ConfigurationService _config;
    private ServiceBusClient? _client;
    private ServiceBusSender? _commandSender;
    private ServiceBusProcessor? _statusProcessor;
    private ServiceBusProcessor? _alarmProcessor;

    public event EventHandler<DeviceStatusDto>? StatusReceived;
    public event EventHandler<AlarmDto>? AlarmReceived;

    public ServiceBusService(ConfigurationService config)
    {
        _config = config;
    }

    public async Task InitializeAsync()
    {
        try
        {
            var connectionString = _config.GetServiceBusConnectionString();
            _client = new ServiceBusClient(connectionString);

            // Sender för commands (Queue)
            _commandSender = _client.CreateSender(_config.Configuration["ServiceBus:CommandQueue"] ?? "device-commands");

            // Processor för status updates (Queue)
            _statusProcessor = _client.CreateProcessor(
                _config.Configuration["ServiceBus:StatusQueue"] ?? "device-status",
                new ServiceBusProcessorOptions
                {
                    AutoCompleteMessages = false,
                    MaxConcurrentCalls = 1
                });

            _statusProcessor.ProcessMessageAsync += OnStatusMessageAsync;
            _statusProcessor.ProcessErrorAsync += OnErrorAsync;

            // Processor för alarms (Queue)
            _alarmProcessor = _client.CreateProcessor(
                _config.Configuration["ServiceBus:AlarmQueue"] ?? "device-alarms",
                new ServiceBusProcessorOptions
                {
                    AutoCompleteMessages = false,
                    MaxConcurrentCalls = 1
                });

            _alarmProcessor.ProcessMessageAsync += OnAlarmMessageAsync;
            _alarmProcessor.ProcessErrorAsync += OnErrorAsync;

            // Start processing
            await _statusProcessor.StartProcessingAsync();
            await _alarmProcessor.StartProcessingAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ServiceBus initialization failed: {ex.Message}");
            throw;
        }
    }

    private async Task OnStatusMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            var body = args.Message.Body.ToString();
            var status = JsonSerializer.Deserialize<DeviceStatusDto>(body);

            if (status != null)
            {
                StatusReceived?.Invoke(this, status);
            }

            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error processing status message: {ex.Message}");
            await args.AbandonMessageAsync(args.Message);
        }
    }

    private async Task OnAlarmMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            var body = args.Message.Body.ToString();
            var alarm = JsonSerializer.Deserialize<AlarmDto>(body);

            if (alarm != null)
            {
                AlarmReceived?.Invoke(this, alarm);
            }

            await args.CompleteMessageAsync(args.Message);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error processing alarm message: {ex.Message}");
            await args.AbandonMessageAsync(args.Message);
        }
    }

    private Task OnErrorAsync(ProcessErrorEventArgs args)
    {
        System.Diagnostics.Debug.WriteLine($"ServiceBus error: {args.Exception.Message}");
        return Task.CompletedTask;
    }

    public async Task SendCommandAsync(string deviceId, DeviceCommandDto command)
    {
        if (_commandSender == null)
            throw new InvalidOperationException("Service not initialized");

        command.DeviceId = deviceId;
        command.Timestamp = DateTime.UtcNow;

        var messageBody = JsonSerializer.Serialize(command);
        var message = new ServiceBusMessage(messageBody)
        {
            Subject = deviceId, // Används för filtrering på device-sidan
            ContentType = "application/json"
        };

        await _commandSender.SendMessageAsync(message);
    }

    public async Task<IEnumerable<DeviceModel>> GetDevicesAsync()
    {
        var devices = new List<DeviceModel>();
        var deviceConfigs = _config.Configuration.GetSection("Devices").GetChildren();

        foreach (var deviceConfig in deviceConfigs)
        {
            devices.Add(new DeviceModel
            {
                DeviceId = deviceConfig["DeviceId"] ?? string.Empty,
                Name = deviceConfig["Name"] ?? string.Empty,
                Type = Enum.Parse<Shared.Enums.DeviceType>(deviceConfig["Type"] ?? "Unknown"),
                Endpoint = deviceConfig["Endpoint"] ?? string.Empty,
                State = Shared.Enums.DeviceState.Offline,
                LastSeen = DateTime.MinValue
            });
        }

        return await Task.FromResult(devices);
    }

    public Task AcknowledgeAlarmAsync(string alarmId)
    {
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_statusProcessor != null)
        {
            await _statusProcessor.StopProcessingAsync();
            await _statusProcessor.DisposeAsync();
        }

        if (_alarmProcessor != null)
        {
            await _alarmProcessor.StopProcessingAsync();
            await _alarmProcessor.DisposeAsync();
        }

        if (_commandSender != null)
        {
            await _commandSender.DisposeAsync();
        }

        if (_client != null)
        {
            await _client.DisposeAsync();
        }
    }
}