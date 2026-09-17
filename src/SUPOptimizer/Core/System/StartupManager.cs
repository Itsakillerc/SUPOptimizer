using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using SUPOptimizer.Core.Logging;

namespace SUPOptimizer.Core.System
{
    public class StartupItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Command { get; set; } = string.Empty;
        public string Location { get; set; } = "HKCU Run";
        public bool IsEnabled { get; set; } = true;
        public string Publisher { get; set; } = "Unknown";
        public string Impact { get; set; } = "Normal";
    }

    public static class StartupManager
    {
        private const string ApprovedRunPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";
        private const string ApprovedFolderUserPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\StartupFolder";

        public static List<StartupItem> GetStartupItems()
        {
            var list = new List<StartupItem>();

            // 1. HKCU Run
            ReadRunKey(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Run", "HKCU Run", list);

            // 2. HKLM Run
            ReadRunKey(Registry.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\Run", "HKLM Run", list);

            // 3. User Startup folder
            try
            {
                string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                if (Directory.Exists(startupFolder))
                {
                    foreach (var file in Directory.GetFiles(startupFolder))
                    {
                        string fileName = Path.GetFileName(file);
                        if (string.Equals(fileName, "desktop.ini", StringComparison.OrdinalIgnoreCase))
                            continue;

                        bool isDisabledByName = file.EndsWith(".disabled", StringComparison.OrdinalIgnoreCase);
                        string cleanName = isDisabledByName ? Path.GetFileNameWithoutExtension(file) : Path.GetFileNameWithoutExtension(file);

                        // Check StartupApproved\StartupFolder
                        bool isDisabledByApproved = IsDisabledInStartupApproved(Registry.CurrentUser, ApprovedFolderUserPath, fileName);

                        bool isEnabled = !isDisabledByName && !isDisabledByApproved;

                        list.Add(new StartupItem
                        {
                            Id = $"Folder_{cleanName}",
                            Name = cleanName,
                            Command = file,
                            Location = "Startup Folder",
                            IsEnabled = isEnabled,
                            Publisher = "Local User",
                            Impact = "Normal"
                        });
                    }
                }
            }
            catch { }

            return list;
        }

        private static void ReadRunKey(RegistryKey rootKey, string path, string locationLabel, List<StartupItem> list)
        {
            try
            {
                using var key = rootKey.OpenSubKey(path, false);
                if (key != null)
                {
                    foreach (var valueName in key.GetValueNames())
                    {
                        var val = key.GetValue(valueName)?.ToString() ?? string.Empty;
                        bool isDisabledByName = valueName.EndsWith(".disabled", StringComparison.OrdinalIgnoreCase);
                        string cleanName = isDisabledByName ? valueName.Substring(0, valueName.Length - 9) : valueName;

                        // Check Windows Task Manager StartupApproved\Run
                        bool isDisabledByApproved = IsDisabledInStartupApproved(rootKey, ApprovedRunPath, cleanName);

                        bool isEnabled = !isDisabledByName && !isDisabledByApproved;

                        list.Add(new StartupItem
                        {
                            Id = $"{locationLabel}_{cleanName}",
                            Name = cleanName,
                            Command = val,
                            Location = locationLabel,
                            IsEnabled = isEnabled,
                            Publisher = InferPublisher(val),
                            Impact = InferImpact(val)
                        });
                    }
                }
            }
            catch { }
        }

        private static bool IsDisabledInStartupApproved(RegistryKey rootKey, string approvedSubKey, string itemName)
        {
            try
            {
                using var approvedKey = rootKey.OpenSubKey(approvedSubKey, false);
                if (approvedKey != null)
                {
                    var rawVal = approvedKey.GetValue(itemName);
                    if (rawVal is byte[] bytes && bytes.Length > 0)
                    {
                        // In Windows Task Manager / MSConfig:
                        // First byte odd (0x01, 0x03) means DISABLED.
                        // First byte even (0x00, 0x02, 0x04) means ENABLED.
                        return (bytes[0] % 2) != 0;
                    }
                }
            }
            catch { }
            return false;
        }

        public static (bool Success, string Message) ToggleStartup(string location, string name, bool enable)
        {
            try
            {
                if (location.StartsWith("HKCU", StringComparison.OrdinalIgnoreCase))
                {
                    return SetRegistryStartupState(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Run", ApprovedRunPath, name, enable);
                }
                else if (location.StartsWith("HKLM", StringComparison.OrdinalIgnoreCase))
                {
                    if (!Security.PrivilegeManager.IsAdministrator())
                        return (false, "Administrator privileges required to modify HKLM startup items.");

                    return SetRegistryStartupState(Registry.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\Run", ApprovedRunPath, name, enable);
                }
                else if (location.Contains("Folder", StringComparison.OrdinalIgnoreCase))
                {
                    string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                    string targetFile = Path.Combine(startupFolder, name);
                    string disabledFile = Path.Combine(startupFolder, $"{name}.disabled");

                    // Also check for .lnk shortcuts
                    if (!File.Exists(targetFile) && !File.Exists(disabledFile))
                    {
                        if (File.Exists($"{targetFile}.lnk")) targetFile = $"{targetFile}.lnk";
                        if (File.Exists($"{disabledFile}.lnk")) disabledFile = $"{disabledFile}.lnk";
                    }

                    if (enable)
                    {
                        if (File.Exists(disabledFile))
                        {
                            File.Move(disabledFile, targetFile, true);
                        }
                        SetStartupApprovedByte(Registry.CurrentUser, ApprovedFolderUserPath, Path.GetFileName(targetFile), 0x02);
                        AuditLogger.Log("Startup", "Enabled Startup Folder Item", name);
                        return (true, $"Startup item '{name}' enabled.");
                    }
                    else
                    {
                        if (File.Exists(targetFile))
                        {
                            File.Move(targetFile, disabledFile, true);
                        }
                        SetStartupApprovedByte(Registry.CurrentUser, ApprovedFolderUserPath, Path.GetFileName(targetFile), 0x03);
                        AuditLogger.Log("Startup", "Disabled Startup Folder Item", name);
                        return (true, $"Startup item '{name}' disabled.");
                    }
                }

                return (false, "Unrecognized startup item location.");
            }
            catch (Exception ex)
            {
                AuditLogger.Log("Startup", "Toggle Failed", $"{name}: {ex.Message}", success: false, errorMessage: ex.Message);
                return (false, $"Error toggling startup item: {ex.Message}");
            }
        }

        private static (bool Success, string Message) SetRegistryStartupState(RegistryKey rootKey, string runPath, string approvedPath, string name, bool enable)
        {
            // 1. Sync with StartupApproved (Windows Task Manager standard)
            SetStartupApprovedByte(rootKey, approvedPath, name, (byte)(enable ? 0x02 : 0x03));

            // 2. Also check if value exists under .disabled name in Run key and restore if enabling
            using var key = rootKey.OpenSubKey(runPath, true);
            if (key != null)
            {
                string disabledName = $"{name}.disabled";
                if (enable && key.GetValue(disabledName) != null)
                {
                    var val = key.GetValue(disabledName);
                    var kind = key.GetValueKind(disabledName);
                    key.SetValue(name, val!, kind);
                    key.DeleteValue(disabledName, false);
                }
                else if (!enable && key.GetValue(name) != null)
                {
                    // Keep the value in Run key so StartupApproved can govern it like Task Manager does
                }
            }

            string stateLabel = enable ? "enabled" : "disabled";
            AuditLogger.Log("Startup", $"Startup Item {stateLabel.ToUpperInvariant()}", name);
            return (true, $"Startup item '{name}' {stateLabel} successfully.");
        }

        private static void SetStartupApprovedByte(RegistryKey rootKey, string approvedPath, string itemName, byte stateByte)
        {
            try
            {
                using var approvedKey = rootKey.CreateSubKey(approvedPath, true);
                if (approvedKey != null)
                {
                    byte[] data;
                    var existing = approvedKey.GetValue(itemName);
                    if (existing is byte[] b && b.Length >= 12)
                    {
                        data = (byte[])b.Clone();
                    }
                    else
                    {
                        data = new byte[12];
                    }
                    data[0] = stateByte;
                    approvedKey.SetValue(itemName, data, RegistryValueKind.Binary);
                }
            }
            catch { }
        }

        private static string InferPublisher(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return "Unknown Publisher";
            string lower = command.ToLowerInvariant();

            if (lower.Contains("microsoft") || lower.Contains("onedrive") || lower.Contains("teams") || lower.Contains("edge"))
                return "Microsoft Corporation";
            if (lower.Contains("discord")) return "Discord Inc.";
            if (lower.Contains("spotify")) return "Spotify AB";
            if (lower.Contains("steam") || lower.Contains("valve")) return "Valve Corporation";
            if (lower.Contains("epic games")) return "Epic Games Inc.";
            if (lower.Contains("google") || lower.Contains("chrome")) return "Google LLC";
            if (lower.Contains("nvidia")) return "NVIDIA Corporation";
            if (lower.Contains("amd") || lower.Contains("radeon")) return "Advanced Micro Devices";
            if (lower.Contains("intel")) return "Intel Corporation";
            if (lower.Contains("adobe")) return "Adobe Inc.";

            return "Verified Developer";
        }

        private static string InferImpact(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return "Low";
            string lower = command.ToLowerInvariant();

            if (lower.Contains("steam") || lower.Contains("discord") || lower.Contains("epic") || lower.Contains("adobe"))
                return "High";
            if (lower.Contains("onedrive") || lower.Contains("spotify") || lower.Contains("chrome"))
                return "Medium";

            return "Low";
        }
    }
}
