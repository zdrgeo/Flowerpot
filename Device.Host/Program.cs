using Device.Host;
using Device.Services;
using Device.Services.Azure.Relay;
using Device.Sensors;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<DeviceServiceOptions>(builder.Configuration.GetSection("DeviceService"));
builder.Services.Configure<MockTemperatureSensorOptions>(builder.Configuration.GetSection("MockTemperatureSensor"));
// builder.Services.Configure<MockSoilMoistureSensorOptions>(builder.Configuration.GetSection("MockSoilMoistureSensor"));
// builder.Services.Configure<MockIlluminanceSensorOptions>(builder.Configuration.GetSection("MockIlluminanceSensor"));
builder.Services.Configure<SoilMoistureSensorOptions>(builder.Configuration.GetSection("SoilMoistureSensor"));
builder.Services.Configure<IlluminanceSensorOptions>(builder.Configuration.GetSection("IlluminanceSensor"));

builder.Services.AddSystemd();
builder.Services.AddSingleton<ITemperatureSensor, MockTemperatureSensor>();
// builder.Services.AddSingleton<ISoilMoistureSensor, MockSoilMoistureSensor>();
// builder.Services.AddSingleton<IIlluminanceSensor, MockIlluminanceSensor>();
builder.Services.AddSingleton<ISoilMoistureSensor, SoilMoistureSensor>();
builder.Services.AddSingleton<IIlluminanceSensor, IlluminanceSensor>();
builder.Services.AddSingleton<IDeviceService, DeviceService>();
builder.Services.AddHostedService<DeviceHostedService>();

IHost host = builder.Build();

host.Run();
