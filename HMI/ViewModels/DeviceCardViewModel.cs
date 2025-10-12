using HMI.Models;
using HMI.Services;
using HMI.Helpers;
using Shared.DTOs;
using Shared.Enums;
using System.Windows.Input;


namespace HMI.ViewModels;

public class DeviceCardViewModel : ObservableObject
{
    private readonly IDeviceService _deviceService;
    private readonly RestApiService _restApiService;
    private readonly DeviceModel _device;

    private DeviceState _state;
    private DateTime _lastSeen;
    private double _currentValue;
    private string _statusText = string.Empty;

    public string DeviceId => _device.DeviceId;
    public string Name => _device.Name;
    public DeviceType Type => _device.Type;

    public DeviceState State
    {
        get => _state;
        set
        {
            SetProperty(ref _state, value);
            OnPropertyChanged(nameof(IsOnline));
            OnPropertyChanged(nameof(IsRunning));
        }
    }

    public DateTime LastSeen
    {
        get => _lastSeen;
        set => SetProperty(ref _lastSeen, value);
    }

    public double CurrentValue
    {
        get => _currentValue;
        set => SetProperty(ref _currentValue, value);
    }

    public string StatusText
    {
        get => _statusText;
        set => SetProperty(ref _statusText, value);
    }

    public bool IsOnline => State != DeviceState.Offline;
    public bool IsRunning => State == DeviceState.Running;

    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand SetValueCommand { get; }

    public DeviceCardViewModel(DeviceModel device, IDeviceService deviceService, RestApiService restApiService)
    {
        _device = device;
        _deviceService = deviceService;
        _restApiService = restApiService;

        _state = device.State;
        _lastSeen = device.LastSeen;

        StartCommand = new RelayCommand(_ => StartDevice(), _ => IsOnline && !IsRunning);
        StopCommand = new RelayCommand(_ => StopDevice(), _ => IsRunning);
        SetValueCommand = new RelayCommand<double?>(SetValue, _ => IsRunning);

        UpdateStatusText();
    }

    public void UpdateStatus(DeviceStatusDto status)
    {
        State = status.State;
        LastSeen = status.Timestamp;

        double GetDoubleValue(object obj)
        {
            if (obj is System.Text.Json.JsonElement jsonElement)
            {
                return jsonElement.GetDouble();
            }
            return Convert.ToDouble(obj);
        }

        if (status.Properties.TryGetValue("Speed", out var speed))
        {
            CurrentValue = GetDoubleValue(speed);
        }
        else if (status.Properties.TryGetValue("Temperature", out var temp))
        {
            CurrentValue = GetDoubleValue(temp);
        }
        else if (status.Properties.TryGetValue("Brightness", out var brightness))
        {
            CurrentValue = GetDoubleValue(brightness);
        }

        UpdateStatusText();
    }

    private async void StartDevice()
    {
        var command = new DeviceCommandDto
        {
            Action = "Start",
            Parameters = new Dictionary<string, object>()
        };

        await _deviceService.SendCommandAsync(DeviceId, command);
    }

    private async void StopDevice()
    {
        var command = new DeviceCommandDto
        {
            Action = "Stop",
            Parameters = new Dictionary<string, object>()
        };

        await _deviceService.SendCommandAsync(DeviceId, command);
    }

    private async void SetValue(double? value)
    {
        if (!value.HasValue) return;

        System.Diagnostics.Debug.WriteLine($"Setting value to: {value.Value}");

        var command = new DeviceCommandDto
        {
            DeviceId = DeviceId, 
            Action = Type switch
            {
                DeviceType.Fan => "SetSpeed",
                DeviceType.Lamp => "SetBrightness",
                _ => "SetValue"
            },
            Parameters = new Dictionary<string, object>
        {
            { "Value", value.Value }
        }
        };

        try
        {
            await _deviceService.SendCommandAsync(DeviceId, command);
            System.Diagnostics.Debug.WriteLine($"Command sent successfully: {command.Action}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to send command: {ex.Message}");
        }
    }

    private void UpdateStatusText()
    {
        StatusText = Type switch
        {
            DeviceType.Fan => $"Speed: {CurrentValue:F1}x",
            DeviceType.TemperatureSensor => $"Temp: {CurrentValue:F1}°C",
            DeviceType.Lamp => $"Brightness: {CurrentValue:F0}%",
            _ => State.ToString()
        };
    }
}
