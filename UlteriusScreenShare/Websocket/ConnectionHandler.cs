#region

using System;
using System.Collections.Concurrent;
using System.IO;
using UlteriusScreenShare.Desktop;
using vtortola.WebSockets;

#endregion

namespace UlteriusScreenShare.Websocket
{
    public class ConnectionHandler
    {
        private readonly WebSocketEventListener _server;
        public readonly ConcurrentDictionary<string, AuthClient> Clients;
        private readonly CommandHandler _commandHandler;
        public ConnectionHandler(WebSocketEventListener server)
        {
            Clients = new ConcurrentDictionary<string, AuthClient>();
            _commandHandler = new CommandHandler(this);
            _server = server;
            _server.OnConnect += HandleConnect;
            _server.OnDisconnect += HandleDisconnect;
            _server.OnMessage += HandleMessage;
            _server.OnError += HandleError;
        }

        private void HandleError(WebSocket websocket, Exception error)
        {
            Console.WriteLine($"Error occured on {websocket.GetHashCode()}: {error.Message}");
        }

        private void HandleMessage(WebSocket websocket, string message)
        {
            _commandHandler.ProcessCommand(websocket, message);
        }

        private void HandleDisconnect(WebSocket websocket)
        {
            AuthClient client;
            if (Clients.TryRemove(websocket.GetHashCode().ToString(), out client))
            {
                Console.WriteLine("Client removed");
            }
        }

        private void HandleConnect(WebSocket websocket)
        {
            var authClient = new AuthClient(websocket) {Authenticated = true};
            if (Clients.TryAdd(websocket.GetHashCode().ToString(), authClient))
            {
                Console.WriteLine("New Client Connected");
            }
        }

       
        
    }
}