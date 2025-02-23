using System.Device.I2c;
using Iot.Device.Ads1115;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Device.Services.Sensors;

public class HumiditySensorOptions
{
}

public class HumiditySensor(IOptions<HumiditySensorOptions> options, ILogger<HumiditySensor> logger) : IHumiditySensor 
{
    private readonly IOptions<HumiditySensorOptions> options = options ?? throw new ArgumentNullException(nameof(options));
    private readonly ILogger<HumiditySensor> logger = logger;
    const int busId = 1;

    public Task<HumidityMeasurment> MeasureAsync(CancellationToken cancellationToken)
    {
        // set I2C bus ID: 1
        // ADS1115 Addr Pin connect to GND
        I2cConnectionSettings connectionSettings = new (busId, (int)I2cAddress.GND);
        I2cDevice device = I2cDevice.Create(connectionSettings);

        // pass in I2cDevice
        // measure the voltage AIN3
        // set the maximum range to 4.096V
        using Ads1115 adc = new (device, InputMultiplexer.AIN3, MeasuringRange.FS4096);

        // read raw data from the sensor
        short raw = adc.ReadRaw();
        // raw data convert to voltage
        double value = adc.RawToVoltage(raw).Volts;

        HumidityMeasurment measurment = new (DateTimeOffset.UtcNow, value);

        return Task.FromResult(measurment);
    }
}
