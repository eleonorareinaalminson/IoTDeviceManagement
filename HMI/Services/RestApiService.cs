using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using HMI.Models;
using Shared.DTOs;

namespace HMI.Services;

public class RestApiService
{
    private readonly HttpClient _httpClient;
    private readonly ConfigurationService _config;

    public RestApiService(ConfigurationService config)
    {
        _config = config;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_config.GetRestApiBaseUrl()),
            Timeout = TimeSpan.FromSeconds(5)
        };
    }

    public async Task<DeviceStatusDto?> GetDeviceStatusAsync(string deviceId)
    {
        try
        {
            var deviceEndpoint = GetDeviceEndpoint(deviceId);
            var response = await _httpClient.GetAsync($"{deviceEndpoint}/api/status");

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<DeviceStatusResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null)
                return null;

            return new DeviceStatusDto
            {
                DeviceId = result.DeviceId,
                DeviceType = (Shared.Enums.DeviceType)result.DeviceType,
                State = (Shared.Enums.DeviceState)result.State,
                Timestamp = result.Timestamp,
                Properties = result.Properties
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"REST API error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> SendCommandAsync(string deviceId, DeviceCommandDto command)
    {
        try
        {
            var deviceEndpoint = GetDeviceEndpoint(deviceId);

            var request = new
            {
                DeviceId = deviceId,
                Action = command.Action,
                Parameters = command.Parameters,
                Timestamp = DateTime.UtcNow
            };

            var response = await _httpClient.PostAsJsonAsync($"{deviceEndpoint}/api/command", request);

            if (response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"Command sent successfully: {command.Action}");
                return true;
            }

            System.Diagnostics.Debug.WriteLine($"Command failed: {response.StatusCode}");
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"REST API error: {ex.Message}");
            return false;
        }
    }

    public async Task<List<HistoryEntry>?> GetDeviceHistoryAsync(string deviceId)
    {
        try
        {
            var deviceEndpoint = GetDeviceEndpoint(deviceId);
            return await _httpClient.GetFromJsonAsync<List<HistoryEntry>>($"{deviceEndpoint}/api/history");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"REST API error: {ex.Message}");
            return null;
        }
    }

    private string GetDeviceEndpoint(string deviceId)
    {
        var devices = _config.Configuration.GetSection("Devices").GetChildren();
        var device = devices.FirstOrDefault(d => d["DeviceId"] == deviceId);
        return device?["Endpoint"] ?? "http://localhost:5001";
    }
}

// Helper class för deserialisering
public class DeviceStatusResponse
{
    public string DeviceId { get; set; } = string.Empty;
    public int DeviceType { get; set; }
    public int State { get; set; }
    public bool IsRunning { get; set; }
    public double Speed { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}