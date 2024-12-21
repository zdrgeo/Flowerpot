using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.PlugAndPlay;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Device.Services.Azure.IoTHub;

public class ProvisioningDeviceServiceOptions
{
    public required string RegistrationId { get; set; }
    public required string GlobalDeviceEndpoint { get; set; }
    public required string IdScope { get; set; }
    public required string Key { get; set; }
}

public class ProvisioningDeviceService(
    PropertyChangeHandlerFactory propertyChangeHandlerFactory,
    CommandHandlerFactory commandHandlerFactory,
    PropertyServiceFactory propertyServiceFactory,
    TelemetryServiceFactory telemetryServiceFactory,
    IOptions<ProvisioningDeviceServiceOptions> options,
    ILogger<ProvisioningDeviceService> logger
) : IDeviceService
{
    const string ModelId = "dtmi:com:github:zdrgeo:Flowerpot;1";
    readonly PropertyChangeHandlerFactory propertyChangeHandlerFactory = propertyChangeHandlerFactory ?? throw new ArgumentNullException(nameof(propertyChangeHandlerFactory));
    readonly CommandHandlerFactory commandHandlerFactory = commandHandlerFactory ?? throw new ArgumentNullException(nameof(commandHandlerFactory));
    readonly PropertyServiceFactory propertyServiceFactory = propertyServiceFactory ?? throw new ArgumentNullException(nameof(propertyServiceFactory));
    readonly TelemetryServiceFactory telemetryServiceFactory = telemetryServiceFactory ?? throw new ArgumentNullException(nameof(telemetryServiceFactory));
    readonly IOptions<ProvisioningDeviceServiceOptions> options = options ?? throw new ArgumentNullException(nameof(options));
    readonly ILogger<ProvisioningDeviceService> logger = logger;
    readonly IOptions<PropertyChangeHandlerOptions> propertyChangeHandlerOptions;
    readonly ILogger<PropertyChangeHandler> propertyChangeHandlerLogger;
    readonly IOptions<CommandHandlerOptions> commandHandlerOptions;
    readonly ILogger<CommandHandler> commandHandlerLogger;
    readonly IOptions<TelemetryServiceOptions> telemetryServiceOptions;
    readonly ILogger<TelemetryService> telemetryServiceLogger;
    readonly IOptions<PropertyServiceOptions> propertyServiceOptions;
    readonly ILogger<PropertyService> propertyServiceLogger;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        DeviceClient deviceClient = await CreateDeviceClientAsync(options.Value, cancellationToken);

        PropertyChangeHandler propertyChangeHandler = propertyChangeHandlerFactory(deviceClient, propertyChangeHandlerOptions, propertyChangeHandlerLogger);

        await propertyChangeHandler.RegisterAsync(cancellationToken);

        CommandHandler commandHandler = commandHandlerFactory(deviceClient, commandHandlerOptions, commandHandlerLogger);

        await commandHandler.RegisterAsync(cancellationToken);

        PropertyService propertyService = propertyServiceFactory(deviceClient, propertyServiceOptions, propertyServiceLogger);
        TelemetryService telemetryService = telemetryServiceFactory(deviceClient, telemetryServiceOptions, telemetryServiceLogger);

        await Task.WhenAll(propertyService.RunAsync(cancellationToken), telemetryService.RunAsync(cancellationToken));
    }

    static async Task<DeviceClient> CreateDeviceClientAsync(ProvisioningDeviceServiceOptions options, CancellationToken cancellationToken)
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
