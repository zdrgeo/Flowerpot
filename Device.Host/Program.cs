using Azure;
using Azure.Identity;
using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Azure;
using Device.Host;
using Device.Services;
using Device.Services.Azure.IoTHub;
// using Device.Services.Azure.EventGrid;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddAzureClients(
    clientBuilder =>
    {
        clientBuilder.UseCredential(new DefaultAzureCredential());

        clientBuilder.AddEventGridPublisherClient(new Uri("<endpoint>"), new AzureKeyCredential("<access-key>"));
    }
);
// builder.Services.AddScoped<ITelemetryService, TelemetryService>();
// builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddSingleton<TelemetryServiceFactory>();
builder.Services.AddScoped<IDeviceService, ProvisioningDeviceService>();
builder.Services.AddHostedService<DeviceHostedService>();

var host = builder.Build();

host.Run();

