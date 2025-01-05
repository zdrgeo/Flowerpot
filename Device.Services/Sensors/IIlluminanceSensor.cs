namespace Device.Services.Sensors;

public readonly record struct IlluminanceMeasurment(DateTimeOffset Timestamp, double Value);

public interface IIlluminanceSensor
{
    Task<IlluminanceMeasurment> MeasureAsync(CancellationToken cancellationToken);
}
