using System.Device.I2c;
using Iot.Device.Ads1115;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Device.Sensors;

public class MockSoilMoistureSensorOptions
{
    public double MinValue { get; set; } = 40;
    public double MaxValue { get; set; } = 60;
}

public class MockSoilMoistureSensor(IOptions<MockSoilMoistureSensorOptions> options, ILogger<MockSoilMoistureSensor> logger) : ISoilMoistureSensor 
{
    private readonly IOptions<MockSoilMoistureSensorOptions> options = options ?? throw new ArgumentNullException(nameof(options));
    private readonly ILogger<MockSoilMoistureSensor> logger = logger;

    public Task<SoilMoistureMeasurment> MeasureAsync(CancellationToken cancellationToken)
    {
        double value = Random.Shared.NextDouble() * (options.Value.MaxValue - options.Value.MinValue) + options.Value.MinValue;

        SoilMoistureMeasurment measurment = new (DateTimeOffset.UtcNow, value);

        return Task.FromResult(measurment);
    }
}
