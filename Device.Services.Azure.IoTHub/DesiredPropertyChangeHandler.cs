using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Device.Services.Azure.IoTHub;

public class DesiredPropertyChangeHandlerOptions { }

public delegate DesiredPropertyChangeHandler DesiredPropertyChangeHandlerFactory(DeviceClient deviceClient, IOptions<DesiredPropertyChangeHandlerOptions> options, ILogger<DesiredPropertyChangeHandler> logger);

public class DesiredPropertyChangeHandler(DeviceClient deviceClient, IOptions<DesiredPropertyChangeHandlerOptions> options, ILogger<DesiredPropertyChangeHandler> logger) : IDesiredPropertyChangeHandler
{
    readonly DeviceClient deviceClient = deviceClient ?? throw new ArgumentNullException(nameof(deviceClient));
    readonly IOptions<DesiredPropertyChangeHandlerOptions> options = options ?? throw new ArgumentNullException(nameof(options));
    readonly ILogger<DesiredPropertyChangeHandler> logger = logger;

    public async Task RegisterAsync(CancellationToken cancellationToken)
    {
        Twin twin = await deviceClient.GetTwinAsync(cancellationToken);

        await ChangeDesiredPropertiesAsync(twin.Properties.Desired, null);

        await deviceClient.SetDesiredPropertyUpdateCallbackAsync(ChangeDesiredPropertiesAsync, null);
    }

    public Task UnregisterAsync(CancellationToken cancellationToken) => deviceClient.SetDesiredPropertyUpdateCallbackAsync(null, null);

    async Task ChangeDesiredPropertiesAsync(TwinCollection desiredProperties, object userContext)
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
