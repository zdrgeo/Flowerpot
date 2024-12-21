using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Device.Services.Azure.IoTHub;

public class CommandHandlerOptions { }

public delegate CommandHandler CommandHandlerFactory(DeviceClient deviceClient, IOptions<CommandHandlerOptions> options, ILogger<CommandHandler> logger);

public class CommandHandler(DeviceClient deviceClient, IOptions<CommandHandlerOptions> options, ILogger<CommandHandler> logger) : ICommandHandler
{
    readonly record struct WaterRequestModel(double Quantity);
    readonly record struct WaterResponseModel(double Quantity);

    readonly DeviceClient deviceClient = deviceClient ?? throw new ArgumentNullException(nameof(deviceClient));
    readonly IOptions<CommandHandlerOptions> options = options ?? throw new ArgumentNullException(nameof(options));
    readonly ILogger<CommandHandler> logger = logger;

    public Task RegisterAsync(CancellationToken cancellationToken) => deviceClient.SetMethodHandlerAsync("water", WaterAsync, null);

    public Task UnregisterAsync(CancellationToken cancellationToken) => deviceClient.SetMethodHandlerAsync("water", null, null);

    Task<MethodResponse> WaterAsync(MethodRequest methodRequest, object userContext)
    {
        WaterRequestModel waterRequestModel = JsonSerializer.Deserialize<WaterRequestModel>(methodRequest.DataAsJson);

        // ...

        WaterResponseModel waterResponseModel = new ();

        string dataAsJson = JsonSerializer.Serialize(waterResponseModel);

        MethodResponse methodResponse = new MethodResponse(Encoding.UTF8.GetBytes(dataAsJson), 200);

        return Task.FromResult(methodResponse);

        // return Task.FromResult(new MethodResponse(200));
    }
}
