using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Device.Services.Azure.IoTHub;

public class DesiredPropertiesChangeHandlerOptions { }

public delegate DesiredPropertiesChangeHandler DesiredPropertiesChangeHandlerFactory(DeviceClient deviceClient, IOptions<DesiredPropertiesChangeHandlerOptions> options, ILogger<DesiredPropertiesChangeHandler> logger);

public class DesiredPropertiesChangeHandler(DeviceClient deviceClient, IOptions<DesiredPropertiesChangeHandlerOptions> options, ILogger<DesiredPropertiesChangeHandler> logger) : IDesiredPropertiesChangeHandler
{
    readonly DeviceClient deviceClient = deviceClient ?? throw new ArgumentNullException(nameof(deviceClient));
    readonly IOptions<DesiredPropertiesChangeHandlerOptions> options = options ?? throw new ArgumentNullException(nameof(options));
    readonly ILogger<DesiredPropertiesChangeHandler> logger = logger;

    public async Task RegisterAsync(CancellationToken cancellationToken)
    {
        Twin twin = await deviceClient.GetTwinAsync(cancellationToken);

        await ChangeDesiredPropertiesAsync(twin.Properties.Desired, null);

        await deviceClient.SetDesiredPropertyUpdateCallbackAsync(ChangeDesiredPropertiesAsync, null, cancellationToken);
    }

    public Task UnregisterAsync(CancellationToken cancellationToken) => deviceClient.SetDesiredPropertyUpdateCallbackAsync(null, null, cancellationToken);

    async Task ChangeDesiredPropertiesAsync(TwinCollection desiredProperties, object? userContext)
    {
        double desiredTemperature = desiredProperties["temperature"].Value<double>();

        TwinCollection reportedProperties = new TwinCollection();

        TwinCollection acknowledgmentProperties = new TwinCollection();

        acknowledgmentProperties["value"] = desiredTemperature;
        acknowledgmentProperties["ac"] = 200; // HTTP response status code 200 OK
        acknowledgmentProperties["av"] = desiredProperties.Version;
        acknowledgmentProperties["ad"] = "Desired temperature updated";

        reportedProperties["temperature"] = acknowledgmentProperties;

        await deviceClient.UpdateReportedPropertiesAsync(reportedProperties);
    }
}
