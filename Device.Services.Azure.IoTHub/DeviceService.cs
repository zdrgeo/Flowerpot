using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.PlugAndPlay;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Device.Services.Azure.IoTHub;

public class DeviceOptions
{
    public string ConnectionString { get; set; }
}

public class DeviceService : IDeviceService
{
    public DeviceService(TelemetryServiceFactory telemetryServiceFactory, IOptions<DeviceOptions> options, ILogger<DeviceService> logger)
    {
        this.telemetryServiceFactory = telemetryServiceFactory ?? throw new ArgumentNullException(nameof(telemetryServiceFactory));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.logger = logger;
    }

    private const string ModelId = "dtmi:com:github:zdrgeo:Flowerpot;1";
    private readonly TelemetryServiceFactory telemetryServiceFactory;
    private readonly IOptions<DeviceOptions> options;
    private readonly ILogger<DeviceService> logger;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        DeviceClient deviceClient = CreateDeviceClient(options.Value);

        TelemetryService telemetryService = telemetryServiceFactory(deviceClient);

        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    static DeviceClient CreateDeviceClient(DeviceOptions options)
    {
        ClientOptions clientOptions = new()
        {
            ModelId = ModelId,
        };

        return DeviceClient.CreateFromConnectionString(options.ConnectionString, TransportType.Amqp, clientOptions);
    }
}
