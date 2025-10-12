
namespace HMI.Models;

public class AlarmModel
{
    public string AlarmId { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool IsAcknowledged { get; set; }
}
