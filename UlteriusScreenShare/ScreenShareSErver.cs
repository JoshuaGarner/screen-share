#region

using System;
using System.Net;
using System.Security;
using System.Threading;
using UlteriusScreenShare.Websocket;
using UlteriusScreenShare.Websocket.Server;
using vtortola.WebSockets;

#endregion

namespace UlteriusScreenShare
{
    public class ScreenShareServer
    {
        private readonly string _serverName;
        private readonly SecureString _password;
        private readonly ConnectionHandler _connectionHandler;
        private readonly WebSocketEventListener _server;

        public ScreenShareServer(string serverName, SecureString password, IPAddress address, int port)
        {
            _serverName = serverName;
            _password = password;

            var cancellation = new CancellationTokenSource();
            var endpoint = new IPEndPoint(address, port);
            _server = new WebSocketEventListener(endpoint, new WebSocketListenerOptions
            {
                PingTimeout = TimeSpan.FromSeconds(15),
                NegotiationTimeout = TimeSpan.FromSeconds(15),
                WebSocketSendTimeout = TimeSpan.FromSeconds(15),
                WebSocketReceiveTimeout = TimeSpan.FromSeconds(15),
                ParallelNegotiations = Environment.ProcessorCount*2,
                NegotiationQueueCapacity = 256,
                TcpBacklog = 1000
            });
            _connectionHandler = new ConnectionHandler(_serverName, _password, _server);
        }


        public void Start()
        {
            _server.Start();
            Console.WriteLine("ScreenShareServer Started");
        }

        public void Stop()
        {
            _server.Stop();
        }
    }
}