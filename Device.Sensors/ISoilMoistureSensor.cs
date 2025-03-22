namespace Device.Sensors;

public readonly record struct SoilMoistureMeasurment(DateTimeOffset Timestamp, double Value);

public interface ISoilMoistureSensor
{
    Task<SoilMoistureMeasurment> MeasureAsync(CancellationToken cancellationToken);
}
