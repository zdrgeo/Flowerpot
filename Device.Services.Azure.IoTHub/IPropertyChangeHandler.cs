namespace Device.Services.Azure.IoTHub;

public interface IPropertyChangeHandler
{
    Task RegisterAsync(CancellationToken cancellationToken);
    Task UnregisterAsync(CancellationToken cancellationToken);
}
