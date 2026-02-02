using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SocketClient.Services
{
    public class SocketService
    {

        private SocketIO _client;

        public SocketService(string route, SocketIOOptions? socketIOOptions)
        {

            if(socketIOOptions != null)
                _client = new SocketIO(route,socketIOOptions);
            else
            {
                _client = new SocketIO(route, new SocketIOOptions
                {
                    EIO = EngineIO.V4,
                    Reconnection = true,
                    ReconnectionAttempts = int.MaxValue,
                    Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
                });
            }

            _client.JsonSerializer = new NewtonsoftJsonSerializer();
            PopulateRegularEvents();
            PopulateEvents();
        }

        public async Task InitializeSocket()
        {
            await _client.ConnectAsync();
            if (_client.Connected)
            {
                PopulateRegularEvents();
                PopulateEvents();
            }
        }

        private async Task UnitializeSocket()
        {
            await _client.DisconnectAsync();
        }

        public async Task EmitEvent(string eventName, params object[] payload)
        {
            await _client.EmitAsync(eventName, payload);
        }

        // For regular events in SocketIoClient library
        protected void PopulateRegularEvents()
        {
            try
            {
                _client.OnConnected += async (sender, e) =>
                {
                    Console.WriteLine($"Connected to socket! \nID: {_client.Id}");
                    Console.WriteLine($"Http Client: {_client.HttpClient}");
                };

                _client.OnDisconnected += async (sender, e) =>
                {
                    Console.WriteLine("Disconnected to socket!");
                };

                _client.OnReconnected += async (sender, e) =>
                {
                    Console.WriteLine($"Reconnected to socket! \nID:{_client.Id}");
                };

                _client.OnReconnectAttempt += async (sender, e) =>
                {
                    Console.WriteLine($"Trying to reconnect to socket.");
                };

                _client.OnReconnectError += async (sender, e) =>
                {
                    Console.WriteLine($"Error reconnecting to socket");
                };

                _client.OnReconnectFailed += async (sender, e) =>
                {
                    Console.WriteLine($"Failed reconnecting to socket");
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception on {nameof(PopulateRegularEvents)}: {ex}");
            }
        }

        protected void PopulateEvents()
        {
            // Write your events here...
            _client.On("Message", (response) =>
            {
                Console.WriteLine("Event: Message");
                Console.WriteLine($"Received: {response}");
            });

            _client.On("ByeFromNode", (response) =>
            {
                Console.WriteLine("Event ByeFromNode: ByeFromNode");
                Console.WriteLine($"Received on ByeFromNode: {response}");
            });

            _client.On("HomeApi", (response) => {
                Console.WriteLine("Event: HomeApi");
                Console.WriteLine($"Received on HomeApi: {response}");
            });

            _client.On("Atualizar rota", (response) =>
            {
                //...
                Console.WriteLine("Event: Atualizar Rota");
                Console.WriteLine($"Received on Atualizar Rota: {response}");

            });

            // Any event that aren't specified
            //_client.OnAny((name, response) =>
            //{
            //    Console.WriteLine($"Event OnAny: {name}");
            //    Console.WriteLine($"Received OnAny: {response}");
            //});
        }
    }
}