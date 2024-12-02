using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.PlugAndPlay;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;

namespace Device.Services.Azure.IoTHub;

public class DeviceService : IDeviceService
{
    class ProvisioningOptions
    {
        public string RegistrationId { get; set; }
        public string GlobalDeviceEndpoint { get; set; }
        public string IdScope { get; set; }
        public string PrimaryKey { get; set; }
        public string SecondaryKey { get; set; }
    }

    private const string ModelId = "dtmi:com:github:zdrgeo:Flowerpot;1";

    private readonly DeviceClient client;

    public Task RunAsync(CancellationToken cancellationToken)
    {
        return Task.Delay(Timeout.Infinite, cancellationToken);
    }

    static DeviceClient CreateDeviceClient(string connectionString)
    {
        ClientOptions options = new()
        {
            ModelId = ModelId,
        };

        return DeviceClient.CreateFromConnectionString(connectionString, TransportType.Amqp, options);
    }

    static async Task<DeviceClient> ProvisionDeviceAndCreateDeviceClientAsync(ProvisioningOptions options, CancellationToken cancellationToken)
    {
        using SecurityProviderSymmetricKey securityProvider = new (options.RegistrationId, options.PrimaryKey, options.SecondaryKey);

        using ProvisioningTransportHandlerAmqp provisioningTransportHandler = new ();

        ProvisioningDeviceClient provisioningDeviceClient = ProvisioningDeviceClient.Create(options.GlobalDeviceEndpoint, options.IdScope, securityProvider, provisioningTransportHandler);

        ProvisioningRegistrationAdditionalData provisioningRegistrationAdditionalData = new ()
        {
            JsonData = $"{{ \"modelId\": \"{ ModelId }\" }}",
        };

        DeviceRegistrationResult deviceRegistrationResult = await provisioningDeviceClient.RegisterAsync(provisioningRegistrationAdditionalData, cancellationToken);

        DeviceAuthenticationWithRegistrySymmetricKey authenticationMethod = new (deviceRegistrationResult.DeviceId, options.PrimaryKey);

        ClientOptions clientOptions = new ()
        {
            ModelId = ModelId,
        };

        return DeviceClient.Create(deviceRegistrationResult.AssignedHub, authenticationMethod, TransportType.Amqp, clientOptions);
    }
}
