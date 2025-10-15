# IoT Device Management - HMI

Graphical control panel (Human Machine Interface) for monitoring and controlling IoT devices via Azure Service Bus and REST API.

## Features

- **Device Control** – Start/Stop and parameter configuration (speed, brightness, etc.)
- **Real-time Monitoring** – Live updates via Service Bus and REST API polling
- **Alarm Management** – Centralized view of active alarms with acknowledgment
- **Event History** – Logging of all system events
- **Dual Communication** – Hybrid REST + Service Bus support for maximum compatibility
- **Responsive UI** – Modern MVVM architecture with WPF

## System Requirements

- .NET 9.0 (Windows)
- Azure Service Bus connection (optional if using REST API only)
- At least one registered IoT device

## Installation & Running

```bash
dotnet build
dotnet run
```

## Configuration

Edit `HMI/appsettings.json`:

```json
{
  "ServiceBus": {
    "ConnectionString": "Endpoint=sb://...",
    "StatusQueue": "device-status",
    "CommandQueue": "device-commands",
    "AlarmQueue": "device-alarms"
  },
  "RestApi": {
    "BaseUrl": "http://localhost:5001"
  },
  "Devices": [
    {
      "DeviceId": "fan-001",
      "Name": "Office Fan",
      "Type": "Fan",
      "Endpoint": "http://localhost:5001"
    }
  ]
}
```

## Communication Methods

The HMI supports two communication channels:

**Service Bus** – Receives status updates and alarms from devices, sends commands via queues  
**REST API** – Polls device status every 2 seconds and sends commands via HTTP

If Service Bus connection fails, the application falls back to REST API polling.

## User Interface

### Main Window
- **Connected Devices** – List of all IoT devices with their status
- **Active Alarms** – Real-time alarms with acknowledgment capability
- **Event History** – All system events in chronological order

### Device Card
- Status indicator (Online/Offline/Running)
- Start/Stop buttons
- Value slider for adjustment (speed, brightness, etc.)

## Architecture

- **MainViewModel** – Main logic, initialization, and REST polling
- **DeviceCardViewModel** – Individual device control and status display
- **ServiceBusService** – Azure Service Bus integration for async messaging
- **RestApiService** – REST API client with device endpoint resolution
- **Shared** – DTOs and common enums

## License

Internal use
