namespace Device.Services;

public interface ICommandHandler
{
    public Task RegisterAsync(CancellationToken cancellationToken);
    public Task UnregisterAsync(CancellationToken cancellationToken);
}
