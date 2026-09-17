using System;
using System.Management;
using System.ServiceProcess;
using Microsoft.Win32;

namespace SUPOptimizer.Core.System
{
    public class SecurityOverview
    {
        public bool DefenderServiceRunning { get; set; }
        public bool DefenderRealTimeProtectionEnabled { get; set; } = true;
        public bool FirewallEnabled { get; set; } = true;
        public bool UacEnabled { get; set; } = true;
        public string UacLevel { get; set; } = "Notify (Default)";
        public string AntivirusProvider { get; set; } = "Microsoft Defender Antivirus";
    }

    public static class SecurityService
    {
        public static SecurityOverview GetOverview()
        {
            var overview = new SecurityOverview();

            // Defender Service
            try
            {
                using var sc = new ServiceController("WinDefend");
                overview.DefenderServiceRunning = (sc.Status == ServiceControllerStatus.Running);
            }
            catch { }

            // SecurityCenter2 Antivirus
            try
            {
                var scope = new ManagementScope(@"\\localhost\root\SecurityCenter2");
                scope.Connect();
                using var searcher = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT displayName, productState FROM AntiVirusProduct"));
                foreach (ManagementObject mo in searcher.Get())
                {
                    overview.AntivirusProvider = mo["displayName"]?.ToString() ?? "Windows Defender";
                    break;
                }
            }
            catch { }

            // Defender Real-Time Protection registry policy check
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection");
                if (key != null)
                {
                    object? disableRt = key.GetValue("DisableRealtimeMonitoring");
                    if (disableRt is int d && d == 1)
                        overview.DefenderRealTimeProtectionEnabled = false;
                }
            }
            catch { }

            // UAC
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System");
                if (key != null)
                {
                    object? lua = key.GetValue("EnableLUA");
                    overview.UacEnabled = (lua is int i && i == 1);

                    object? prompt = key.GetValue("ConsentPromptBehaviorAdmin");
                    if (prompt is int p)
                    {
                        overview.UacLevel = p switch
                        {
                            0 => "Elevate without prompting",
                            2 => "Prompt for credentials on secure desktop",
                            5 => "Prompt for consent for non-Windows binaries",
                            _ => "Prompt for consent on secure desktop"
                        };
                    }
                }
            }
            catch { }

            return overview;
        }
    }
}
