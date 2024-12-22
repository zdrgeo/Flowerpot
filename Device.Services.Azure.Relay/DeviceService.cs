using System.Net;
using Microsoft.Azure.Relay;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Device.Services.Azure.Relay;

public class DeviceServiceOptions {
    public required string Address { get; set; }
    public required string KeyName { get; set; }
    public required string Key { get; set; }
}

public class DeviceService(IOptions<DeviceServiceOptions> options, ILogger<DeviceService> logger) : IDeviceService
{
    private readonly IOptions<DeviceServiceOptions> options = options ?? throw new ArgumentNullException(nameof(options));
    private readonly ILogger<DeviceService> logger = logger;

    const int Interval = 60_000;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        TokenProvider tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(options.Value.KeyName, options.Value.Key);

        HybridConnectionListener listener = new (new (options.Value.Address), tokenProvider);

        listener.RequestHandler = HandleRequestAsync;

        await listener.OpenAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("listening at: {time}", DateTimeOffset.Now);
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Reading sensors data at: {time}", DateTimeOffset.Now);
            }

            // Reading sensors data...

            try { await Task.Delay(Interval, cancellationToken); } catch (TaskCanceledException) { }
        }

        await listener.CloseAsync(CancellationToken.None);
    }

    private async void HandleRequestAsync(RelayedHttpListenerContext context)
    {
        using StreamReader streamReader = new (context.Request.InputStream);

        context.Response.StatusCode = HttpStatusCode.OK;
        context.Response.StatusDescription = "OK";

        using StreamWriter streamWriter = new (context.Response.OutputStream);

        await streamWriter.WriteLineAsync("Hello from the device!");

        await context.Response.CloseAsync();
    }
}
