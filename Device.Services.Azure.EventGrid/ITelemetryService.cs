namespace Device.Services.Azure.EventGrid;

public interface ITelemetryService
{
    Task RunAsync(CancellationToken cancellationToken);
}
