namespace Device.Services.Azure.EventGrid;

public class DeviceService : IDeviceService
{
    public Task RunAsync(CancellationToken cancellationToken)
    {
        return Task.Delay(Timeout.Infinite, cancellationToken);
    }
}
