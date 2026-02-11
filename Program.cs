using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SocketClient.Services;
using SocketIOClient;

var host = Host.CreateDefaultBuilder(args).ConfigureServices(services =>
{
    services.AddLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    });

    services.AddSingleton<GenericWebSocket>(sp =>
    {
        var logger = sp.GetRequiredService<ILogger<GenericWebSocket>>();

        var route = "http://localhost:3000";

        var options = new SocketIOOptions
        {
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        };

        return new GenericWebSocket(logger, route, options);
    });

}).Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
var socket = host.Services.GetRequiredService<GenericWebSocket>();

logger.LogInformation("Console app started");

await socket.InitializeSocket();

socket.RegisterNewEvent<string>("Ping", async (msg) =>
{
    //Console.WriteLine("Message info on event 'HealthCheck':", msg);
    Console.WriteLine($"\nEvent: Ping \nMessage: {msg}");
    await socket.EmitEvent("Pong", "Pong");
});

socket.RegisterNewEvent<string>("ByeFromServer", (msg) =>
{
    //Console.WriteLine("Message info on event 'ByeFromServer':", msg);
    Console.WriteLine($"\nEvent: ByeFromServer \nMessage: {msg}");
});

await socket.EmitEvent("HelloFromCSharp", "Hello server!");

// mantém vivo
await Task.Delay(Timeout.Infinite);
