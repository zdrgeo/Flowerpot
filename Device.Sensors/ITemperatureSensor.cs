namespace Device.Sensors;

public readonly record struct TemperatureMeasurment(DateTimeOffset Timestamp, double Value);

public interface ITemperatureSensor
{
    Task<TemperatureMeasurment> MeasureAsync(CancellationToken cancellationToken);
}
