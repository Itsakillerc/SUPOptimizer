using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using SUPOptimizer.Core.Logging;
using SUPOptimizer.Server;

namespace SUPOptimizer.Host
{
    public class MainWindow : Form
    {
        private readonly LocalApiServer _server;
        private WebView2? _webView;
        public bool IsExiting { get; set; } = false;
        public bool IsMinimizedStartup { get; set; } = false;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_BORDER_COLOR = 34;
        private const int DWMWA_CAPTION_COLOR = 35;
        private const int DWMWA_COLOR_NONE = unchecked((int)0xFFFFFFFE);

        public MainWindow(LocalApiServer server)
        {
            _server = server;

            Text = "SUPOptimizer";
            Icon = NativeMethods.CreateSUPIcon();
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1300, 850);
            MinimumSize = new Size(960, 640);
            BackColor = Color.FromArgb(17, 17, 19);
            FormBorderStyle = FormBorderStyle.None;
            Padding = Padding.Empty;
            DoubleBuffered = true;

            InitializeWebView();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            ApplyDarkDwmAttributes();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ApplyDarkDwmAttributes();
        }

        private void ApplyDarkDwmAttributes()
        {
            try
            {
                int darkMode = 1;
                DwmSetWindowAttribute(Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));
                DwmSetWindowAttribute(Handle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref darkMode, sizeof(int));

                // Suppress DWM border completely so no white line appears around the borderless window
                int noBorder = DWMWA_COLOR_NONE;
                int hr = DwmSetWindowAttribute(Handle, DWMWA_BORDER_COLOR, ref noBorder, sizeof(int));
                if (hr != 0)
                {
                    int darkBorder = 0x00131111;
                    DwmSetWindowAttribute(Handle, DWMWA_BORDER_COLOR, ref darkBorder, sizeof(int));
                }

                int darkCaption = 0x00131111;
                DwmSetWindowAttribute(Handle, DWMWA_CAPTION_COLOR, ref darkCaption, sizeof(int));
            }
            catch { }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style |= 0x00040000; // WS_THICKFRAME / WS_SIZEBOX
                cp.Style |= 0x00020000; // WS_MINIMIZEBOX
                cp.Style |= 0x00010000; // WS_MAXIMIZEBOX
                return cp;
            }
        }

        private async void InitializeWebView()
        {
            try
            {
                _webView = new WebView2
                {
                    Dock = DockStyle.Fill,
                    DefaultBackgroundColor = Color.FromArgb(17, 17, 19)
                };

                Controls.Add(_webView);

                // Set user data folder in LocalAppData to avoid permission issues
                string userDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "SUPOptimizer", "WebView2Data");

                var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
                await _webView.EnsureCoreWebView2Async(env);

                _webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
                _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                _webView.CoreWebView2.Settings.IsStatusBarEnabled = false;

                _webView.Source = new Uri(_server.Url);

                AuditLogger.Log("Host", "WebView2 Initialized", $"Navigated to {_server.Url}");
            }
            catch (Exception ex)
            {
                AuditLogger.Log("Host", "WebView2 Failed", ex.Message, success: false, errorMessage: ex.Message);

                // Graceful fallback to default browser if WebView2 runtime isn't present
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = _server.Url,
                        UseShellExecute = true
                    });
                }
                catch { }

                if (!IsMinimizedStartup)
                {
                    MessageBox.Show(
                        "WebView2 runtime was not detected or failed to initialize.\n" +
                        "SUPOptimizer has opened the dashboard interface in your default system browser and remains active in the System Tray.\n\n" +
                        $"Error Details: {ex.Message}",
                        "SUPOptimizer - Runtime Warning",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }

                Hide();
            }
        }

        public void ShowAndActivate()
        {
            if (WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal;
            }

            Show();
            BringToFront();
            Activate();
            NativeMethods.SetForegroundWindow(Handle);
        }

        public void NavigateTab(string tabId)
        {
            try
            {
                if (_webView?.CoreWebView2 != null)
                {
                    _webView.CoreWebView2.ExecuteScriptAsync($"window.navigateToTab && window.navigateToTab('{tabId}');");
                }
            }
            catch { }
        }

        public void ToggleMaximize()
        {
            if (WindowState == FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Normal;
            }
            else
            {
                var screen = Screen.FromHandle(Handle);
                MaximizedBounds = screen.WorkingArea;
                WindowState = FormWindowState.Maximized;
            }
        }

        public void DragWindow()
        {
            NativeMethods.ReleaseCapture();
            NativeMethods.SendMessage(Handle, NativeMethods.WM_NCLBUTTONDOWN, NativeMethods.HTCAPTION, 0);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (WindowState == FormWindowState.Normal)
            {
                var screen = Screen.FromHandle(Handle);
                MaximizedBounds = screen.WorkingArea;
            }
        }

        private const int WM_GETMINMAXINFO = 0x0024;
        private const int WM_NCCALCSIZE = 0x0083;
        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 1;
        private const int HTLEFT = 10;
        private const int HTRIGHT = 11;
        private const int HTTOP = 12;
        private const int HTTOPLEFT = 13;
        private const int HTTOPRIGHT = 14;
        private const int HTBOTTOM = 15;
        private const int HTBOTTOMLEFT = 16;
        private const int HTBOTTOMRIGHT = 17;
        private const int ResizeBorder = 8;
        private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public int dwFlags;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_GETMINMAXINFO)
            {
                base.WndProc(ref m);

                IntPtr hMonitor = MonitorFromWindow(m.HWnd, MONITOR_DEFAULTTONEAREST);
                if (hMonitor != IntPtr.Zero)
                {
                    MONITORINFO mi = new MONITORINFO();
                    mi.cbSize = Marshal.SizeOf(typeof(MONITORINFO));
                    if (GetMonitorInfo(hMonitor, ref mi))
                    {
                        MINMAXINFO mmi = (MINMAXINFO)Marshal.PtrToStructure(m.LParam, typeof(MINMAXINFO))!;

                        int workLeft = mi.rcWork.Left;
                        int workTop = mi.rcWork.Top;
                        int workWidth = mi.rcWork.Right - mi.rcWork.Left;
                        int workHeight = mi.rcWork.Bottom - mi.rcWork.Top;

                        mmi.ptMaxPosition.X = workLeft - mi.rcMonitor.Left;
                        mmi.ptMaxPosition.Y = workTop - mi.rcMonitor.Top;
                        mmi.ptMaxSize.X = workWidth;
                        mmi.ptMaxSize.Y = workHeight;
                        mmi.ptMaxTrackSize.X = workWidth;
                        mmi.ptMaxTrackSize.Y = workHeight;

                        mmi.ptMinTrackSize.X = MinimumSize.Width;
                        mmi.ptMinTrackSize.Y = MinimumSize.Height;

                        Marshal.StructureToPtr(mmi, m.LParam, true);
                    }
                }
                m.Result = IntPtr.Zero;
                return;
            }

            if (m.Msg == WM_NCCALCSIZE && m.WParam != IntPtr.Zero)
            {
                // Suppress default Windows title bar chrome while allowing DWM to provide sizing borders and Aero-snap
                m.Result = IntPtr.Zero;
                return;
            }

            if (m.Msg == WM_NCHITTEST && WindowState != FormWindowState.Maximized)
            {
                base.WndProc(ref m);
                Point pt = PointToClient(new Point(m.LParam.ToInt32()));
                if (pt.X <= ResizeBorder && pt.Y <= ResizeBorder) { m.Result = (IntPtr)HTTOPLEFT; return; }
                if (pt.X >= ClientSize.Width - ResizeBorder && pt.Y <= ResizeBorder) { m.Result = (IntPtr)HTTOPRIGHT; return; }
                if (pt.X <= ResizeBorder && pt.Y >= ClientSize.Height - ResizeBorder) { m.Result = (IntPtr)HTBOTTOMLEFT; return; }
                if (pt.X >= ClientSize.Width - ResizeBorder && pt.Y >= ClientSize.Height - ResizeBorder) { m.Result = (IntPtr)HTBOTTOMRIGHT; return; }
                if (pt.X <= ResizeBorder) { m.Result = (IntPtr)HTLEFT; return; }
                if (pt.X >= ClientSize.Width - ResizeBorder) { m.Result = (IntPtr)HTRIGHT; return; }
                if (pt.Y <= ResizeBorder) { m.Result = (IntPtr)HTTOP; return; }
                if (pt.Y >= ClientSize.Height - ResizeBorder) { m.Result = (IntPtr)HTBOTTOM; return; }
                return;
            }

            base.WndProc(ref m);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !IsExiting)
            {
                e.Cancel = true;
                if (_webView?.CoreWebView2 != null)
                {
                    _webView.CoreWebView2.ExecuteScriptAsync("window.supApp && window.supApp.promptCloseAction && window.supApp.promptCloseAction();");
                }
                else
                {
                    Hide();
                }
                AuditLogger.Log("Host", "Window Close Prompted", "User prompted for tray minimization vs termination");
                return;
            }

            base.OnFormClosing(e);
        }
    }
}
