using HMI.Models;
using HMI.Services;
using HMI.Helpers;
using Shared.DTOs;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace HMI.ViewModels;


public class MainViewModel : ObservableObject
{
    private readonly IDeviceService _deviceService;
    private readonly RestApiService _restApiService;
    private string _statusMessage = "Initializing...";
    private bool _isConnected;

    public ObservableCollection<DeviceCardViewModel> Devices { get; }
    public ObservableCollection<AlarmModel> Alarms { get; }
    public ObservableCollection<HistoryEntry> History { get; }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsConnected
    {
        get => _isConnected;
        set => SetProperty(ref _isConnected, value);
    }

    public ICommand RefreshDevicesCommand { get; }
    public ICommand AcknowledgeAlarmCommand { get; }

    public MainViewModel(IDeviceService deviceService, RestApiService restApiService)
    {
        _deviceService = deviceService;
        _restApiService = restApiService;

        Devices = new ObservableCollection<DeviceCardViewModel>();
        Alarms = new ObservableCollection<AlarmModel>();
        History = new ObservableCollection<HistoryEntry>();

        RefreshDevicesCommand = new RelayCommand(_ => RefreshDevices());
        AcknowledgeAlarmCommand = new RelayCommand<AlarmModel>(AcknowledgeAlarm);

        // Subscribe to events
        _deviceService.StatusReceived += OnStatusReceived;
        _deviceService.AlarmReceived += OnAlarmReceived;

        // Initialize
        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        try
        {
            await _deviceService.InitializeAsync();
            await LoadDevices();

            IsConnected = true;
            StatusMessage = "Connected to Azure Service Bus";
        }
        catch (Exception ex)
        {
            IsConnected = false;
            StatusMessage = $"Connection failed: {ex.Message}";
            MessageBox.Show($"Failed to connect to services: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadDevices()
    {
        var devices = await _deviceService.GetDevicesAsync();

        Application.Current.Dispatcher.Invoke(() =>
        {
            Devices.Clear();
            foreach (var device in devices)
            {
                Devices.Add(new DeviceCardViewModel(device, _deviceService, _restApiService));
            }
        });
    }

    private void OnStatusReceived(object? sender, DeviceStatusDto status)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var deviceVm = Devices.FirstOrDefault(d => d.DeviceId == status.DeviceId);
            if (deviceVm != null)
            {
                deviceVm.UpdateStatus(status);
            }

            History.Insert(0, new HistoryEntry
            {
                Timestamp = status.Timestamp,
                DeviceId = status.DeviceId,
                Event = $"Status: {status.State}",
                Details = string.Join(", ", status.Properties.Select(p => $"{p.Key}={p.Value}"))
            });

            // Keep only last 100 entries
            while (History.Count > 100)
            {
                History.RemoveAt(History.Count - 1);
            }
        });
    }

    private void OnAlarmReceived(object? sender, AlarmDto alarm)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var device = Devices.FirstOrDefault(d => d.DeviceId == alarm.DeviceId);

            Alarms.Insert(0, new AlarmModel
            {
                AlarmId = alarm.AlarmId,
                DeviceId = alarm.DeviceId,
                DeviceName = device?.Name ?? alarm.DeviceId,
                Message = alarm.Message,
                Severity = alarm.Severity,
                Timestamp = alarm.Timestamp,
                IsAcknowledged = alarm.IsAcknowledged
            });

            History.Insert(0, new HistoryEntry
            {
                Timestamp = alarm.Timestamp,
                DeviceId = alarm.DeviceId,
                Event = $"ALARM: {alarm.Severity}",
                Details = alarm.Message
            });
        });
    }

    private async void RefreshDevices()
    {
        StatusMessage = "Refreshing devices...";
        await LoadDevices();
        StatusMessage = "Devices refreshed";
    }

    private async void AcknowledgeAlarm(AlarmModel? alarm)
    {
        if (alarm == null) return;

        alarm.IsAcknowledged = true;
        await _deviceService.AcknowledgeAlarmAsync(alarm.AlarmId);
    }
}
