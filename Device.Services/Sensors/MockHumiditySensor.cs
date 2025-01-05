using System.Device.I2c;
using Iot.Device.Ads1115;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Device.Services.Sensors;

public class MockHumiditySensorOptions
{
    public double MinValue { get; set; } = 0;
    public double MaxValue { get; set; } = 100;
}

public class MockHumiditySensor(IOptions<MockHumiditySensorOptions> options, ILogger<MockHumiditySensor> logger) : IHumiditySensor 
{
    private readonly IOptions<MockHumiditySensorOptions> options = options ?? throw new ArgumentNullException(nameof(options));
    private readonly ILogger<MockHumiditySensor> logger = logger;

    public Task<HumidityMeasurment> MeasureAsync(CancellationToken cancellationToken)
    {
        double value = Random.Shared.NextDouble() * (options.Value.MaxValue - options.Value.MinValue) + options.Value.MinValue;

        HumidityMeasurment measurment = new (DateTimeOffset.UtcNow, value);

        return Task.FromResult(measurment);
    }
}
