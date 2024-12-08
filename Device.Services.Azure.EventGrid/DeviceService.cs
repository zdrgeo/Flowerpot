using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Device.Services.Azure.EventGrid;

public class DeviceServiceOptions { }

public class DeviceService(TelemetryService telemetryService, IOptions<DeviceServiceOptions> options, ILogger<DeviceService> logger) : IDeviceService
{
    readonly TelemetryService telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
    readonly IOptions<DeviceServiceOptions> options = options ?? throw new ArgumentNullException(nameof(options));
    readonly ILogger<DeviceService> logger = logger;

    public Task RunAsync(CancellationToken cancellationToken) => telemetryService.RunAsync(cancellationToken);
}
