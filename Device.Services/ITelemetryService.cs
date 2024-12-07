namespace Device.Services;

public readonly record struct TelemetryEvent(double Temperature, double Humidity, double Illuminance);

public interface ITelemetryService
{
    public Task SendEventsAsync(IReadOnlyList<TelemetryEvent> telemetryEvents, CancellationToken cancellationToken);
}
