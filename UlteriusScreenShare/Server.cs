#region

using System;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading;
using Ionic.Zlib;
using UlteriusScreenShare.Desktop;
using UlteriusScreenShare.Websocket;
using UlteriusScreenShare.Win32Api;
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
            var frameBackgroundWorker = new BackgroundWorker
            {
                WorkerSupportsCancellation = true
            };
            frameBackgroundWorker.DoWork += FrameBackgroundWorkerOnDoWork;
            frameBackgroundWorker.ProgressChanged += FrameBackgroundWorkerOnProgressChanged;
            frameBackgroundWorker.RunWorkerAsync();
        }


        private void FrameBackgroundWorkerOnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void FrameBackgroundWorkerOnDoWork(object sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker) sender;
            while (!worker.CancellationPending)
            {
                var shit = Win32.GetCursorPosition();
                Console.WriteLine(shit.x + " " + shit.y);
                var clientSize = _connectionHandler.Clients.Count;
                if (clientSize > 0)
                {
                    using (var jpegStream = new MemoryStream())
                    {
                        var screenGrab = Capture.CaptureDesktop();
                        screenGrab.Save(jpegStream, ImageFormat.Jpeg);
                        var compressed = ZlibStream.CompressBuffer(jpegStream.ToArray());
                        _connectionHandler.SendFrameData(compressed);
              
                    }
                }
            }
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