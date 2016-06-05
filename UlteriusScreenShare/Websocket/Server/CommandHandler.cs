#region

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;
using Ionic.Zlib;
using Newtonsoft.Json.Linq;
using UlteriusScreenShare.Desktop;
using vtortola.WebSockets;

#endregion

namespace UlteriusScreenShare.Websocket.Server
{
    internal class CommandHandler
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
            var packet = JObject.Parse(message);
            var eventType = (string) packet["EventType"];
            var eventAction = (string) packet["Action"];
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
                    case "Scroll":
                        HandleScroll(packet);
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
                switch (eventAction)
                {
                    case "KeyDown":
                        HandleKeyDown(packet);
                        break;
                }
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

        private void HandleScroll(JObject packet)
        {
            var delta = (int) packet["delta"];
            delta = ~delta;

            _simulator.Mouse.VerticalScroll(delta);
        }

        private string ToHex(int value)
        {
            return $"0x{value:X}";
        }

        private void HandleKeyDown(JObject packet)
        {
            var keyCodes = packet["KeyCodes"];
            var codes =
                keyCodes.Select(code => ToHex(int.Parse(code.ToString())))
                    .Select(hexString => Convert.ToInt32(hexString, 16))
                    .ToList();

            if (codes.Count >= 2)
            {
                foreach (var code in codes)
                {
                    var virtualKey = (VirtualKeyCode) code;
                    _simulator.Keyboard.KeyDown(virtualKey);
                }
                //fuck.gif
                foreach (var code in codes)
                {
                    var virtualKey = (VirtualKeyCode) code;
                    _simulator.Keyboard.KeyUp(virtualKey);
                }
            }
            else
            {
                var virtualKey = (VirtualKeyCode) codes[0];
                _simulator.Keyboard.KeyPress(virtualKey);
            }
        }

        private void HandleFullFrame(WebSocket client)
        {
            using (var jpegStream = new MemoryStream())
            {
                var screenGrab = Capture.CaptureDesktop();
                if (screenGrab == null)
                {
                    return;
                }
                screenGrab.Save(jpegStream, ImageFormat.Jpeg);
                var compressed = ZlibStream.CompressBuffer(jpegStream.ToArray());
                SendFrameData(client, compressed);
            }
        }

        private void SendFrameData(WebSocket client, byte[] compressed)
        {
            var encryptedData = MessageHandler.EncryptBuffer(compressed, client);
            if (encryptedData != null && client.IsConnected)
            {
                try
                {
                    using (var messageWriter = client.CreateMessageWriter(WebSocketMessageType.Binary))
                    {
                        using (var stream = new MemoryStream(encryptedData))
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