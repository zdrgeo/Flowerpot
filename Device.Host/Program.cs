using Device.Host;
using Device.Services;
using Device.Services.Azure.Relay;
using Device.Sensors;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<DeviceServiceOptions>(builder.Configuration.GetSection("DeviceService"));
// builder.Services.Configure<MockTemperatureSensorOptions>(builder.Configuration.GetSection("MockTemperatureSensor"));
// builder.Services.Configure<MockHumiditySensorOptions>(builder.Configuration.GetSection("MockHumiditySensor"));
// builder.Services.Configure<MockIlluminanceSensorOptions>(builder.Configuration.GetSection("MockIlluminanceSensor"));
// builder.Services.Configure<MockSoilMoistureSensorOptions>(builder.Configuration.GetSection("MockSoilMoistureSensor"));
builder.Services.Configure<TemperatureAndHumiditySensorOptions>(builder.Configuration.GetSection("TemperatureAndHumiditySensor"));
builder.Services.Configure<ProxyTemperatureSensorOptions>(builder.Configuration.GetSection("ProxyTemperatureSensor"));
builder.Services.Configure<ProxyHumiditySensorOptions>(builder.Configuration.GetSection("ProxyHumiditySensor"));
builder.Services.Configure<IlluminanceSensorOptions>(builder.Configuration.GetSection("IlluminanceSensor"));
builder.Services.Configure<SoilMoistureSensorOptions>(builder.Configuration.GetSection("SoilMoistureSensor"));

builder.Services.AddSystemd();
// builder.Services.AddSingleton<ITemperatureSensor, MockTemperatureSensor>();
// builder.Services.AddSingleton<IHumiditySensor, MockHumiditySensor>();
// builder.Services.AddSingleton<IIlluminanceSensor, MockIlluminanceSensor>();
// builder.Services.AddSingleton<ISoilMoistureSensor, MockSoilMoistureSensor>();
builder.Services.AddSingleton<TemperatureAndHumiditySensor>();
builder.Services.AddSingleton<ITemperatureSensor, ProxyTemperatureSensor>();
builder.Services.AddSingleton<IHumiditySensor, ProxyHumiditySensor>();
builder.Services.AddSingleton<IIlluminanceSensor, IlluminanceSensor>();
builder.Services.AddSingleton<ISoilMoistureSensor, SoilMoistureSensor>();
builder.Services.AddSingleton<IDeviceService, DeviceService>();
builder.Services.AddHostedService<DeviceHostedService>();

IHost host = builder.Build();

host.Run();
