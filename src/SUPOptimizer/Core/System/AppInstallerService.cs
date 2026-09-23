using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SUPOptimizer.Core.Logging;

namespace SUPOptimizer.Core.System
{
    public class CatalogApp
    {
        public string Id { get; set; } = string.Empty;           // Winget Package ID
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsInstalled { get; set; }
    }

    public class UpgradableApp
    {
        public string Name { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string InstalledVersion { get; set; } = string.Empty;
        public string AvailableVersion { get; set; } = string.Empty;
    }

    public static class AppInstallerService
    {
        private static readonly List<CatalogApp> _catalog = new()
        {
            // Browsers
            new CatalogApp { Id = "Google.Chrome", Name = "Google Chrome", Category = "Browsers", Description = "Fast, secure web browser built by Google." },
            new CatalogApp { Id = "Mozilla.Firefox", Name = "Mozilla Firefox", Category = "Browsers", Description = "Privacy-focused, independent open-source web browser." },
            new CatalogApp { Id = "BraveSoftware.BraveBrowser", Name = "Brave Browser", Category = "Browsers", Description = "Chromium-based browser with built-in ad and tracker blocking." },

            // Compression
            new CatalogApp { Id = "7zip.7zip", Name = "7-Zip", Category = "Compression", Description = "High-ratio open source file archiver with 7z/ZIP/RAR support." },
            new CatalogApp { Id = "Giorgiotani.PeaZip", Name = "PeaZip", Category = "Compression", Description = "Free archiver and file manager supporting over 200 archive formats." },

            // Utilities
            new CatalogApp { Id = "Microsoft.PowerToys", Name = "Microsoft PowerToys", Category = "Utilities", Description = "Set of system utilities for power users (FancyZones, ColorPicker, Run)." },
            new CatalogApp { Id = "voidtools.Everything", Name = "Everything Search", Category = "Utilities", Description = "Instantaneous filename search engine for Windows NTFS drives." },
            new CatalogApp { Id = "ShareX.ShareX", Name = "ShareX", Category = "Utilities", Description = "Screen capture, file sharing and productivity tool." },
            new CatalogApp { Id = "Notepad++.Notepad++", Name = "Notepad++", Category = "Utilities", Description = "Powerful source code and text editor supporting syntax highlighting." },
            new CatalogApp { Id = "SumatraPDF.SumatraPDF", Name = "Sumatra PDF", Category = "Utilities", Description = "Slim, ultra-fast PDF, eBook, and comic reader." },
            new CatalogApp { Id = "WinSCP.WinSCP", Name = "WinSCP", Category = "Utilities", Description = "Free SFTP, FTP, S3, and SCP client for Windows." },

            // Development
            new CatalogApp { Id = "Microsoft.VisualStudioCode", Name = "Visual Studio Code", Category = "Development", Description = "Code editor redefined and optimized for building modern web and cloud apps." },
            new CatalogApp { Id = "Git.Git", Name = "Git for Windows", Category = "Development", Description = "Distributed version control system command-line and GUI." },
            new CatalogApp { Id = "Python.Python.3.12", Name = "Python 3.12", Category = "Development", Description = "Interpreted, high-level, general-purpose programming language." },
            new CatalogApp { Id = "OpenJS.NodeJS.LTS", Name = "Node.js (LTS)", Category = "Development", Description = "JavaScript runtime environment for server and tooling applications." },
            new CatalogApp { Id = "Microsoft.WindowsTerminal", Name = "Windows Terminal", Category = "Development", Description = "Modern, fast, and powerful terminal emulator from Microsoft." },

            // Communication
            new CatalogApp { Id = "Discord.Discord", Name = "Discord", Category = "Communication", Description = "Voice, video, and text communication service for gaming communities." },
            new CatalogApp { Id = "Telegram.TelegramDesktop", Name = "Telegram Desktop", Category = "Communication", Description = "Fast and secure cloud-based messaging desktop app." },
            new CatalogApp { Id = "Zoom.Zoom", Name = "Zoom Workplace", Category = "Communication", Description = "Video conferencing and virtual meeting client." },

            // Media
            new CatalogApp { Id = "VideoLAN.VLC", Name = "VLC Media Player", Category = "Media", Description = "Universal multimedia player that plays most codecs out of the box." },
            new CatalogApp { Id = "OBSProject.OBSStudio", Name = "OBS Studio", Category = "Media", Description = "Open source software for video recording and live broadcasting." },
            new CatalogApp { Id = "Audacity.Audacity", Name = "Audacity", Category = "Media", Description = "Multi-track audio editor and recorder." },

            // Gaming
            new CatalogApp { Id = "Valve.Steam", Name = "Steam", Category = "Gaming", Description = "Digital distribution platform for PC games and multiplayer community." },
            new CatalogApp { Id = "EpicGames.EpicGamesLauncher", Name = "Epic Games Launcher", Category = "Gaming", Description = "Launcher for Unreal Engine, Fortnite, and weekly free PC titles." },
            new CatalogApp { Id = "GOG.Galaxy", Name = "GOG Galaxy", Category = "Gaming", Description = "DRM-free gaming client connecting gaming platforms in one place." }
        };

        public static List<CatalogApp> GetCatalog()
        {
            // Update install status
            var installedApps = AppManager.GetInstalledApps();

            foreach (var item in _catalog)
            {
                item.IsInstalled = installedApps.Any(a =>
                    a.DisplayName.Contains(item.Name, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(a.Publisher) && item.Name.Contains(a.Publisher, StringComparison.OrdinalIgnoreCase)));
            }

            return _catalog;
        }

        public static (bool Success, string Message) InstallApp(string packageId)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "winget.exe",
                    Arguments = $"install --id \"{packageId}\" -e --silent --accept-source-agreements --accept-package-agreements",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var proc = Process.Start(psi);
                if (proc == null)
                    return (false, "Could not start winget process.");

                proc.WaitForExit(180000); // 3-minute timeout

                if (proc.ExitCode == 0)
                {
                    AuditLogger.Log("Installer", "Installed Application", packageId);
                    return (true, $"Application '{packageId}' installed successfully.");
                }
                else
                {
                    string err = proc.StandardError.ReadToEnd();
                    string outMsg = proc.StandardOutput.ReadToEnd();
                    string combined = string.IsNullOrEmpty(err) ? outMsg : err;
                    AuditLogger.Log("Installer", "Install Failed", $"{packageId}: {combined}", success: false, errorMessage: combined);
                    return (false, $"Installer exited with code {proc.ExitCode}: {combined}");
                }
            }
            catch (Exception ex)
            {
                AuditLogger.Log("Installer", "Install Error", ex.Message, success: false, errorMessage: ex.Message);
                return (false, $"Installation error: {ex.Message}");
            }
        }

        public static List<UpgradableApp> GetUpgradableApps()
        {
            var list = new List<UpgradableApp>();
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "winget.exe",
                    Arguments = "upgrade --include-unknown",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var proc = Process.Start(psi);
                if (proc == null) return list;

                string output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit(45000);

                var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                bool tableStarted = false;
                foreach (var line in lines)
                {
                    if (line.StartsWith("---") || line.Contains("---"))
                    {
                        tableStarted = true;
                        continue;
                    }
                    if (!tableStarted) continue;
                    if (line.Contains("upgrades available", StringComparison.OrdinalIgnoreCase) || 
                        line.Contains("aggiornamenti disponibili", StringComparison.OrdinalIgnoreCase)) 
                        break;

                    var parts = Regex.Split(line.Trim(), @"\s{2,}");
                    if (parts.Length >= 4)
                    {
                        list.Add(new UpgradableApp
                        {
                            Name = parts[0].Trim(),
                            Id = parts[1].Trim(),
                            InstalledVersion = parts[2].Trim(),
                            AvailableVersion = parts[3].Trim()
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                AuditLogger.Log("Installer", "Check Upgrades Error", ex.Message, success: false, errorMessage: ex.Message);
            }
            return list;
        }

        public static (bool Success, string Message) UpgradeApp(string packageId)
        {
            try
            {
                string args = string.Equals(packageId, "all", StringComparison.OrdinalIgnoreCase)
                    ? "upgrade --all --silent --accept-source-agreements --accept-package-agreements"
                    : $"upgrade --id \"{packageId}\" -e --silent --accept-source-agreements --accept-package-agreements";

                var psi = new ProcessStartInfo
                {
                    FileName = "winget.exe",
                    Arguments = args,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var proc = Process.Start(psi);
                if (proc == null)
                    return (false, "Could not start winget process.");

                proc.WaitForExit(300000);

                if (proc.ExitCode == 0)
                {
                    AuditLogger.Log("Installer", "Upgraded Package", packageId);
                    return (true, $"Package '{packageId}' upgraded successfully.");
                }
                else
                {
                    string outMsg = proc.StandardOutput.ReadToEnd();
                    string err = proc.StandardError.ReadToEnd();
                    string combined = string.IsNullOrEmpty(err) ? outMsg : err;
                    AuditLogger.Log("Installer", "Upgrade Result", $"{packageId} exited with {proc.ExitCode}: {combined}");
                    return (true, $"Upgrade operation completed (Code {proc.ExitCode}).");
                }
            }
            catch (Exception ex)
            {
                AuditLogger.Log("Installer", "Upgrade Error", ex.Message, success: false, errorMessage: ex.Message);
                return (false, $"Upgrade failed: {ex.Message}");
            }
        }
    }
}
