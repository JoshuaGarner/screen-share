#region

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using UlteriusScreenShare.Win32Api;
#endregion

namespace UlteriusScreenShare.Desktop
{
    internal class Capture
    {
        public static Bitmap CaptureDesktop()
        {
            Bitmap bmp = null;
            {
                var hDc = IntPtr.Zero;
                try
                {
                    SIZE size;
                    hDc = Win32.GetDC(Win32.GetDesktopWindow());
                    var hMemDc = Gdi.CreateCompatibleDC(hDc);

                    size.Cx = Win32.GetSystemMetrics
                        (Win32.SmCxscreen);

                    size.Cy = Win32.GetSystemMetrics
                        (Win32.SmCyscreen);

                    var hBitmap = Gdi.CreateCompatibleBitmap(hDc, size.Cx, size.Cy);

                    if (hBitmap != IntPtr.Zero)
                    {
                        var hOld = Gdi.SelectObject
                            (hMemDc, hBitmap);

                        Gdi.BitBlt(hMemDc, 0, 0, size.Cx, size.Cy, hDc,
                            0, 0, Gdi.Srccopy);

                        Gdi.SelectObject(hMemDc, hOld);
                        Gdi.DeleteDC(hMemDc);
                        bmp = Image.FromHbitmap(hBitmap);
                        Gdi.DeleteObject(hBitmap);
                        // GC.Collect();
                    }
                }
                finally
                {
                    if (hDc != IntPtr.Zero)
                    {
                        Win32.ReleaseDC(Win32.GetDesktopWindow(), hDc);
                    }
                }
            }
            return bmp;
        }


        public struct SIZE
        {
            public int Cx;
            public int Cy;
        }
    }
}