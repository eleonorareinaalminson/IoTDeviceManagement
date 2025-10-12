namespace Shared.DTOs;
public class AlarmDto
{
    public string AlarmId { get; set; } = Guid.NewGuid().ToString();
    public string DeviceId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "Warning"; // Info, Warning, Critical
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsAcknowledged { get; set; }
}
