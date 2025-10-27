using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using Device.Sensors;
using Microsoft.Azure.Relay;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Device.Services.Azure.Relay;

readonly record struct MeasurementModel(DateTimeOffset Timestamp, double Temperature, double Humidity, double Illuminance, double SoilMoisture);
readonly record struct MeasurementRequestModel;
readonly record struct MeasurementResponseModel()
{
    public List<MeasurementModel> Measurements { get; } = [];
}

public class DeviceServiceOptions
{
    public required string ConnectionString { get; set; }
    public required TimeSpan MeasurementsInterval { get; set; } = TimeSpan.FromSeconds(5);
    public required int MeasurementsCount { get; set; } = 10;
}

public class DeviceService(
    ITemperatureSensor temperatureSensor,
    IHumiditySensor humiditySensor,
    IIlluminanceSensor illuminanceSensor,
    ISoilMoistureSensor soilMoistureSensor,
    IOptions<DeviceServiceOptions> options,
    ILogger<DeviceService> logger
) : IDeviceService
{
    private readonly ITemperatureSensor temperatureSensor = temperatureSensor ?? throw new ArgumentNullException(nameof(temperatureSensor));
    private readonly IHumiditySensor humiditySensor = humiditySensor ?? throw new ArgumentNullException(nameof(humiditySensor));
    private readonly IIlluminanceSensor illuminanceSensor = illuminanceSensor ?? throw new ArgumentNullException(nameof(illuminanceSensor));
    private readonly ISoilMoistureSensor soilMoistureSensor = soilMoistureSensor ?? throw new ArgumentNullException(nameof(soilMoistureSensor));
    private readonly IOptions<DeviceServiceOptions> options = options ?? throw new ArgumentNullException(nameof(options));
    private readonly ILogger<DeviceService> logger = logger;
    readonly Lock measurementsLock = new ();
    readonly LinkedList<MeasurementModel> measurements = [];

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        // Configure and start an HTTP server.
        // The "application" endpoint serves the application UI (in HTML format), while the "measurement" endpoint serves the application data (in JSON format).
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
        // Configure and start an HTTP server.

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Listening at: {time}", DateTimeOffset.Now);
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Reading sensors data at: {time}", DateTimeOffset.Now);
            }

            DateTimeOffset timestamp = DateTimeOffset.UtcNow;

            // Read the sensors' measurements.
            double temperature = (await temperatureSensor.MeasureAsync(cancellationToken)).Value;
            double humidity = (await humiditySensor.MeasureAsync(cancellationToken)).Value;
            double illuminance = (await illuminanceSensor.MeasureAsync(cancellationToken)).Value;
            double soilMoisture = (await soilMoistureSensor.MeasureAsync(cancellationToken)).Value;
            // Read the sensors' measurements.

            // Save a series of the last few measurements.
            lock (measurementsLock)
            {
                measurements.AddLast(new MeasurementModel(timestamp, temperature, humidity, illuminance, soilMoisture));

                while (measurements.Count > options.Value.MeasurementsCount)
                {
                    measurements.RemoveFirst();
                }
            }
            // Save a series of the last few measurements.

            try { await Task.Delay(options.Value.MeasurementsInterval, cancellationToken); } catch (TaskCanceledException) { }
        }

        await listener.CloseAsync(CancellationToken.None);
    }

    // Handles the "application" endpoint returning the application UI in HTML format.
    private async Task HandleApplicationAsync(RelayedHttpListenerContext context)
    {
        context.Response.StatusCode = HttpStatusCode.OK;
        context.Response.StatusDescription = "OK";
        context.Response.Headers.Add(HttpRequestHeader.ContentType, "text/html");

        await using FileStream fileStream = new (Path.Combine(AppContext.BaseDirectory, "Application.html"), FileMode.Open, FileAccess.Read);

        await fileStream.CopyToAsync(context.Response.OutputStream);

        await context.Response.CloseAsync();
    }

    // Handles the "measurement" endpoint Returning the application data in JSON format.
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
