using System.Device.I2c;
using Iot.Device.Bh1750fvi;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UnitsNet;

namespace Device.Sensors;

public class IlluminanceSensorOptions {}

public class IlluminanceSensor(IOptions<IlluminanceSensorOptions> options, ILogger<IlluminanceSensor> logger) : IIlluminanceSensor
{
    private readonly IOptions<IlluminanceSensorOptions> options = options ?? throw new ArgumentNullException(nameof(options));
    private readonly ILogger<IlluminanceSensor> logger = logger;
    const int busId = 1;

    public Task<IlluminanceMeasurment> MeasureAsync(CancellationToken cancellationToken)
    {
        I2cConnectionSettings connectionSettings = new (busId, (int)I2cAddress.AddPinLow);

        using I2cDevice device = I2cDevice.Create(connectionSettings);
        using Bh1750fvi bh1750fvi = new (device);

        Illuminance illuminance = bh1750fvi.Illuminance;

        double value = illuminance.Value;

        IlluminanceMeasurment measurment = new (DateTimeOffset.UtcNow, value);

        return Task.FromResult(measurment);
    }
}
