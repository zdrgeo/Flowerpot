using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Device.Services.Azure.IoTHub;

public class PropertyChangeHandlerOptions { }

public delegate PropertyChangeHandler PropertyChangeHandlerFactory(DeviceClient deviceClient, IOptions<PropertyChangeHandlerOptions> options, ILogger<PropertyChangeHandler> logger);

public class PropertyChangeHandler(DeviceClient deviceClient, IOptions<PropertyChangeHandlerOptions> options, ILogger<PropertyChangeHandler> logger) : IPropertyChangeHandler
{
    readonly DeviceClient deviceClient = deviceClient ?? throw new ArgumentNullException(nameof(deviceClient));
    readonly IOptions<PropertyChangeHandlerOptions> options = options ?? throw new ArgumentNullException(nameof(options));
    readonly ILogger<PropertyChangeHandler> logger = logger;

    public Task RegisterAsync(CancellationToken cancellationToken) => deviceClient.SetDesiredPropertyUpdateCallbackAsync(ChangePropertiesAsync, null);

    public Task UnregisterAsync(CancellationToken cancellationToken) => deviceClient.SetDesiredPropertyUpdateCallbackAsync(null, null);

    async Task ChangePropertiesAsync(TwinCollection desiredProperties, object userContext)
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
