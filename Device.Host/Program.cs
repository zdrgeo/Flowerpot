using Azure;
using Azure.Messaging.EventGrid;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Azure;
using Device.Host;
using Device.Services;
// using Device.Services.Azure.IoTHub;
using Device.Services.Azure.EventGrid;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddAzureClients(
    clientBuilder =>
    {
        clientBuilder.AddEventGridPublisherClient(new Uri("<endpoint>"), new AzureKeyCredential("<access-key>"));
    }
);
builder.Services.AddScoped<ITelemetryService, TelemetryService>();
builder.Services.AddHostedService<CollectorService>();

var host = builder.Build();

host.Run();
