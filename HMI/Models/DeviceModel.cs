using Shared.Enums;

namespace HMI.Models;

public class DeviceModel
{
    public string DeviceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DeviceType Type { get; set; }
    public DeviceState State { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public DateTime LastSeen { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}