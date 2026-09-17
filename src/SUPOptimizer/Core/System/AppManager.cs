using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using SUPOptimizer.Core.Logging;

namespace SUPOptimizer.Core.System
{
    public class InstalledApp
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string DisplayVersion { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public string InstallDate { get; set; } = string.Empty;
        public string InstallLocation { get; set; } = string.Empty;
        public long EstimatedSizeMb { get; set; }
        public string UninstallString { get; set; } = string.Empty;
    }

    public static class AppManager
    {
        public static List<InstalledApp> GetInstalledApps()
        {
            var apps = new Dictionary<string, InstalledApp>(StringComparer.OrdinalIgnoreCase);

            ReadUninstallKey(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", apps);
            ReadUninstallKey(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall", apps);
            ReadUninstallKey(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Uninstall", apps);

            return apps.Values.OrderBy(a => a.DisplayName).ToList();
        }

        private static void ReadUninstallKey(RegistryKey rootKey, string subKeyPath, Dictionary<string, InstalledApp> dict)
        {
            try
            {
                using var key = rootKey.OpenSubKey(subKeyPath, false);
                if (key == null) return;

                foreach (var subKeyName in key.GetSubKeyNames())
                {
                    try
                    {
                        using var appKey = key.OpenSubKey(subKeyName, false);
                        if (appKey == null) continue;

                        string displayName = appKey.GetValue("DisplayName")?.ToString() ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(displayName)) continue;

                        // Skip system updates or hidden system components
                        object? sysComp = appKey.GetValue("SystemComponent");
                        if (sysComp is int sc && sc == 1) continue;

                        object? parentKey = appKey.GetValue("ParentKeyName");
                        if (parentKey != null) continue;

                        string version = appKey.GetValue("DisplayVersion")?.ToString() ?? string.Empty;
                        string publisher = appKey.GetValue("Publisher")?.ToString() ?? string.Empty;
                        string installDate = appKey.GetValue("InstallDate")?.ToString() ?? string.Empty;
                        string installLocation = appKey.GetValue("InstallLocation")?.ToString() ?? string.Empty;
                        string uninstallString = appKey.GetValue("UninstallString")?.ToString() ?? string.Empty;

                        long sizeMb = 0;
                        object? sizeVal = appKey.GetValue("EstimatedSize");
                        if (sizeVal is int kb)
                        {
                            sizeMb = kb / 1024;
                        }

                        var app = new InstalledApp
                        {
                            Id = subKeyName,
                            DisplayName = displayName,
                            DisplayVersion = version,
                            Publisher = publisher,
                            InstallDate = installDate,
                            InstallLocation = installLocation,
                            EstimatedSizeMb = sizeMb,
                            UninstallString = uninstallString
                        };

                        if (!dict.ContainsKey(displayName))
                        {
                            dict[displayName] = app;
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        public static (bool Success, string Message) UninstallApp(string uninstallString)
        {
            if (string.IsNullOrWhiteSpace(uninstallString))
                return (false, "No valid uninstaller string found for this application.");

            try
            {
                // Parse executable and arguments
                string trimmed = uninstallString.Trim();
                string fileName;
                string args = string.Empty;

                if (trimmed.StartsWith("\""))
                {
                    int endQuote = trimmed.IndexOf('\"', 1);
                    if (endQuote > 1)
                    {
                        fileName = trimmed.Substring(1, endQuote - 1);
                        args = trimmed.Substring(endQuote + 1).Trim();
                    }
                    else
                    {
                        fileName = trimmed.Trim('\"');
                    }
                }
                else
                {
                    int firstSpace = trimmed.IndexOf(' ');
                    if (firstSpace > 0)
                    {
                        fileName = trimmed.Substring(0, firstSpace);
                        args = trimmed.Substring(firstSpace + 1).Trim();
                    }
                    else
                    {
                        fileName = trimmed;
                    }
                }

                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = args,
                    UseShellExecute = true
                };

                Process.Start(psi);
                AuditLogger.Log("Apps", "Launched Uninstaller", fileName);
                return (true, "Uninstaller launched successfully.");
            }
            catch (Exception ex)
            {
                AuditLogger.Log("Apps", "Uninstall Failed", ex.Message, success: false, errorMessage: ex.Message);
                return (false, $"Failed to launch uninstaller: {ex.Message}");
            }
        }

        public static (bool Success, string Message) OpenLocation(string location)
        {
            if (string.IsNullOrWhiteSpace(location) || !Directory.Exists(location))
                return (false, "Installation location does not exist or is inaccessible.");

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"\"{location}\"",
                    UseShellExecute = true
                });
                return (true, "Opened folder in Explorer.");
            }
            catch (Exception ex)
            {
                return (false, $"Could not open folder: {ex.Message}");
            }
        }
    }
}
