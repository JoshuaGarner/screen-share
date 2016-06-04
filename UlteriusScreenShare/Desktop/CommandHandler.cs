#region

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using WindowsInput;
using Ionic.Zlib;
using Newtonsoft.Json.Linq;
using UlteriusScreenShare.Websocket;
using vtortola.WebSockets;

#endregion

namespace UlteriusScreenShare.Desktop
{
    public class CommandHandler
    {
        private readonly ConnectionHandler _connectionHandler;
        private readonly Screen[] _screens = Screen.AllScreens;
        private readonly InputSimulator _simulator = new InputSimulator();

        public CommandHandler(ConnectionHandler connectionHandler)
        {
            _connectionHandler = connectionHandler;
        }

        public void ProcessCommand(WebSocket client, string message)
        {
            try
            {
                var packet = JObject.Parse(message);
                var eventType = (string) packet["eventType"];
                var eventAction = (string) packet["action"];
                if (eventType.Equals("Mouse"))
                {
                    switch (eventAction)
                    {
                        case "Move":
                            MoveMouse(packet);
                            break;
                        case "Down":
                            HandleMouseDown();
                            break;
                        case "Up":
                            HandleMouseUp();
                            break;
                        case "LeftClick":
                            HandleLeftClick();
                            break;
                        case "LeftDblClick":
                            HandleDoubleClick();
                            break;
                        case "RightClick":
                            HandleRightClick();
                            break;
                    }
                }
                else if (eventType.Equals("Keyboard"))
                {
                    Console.WriteLine("Keyboard event");
                }
                else if (eventType.Equals("Frame"))
                {
                    switch (eventAction)
                    {
                        case "Full":

                            HandleFullFrame(client);
                            break;
                    }
                }
            }
            catch (Exception)
            {
                //who cares
            }
        }

        private void HandleFullFrame(WebSocket client)
        {
            using (var jpegStream = new MemoryStream())
            {
                var screenGrab = Capture.CaptureDesktop();
                screenGrab.Save(jpegStream, ImageFormat.Jpeg);
                var compressed = ZlibStream.CompressBuffer(jpegStream.ToArray());
                SendFrameData(client, compressed);
            }
        }

        private void SendFrameData(WebSocket client, byte[] compressed)
        {
            AuthClient authClient;
            if (_connectionHandler.Clients.TryGetValue(client.GetHashCode().ToString(), out authClient))
            {
                if (authClient.Authenticated && client.IsConnected)
                {
                    try
                    {
                        using (var messageWriter = client.CreateMessageWriter(WebSocketMessageType.Binary))
                        {
                            using (var stream = new MemoryStream(compressed))
                            {
                                stream.CopyTo(messageWriter);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }

        private void HandleDoubleClick()
        {
            Console.WriteLine("Double click fired");
            _simulator.Mouse.LeftButtonClick();
        }

        private void HandleMouseDown()
        {
            Console.WriteLine("Mouse down");
            _simulator.Mouse.LeftButtonDown();
          
        }

        private void HandleMouseUp()
        {
            Console.WriteLine("Mouse up");
          _simulator.Mouse.LeftButtonUp();
        }

        private void HandleRightClick()
        {
            Console.WriteLine("Right click");
            _simulator.Mouse.RightButtonClick();
        }

        private void HandleLeftClick()
        {
            Console.WriteLine("Left click");
           // _simulator.Mouse.LeftButtonClick();
        }

        private void MoveMouse(JObject packet)
        {
            try
            {
                int y = Convert.ToInt16(packet["PointerY"], CultureInfo.InvariantCulture);
                int x = Convert.ToInt16(packet["PointerX"], CultureInfo.InvariantCulture);
                var device = _screens[0];
                if (x < 0 || x >= device.Bounds.Width || y < 0 || y >= device.Bounds.Height)
                {
                    return;
                }
                Cursor.Position = new Point(x, y);
            }
            catch
            {
                Console.WriteLine("Error moving mouse");
            }
        }
    }
}