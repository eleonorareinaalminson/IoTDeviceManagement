
namespace HMI.Models;

public class HistoryEntry
{
    public DateTime Timestamp { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string Event { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}