using System.Text;
using System.Text.Json;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Device.Services.Azure.IoTHub;

public class TelemetryServiceOptions { }

public delegate TelemetryService TelemetryServiceFactory(DeviceClient deviceClient, IOptions<TelemetryServiceOptions> options, ILogger<TelemetryService> logger);

public class TelemetryService(DeviceClient deviceClient, IOptions<TelemetryServiceOptions> options, ILogger<TelemetryService> logger) : ITelemetryService
{
    readonly record struct EventModel(double Temperature, double Humidity, double Illuminance);

    const int Interval = 60_000;
    private readonly DeviceClient deviceClient = deviceClient ?? throw new ArgumentNullException(nameof(deviceClient));
    private readonly IOptions<TelemetryServiceOptions> options = options ?? throw new ArgumentNullException(nameof(options));
    private readonly ILogger<TelemetryService> logger = logger;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Sending telemetry events at: {time}", DateTimeOffset.Now);
            }

            await SendEventsAsync([new EventModel()], cancellationToken);

            await Task.Delay(Interval, cancellationToken);
        }
    }

    async Task SendEventsAsync(IReadOnlyList<EventModel> eventModels, CancellationToken cancellationToken)
    {
        List<Message> messages = [];

        foreach (EventModel eventModel in eventModels)
        {
            // PnpConvention.CreateMessage()

            using var message = new Message(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(eventModel)))
            {
                ContentType = "application/json",
                ContentEncoding = "utf-8",
            };

            message.Properties.Add("temperatureAlert", RaiseTemperatureAlert(eventModel.Temperature) ? "true" : "false");
            message.Properties.Add("humidityAlert", RaiseHumidityAlert(eventModel.Humidity) ? "true" : "false");
            message.Properties.Add("illuminanceAlert", RaiseIlluminanceAlert(eventModel.Illuminance) ? "true" : "false");

            messages.Add(message);
        }

        await deviceClient.SendEventBatchAsync(messages, cancellationToken);
    }

    private static bool RaiseTemperatureAlert(double temperature) => temperature < 10 || temperature > 45; 
    private static bool RaiseHumidityAlert(double humidity) => humidity < 60 || humidity > 95;
    private static bool RaiseIlluminanceAlert(double illuminance) => illuminance <= 20;
}
