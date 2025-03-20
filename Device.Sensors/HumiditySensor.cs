using System.Device.I2c;
using Iot.Device.Ads1115;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Device.Sensors;

public class HumiditySensorOptions{ }

public class HumiditySensor : IHumiditySensor, IDisposable
{
    public HumiditySensor(IOptions<HumiditySensorOptions> options, ILogger<HumiditySensor> logger)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.logger = logger;

        I2cConnectionSettings connectionSettings = new (busId, (int)I2cAddress.GND);

        device = I2cDevice.Create(connectionSettings);

        ads1115 = new (device, InputMultiplexer.AIN3, MeasuringRange.FS4096);
    }

    const int busId = 1;
    private readonly IOptions<HumiditySensorOptions> options;
    private readonly ILogger<HumiditySensor> logger;
    private readonly I2cDevice device;
    private readonly Ads1115 ads1115;
    private bool disposed;

    public Task<HumidityMeasurment> MeasureAsync(CancellationToken cancellationToken)
    {
        short raw = ads1115.ReadRaw();

        double value = ads1115.RawToVoltage(raw).Volts;

        HumidityMeasurment measurment = new (DateTimeOffset.UtcNow, value);

        return Task.FromResult(measurment);
    }

    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                ads1115?.Dispose();
                device?.Dispose();
            }

            disposed = true;
        }
    }
}
