namespace Device.Services.Azure.IoTHub;

public interface ICommandHandler
{
    Task RegisterAsync(CancellationToken cancellationToken);
    Task UnregisterAsync(CancellationToken cancellationToken);
}
