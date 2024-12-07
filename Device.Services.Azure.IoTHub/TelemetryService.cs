using System.Text;
using System.Text.Json;
using Microsoft.Azure.Devices.Client;

namespace Device.Services.Azure.IoTHub;

internal record struct TelemetryModel(double Temperature, double Humidity, double Illuminance);

public delegate TelemetryService TelemetryServiceFactory(DeviceClient client);

public class TelemetryService : ITelemetryService
{
    private readonly DeviceClient deviceClient;

    public TelemetryService(DeviceClient deviceClient)
    {
        this.deviceClient = deviceClient ?? throw new ArgumentNullException(nameof (deviceClient));
    }

    public async Task SendEventsAsync(IReadOnlyList<TelemetryEvent> telemetryEvents, CancellationToken cancellationToken)
    {
        List<Message> messages = [];
        
        foreach (TelemetryEvent telemetryEvent in telemetryEvents)
        {
            TelemetryModel telemetryModel = new (telemetryEvent.Temperature, telemetryEvent.Humidity, telemetryEvent.Illuminance);

            // PnpConvention.CreateMessage()
            using var message = new Message(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(telemetryModel)))
            {
                ContentType = "application/json",
                ContentEncoding = "utf-8",
            };

            message.Properties.Add("temperatureAlert", RaiseTemperatureAlert(telemetryEvent.Temperature) ? "true" : "false");
            message.Properties.Add("humidityAlert", RaiseHumidityAlert(telemetryEvent.Humidity) ? "true" : "false");
        }

        await deviceClient.SendEventBatchAsync(messages, cancellationToken);
    }

    private static bool RaiseTemperatureAlert(double temperature) => temperature < 0 || temperature > 45; 
    private static bool RaiseHumidityAlert(double humidity) => humidity < 60 || humidity > 95;
}
