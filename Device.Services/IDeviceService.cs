namespace Device.Services;

public interface IDeviceService
{
    public Task RunAsync(CancellationToken cancellationToken);
}
