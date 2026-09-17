using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace SUPOptimizer.Host
{
    public static class NativeMethods
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HTCAPTION = 0x2;

        public const int SW_RESTORE = 9;
        public const int SW_SHOW = 5;
        public const int SW_HIDE = 0;

        /// <summary>
        /// Generates a crisp, high-DPI futuristic dark & teal application icon dynamically
        /// </summary>
        public static Icon CreateSUPIcon()
        {
            using var bitmap = new Bitmap(64, 64);
            using var g = Graphics.FromImage(bitmap);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // Background circle with dark gradient
            using var bgBrush = new LinearGradientBrush(
                new Point(0, 0), new Point(64, 64),
                Color.FromArgb(13, 17, 23),
                Color.FromArgb(22, 27, 34));
            g.FillEllipse(bgBrush, 2, 2, 60, 60);

            // Glowing teal outer border
            using var borderPen = new Pen(Color.FromArgb(0, 242, 254), 2.5f);
            g.DrawEllipse(borderPen, 3, 3, 58, 58);

            // Inner glowing stylized 'S' symbol
            using var path = new GraphicsPath();
            // High-tech geometric 'S' glyph
            path.AddPolygon(new[]
            {
                new PointF(44, 18),
                new PointF(22, 18),
                new PointF(22, 32),
                new PointF(42, 32),
                new PointF(42, 46),
                new PointF(20, 46),
                new PointF(20, 50),
                new PointF(46, 50),
                new PointF(46, 28),
                new PointF(26, 28),
                new PointF(26, 22),
                new PointF(44, 22)
            });

            using var iconBrush = new LinearGradientBrush(
                new Point(20, 18), new Point(46, 50),
                Color.FromArgb(0, 242, 254),
                Color.FromArgb(79, 172, 254));
            g.FillPath(iconBrush, path);

            // Core tech accent dot
            using var dotBrush = new SolidBrush(Color.FromArgb(56, 239, 125));
            g.FillEllipse(dotBrush, 36, 21, 4, 4);
            g.FillEllipse(dotBrush, 24, 43, 4, 4);

            IntPtr hIcon = bitmap.GetHicon();
            return Icon.FromHandle(hIcon);
        }
    }
}
