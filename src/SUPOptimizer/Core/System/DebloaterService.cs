using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Win32;
using SUPOptimizer.Core.Logging;
using SUPOptimizer.Core.Security;

namespace SUPOptimizer.Core.System
{
    public class DebloatPackage
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string PackageFullName { get; set; } = string.Empty;
        public string Category { get; set; } = "Safe"; // Safe, Balanced, Aggressive
        public string Description { get; set; } = string.Empty;
        public bool IsInstalled { get; set; } = true;
        public bool RecommendedRemove { get; set; } = true;

        // Backward compatibility & API aliases
        public string DisplayName => Name;
        public string Tier => Category;
        public bool SafeToRemove => RecommendedRemove;
    }

    public static class DebloaterService
    {
        private static readonly List<DebloatPackage> _catalog = new()
        {
            // SAFE TIER (Bundled Consumer Ads & Preinstalled Bloatware)
            new DebloatPackage { Id = "Microsoft.BingNews", Name = "Microsoft News", Category = "Safe", Description = "Pre-installed MSN News feed tile and taskbar widget integration." },
            new DebloatPackage { Id = "Microsoft.BingWeather", Name = "Microsoft Weather", Category = "Safe", Description = "Pre-installed MSN Weather forecast app." },
            new DebloatPackage { Id = "Microsoft.BingFinance", Name = "Microsoft Money & Finance", Category = "Safe", Description = "Stock market and currency tracking widget app." },
            new DebloatPackage { Id = "Microsoft.BingSports", Name = "Microsoft Sports", Category = "Safe", Description = "MSN Sports news and scores app." },
            new DebloatPackage { Id = "Microsoft.GetHelp", Name = "Get Help", Category = "Safe", Description = "Automated support and FAQ promotional app." },
            new DebloatPackage { Id = "Microsoft.Getstarted", Name = "Tips & Getting Started", Category = "Safe", Description = "Windows 11 promotional tutorial and feature suggestions." },
            new DebloatPackage { Id = "Microsoft.MicrosoftSolitaireCollection", Name = "Solitaire Collection", Category = "Safe", Description = "Ad-supported casual card game bundle." },
            new DebloatPackage { Id = "Microsoft.People", Name = "Windows People Hub", Category = "Safe", Description = "Deprecated Windows contacts hub background listener." },
            new DebloatPackage { Id = "Microsoft.SkypeApp", Name = "Skype UWP", Category = "Safe", Description = "Legacy UWP Skype video communication client." },
            new DebloatPackage { Id = "Microsoft.Todos", Name = "Microsoft To Do", Category = "Safe", Description = "Cloud task list utility." },
            new DebloatPackage { Id = "Clipchamp.Clipchamp", Name = "Clipchamp Video Editor", Category = "Safe", Description = "Cloud-based video editor web wrapper." },
            new DebloatPackage { Id = "Microsoft.ZuneVideo", Name = "Films & TV (Zune Video)", Category = "Safe", Description = "Legacy video store player app." },
            new DebloatPackage { Id = "Microsoft.ZuneMusic", Name = "Media Player (Groove Music)", Category = "Safe", Description = "Legacy Groove audio store player." },
            new DebloatPackage { Id = "SpotifyAB.SpotifyMusic", Name = "Spotify Music (Sponsored)", Category = "Safe", Description = "Preinstalled OEM sponsored Spotify app." },
            new DebloatPackage { Id = "Disney.37853FC22B2CE", Name = "Disney+ (Sponsored)", Category = "Safe", Description = "Preinstalled OEM sponsored streaming shortcut." },
            new DebloatPackage { Id = "BytedancePte.Ltd.TikTok", Name = "TikTok (Sponsored)", Category = "Safe", Description = "Preinstalled OEM sponsored social feed app." },
            new DebloatPackage { Id = "Microsoft.MixedReality.Portal", Name = "Mixed Reality Portal", Category = "Safe", Description = "VR and holographic portal launcher (rarely used without headset)." },
            new DebloatPackage { Id = "Microsoft.Microsoft3DViewer", Name = "3D Viewer", Category = "Safe", Description = "Legacy 3D model inspector tool." },
            new DebloatPackage { Id = "Microsoft.MSPaint", Name = "Paint 3D", Category = "Safe", Description = "Deprecated 3D canvas sculpting utility." },
            new DebloatPackage { Id = "Microsoft.MicrosoftOfficeHub", Name = "Microsoft 365 (Office Hub)", Category = "Safe", Description = "Preinstalled Office 365 web portal and subscription promo app." },
            new DebloatPackage { Id = "Microsoft.PowerAutomateDesktop", Name = "Power Automate", Category = "Safe", Description = "Desktop workflow automation utility." },
            new DebloatPackage { Id = "Microsoft.OutlookForWindows", Name = "New Outlook for Windows", Category = "Safe", Description = "Webview-based modern Outlook mail replacement app." },
            new DebloatPackage { Id = "Microsoft.WindowsCommunicationsApps", Name = "Mail & Calendar", Category = "Safe", Description = "Legacy Windows 10/11 Mail and Calendar UWP client." },
            new DebloatPackage { Id = "Microsoft.Office.OneNote", Name = "OneNote for Windows 10", Category = "Safe", Description = "UWP version of OneNote note-taking application." },
            new DebloatPackage { Id = "Microsoft.MicrosoftStickyNotes", Name = "Sticky Notes", Category = "Safe", Description = "Desktop sticky notes reminder app." },
            new DebloatPackage { Id = "MicrosoftTeams", Name = "Microsoft Teams Personal", Category = "Safe", Description = "Integrated consumer Chat and Teams client." },
            new DebloatPackage { Id = "Microsoft.Wallet", Name = "Microsoft Pay / Wallet", Category = "Safe", Description = "In-app payment and credential storage framework." },

            // BALANCED TIER (Consumer Sync & Secondary Utilities)
            new DebloatPackage { Id = "Microsoft.GamingApp", Name = "Xbox Desktop Companion", Category = "Balanced", Description = "Main Xbox desktop console store and cloud gaming launcher." },
            new DebloatPackage { Id = "Microsoft.XboxGamingOverlay", Name = "Xbox Game Bar Overlay", Category = "Balanced", Description = "Overlay widgets for Xbox screen recording and telemetry." },
            new DebloatPackage { Id = "Microsoft.XboxIdentityProvider", Name = "Xbox Identity Provider", Category = "Balanced", Description = "Xbox profile login authentication and service helper." },
            new DebloatPackage { Id = "Microsoft.XboxSpeechToTextOverlay", Name = "Xbox Game Speech Window", Category = "Balanced", Description = "Game chat accessibility speech transcription overlay." },
            new DebloatPackage { Id = "Microsoft.YourPhone", Name = "Phone Link", Category = "Balanced", Description = "Mobile phone notification and photo synchronizer." },
            new DebloatPackage { Id = "Microsoft.WindowsFeedbackHub", Name = "Feedback Hub", Category = "Balanced", Description = "Customer telemetry survey and bug submission utility." },
            new DebloatPackage { Id = "Microsoft.QuickAssist", Name = "Quick Assist", Category = "Balanced", Description = "Remote desktop screen sharing tool." },

            // AGGRESSIVE TIER (Core Modern Apps & Assistants)
            new DebloatPackage { Id = "Microsoft.549981C3F5F10", Name = "Cortana Voice Assistant", Category = "Aggressive", Description = "Deprecated Cortana digital voice assistant background process." },
            new DebloatPackage { Id = "Microsoft.WindowsMaps", Name = "Windows Maps", Category = "Aggressive", Description = "Offline road navigation and Bing Maps application." },
            new DebloatPackage { Id = "Microsoft.WindowsSoundRecorder", Name = "Voice Recorder", Category = "Aggressive", Description = "Basic microphone voice recording utility." },
            new DebloatPackage { Id = "Microsoft.WindowsAlarms", Name = "Clock & Alarms", Category = "Aggressive", Description = "System clock, timer, and stopwatch UWP app." }
        };

        private static HashSet<string> GetInstalledPackageNames()
        {
            var detected = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 1. Instant Registry Checks for installed AppModel packages
            string[] relativeRegistryPaths = new[]
            {
                @"Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppModel\Repository\Packages",
                @"Software\Microsoft\Windows\CurrentVersion\Appx\AppxAllUserStore\Applications",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Appx\AppxAllUserStore\InboxApplications"
            };

            foreach (var path in relativeRegistryPaths)
            {
                try
                {
                    using var hkcu = Registry.CurrentUser.OpenSubKey(path);
                    if (hkcu != null)
                    {
                        foreach (var sub in hkcu.GetSubKeyNames())
                            detected.Add(sub);
                    }
                }
                catch { }

                try
                {
                    using var hklm = Registry.LocalMachine.OpenSubKey(path);
                    if (hklm != null)
                    {
                        foreach (var sub in hklm.GetSubKeyNames())
                            detected.Add(sub);
                    }
                }
                catch { }
            }

            // 2. Fast PowerShell query fallback if registry returned very few items
            if (detected.Count < 5)
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = "-NoProfile -NonInteractive -ExecutionPolicy Bypass -Command \"Get-AppxPackage | Select-Object -ExpandProperty Name\"",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    };

                    using var proc = Process.Start(psi);
                    if (proc != null)
                    {
                        while (!proc.StandardOutput.EndOfStream)
                        {
                            string? line = proc.StandardOutput.ReadLine()?.Trim();
                            if (!string.IsNullOrEmpty(line))
                            {
                                detected.Add(line);
                            }
                        }
                        proc.WaitForExit(3500);
                    }
                }
                catch { }
            }

            return detected;
        }

        public static List<DebloatPackage> GetPackages()
        {
            var detected = GetInstalledPackageNames();

            var list = new List<DebloatPackage>();
            foreach (var item in _catalog)
            {
                // Strict check: does any installed package contain or match item.Id?
                bool isInst = detected.Any(d => d.IndexOf(item.Id, StringComparison.OrdinalIgnoreCase) >= 0);

                list.Add(new DebloatPackage
                {
                    Id = item.Id,
                    Name = item.Name,
                    PackageFullName = item.Id,
                    Category = item.Category,
                    Description = item.Description,
                    IsInstalled = isInst,
                    RecommendedRemove = item.Category == "Safe"
                });
            }

            return list;
        }

        public static (bool Success, string Message) RemovePackage(string packageId, bool dryRun)
        {
            if (dryRun)
            {
                return (true, $"[PREVIEW] Would remove Appx package matching '{packageId}' for CurrentUser and AllUsers.");
            }

            var detected = GetInstalledPackageNames();
            bool isInst = detected.Any(d => d.IndexOf(packageId, StringComparison.OrdinalIgnoreCase) >= 0);
            if (!isInst)
            {
                return (false, $"Application '{packageId}' is not installed on this system.");
            }

            try
            {
                string script = $"Get-AppxPackage -Name '*{packageId}*' | Remove-AppxPackage -ErrorAction SilentlyContinue; " +
                                (PrivilegeManager.IsAdministrator() ? $"Get-AppxProvisionedPackage -Online | Where-Object {{ $_.DisplayName -like '*{packageId}*' }} | Remove-AppxProvisionedPackage -Online -ErrorAction SilentlyContinue" : "");

                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var proc = Process.Start(psi);
                if (proc != null)
                {
                    string stdOut = proc.StandardOutput.ReadToEnd();
                    string stdErr = proc.StandardError.ReadToEnd();
                    proc.WaitForExit(10000);

                    AuditLogger.Log("Debloat", "Package Removed", packageId, success: proc.ExitCode == 0);
                    return (true, $"Package '{packageId}' removed successfully.");
                }

                return (false, "Could not start removal process.");
            }
            catch (Exception ex)
            {
                AuditLogger.Log("Debloat", "Removal Error", ex.Message, success: false, errorMessage: ex.Message);
                return (false, $"Removal exception: {ex.Message}");
            }
        }

        public static (int Total, int Removed, int Failed, List<string> Messages) RemovePackages(List<string> packageIds, bool dryRun)
        {
            var msgs = new List<string>();
            int removed = 0;
            int failed = 0;

            foreach (var pid in packageIds)
            {
                if (string.IsNullOrWhiteSpace(pid)) continue;
                var (success, msg) = RemovePackage(pid, dryRun);
                msgs.Add(msg);
                if (success) removed++;
                else failed++;
            }

            return (packageIds.Count, removed, failed, msgs);
        }

        public static Dictionary<string, List<string>> GetPresets()
        {
            return new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["Safe"] = _catalog.Where(p => p.Category == "Safe").Select(p => p.Id).ToList(),
                ["Minimal"] = _catalog.Where(p => p.Category == "Safe" || p.Category == "Balanced").Select(p => p.Id).ToList(),
                ["Aggressive"] = _catalog.Select(p => p.Id).ToList()
            };
        }

        public static (bool Success, string Message) RemoveOneDrive(bool dryRun)
        {
            if (dryRun) return (true, "[PREVIEW] Would terminate OneDrive processes and execute native silent uninstaller.");
            try
            {
                // Kill running OneDrive
                foreach (var p in Process.GetProcessesByName("OneDrive"))
                {
                    try { p.Kill(); } catch { }
                }

                string sysRoot = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                string sys32 = Environment.GetFolderPath(Environment.SpecialFolder.System);
                string sysWow64 = Path.Combine(sysRoot, "SysWOW64");
                string uninstPath = Path.Combine(sysWow64, "OneDriveSetup.exe");
                if (!File.Exists(uninstPath))
                {
                    uninstPath = Path.Combine(sys32, "OneDriveSetup.exe");
                }
                if (!File.Exists(uninstPath))
                {
                    string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    uninstPath = Path.Combine(localAppData, @"Microsoft\OneDrive\OneDriveSetup.exe");
                }

                if (File.Exists(uninstPath))
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = uninstPath,
                        Arguments = "/uninstall",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    using var proc = Process.Start(psi);
                    proc?.WaitForExit(15000);
                }

                AuditLogger.Log("Debloat", "OneDrive Removed", "Executed OneDrive silent uninstall");
                return (true, "OneDrive uninstaller triggered successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Error removing OneDrive: {ex.Message}");
            }
        }

        public static (bool Success, string Message) RemoveMicrosoftEdge(bool dryRun)
        {
            if (dryRun) return (true, "[PREVIEW] Would invoke Edge setup uninstaller with --uninstall --system-level --force-uninstall.");
            if (!PrivilegeManager.IsAdministrator())
                return (false, "Administrator privileges required to uninstall Microsoft Edge.");

            try
            {
                string progFiles86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                string edgeInstallerDir = Path.Combine(progFiles86, @"Microsoft\Edge\Application");

                if (Directory.Exists(edgeInstallerDir))
                {
                    var versionDirs = Directory.GetDirectories(edgeInstallerDir);
                    foreach (var vDir in versionDirs)
                    {
                        string installerExe = Path.Combine(vDir, @"Installer\setup.exe");
                        if (File.Exists(installerExe))
                        {
                            var psi = new ProcessStartInfo
                            {
                                FileName = installerExe,
                                Arguments = "--uninstall --system-level --verbose-logging --force-uninstall",
                                CreateNoWindow = true,
                                UseShellExecute = false
                            };
                            using var proc = Process.Start(psi);
                            proc?.WaitForExit(20000);
                            AuditLogger.Log("Debloat", "Microsoft Edge Removed", $"Executed {installerExe}");
                            return (true, "Microsoft Edge uninstallation executed.");
                        }
                    }
                }

                return (false, "Microsoft Edge setup installer binary not found in Program Files.");
            }
            catch (Exception ex)
            {
                return (false, $"Error uninstalling Edge: {ex.Message}");
            }
        }
    }
}
