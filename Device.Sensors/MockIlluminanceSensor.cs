using System.Device.I2c;
using Iot.Device.Ads1115;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Device.Sensors;

public class MockIlluminanceSensorOptions
{
    public double MinValue { get; set; } = 50;
    public double MaxValue { get; set; } = 500;
}

public class MockIlluminanceSensor(IOptions<MockIlluminanceSensorOptions> options, ILogger<MockIlluminanceSensor> logger) : IIlluminanceSensor 
{
    private readonly IOptions<MockIlluminanceSensorOptions> options = options ?? throw new ArgumentNullException(nameof(options));
    private readonly ILogger<MockIlluminanceSensor> logger = logger;

    public Task<IlluminanceMeasurment> MeasureAsync(CancellationToken cancellationToken)
    {
        double value = Random.Shared.NextDouble() * (options.Value.MaxValue - options.Value.MinValue) + options.Value.MinValue;

        IlluminanceMeasurment measurment = new (DateTimeOffset.UtcNow, value);

        return Task.FromResult(measurment);
    }
}
