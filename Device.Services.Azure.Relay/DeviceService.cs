using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using Microsoft.Azure.Relay;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Device.Services.Azure.Relay;

readonly record struct MeasurementModel(DateTimeOffset Timestamp, double Temperature, double Humidity, double Illuminance);
readonly record struct MeasurementRequestModel;
readonly record struct MeasurementResponseModel()
{
    public List<MeasurementModel> Measurements { get; } = [];
}

public class DeviceServiceOptions
{
    public required string ConnectionString { get; set; }
    public required TimeSpan MeasurementsInterval { get; set; } = TimeSpan.FromMinutes(1);
    public required int MeasurementsCount { get; set; } = 10;
}

public class DeviceService(IOptions<DeviceServiceOptions> options, ILogger<DeviceService> logger) : IDeviceService
{
    private readonly IOptions<DeviceServiceOptions> options = options ?? throw new ArgumentNullException(nameof(options));
    private readonly ILogger<DeviceService> logger = logger;
    readonly Lock measurementsLock = new ();
    readonly LinkedList<MeasurementModel> measurements = [];

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        HybridConnectionListener listener = new (options.Value.ConnectionString);

        listener.RequestHandler = async (context) => await (
            context.Request.Url.Segments switch
            {
                ["/", "flowerpot/", "application" or "application/"] => HandleApplicationAsync(context),
                ["/", "flowerpot/", "measurement" or "measurement/"] => HandleMeasurementAsync(context),
                _ => HandleAsync(context)
            }
        );

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

            DateTimeOffset timestamp = DateTimeOffset.UtcNow;

            // Reading sensors data...
            double temperature = Random.Shared.NextDouble() * 100;
            double humidity = Random.Shared.NextDouble() * 100;
            double illuminance = Random.Shared.NextDouble() * 100;
            // Reading sensors data...

            lock (measurementsLock)
            {
                measurements.AddLast(new MeasurementModel(timestamp, temperature, humidity, illuminance));

                while (measurements.Count > options.Value.MeasurementsCount)
                {
                    measurements.RemoveFirst();
                }
            }

            try { await Task.Delay(options.Value.MeasurementsInterval, cancellationToken); } catch (TaskCanceledException) { }
        }

        await listener.CloseAsync(CancellationToken.None);
    }

    private async Task HandleApplicationAsync(RelayedHttpListenerContext context)
    {
        context.Response.StatusCode = HttpStatusCode.OK;
        context.Response.StatusDescription = "OK";
        context.Response.Headers.Add(HttpRequestHeader.ContentType, "text/html");

        await using FileStream fileStream = new (Path.Combine(AppContext.BaseDirectory, "Application.html"), FileMode.Open, FileAccess.Read);

        await fileStream.CopyToAsync(context.Response.OutputStream);

        await context.Response.CloseAsync();
    }

    private async Task HandleMeasurementAsync(RelayedHttpListenerContext context)
    {
        // MeasurementRequestModel measurementRequest = await JsonSerializer.DeserializeAsync<MeasurementRequestModel>(context.Request.InputStream);

        context.Response.StatusCode = HttpStatusCode.OK;
        context.Response.StatusDescription = "OK";
        context.Response.Headers.Add(HttpRequestHeader.ContentType, "application/json");

        MeasurementResponseModel measurementResponse = new ();

        lock (measurementsLock)
        {
            measurementResponse.Measurements.AddRange(measurements);
        }

        await JsonSerializer.SerializeAsync(context.Response.OutputStream, measurementResponse);

        await context.Response.CloseAsync();
    }

    private async Task HandleAsync(RelayedHttpListenerContext context)
    {
        context.Response.StatusCode = HttpStatusCode.NotFound;
        context.Response.StatusDescription = "Not Found";

        await context.Response.CloseAsync();
    }
}
