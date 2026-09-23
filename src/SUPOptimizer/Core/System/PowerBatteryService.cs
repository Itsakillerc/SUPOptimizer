using System;
using System.Diagnostics;
using System.IO;
using System.Management;
using SUPOptimizer.Core.Logging;

namespace SUPOptimizer.Core.System
{
    public class BatteryDiagnosticInfo
    {
        public bool HasBattery { get; set; }
        public int BatteryPercent { get; set; } = 100;
        public string PowerLineStatus { get; set; } = "AC Connected";
        public string BatteryStatus { get; set; } = "Not Present";
        public int DesignCapacityMwh { get; set; }
        public int FullChargeCapacityMwh { get; set; }
        public double HealthPercent { get; set; } = 100.0;
        public int CycleCount { get; set; }
        public string ActivePowerPlan { get; set; } = "Balanced";
        public string ReportPath { get; set; } = string.Empty;
    }

    public static class PowerBatteryService
    {
        public static BatteryDiagnosticInfo GetBatteryInfo()
        {
            var info = new BatteryDiagnosticInfo();

            // 1. Query Win32_Battery
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Battery");
                var collection = searcher.Get();

                if (collection.Count > 0)
                {
                    info.HasBattery = true;
                    foreach (ManagementObject mo in collection)
                    {
                        info.BatteryPercent = Convert.ToInt32(mo["EstimatedChargeRemaining"] ?? 100);
                        ushort status = Convert.ToUInt16(mo["BatteryStatus"] ?? 1);
                        info.BatteryStatus = status switch
                        {
                            1 => "Discharging",
                            2 => "AC Connected / Charging",
                            3 => "Fully Charged",
                            4 => "Low",
                            5 => "Critical",
                            6 => "Charging",
                            _ => "Normal"
                        };

                        info.DesignCapacityMwh = Convert.ToInt32(mo["DesignCapacity"] ?? 0);
                        info.FullChargeCapacityMwh = Convert.ToInt32(mo["FullChargeCapacity"] ?? 0);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                AuditLogger.Log("Power", "Query Battery Error", ex.Message, success: false, errorMessage: ex.Message);
            }

            // 2. Query root\wmi for high-accuracy cycle count and capacities if missing
            if (info.HasBattery)
            {
                try
                {
                    var scope = new ManagementScope(@"\\localhost\root\wmi");
                    scope.Connect();

                    if (info.DesignCapacityMwh <= 0)
                    {
                        using var searcherStatic = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT DesignedCapacity FROM BatteryStaticData"));
                        foreach (ManagementObject mo in searcherStatic.Get())
                        {
                            info.DesignCapacityMwh = Convert.ToInt32(mo["DesignedCapacity"] ?? 0);
                            break;
                        }
                    }

                    if (info.FullChargeCapacityMwh <= 0)
                    {
                        using var searcherFull = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT FullChargedCapacity FROM BatteryFullChargedCapacity"));
                        foreach (ManagementObject mo in searcherFull.Get())
                        {
                            info.FullChargeCapacityMwh = Convert.ToInt32(mo["FullChargedCapacity"] ?? 0);
                            break;
                        }
                    }

                    using var searcherCycle = new ManagementObjectSearcher(scope, new ObjectQuery("SELECT CycleCount FROM BatteryCycleCount"));
                    foreach (ManagementObject mo in searcherCycle.Get())
                    {
                        info.CycleCount = Convert.ToInt32(mo["CycleCount"] ?? 0);
                        break;
                    }
                }
                catch { }

                // Calculate Health Percent
                if (info.DesignCapacityMwh > 0 && info.FullChargeCapacityMwh > 0)
                {
                    double ratio = (double)info.FullChargeCapacityMwh / info.DesignCapacityMwh * 100.0;
                    info.HealthPercent = Math.Min(100.0, Math.Round(ratio, 1));
                }
            }
            else
            {
                info.BatteryStatus = "Desktop / No Battery Installed";
                info.PowerLineStatus = "Direct AC Power";
            }

            // 3. Query Active Power Scheme
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powercfg.exe",
                    Arguments = "/getactivescheme",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
                using var p = Process.Start(psi);
                if (p != null)
                {
                    string output = p.StandardOutput.ReadToEnd();
                    p.WaitForExit();
                    // Example: "Power Scheme GUID: 381b4222-f694-41f0-9685-ff5bb260df2e  (Balanced)"
                    int startIdx = output.IndexOf('(');
                    int endIdx = output.IndexOf(')');
                    if (startIdx >= 0 && endIdx > startIdx)
                    {
                        info.ActivePowerPlan = output.Substring(startIdx + 1, endIdx - startIdx - 1);
                    }
                }
            }
            catch { }

            string tempReport = Path.Combine(Path.GetTempPath(), "supoptimizer-battery-report.html");
            info.ReportPath = tempReport;

            return info;
        }

        public static (bool Success, string Message, string Path) GenerateAndOpenReport()
        {
            string outPath = Path.Combine(Path.GetTempPath(), "supoptimizer-battery-report.html");
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "powercfg.exe",
                    Arguments = $"/batteryreport /output \"{outPath}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var proc = Process.Start(psi);
                if (proc == null)
                    return (false, "Could not start powercfg process.", "");

                proc.WaitForExit(15000);

                if (File.Exists(outPath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = outPath,
                        UseShellExecute = true
                    });
                    AuditLogger.Log("Power", "Generated Battery Report", outPath);
                    return (true, "Battery report generated and opened in browser.", outPath);
                }

                return (false, "Report was not generated (no battery detected or operation unsupported).", "");
            }
            catch (Exception ex)
            {
                return (false, $"Error generating battery report: {ex.Message}", "");
            }
        }
    }
}
