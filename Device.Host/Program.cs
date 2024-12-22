using Azure;
using Azure.Identity;
using Microsoft.Extensions.Azure;
using Device.Host;
using Device.Services;
// using Device.Services.Azure.IoTHub;
// using Device.Services.Azure.EventGrid;
using Device.Services.Azure.Relay;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddAzureClients(
    clientBuilder =>
    {
        clientBuilder.UseCredential(new DefaultAzureCredential());

        clientBuilder.AddEventGridPublisherClient(new Uri("<endpoint>"), new AzureKeyCredential("<access-key>"));
    }
);

// Azure IoT Hub
// builder.Services.AddSingleton<DesiredPropertiesChangeHandlerFactory>();
// builder.Services.AddSingleton<CommandHandlerFactory>();
// builder.Services.AddSingleton<PropertyServiceFactory>();
// builder.Services.AddSingleton<TelemetryServiceFactory>();
// // builder.Services.AddScoped<IDeviceService, DeviceService>();
// builder.Services.AddScoped<IDeviceService, ProvisioningDeviceService>();
// Azure IoT Hub

// Azure Event Grid
// builder.Services.AddScoped<ITelemetryService, TelemetryService>();
// builder.Services.AddScoped<IDeviceService, DeviceService>();
// Azure Event Grid

// Azure Relay
builder.Services.AddScoped<IDeviceService, DeviceService>();
// Azure Relay

builder.Services.AddHostedService<DeviceHostedService>();

IHost host = builder.Build();

host.Run();

