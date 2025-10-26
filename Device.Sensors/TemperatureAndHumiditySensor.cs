using System.Device.I2c;
using Iot.Device.Si7021;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UnitsNet;

namespace Device.Sensors;

public class TemperatureAndHumiditySensorOptions {}

public class TemperatureAndHumiditySensor : IDisposable
{
    public TemperatureAndHumiditySensor(IOptions<TemperatureAndHumiditySensorOptions> options, ILogger<TemperatureAndHumiditySensor> logger)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.logger = logger;

        I2cConnectionSettings connectionSettings = new (busId, deviceAddress);

        device = I2cDevice.Create(connectionSettings);

        si7021 = new (device);
    }

    const int busId = 1;
    const int deviceAddress = Si7021.DefaultI2cAddress;
    private readonly IOptions<TemperatureAndHumiditySensorOptions> options;
    private readonly ILogger<TemperatureAndHumiditySensor> logger;
    private readonly I2cDevice device;
    private readonly Si7021 si7021;
    private bool disposed;

    public Task<TemperatureMeasurment> MeasureTemperatureAsync(CancellationToken cancellationToken)
    {
        Temperature temperature = si7021.Temperature;

        double value = temperature.DegreesCelsius;

        TemperatureMeasurment measurment = new (DateTimeOffset.UtcNow, value);

        return Task.FromResult(measurment);
    }

    public Task<HumidityMeasurment> MeasureHumidityAsync(CancellationToken cancellationToken)
    {
        RelativeHumidity humidity = si7021.Humidity;

        double value = humidity.Percent;

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
                si7021?.Dispose();
                device?.Dispose();
            }

            disposed = true;
        }
    }
}
