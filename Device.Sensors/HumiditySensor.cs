using System.Device.I2c;
using Iot.Device.Ads1115;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Device.Sensors;

public class HumiditySensorOptions{ }

public class HumiditySensor(IOptions<HumiditySensorOptions> options, ILogger<HumiditySensor> logger) : IHumiditySensor 
{
    private readonly IOptions<HumiditySensorOptions> options = options ?? throw new ArgumentNullException(nameof(options));
    private readonly ILogger<HumiditySensor> logger = logger;
    const int busId = 2;

    public Task<HumidityMeasurment> MeasureAsync(CancellationToken cancellationToken)
    {
        I2cConnectionSettings connectionSettings = new (busId, (int)I2cAddress.GND);

        using I2cDevice device = I2cDevice.Create(connectionSettings);

        using Ads1115 adc = new (device, InputMultiplexer.AIN3, MeasuringRange.FS4096);

        short raw = adc.ReadRaw();

        double value = adc.RawToVoltage(raw).Volts;

        HumidityMeasurment measurment = new (DateTimeOffset.UtcNow, value);

        return Task.FromResult(measurment);
    }
}
