using Shared.DTOs;
using System.Net.Http;
using System.Net.Http.Json;
using HMI.Models;



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
            BaseAddress = new Uri(_config.GetRestApiBaseUrl())
        };
    }

    public async Task<DeviceStatusDto?> GetDeviceStatusAsync(string deviceId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<DeviceStatusDto>($"/api/devices/{deviceId}/status");
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
            var response = await _httpClient.PostAsJsonAsync($"/api/devices/{deviceId}/command", command);
            return response.IsSuccessStatusCode;
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
            return await _httpClient.GetFromJsonAsync<List<HistoryEntry>>($"/api/devices/{deviceId}/history");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"REST API error: {ex.Message}");
            return null;
        }
    }
}