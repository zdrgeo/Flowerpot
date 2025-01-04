namespace Device.Services.Sensors;

public interface IHumiditySensor
{
    Task<double> MeasureAsync(CancellationToken cancellationToken);
}
