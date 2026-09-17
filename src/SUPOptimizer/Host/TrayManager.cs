using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using SUPOptimizer.Core.System;

namespace SUPOptimizer.Host
{
    public class TrayManager : IDisposable
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly ContextMenuStrip _contextMenu;
        private readonly Action _onOpen;
        private readonly Action _onExit;
        private readonly Action<string> _onNavigate;
        private readonly string _serverUrl;

        public TrayManager(Action onOpen, Action onExit, Action<string> onNavigate, string serverUrl)
        {
            _onOpen = onOpen;
            _onExit = onExit;
            _onNavigate = onNavigate;
            _serverUrl = serverUrl;

            _contextMenu = new ContextMenuStrip();
            _contextMenu.RenderMode = ToolStripRenderMode.System;

            // 1. Open SUPOptimizer (Native Host)
            var openItem = new ToolStripMenuItem("Open SUPOptimizer", null, (s, e) => _onOpen());
            openItem.Font = new Font(openItem.Font, FontStyle.Bold);
            _contextMenu.Items.Add(openItem);

            // 2. Open in Web Browser (Chrome / Edge / Default Browser)
            var webItem = new ToolStripMenuItem("Open in Web Browser (Chrome / Edge)", null, (s, e) =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = _serverUrl,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to open web browser: {ex.Message}", "SUPOptimizer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });
            _contextMenu.Items.Add(webItem);

            _contextMenu.Items.Add(new ToolStripSeparator());

            // 3. Navigation shortcuts
            _contextMenu.Items.Add(new ToolStripMenuItem("Dashboard", null, (s, e) => { _onOpen(); _onNavigate("dashboard"); }));

            // 4. Quick Optimize
            _contextMenu.Items.Add(new ToolStripMenuItem("Quick Optimize (Safe Profile)", null, (s, e) =>
            {
                var res = AutomationService.ApplyProfile("safe_optimize", false);
                ShowBalloon("Quick Optimize", $"Safe optimization complete: {res.Succeeded} tweaks applied.");
            }));

            // 5. System Health Scan
            _contextMenu.Items.Add(new ToolStripMenuItem("System Health Scan", null, (s, e) => { _onOpen(); _onNavigate("health"); }));

            _contextMenu.Items.Add(new ToolStripSeparator());

            // 6. Settings
            _contextMenu.Items.Add(new ToolStripMenuItem("Settings & Backups", null, (s, e) => { _onOpen(); _onNavigate("settings"); }));

            // 7. About
            _contextMenu.Items.Add(new ToolStripMenuItem("About SUPOptimizer", null, (s, e) =>
            {
                MessageBox.Show(
                    "SUPOptimizer v1.0.0 Portable\n\nModern Windows Native System Optimization & Maintenance Platform.\nSelf-contained portable single executable.\n\nLocal Web UI: " + _serverUrl,
                    "About SUPOptimizer",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }));

            // 8. Restart Application
            _contextMenu.Items.Add(new ToolStripMenuItem("Restart Application", null, (s, e) =>
            {
                Process.Start(Environment.ProcessPath ?? Application.ExecutablePath);
                _onExit();
            }));

            _contextMenu.Items.Add(new ToolStripSeparator());

            // 9. Exit
            var closeItem = new ToolStripMenuItem("Exit SUPOptimizer", null, (s, e) => _onExit());
            _contextMenu.Items.Add(closeItem);

            _notifyIcon = new NotifyIcon
            {
                Text = "SUPOptimizer - Active",
                Icon = NativeMethods.CreateSUPIcon(),
                ContextMenuStrip = _contextMenu,
                Visible = true
            };

            _notifyIcon.Click += (s, e) =>
            {
                if (e is MouseEventArgs me && me.Button == MouseButtons.Left)
                {
                    _onOpen();
                }
            };

            _notifyIcon.DoubleClick += (s, e) => _onOpen();
        }

        public void ShowBalloon(string title, string text, ToolTipIcon icon = ToolTipIcon.Info)
        {
            _notifyIcon.ShowBalloonTip(3000, title, text, icon);
        }

        public void Dispose()
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _contextMenu.Dispose();
        }
    }
}
