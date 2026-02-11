using Microsoft.Extensions.Logging;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketClient.Services
{
    public class GenericWebSocket
    {
        #region Private Fields
        private SocketIO _client;
        private ILogger<GenericWebSocket> _logger;
        //private Dictionary<string, bool> _registeredEvents;
        private HashSet<string> _registeredEvents;
        #endregion

        #region Constructors

        public GenericWebSocket(ILogger<GenericWebSocket> logger, string route)
        {
            _registeredEvents = new HashSet<string>();
            _logger = logger;
            _client = new SocketIO(route, new SocketIOOptions
            {
                EIO = EngineIO.V4,
                Reconnection = true,
                ReconnectionAttempts = int.MaxValue,
                Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
            });

            _client.JsonSerializer = new NewtonsoftJsonSerializer();

            PopulateRegularEvents();
            PopulateEvents();
            _registeredEvents.Add("Message");
        }


        public GenericWebSocket(ILogger<GenericWebSocket> logger, string route, SocketIOOptions options)
        {
            _registeredEvents = new HashSet<string>();

            _logger = logger;
            _client = new SocketIO(route, options);
            _client.JsonSerializer = new NewtonsoftJsonSerializer();

            PopulateRegularEvents();
            PopulateEvents();
            _registeredEvents.Add("Message");
        }

        #endregion

        #region Public Methods
        public async Task<bool> InitializeSocket()
        {
            try
            {
                if (_client == null) throw new InvalidOperationException("Socket client is not even constructed. Construct first, initialize after");

                await _client.ConnectAsync();
                if (_client.Connected)
                    return true;
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception on {nameof(InitializeSocket)}: {ex.Message}");
                throw;
            }
        }

        public async Task UnitializeSocket()
        {
            try
            {
                if (_client == null) throw new InvalidOperationException("Socket client is not even constructed. Construct first, initialize after");
                await _client.DisconnectAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception on {nameof(UnitializeSocket)}: {ex.Message}");
                throw;
            }
        }

        public async Task EmitEvent(string eventName, params object[] payload)
        {
            try
            {
                if (_client == null) throw new InvalidOperationException("Socket client is not even constructed. Construct first, initialize after");
                await _client.EmitAsync(eventName, payload);
                _logger.LogInformation($"\nEmitted event: {eventName} with payload: {string.Join(", ", payload)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception on {nameof(EmitEvent)}: {ex.Message}");
                throw;
            }

        }

        public void RegisterNewEvent<T>(string eventName, Action<T> callback)
        {
            try
            {
                if (_client == null) throw new InvalidOperationException("Socket client is not even constructed. Construct first, initialize after");

                if (!_registeredEvents.Contains(eventName))
                {
                    _client.On(eventName, (response) =>
                    {
                        T data = response.GetValue<T>();
                        callback(data);
                    });

                    _registeredEvents.Add(eventName);
                    _logger.LogInformation($"Event name: {eventName} registered in socket!");
                }
                else
                    _logger.LogInformation($"Event name: {eventName} has been registered before in socket!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception on {nameof(RegisterNewEvent)}: {ex.Message}");
                throw;
            }
        }

        public void UnregisterEvent(string eventName)
        {
            try
            {
                _client.Off(eventName);
                _registeredEvents.Remove(eventName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception on {nameof(UnregisterEvent)}: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Private Methods
        // For regular events in SocketIoClient library
        private void PopulateRegularEvents()
        {
            try
            {
                if (_client == null) throw new InvalidOperationException("Socket client is not even constructed. Construct first, initialize after");

                _client.OnConnected += async (sender, e) =>
                {
                    Console.WriteLine($"\nConnected to socket! \nID: {_client.Id}");
                    _logger.LogInformation($"\nConnected to socket! \nID: {_client.Id}");
                };

                _client.OnDisconnected += async (sender, e) =>
                {
                    Console.WriteLine("\nDisconnected to socket!");
                    _logger.LogInformation("Disconnected to socket!");
                };

                _client.OnReconnected += async (sender, e) =>
                {
                    Console.WriteLine($"\nReconnected to socket! \nID:{_client.Id}");
                    _logger.LogInformation($"Reconnected to socket! \nID:{_client.Id}");
                };

                _client.OnReconnectAttempt += async (sender, e) =>
                {
                    Console.WriteLine($"\nTrying to reconnect to socket.");
                    _logger.LogInformation("Trying to reconnect to socket");
                };

                _client.OnReconnectError += async (sender, e) =>
                {
                    Console.WriteLine($"\nError reconnecting to socket");
                    _logger.LogInformation("Error reconnecting to socket");
                };

                _client.OnReconnectFailed += async (sender, e) =>
                {
                    Console.WriteLine($"\nFailed reconnecting to socket");
                    _logger.LogInformation("Failed reconnecting to socket");
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception on {nameof(PopulateRegularEvents)}: {ex.Message}");
            }
        }

        private void PopulateEvents()
        {
            try
            {
                if (_client == null) throw new InvalidOperationException("Socket client is not even constructed. Construct first, initialize after");

                _client.On("Message", (response) =>
                {
                    Console.WriteLine("Event: Message");
                    Console.WriteLine($"Received: {response}");
                });

                _client.OnAny((eventname, response) =>
                {
                    if (!_registeredEvents.Contains(eventname))
                    {
                        Console.WriteLine($"\nUnknown event: {eventname}");
                        Console.WriteLine($"Receivd: {response}");
                        Console.WriteLine("If you gonna receive more from this event, please add him to socket events.");
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception on {nameof(PopulateEvents)}: {ex}");
            }
        }
        #endregion
    }
}
