using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;

namespace SocketClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new SocketIOClient.SocketIO("http://localhost:3000", new SocketIOOptions
            {
                Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
                EIO = EngineIO.V4,
                Reconnection = true
            });

            client.JsonSerializer = new NewtonsoftJsonSerializer();

            client.OnConnected += async (sender, e) =>
            {
                Console.WriteLine($"Conectado, ID: {client.Id}");
                await client.EmitAsync("HelloFromC#", "Conexão C# -> Node");
            };

            client.OnAny((name, response) => {
                Console.WriteLine($"Evento: {name}");
                Console.WriteLine($"Recebido: {response}");
            });

            client.On("Test", response =>
            {
                var dados = response.GetValue<string>();
                Console.WriteLine($"Mensagem do server ao event ('Test'): {dados}");
            });

            try
            {
                await client.ConnectAsync();
                await Task.Delay(-1); // loop infinito
            }
            catch(ConnectionException ex)
            {
                Console.WriteLine(ex.Message);
                await Task.Delay(10000);
                await client.ConnectAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Deu ruim1: {ex.Message}");
            }
        }
    }
}