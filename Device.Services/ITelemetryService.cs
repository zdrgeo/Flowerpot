﻿namespace Device.Services;

public interface ITelemetryService
{
    public Task RunAsync(CancellationToken cancellationToken);
}
