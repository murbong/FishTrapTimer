using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace FishTrapTimer.Core
{

    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public RECT(int l, int t, int r, int b)
        {
            this.Left = l;
            this.Top = t;
            this.Right = r;
            this.Bottom = b;
        }

        public static RECT operator +(RECT a, RECT b)
        {
            return new RECT { Left = a.Left + b.Left, Top = a.Top + b.Top, Bottom = a.Bottom + b.Bottom, Right = a.Right + b.Right };
        }

        public static RECT operator -(RECT a, RECT b)
        {
            return new RECT { Left = a.Left - b.Left, Top = a.Top - b.Top, Bottom = a.Bottom - b.Bottom, Right = a.Right - b.Right };
        }

        public int Width => Right - Left;

        public int Height => Bottom - Top;

        public override string ToString()
        {
            return Left + ", " + Top + ", " + Bottom + ", " + Right;
        }
    }


    public static class ImageCapture
    {

        [DllImport("user32")] public static extern bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);
        [DllImport("user32")] public static extern int GetWindowRgn(IntPtr hWnd, IntPtr hRgn);
        [DllImport("gdi32")] public static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);
        [DllImport("user32")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        public static Bitmap CopyScreenCrop(IntPtr hwnd)
        {

            GetWindowRect(hwnd, out RECT rect);

            Size size = new Size();

            size.Width = rect.Right - rect.Left;
            size.Height = rect.Bottom - rect.Top;
            Bitmap bmp = new Bitmap(size.Width, size.Height, PixelFormat.Format24bppRgb);

            try
            {
                using (Graphics gr = Graphics.FromImage(bmp))
                {
                    gr.CopyFromScreen(rect.Left, rect.Top, 0, 0, size);
                }

            }
            catch
            {

            }
            return bmp;
        }

        public static Bitmap PrintWindow(IntPtr hwnd)
        {
            GetWindowRect(hwnd, out RECT rc);
            Bitmap bmp = new Bitmap(rc.Width, rc.Height, PixelFormat.Format24bppRgb);

            using (Graphics gfxBmp = Graphics.FromImage(bmp))
            {
                IntPtr hdcBitmap = gfxBmp.GetHdc();
                try
                {
                    PrintWindow(hwnd, hdcBitmap, 0x2);
                }
                finally
                {
                    gfxBmp.ReleaseHdc(hdcBitmap);
                }
            }

            return bmp;
        }
    }
}
