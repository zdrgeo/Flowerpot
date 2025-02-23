using Device.Host;
using Device.Services;
using Device.Services.Azure.Relay;
using Device.Sensors;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<DeviceServiceOptions>(builder.Configuration.GetSection("DeviceService"));
builder.Services.Configure<MockTemperatureSensorOptions>(builder.Configuration.GetSection("MockTemperatureSensor"));
// builder.Services.Configure<MockHumiditySensorOptions>(builder.Configuration.GetSection("MockHumiditySensor"));
builder.Services.Configure<MockIlluminanceSensorOptions>(builder.Configuration.GetSection("MockIlluminanceSensor"));
builder.Services.Configure<HumiditySensorOptions>(builder.Configuration.GetSection("HumiditySensor"));

builder.Services.AddSystemd();
builder.Services.AddSingleton<ITemperatureSensor, MockTemperatureSensor>();
// builder.Services.AddSingleton<IHumiditySensor, MockHumiditySensor>();
builder.Services.AddSingleton<IIlluminanceSensor, MockIlluminanceSensor>();
builder.Services.AddSingleton<IHumiditySensor, HumiditySensor>();
builder.Services.AddSingleton<IDeviceService, DeviceService>();
builder.Services.AddHostedService<DeviceHostedService>();

IHost host = builder.Build();

host.Run();

