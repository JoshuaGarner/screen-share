#region

using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;
using Newtonsoft.Json.Linq;
using UlteriusScreenShare.Win32Api;

#endregion

namespace UlteriusScreenShare.Desktop
{
    public class CommandHandler
    {
        private readonly Screen[] _screens = Screen.AllScreens;
        private readonly  InputSimulator _simulator = new InputSimulator();

        public void ProcessCommand(string message)
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
                }
            }
            catch (Exception)
            {
                //who cares
            }
        }

        private void HandleDoubleClick()
        {
            _simulator.Mouse.LeftButtonDoubleClick();
        }
        private void HandleMouseDown()
        {
            _simulator.Mouse.LeftButtonDown();
        }

        private void HandleMouseUp()
        {
            _simulator.Mouse.LeftButtonUp();
        }

        private void HandleRightClick()
        {
         
            _simulator.Mouse.RightButtonClick();
        }
       
        private void HandleLeftClick()
        {
            _simulator.Mouse.LeftButtonClick();
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