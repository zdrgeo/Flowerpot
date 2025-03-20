using System.Device.I2c;
using Iot.Device.Bh1750fvi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UnitsNet;

namespace Device.Sensors;

public class IlluminanceSensorOptions {}

public class IlluminanceSensor : IIlluminanceSensor, IDisposable
{
    public IlluminanceSensor(IOptions<IlluminanceSensorOptions> options, ILogger<IlluminanceSensor> logger)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.logger = logger;

        I2cConnectionSettings connectionSettings = new (busId, (int)I2cAddress.AddPinLow);

        device = I2cDevice.Create(connectionSettings);

        bh1750fvi = new (device);
    }

    const int busId = 1;
    private readonly IOptions<IlluminanceSensorOptions> options;
    private readonly ILogger<IlluminanceSensor> logger;
    private readonly I2cDevice device;
    private readonly Bh1750fvi bh1750fvi;
    private bool disposed;

    public Task<IlluminanceMeasurment> MeasureAsync(CancellationToken cancellationToken)
    {
        Illuminance illuminance = bh1750fvi.Illuminance;

        double value = illuminance.Value;

        IlluminanceMeasurment measurment = new (DateTimeOffset.UtcNow, value);

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
                bh1750fvi?.Dispose();
                device?.Dispose();
            }

            disposed = true;
        }
    }
}
