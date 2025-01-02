using Azure;
using Azure.Identity;
using Microsoft.Extensions.Azure;
using Device.Host;
using Device.Services;
// using Device.Services.Azure.IoTHub;
using Device.Services.Azure.Relay;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<DeviceServiceOptions>(builder.Configuration.GetSection("DeviceService"));

builder.Services.AddAzureClients(
    clientBuilder =>
    {
        clientBuilder.UseCredential(new DefaultAzureCredential());
    }
);

// Azure IoT Hub
// builder.Services.AddSingleton<DesiredPropertiesChangeHandlerFactory>();
// builder.Services.AddSingleton<CommandHandlerFactory>();
// builder.Services.AddSingleton<PropertyServiceFactory>();
// builder.Services.AddSingleton<TelemetryServiceFactory>();
// // builder.Services.AddSingleton<IDeviceService, DeviceService>();
// builder.Services.AddSingleton<IDeviceService, ProvisioningDeviceService>();
// Azure IoT Hub

// Azure Relay
builder.Services.AddSingleton<IDeviceService, DeviceService>();
// Azure Relay

builder.Services.AddHostedService<DeviceHostedService>();

IHost host = builder.Build();

host.Run();

