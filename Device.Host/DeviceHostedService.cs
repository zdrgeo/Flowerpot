using Device.Services;

namespace Device.Host;

public class DeviceHostedService(IDeviceService deviceService, ILogger<DeviceHostedService> logger) : BackgroundService
{
    readonly IDeviceService deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
    readonly ILogger<DeviceHostedService> logger = logger;

    protected override Task ExecuteAsync(CancellationToken cancellationToken) => deviceService.RunAsync(cancellationToken);
}
