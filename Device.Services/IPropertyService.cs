namespace Device.Services;

public interface IPropertyService
{
    public Task RunAsync(CancellationToken cancellationToken);
}
