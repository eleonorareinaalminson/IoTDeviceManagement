using HMI.Models;
using Shared.DTOs;

namespace HMI.Services;

public interface IDeviceService
{
    event EventHandler<DeviceStatusDto>? StatusReceived;
    event EventHandler<AlarmDto>? AlarmReceived;

    Task InitializeAsync();
    Task SendCommandAsync(string deviceId, DeviceCommandDto command);
    Task<IEnumerable<DeviceModel>> GetDevicesAsync();
    Task AcknowledgeAlarmAsync(string alarmId);
}