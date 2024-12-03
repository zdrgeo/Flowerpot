using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Device.Services.Azure.EventGrid;

public class DeviceService : IDeviceService
{
    private readonly IOptions<DeviceService> options;
    private readonly ILogger<DeviceService> logger;

    public DeviceService(IOptions<DeviceService> options, ILogger<DeviceService> logger)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.logger = logger;
    }

    public Task RunAsync(CancellationToken cancellationToken)
    {
        return Task.Delay(Timeout.Infinite, cancellationToken);
    }
}
