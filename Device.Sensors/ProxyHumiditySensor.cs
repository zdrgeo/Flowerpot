using System.Device.I2c;
using Iot.Device.Sht3x;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UnitsNet;

namespace Device.Sensors;

public class ProxyHumiditySensorOptions {}

public class ProxyHumiditySensor : IHumiditySensor
{
    public ProxyHumiditySensor(TemperatureAndHumiditySensor temperatureAndHumiditySensor, IOptions<ProxyHumiditySensorOptions> options, ILogger<ProxyHumiditySensor> logger)
    {
        this.temperatureAndHumiditySensor = temperatureAndHumiditySensor ?? throw new ArgumentNullException(nameof(temperatureAndHumiditySensor));
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.logger = logger;
    }

    private readonly TemperatureAndHumiditySensor temperatureAndHumiditySensor;
    private readonly IOptions<ProxyHumiditySensorOptions> options;
    private readonly ILogger<ProxyHumiditySensor> logger;

    public Task<HumidityMeasurment> MeasureAsync(CancellationToken cancellationToken) => temperatureAndHumiditySensor.MeasureHumidityAsync(cancellationToken);
}
