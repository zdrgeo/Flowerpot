using Device.Services;

namespace Device.Host;

public class DeviceHostService : BackgroundService
{
    private readonly IDeviceService deviceService;
    private readonly ILogger<DeviceHostService> logger;

    public DeviceHostService(IDeviceService deviceService, ILogger<DeviceHostService> logger)
    {
        this.deviceService = deviceService ?? throw new ArgumentNullException(nameof(deviceService));
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Collector running at: {time}", DateTimeOffset.Now);
            }

            await Task.Delay(1000, cancellationToken);
        }
    }
}
