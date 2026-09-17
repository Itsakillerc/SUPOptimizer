using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SUPOptimizer.Core.Logging;
using SUPOptimizer.Core.Security;

namespace SUPOptimizer.Core.System
{
    public class OptionalFeatureItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DisplayName => Name;
        public string Description { get; set; } = string.Empty;
        public string FeatureName { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public string State => IsEnabled ? "Enabled" : "Disabled";
        public bool RequiresReboot { get; set; }
        public string Category { get; set; } = "General";
    }

    public static class FeaturesService
    {
        private static readonly List<OptionalFeatureItem> _knownFeatures = new()
        {
            new OptionalFeatureItem
            {
                Id = "sandbox",
                Name = "Windows Sandbox",
                FeatureName = "Containers-DisposableClientVM",
                Category = "Virtualization",
                Description = "Lightweight isolated desktop environment to safely test untrusted applications and files without persisting changes."
            },
            new OptionalFeatureItem
            {
                Id = "wsl",
                Name = "Windows Subsystem for Linux (WSL)",
                FeatureName = "Microsoft-Windows-Subsystem-Linux",
                Category = "Virtualization",
                Description = "Run native Linux command-line tools, utilities, and applications directly on Windows without a traditional VM overhead."
            },
            new OptionalFeatureItem
            {
                Id = "vmp",
                Name = "Virtual Machine Platform",
                FeatureName = "VirtualMachinePlatform",
                Category = "Virtualization",
                Description = "Enables platform virtualization support required for WSL 2, Android subsystem, and virtualization-based security."
            },
            new OptionalFeatureItem
            {
                Id = "hyperv",
                Name = "Hyper-V Virtualization Engine",
                FeatureName = "Microsoft-Hyper-V-All",
                Category = "Virtualization",
                Description = "Enterprise hardware-assisted hypervisor suite for managing and running Windows and Linux virtual machines."
            },
            new OptionalFeatureItem
            {
                Id = "directplay",
                Name = "DirectPlay (DirectX 9 / Retro Games)",
                FeatureName = "DirectPlay",
                Category = "Gaming",
                Description = "Legacy DirectX networking component required by many classic 1998-2010 PC games (e.g., GTA, Age of Empires)."
            },
            new OptionalFeatureItem
            {
                Id = "telnet",
                Name = "Telnet Client",
                FeatureName = "TelnetClient",
                Category = "Networking",
                Description = "Command-line tool for communicating with remote servers and testing raw TCP port connectivity."
            },
            new OptionalFeatureItem
            {
                Id = "tftp",
                Name = "TFTP Client",
                FeatureName = "TFTP",
                Category = "Networking",
                Description = "Trivial File Transfer Protocol client for transferring files to network routers and embedded firmware devices."
            },
            new OptionalFeatureItem
            {
                Id = "wmp",
                Name = "Windows Media Player (Legacy)",
                FeatureName = "WindowsMediaPlayer",
                Category = "Media",
                Description = "Legacy classic Windows Media Player playback engine and DirectShow media codecs."
            }
        };

        public static List<OptionalFeatureItem> GetFeatures()
        {
            var enabledSet = QueryEnabledFeatures();

            return _knownFeatures.Select(f => new OptionalFeatureItem
            {
                Id = f.Id,
                Name = f.Name,
                FeatureName = f.FeatureName,
                Category = f.Category,
                Description = f.Description,
                IsEnabled = enabledSet.Contains(f.FeatureName)
            }).ToList();
        }

        private static HashSet<string> QueryEnabledFeatures()
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "dism.exe",
                    Arguments = "/online /get-features /format:table",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };

                using var proc = Process.Start(psi);
                if (proc != null)
                {
                    while (!proc.StandardOutput.EndOfStream)
                    {
                        string? line = proc.StandardOutput.ReadLine();
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        if (line.IndexOf("Enabled", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            var parts = line.Split('|');
                            if (parts.Length > 0)
                            {
                                string fName = parts[0].Trim();
                                set.Add(fName);
                            }
                        }
                    }
                    proc.WaitForExit(8000);
                }
            }
            catch (Exception ex)
            {
                AuditLogger.Log("Features", "Query Error", ex.Message, success: false, errorMessage: ex.Message);
            }

            return set;
        }

        public static (bool Success, string Message) SetFeature(string featureId, bool enable)
        {
            if (!PrivilegeManager.IsAdministrator())
                return (false, "Administrator privileges required to toggle Windows Optional Features.");

            var feature = _knownFeatures.FirstOrDefault(f => f.Id.Equals(featureId, StringComparison.OrdinalIgnoreCase) || f.FeatureName.Equals(featureId, StringComparison.OrdinalIgnoreCase));
            if (feature == null)
                return (false, $"Unknown optional feature '{featureId}'.");

            try
            {
                string action = enable ? "enable-feature" : "disable-feature";
                string args = $"/online /{action} /featurename:{feature.FeatureName} /norestart" + (enable ? " /all" : "");

                var psi = new ProcessStartInfo
                {
                    FileName = "dism.exe",
                    Arguments = args,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var proc = Process.Start(psi);
                if (proc != null)
                {
                    string stdOut = proc.StandardOutput.ReadToEnd();
                    proc.WaitForExit(45000);

                    bool ok = proc.ExitCode == 0 || proc.ExitCode == 3010; // 3010 = reboot required
                    AuditLogger.Log("Features", $"Set {feature.Name} {(enable ? "Enabled" : "Disabled")}", $"Exit: {proc.ExitCode}", success: ok);

                    string rebootMsg = proc.ExitCode == 3010 ? " A system restart is required for changes to take full effect." : "";
                    return (ok, ok ? $"Successfully {(enable ? "enabled" : "disabled")} '{feature.Name}'.{rebootMsg}" : $"DISM exited with code {proc.ExitCode}: {stdOut}");
                }

                return (false, "Could not start DISM process.");
            }
            catch (Exception ex)
            {
                return (false, $"Error configuring feature: {ex.Message}");
            }
        }
    }
}
