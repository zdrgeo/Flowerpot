using System.Text.Json;
using Azure.Core.Serialization;
using Azure.Messaging;
using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Device.Services.Azure.EventGrid;

public class TelemetryServiceOptions { }

public class TelemetryService(EventGridPublisherClient client, IOptions<TelemetryServiceOptions> options, ILogger<TelemetryService> logger) : ITelemetryService
{
    readonly record struct EventModel(double Temperature, double Humidity, double Illuminance);

    const int Interval = 60_000;
    readonly EventGridPublisherClient client = client ?? throw new ArgumentNullException(nameof(client));
    readonly IOptions<TelemetryServiceOptions> options = options ?? throw new ArgumentNullException(nameof(options));
    readonly ILogger<TelemetryService> logger = logger;

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
        JsonObjectSerializer serializer = new (
            new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }
        );

        List<EventGridEvent> eventGridEvents = [];

        foreach (EventModel eventModel in eventModels) {
            EventGridEvent eventGridEvent = new (
                "ExampleEventSubject",
                "Example.EventType",
                "1.0",
                serializer.Serialize(eventModel)
            );

            eventGridEvents.Add(eventGridEvent);
        }

        _ = await client.SendEventsAsync(eventGridEvents, cancellationToken);

        // If we prefer to use Cloud Events instead of Event Grid Events.

        // List<CloudEvent> cloudEvents = [];

        // foreach (EventModel eventModel in eventModels) {

        //     CloudEvent cloudEvent = new (
        //         "/cloudevents/example/source",
        //         "Example.EventType",
        //         serializer.Serialize(eventModel)
        //     );

        //     cloudEvents.Add(cloudEvent);
        // }

        // _ = await client.SendEventsAsync(cloudEvents, cancellationToken);
    }
}
