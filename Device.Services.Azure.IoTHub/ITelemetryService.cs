namespace Device.Services.Azure.IoTHub;

public interface ITelemetryService
{
    Task RunAsync(CancellationToken cancellationToken);
}
