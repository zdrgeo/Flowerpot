namespace Device.Services;

public interface IDeviceService
{
    Task RunAsync(CancellationToken cancellationToken);
}
