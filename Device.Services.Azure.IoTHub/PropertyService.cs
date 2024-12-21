using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Device.Services.Azure.IoTHub;

public class PropertyServiceOptions { }

public delegate PropertyService PropertyServiceFactory(DeviceClient deviceClient, IOptions<PropertyServiceOptions> options, ILogger<PropertyService> logger);

public class PropertyService(DeviceClient deviceClient, IOptions<PropertyServiceOptions> options, ILogger<PropertyService> logger) : IPropertyService
{
    const int Interval = 600_000;
    private readonly DeviceClient deviceClient = deviceClient ?? throw new ArgumentNullException(nameof(deviceClient));
    readonly IOptions<PropertyServiceOptions> options = options ?? throw new ArgumentNullException(nameof(options));
    readonly ILogger<PropertyService> logger = logger;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Reporting properties at: {time}", DateTimeOffset.Now);
            }

            TwinCollection reportedProperties = new TwinCollection();

            reportedProperties["waterQuantity"] = 60;

            await deviceClient.UpdateReportedPropertiesAsync(reportedProperties);

            await Task.Delay(Interval, cancellationToken);
        }
    }
}
