namespace Device.Services.Sensors;

public readonly record struct HumidityMeasurment(DateTimeOffset Timestamp, double Value);

public interface IHumiditySensor
{
    Task<HumidityMeasurment> MeasureAsync(CancellationToken cancellationToken);
}
