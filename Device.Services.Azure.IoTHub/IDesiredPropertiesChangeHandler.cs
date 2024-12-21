namespace Device.Services.Azure.IoTHub;

public interface IDesiredPropertiesChangeHandler
{
    Task RegisterAsync(CancellationToken cancellationToken);
    Task UnregisterAsync(CancellationToken cancellationToken);
}
