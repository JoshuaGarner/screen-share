#region

using System;
using System.Net;
using System.Threading;
using UlteriusScreenShare.Websocket;
using vtortola.WebSockets;

#endregion

namespace UlteriusScreenShare
{
    public class Server
    {
        private readonly ConnectionHandler _connectionHandler;
        private readonly WebSocketEventListener _server;

        public Server(IPAddress address, int port)
        {
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
            _connectionHandler = new ConnectionHandler(_server);
        }


        public void Start()
        {
            _server.Start();
            Console.WriteLine("Server Started");
        }

        public void Stop()
        {
            _server.Stop();
        }
    }
}