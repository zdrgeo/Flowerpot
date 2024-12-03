using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.PlugAndPlay;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Device.Services.Azure.IoTHub;

public class ProvisioningDeviceOptions
{
    public string RegistrationId { get; set; }
    public string GlobalDeviceEndpoint { get; set; }
    public string IdScope { get; set; }
    public string Key { get; set; }
}

public class ProvisioningDeviceService : IDeviceService
{
    public ProvisioningDeviceService(TelemetryServiceFactory telemetryServiceFactory, IOptions<ProvisioningDeviceOptions> options, ILogger<ProvisioningDeviceService> logger)
    {
        this.telemetryServiceFactory = telemetryServiceFactory ?? throw new ArgumentNullException(nameof (telemetryServiceFactory));
        this.options = options ?? throw new ArgumentNullException(nameof (options));
        this.logger = logger;
    }

    private const string ModelId = "dtmi:com:github:zdrgeo:Flowerpot;1";
    private readonly TelemetryServiceFactory telemetryServiceFactory;
    private readonly IOptions<ProvisioningDeviceOptions> options;
    private readonly ILogger<ProvisioningDeviceService> logger;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        DeviceClient deviceClient = await CreateDeviceClientAsync(options.Value, cancellationToken);

        TelemetryService telemetryService = telemetryServiceFactory(deviceClient);

        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    static async Task<DeviceClient> CreateDeviceClientAsync(ProvisioningDeviceOptions options, CancellationToken cancellationToken)
    {
        using SecurityProviderSymmetricKey securityProvider = new (options.RegistrationId, options.Key, null);

        using ProvisioningTransportHandlerAmqp provisioningTransportHandler = new ();

        ProvisioningDeviceClient provisioningDeviceClient = ProvisioningDeviceClient.Create(options.GlobalDeviceEndpoint, options.IdScope, securityProvider, provisioningTransportHandler);

        ProvisioningRegistrationAdditionalData provisioningRegistrationAdditionalData = new ()
        {
            JsonData = $"{{ \"modelId\": \"{ ModelId }\" }}",
        };

        DeviceRegistrationResult deviceRegistrationResult = await provisioningDeviceClient.RegisterAsync(provisioningRegistrationAdditionalData, cancellationToken);

        DeviceAuthenticationWithRegistrySymmetricKey authenticationMethod = new (deviceRegistrationResult.DeviceId, options.Key);

        ClientOptions clientOptions = new ()
        {
            ModelId = ModelId,
        };

        return DeviceClient.Create(deviceRegistrationResult.AssignedHub, authenticationMethod, TransportType.Amqp, clientOptions);
    }
}
