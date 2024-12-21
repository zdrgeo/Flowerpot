namespace Device.Services.Azure.IoTHub;

public interface IPropertyService
{
    Task RunAsync(CancellationToken cancellationToken);
}
