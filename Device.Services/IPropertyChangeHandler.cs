namespace Device.Services;

public interface IPropertyChangeHandler
{
    public Task RegisterAsync(CancellationToken cancellationToken);
    public Task UnregisterAsync(CancellationToken cancellationToken);
}
