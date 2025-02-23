using System.Device.I2c;
using Iot.Device.Ads1115;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Device.Sensors;

public class MockTemperatureSensorOptions
{
    public double MinValue { get; set; } = 20;
    public double MaxValue { get; set; } = 25;
}

public class MockTemperatureSensor(IOptions<MockTemperatureSensorOptions> options, ILogger<MockTemperatureSensor> logger) : ITemperatureSensor 
{
    private readonly IOptions<MockTemperatureSensorOptions> options = options ?? throw new ArgumentNullException(nameof(options));
    private readonly ILogger<MockTemperatureSensor> logger = logger;

    public Task<TemperatureMeasurment> MeasureAsync(CancellationToken cancellationToken)
    {
        double value = Random.Shared.NextDouble() * (options.Value.MaxValue - options.Value.MinValue) + options.Value.MinValue;

        TemperatureMeasurment measurment = new (DateTimeOffset.UtcNow, value);

        return Task.FromResult(measurment);
    }
}
