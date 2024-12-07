using System.Text.Json;
using Azure.Core.Serialization;
using Azure.Messaging;
using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Device.Services.Azure.EventGrid;

internal record struct TelemetryModel(double Temperature, double Humidity, double Illuminance);

public class TelemetryService : ITelemetryService
{
    private readonly EventGridPublisherClient client;
    private readonly ILogger<TelemetryService> logger;

    public TelemetryService(EventGridPublisherClient client, ILogger<TelemetryService> logger)
    {
        this.client = client ?? throw new ArgumentNullException(nameof (client));
        this.logger = logger;
    }

    public async Task SendEventsAsync(IReadOnlyList<TelemetryEvent> telemetryEvents, CancellationToken cancellationToken)
    {
        JsonObjectSerializer serializer = new (
            new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }
        );

        List<EventGridEvent> eventGridEvents = [];

        foreach (TelemetryEvent telemetryEvent in telemetryEvents) {
            TelemetryModel telemetryModel = new (telemetryEvent.Temperature, telemetryEvent.Humidity, telemetryEvent.Illuminance);

            EventGridEvent eventGridEvent = new (
                "ExampleEventSubject",
                "Example.EventType",
                "1.0",
                serializer.Serialize(telemetryModel)
            );

            eventGridEvents.Add(eventGridEvent);
        }

        _ = await client.SendEventsAsync(eventGridEvents, cancellationToken);

        // If we prefer to use Cloud Events instead of Event Grid Events.

        // List<CloudEvent> cloudEvents = [];

        // foreach (TelemetryEvent telemetryEvent in telemetryEvents) {
        //     TelemetryModel telemetryModel = new (telemetryEvent.Temperature, telemetryEvent.Humidity, telemetryEvent.Illuminance);

        //     CloudEvent cloudEvent = new (
        //         "/cloudevents/example/source",
        //         "Example.EventType",
        //         serializer.Serialize(telemetryModel)
        //     );

        //     cloudEvents.Add(cloudEvent);
        // }

        // _ = await client.SendEventsAsync(cloudEvents, cancellationToken);
    }
}
