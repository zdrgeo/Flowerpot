using Device.Services;

namespace Device.Host;

public class CollectorService : BackgroundService
{
    private readonly ITelemetryService telemetryService;
    private readonly ILogger<CollectorService> logger;

    public CollectorService(ITelemetryService telemetryService, ILogger<CollectorService> logger)
    {
        this.telemetryService = telemetryService ?? throw new ArgumentNullException(nameof(telemetryService));
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Collector running at: {time}", DateTimeOffset.Now);
            }

            await Task.Delay(1000, cancellationToken);
        }
    }
}
