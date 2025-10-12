namespace Shared.DTOs;
public class DeviceCommandDto
{
    public string DeviceId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // "Start", "Stop", "SetSpeed", etc.
    public Dictionary<string, object> Parameters { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
