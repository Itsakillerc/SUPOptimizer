using System;
using System.Diagnostics;
using System.Management;
using Microsoft.Win32;
using SUPOptimizer.Core.Logging;

namespace SUPOptimizer.Core.System
{
    public class LicenseInfo
    {
        public bool IsActivated { get; set; }
        public string LicenseStatus { get; set; } = "Unknown";
        public string Channel { get; set; } = "Unknown";
        public string PartialKey { get; set; } = "None";
        public string Edition { get; set; } = "Windows";
        public string Description { get; set; } = string.Empty;
        public string ExpirationDate { get; set; } = "Permanent";
    }

    public static class LicenseService
    {
        public static LicenseInfo GetLicenseInfo()
        {
            var info = new LicenseInfo();
            try
            {
                // Query SoftwareLicensingProduct for the operating system product
                // ApplicationId for Windows OS is 55c92734-d682-4d71-9614-3a7f607a63bc
                using var searcher = new ManagementObjectSearcher(
                    "SELECT Name, Description, LicenseStatus, PartialProductKey, GracePeriodRemaining FROM SoftwareLicensingProduct WHERE PartialProductKey IS NOT NULL AND ApplicationId = '55c92734-d682-4d71-9614-3a7f607a63bc'");

                foreach (ManagementObject mo in searcher.Get())
                {
                    uint status = Convert.ToUInt32(mo["LicenseStatus"] ?? 0);
                    info.PartialKey = mo["PartialProductKey"]?.ToString() ?? "N/A";
                    info.Description = mo["Description"]?.ToString() ?? "";
                    info.Edition = mo["Name"]?.ToString() ?? "Windows";

                    // Determine Channel from Description
                    if (info.Description.Contains("RETAIL", StringComparison.OrdinalIgnoreCase))
                        info.Channel = "Retail";
                    else if (info.Description.Contains("OEM", StringComparison.OrdinalIgnoreCase))
                        info.Channel = "OEM";
                    else if (info.Description.Contains("VOLUME", StringComparison.OrdinalIgnoreCase) || info.Description.Contains("KMS", StringComparison.OrdinalIgnoreCase))
                        info.Channel = "Volume (KMS/MAK)";
                    else
                        info.Channel = "Digital License";

                    // LicenseStatus:
                    // 0 = Unlicensed, 1 = Licensed, 2 = OOBGrace, 3 = OOTGrace, 4 = NonGenuineGrace, 5 = Notification, 6 = ExtendedGrace
                    switch (status)
                    {
                        case 1:
                            info.IsActivated = true;
                            info.LicenseStatus = "Licensed (Permanently Activated)";
                            info.ExpirationDate = "Permanent";
                            break;
                        case 2:
                        case 3:
                        case 6:
                            info.IsActivated = false;
                            info.LicenseStatus = "Grace Period";
                            uint grace = Convert.ToUInt32(mo["GracePeriodRemaining"] ?? 0);
                            info.ExpirationDate = $"{grace / 60 / 24} days remaining";
                            break;
                        case 4:
                            info.IsActivated = false;
                            info.LicenseStatus = "Non-Genuine Grace";
                            info.ExpirationDate = "Action Required";
                            break;
                        case 5:
                            info.IsActivated = false;
                            info.LicenseStatus = "Notification / Expired";
                            info.ExpirationDate = "Expired";
                            break;
                        default:
                            info.IsActivated = false;
                            info.LicenseStatus = "Unlicensed";
                            info.ExpirationDate = "Not Activated";
                            break;
                    }
                    break;
                }
            }
            catch (Exception ex)
            {
                AuditLogger.Log("License", "Query License Info Error", ex.Message, success: false, errorMessage: ex.Message);
            }

            // Fallback / supplement using registry if edition is still generic
            if (string.IsNullOrEmpty(info.Edition) || info.Edition == "Windows")
            {
                try
                {
                    using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                    if (key != null)
                    {
                        info.Edition = key.GetValue("ProductName")?.ToString() ?? "Windows 10/11";
                    }
                }
                catch { }
            }

            return info;
        }

        public static (bool Success, string Message) OpenActivationSettings()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "ms-settings:activation",
                    UseShellExecute = true
                });
                return (true, "Opened Windows Activation Settings.");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to open settings: {ex.Message}");
            }
        }
    }
}
