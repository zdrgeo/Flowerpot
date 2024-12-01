using Microsoft.Azure.Devices.Client;

namespace Device.Services.Azure.IoTHub;

public class DeviceService : IDeviceService
{
    private readonly DeviceClient client;

    public DeviceService(DeviceClient client)
    {
        this.client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public Task RunAsync(CancellationToken cancellationToken)
    {
        return Task.Delay(Timeout.Infinite, cancellationToken);
    }
}
