using System.Device.I2c;
using Iot.Device.Sht3x;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UnitsNet;

namespace Device.Sensors;

public class ProxyTemperatureSensorOptions {}

public class ProxyTemperatureSensor : ITemperatureSensor
{
    public ProxyTemperatureSensor(TemperatureAndHumiditySensor temperatureAndHumiditySensor, IOptions<ProxyTemperatureSensorOptions> options, ILogger<ProxyTemperatureSensor> logger)
    {
        this.temperatureAndHumiditySensor = temperatureAndHumiditySensor ?? throw new ArgumentNullException(nameof(temperatureAndHumiditySensor));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.logger = logger;
    }

    private readonly TemperatureAndHumiditySensor temperatureAndHumiditySensor;
    private readonly IOptions<ProxyTemperatureSensorOptions> options;
    private readonly ILogger<ProxyTemperatureSensor> logger;

    public Task<TemperatureMeasurment> MeasureAsync(CancellationToken cancellationToken) => temperatureAndHumiditySensor.MeasureTemperatureAsync(cancellationToken);
}
