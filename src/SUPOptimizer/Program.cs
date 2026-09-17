using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using SUPOptimizer.Core.Backup;
using SUPOptimizer.Core.Logging;
using SUPOptimizer.Host;
using SUPOptimizer.Server;

namespace SUPOptimizer
{
    internal static class Program
    {
        private const string MutexName = @"Global\SUPOptimizer_SingleInstance_Mutex";
        private static Mutex? _singleInstanceMutex;

        [STAThread]
        static void Main(string[] args)
        {
            ApplicationConfiguration.Initialize();

            // 1. Single Instance Check via Global Mutex
            bool createdNew;
            try
            {
                _singleInstanceMutex = new Mutex(true, MutexName, out createdNew);
            }
            catch
            {
                createdNew = true;
            }

            if (!createdNew)
            {
                // Already running
                MessageBox.Show(
                    "Another instance of SUPOptimizer is already running in the system tray.",
                    "SUPOptimizer",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            // 2. Resolve Portable Data Directory
            string baseDataDir = ResolveDataDirectory();

            // 3. Initialize Core Subsystems
            AuditLogger.Initialize(baseDataDir);
            BackupManager.Initialize(baseDataDir);

            // 3.1 Silent Execution Support (CLI template automation)
            bool isSilent = false;
            foreach (var a in args)
            {
                if (a.Equals("--silent", StringComparison.OrdinalIgnoreCase) || a.Equals("-s", StringComparison.OrdinalIgnoreCase))
                {
                    isSilent = true;
                    break;
                }
            }

            if (isSilent)
            {
                HandleSilentMode(args);
                _singleInstanceMutex?.ReleaseMutex();
                _singleInstanceMutex?.Dispose();
                return;
            }

            // 4. Check port argument and start Local API Web Server
            int? preferredPort = null;
            for (int i = 0; i < args.Length; i++)
            {
                string a = args[i].Trim();
                if (a.Equals("--port", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length && int.TryParse(args[i + 1], out int p1))
                {
                    preferredPort = p1;
                }
                else if (a.StartsWith("--port=", StringComparison.OrdinalIgnoreCase) && int.TryParse(a.Substring(7), out int p2))
                {
                    preferredPort = p2;
                }
                else if (a.Contains("--port"))
                {
                    var parts = a.Split(new[] { ' ', '=' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < parts.Length; j++)
                    {
                        if (parts[j].Equals("--port", StringComparison.OrdinalIgnoreCase) && j + 1 < parts.Length && int.TryParse(parts[j + 1], out int p3))
                        {
                            preferredPort = p3;
                        }
                    }
                }
            }

            var apiServer = new LocalApiServer(preferredPort);
            try
            {
                apiServer.Start();
                File.WriteAllText(Path.Combine(baseDataDir, "port.txt"), apiServer.Port.ToString());
                File.WriteAllText(Path.Combine(baseDataDir, "url.txt"), apiServer.Url);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Critical error while starting local server: {ex.Message}",
                    "SUPOptimizer - Server Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            // 5. Handle startup flags
            bool startMinimized = false;
            foreach (var arg in args)
            {
                if (arg.IndexOf("minimized", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    arg.Equals("-m", StringComparison.OrdinalIgnoreCase))
                {
                    startMinimized = true;
                }
            }

            // 6. Initialize Main Window and System Tray
            var mainWindow = new MainWindow(apiServer)
            {
                IsMinimizedStartup = startMinimized
            };

            var trayManager = new TrayManager(
                onOpen: () => mainWindow.ShowAndActivate(),
                onExit: () =>
                {
                    mainWindow.IsExiting = true;
                    apiServer.Stop();
                    Application.Exit();
                },
                onNavigate: (tab) => mainWindow.NavigateTab(tab),
                serverUrl: apiServer.Url
            );

            var appCtx = new ApplicationContext();

            // Ensure window handle is created for message loop and WebView2
            var _ = mainWindow.Handle;

            // Connect Web API events to native host
            LocalApiServer.RequestMinimize += () =>
            {
                if (mainWindow.IsHandleCreated)
                    mainWindow.Invoke(new Action(() => mainWindow.Hide()));
            };

            LocalApiServer.RequestMaximize += () =>
            {
                if (mainWindow.IsHandleCreated)
                    mainWindow.Invoke(new Action(() => mainWindow.ToggleMaximize()));
            };

            LocalApiServer.RequestDrag += () =>
            {
                if (mainWindow.IsHandleCreated)
                    mainWindow.Invoke(new Action(() => mainWindow.DragWindow()));
            };

            LocalApiServer.RequestExit += () =>
            {
                mainWindow.IsExiting = true;
                apiServer.Stop();
                appCtx.ExitThread();
            };

            if (!startMinimized)
            {
                mainWindow.Show();
            }

            // 7. Run Native Application Loop with explicit ApplicationContext
            Application.Run(appCtx);

            // Cleanup
            trayManager.Dispose();
            _singleInstanceMutex?.ReleaseMutex();
            _singleInstanceMutex?.Dispose();
        }

        private static string ResolveDataDirectory()
        {
            try
            {
                string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                string localData = Path.Combine(exeDir, "data");

                // Test if exe directory is writable (e.g. portable USB drive)
                string testFile = Path.Combine(exeDir, $".perm_{Guid.NewGuid():N}.tmp");
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);

                if (!Directory.Exists(localData))
                    Directory.CreateDirectory(localData);

                return localData;
            }
            catch
            {
                // Fallback to LocalAppData if exe folder is read-only (e.g. Program Files)
                string appData = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "SUPOptimizer");

                if (!Directory.Exists(appData))
                    Directory.CreateDirectory(appData);

                return appData;
            }
        }

        private static void HandleSilentMode(string[] args)
        {
            AuditLogger.Log("CLI", "Silent Execution Started", string.Join(" ", args));

            string? preset = null;
            string? templatePath = null;

            foreach (var arg in args)
            {
                if (arg.StartsWith("--preset=", StringComparison.OrdinalIgnoreCase))
                    preset = arg.Substring(9).Trim('"');
                else if (arg.StartsWith("--apply-preset=", StringComparison.OrdinalIgnoreCase))
                    preset = arg.Substring(15).Trim('"');
                else if (arg.StartsWith("--template=", StringComparison.OrdinalIgnoreCase))
                    templatePath = arg.Substring(11).Trim('"');
            }

            if (!string.IsNullOrEmpty(templatePath) && File.Exists(templatePath))
            {
                try
                {
                    string json = File.ReadAllText(templatePath);
                    using var doc = System.Text.Json.JsonDocument.Parse(json);
                    var tweakIds = new System.Collections.Generic.List<string>();
                    if (doc.RootElement.TryGetProperty("tweakIds", out var idsEl))
                    {
                        foreach (var item in idsEl.EnumerateArray())
                        {
                            var s = item.GetString();
                            if (!string.IsNullOrEmpty(s)) tweakIds.Add(s);
                        }
                    }

                    if (tweakIds.Count > 0)
                    {
                        var res = SUPOptimizer.Core.Tweaks.TweakEngine.ApplyBatch(tweakIds, false);
                        AuditLogger.Log("CLI", "Silent Template Applied", $"Success: {res.Succeeded}, Failed: {res.Failed}");
                    }
                }
                catch (Exception ex)
                {
                    AuditLogger.Log("CLI", "Silent Template Error", ex.Message, success: false, errorMessage: ex.Message);
                }
            }
            else if (!string.IsNullOrEmpty(preset))
            {
                var res = SUPOptimizer.Core.System.AutomationService.ApplyProfile(preset, false);
                AuditLogger.Log("CLI", $"Silent Preset Applied [{preset}]", $"Success: {res.Succeeded}, Failed: {res.Failed}");
            }
            else
            {
                // Default safe preset
                var res = SUPOptimizer.Core.System.AutomationService.ApplyProfile("safe_optimize", false);
                AuditLogger.Log("CLI", "Silent Default Applied", $"Success: {res.Succeeded}, Failed: {res.Failed}");
            }
        }
    }
}
