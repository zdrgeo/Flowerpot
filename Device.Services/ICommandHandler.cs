﻿namespace Device.Services;

public interface ICommandHandler
{
    Task RegisterAsync(CancellationToken cancellationToken);
    Task UnregisterAsync(CancellationToken cancellationToken);
}
