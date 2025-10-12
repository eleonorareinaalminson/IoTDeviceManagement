namespace Shared.DTOs;
public class DeviceStatusDto
{
    public string DeviceId { get; set; } = string.Empty;
    public DeviceType DeviceType { get; set; }
    public DeviceState State { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}