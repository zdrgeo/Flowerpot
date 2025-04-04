using System.Device.I2c;
using Iot.Device.Ads1115;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Device.Sensors;

public class SoilMoistureSensorOptions
{
    public double MinVoltage { get; set; } = 0;
    public double MaxVoltage { get; set; } = 5;
}

public class SoilMoistureSensor : ISoilMoistureSensor, IDisposable
{
    public SoilMoistureSensor(IOptions<SoilMoistureSensorOptions> options, ILogger<SoilMoistureSensor> logger)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.logger = logger;

        I2cConnectionSettings connectionSettings = new (busId, (int)I2cAddress.GND);

        device = I2cDevice.Create(connectionSettings);

        ads1115 = new (device, InputMultiplexer.AIN3, MeasuringRange.FS6144);
    }

    const int busId = 1;
    private readonly IOptions<SoilMoistureSensorOptions> options;
    private readonly ILogger<SoilMoistureSensor> logger;
    private readonly I2cDevice device;
    private readonly Ads1115 ads1115;
    private bool disposed;

    public Task<SoilMoistureMeasurment> MeasureAsync(CancellationToken cancellationToken)
    {
        double voltage = ads1115.ReadVoltage().Volts;

        if (voltage < options.Value.MinVoltage)
        {
            logger.LogWarning($"Voltage {voltage} below minimum {options.Value.MinVoltage}.");

            voltage = options.Value.MinVoltage;
        }

        if (voltage > options.Value.MaxVoltage)
        {
            logger.LogWarning($"Voltage {voltage} above maximum {options.Value.MaxVoltage}.");

            voltage = options.Value.MaxVoltage;
        }

        double value = (options.Value.MaxVoltage - voltage) / (options.Value.MaxVoltage - options.Value.MinVoltage) * 100;

        SoilMoistureMeasurment measurment = new (DateTimeOffset.UtcNow, value);

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
