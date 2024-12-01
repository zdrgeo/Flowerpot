using Azure;
using Azure.Identity;
using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Azure;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.PlugAndPlay;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Device.Host;
using Device.Services;
// using Device.Services.Azure.IoTHub;
using Device.Services.Azure.EventGrid;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddAzureClients(
    clientBuilder =>
    {
        clientBuilder.UseCredential(new DefaultAzureCredential());

        clientBuilder.AddEventGridPublisherClient(new Uri("<endpoint>"), new AzureKeyCredential("<access-key>"));
    }
);
builder.Services.AddScoped<ITelemetryService, TelemetryService>();
builder.Services.AddHostedService<CollectorService>();

var host = builder.Build();

host.Run();

const string ModelId = "";

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

class ProvisioningOptions
{
    public string RegistrationId { get; set; }
    public string GlobalDeviceEndpoint { get; set; }
    public string IdScope { get; set; }
    public string PrimaryKey { get; set; }
    public string SecondaryKey { get; set; }
}
