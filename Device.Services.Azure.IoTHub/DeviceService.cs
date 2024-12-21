using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Device.Services.Azure.IoTHub;

public class DeviceServiceOptions
{
    public required string ConnectionString { get; set; }
}

public class DeviceService(
    DesiredPropertiesChangeHandlerFactory desiredPropertiesChangeHandlerFactory,
    CommandHandlerFactory commandHandlerFactory,
    PropertyServiceFactory propertyServiceFactory,
    TelemetryServiceFactory telemetryServiceFactory,
    IOptions<DeviceServiceOptions> options,
    ILogger<DeviceService> logger
) : IDeviceService
{
    const string ModelId = "dtmi:com:github:zdrgeo:Flowerpot;1";
    readonly DesiredPropertiesChangeHandlerFactory desiredPropertiesChangeHandlerFactory = desiredPropertiesChangeHandlerFactory ?? throw new ArgumentNullException(nameof(desiredPropertiesChangeHandlerFactory));
    readonly CommandHandlerFactory commandHandlerFactory = commandHandlerFactory ?? throw new ArgumentNullException(nameof(commandHandlerFactory));
    readonly PropertyServiceFactory propertyServiceFactory = propertyServiceFactory ?? throw new ArgumentNullException(nameof(propertyServiceFactory));
    readonly TelemetryServiceFactory telemetryServiceFactory = telemetryServiceFactory ?? throw new ArgumentNullException(nameof(telemetryServiceFactory));
    readonly IOptions<DeviceServiceOptions> options = options ?? throw new ArgumentNullException(nameof(options));
    readonly ILogger<DeviceService> logger = logger;
    readonly IOptions<DesiredPropertiesChangeHandlerOptions> desiredPropertiesChangeHandlerOptions;
    readonly ILogger<DesiredPropertiesChangeHandler> desiredPropertiesChangeHandlerLogger;
    readonly IOptions<CommandHandlerOptions> commandHandlerOptions;
    readonly ILogger<CommandHandler> commandHandlerLogger;
    readonly IOptions<TelemetryServiceOptions> telemetryServiceOptions;
    readonly ILogger<TelemetryService> telemetryServiceLogger;
    readonly IOptions<PropertyServiceOptions> propertyServiceOptions;
    readonly ILogger<PropertyService> propertyServiceLogger;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        DeviceClient deviceClient = CreateDeviceClient(options.Value);

        DesiredPropertiesChangeHandler desiredPropertiesChangeHandler = desiredPropertiesChangeHandlerFactory(deviceClient, desiredPropertiesChangeHandlerOptions, desiredPropertiesChangeHandlerLogger);

        await desiredPropertiesChangeHandler.RegisterAsync(cancellationToken);

        CommandHandler commandHandler = commandHandlerFactory(deviceClient, commandHandlerOptions, commandHandlerLogger);

        await commandHandler.RegisterAsync(cancellationToken);

        PropertyService propertyService = propertyServiceFactory(deviceClient, propertyServiceOptions, propertyServiceLogger);
        TelemetryService telemetryService = telemetryServiceFactory(deviceClient, telemetryServiceOptions, telemetryServiceLogger);

        await Task.WhenAll(propertyService.RunAsync(cancellationToken), telemetryService.RunAsync(cancellationToken));
    }

    static DeviceClient CreateDeviceClient(DeviceServiceOptions options)
    {
        ClientOptions clientOptions = new()
        {
            ModelId = ModelId,
        };

        return DeviceClient.CreateFromConnectionString(options.ConnectionString, TransportType.Amqp, clientOptions);
    }
}
