using Device.Host;
using Device.Services;
using Device.Services.Azure.Relay;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<DeviceServiceOptions>(builder.Configuration.GetSection("DeviceService"));

builder.Services.AddSystemd();
builder.Services.AddSingleton<IDeviceService, DeviceService>();
builder.Services.AddHostedService<DeviceHostedService>();

IHost host = builder.Build();

host.Run();

