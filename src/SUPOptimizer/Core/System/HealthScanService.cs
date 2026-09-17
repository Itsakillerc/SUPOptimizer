using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using SUPOptimizer.Core.Tweaks;

namespace SUPOptimizer.Core.System
{
    public enum HealthSeverity
    {
        Ok,
        Info,
        Attention,
        Warning,
        Critical
    }

    public class HealthFinding
    {
        public string Category { get; set; } = string.Empty;
        public HealthSeverity Severity { get; set; } = HealthSeverity.Ok;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
        public string? TweakId { get; set; }
    }

    public class HealthReport
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public int Score { get; set; } = 100;
        public string Status { get; set; } = "OPTIMIZED";
        public List<HealthFinding> Findings { get; set; } = new();
        public int OkCount { get; set; }
        public int InfoCount { get; set; }
        public int AttentionCount { get; set; }
        public int WarningCount { get; set; }
        public int CriticalCount { get; set; }
    }

    public static class HealthScanService
    {
        public static HealthReport RunScan()
        {
            var report = new HealthReport();
            int deductions = 0;

            // 1. Check Telemetry
            var telemetryTweak = TweakRegistry.GetTweak("privacy_telemetry");
            if (telemetryTweak != null)
            {
                var state = telemetryTweak.GetCurrentState();
                if (state != TweakState.Enabled)
                {
                    report.Findings.Add(new HealthFinding
                    {
                        Category = "Privacy",
                        Severity = HealthSeverity.Warning,
                        Title = "Full Windows Diagnostic Telemetry Active",
                        Description = "Your system is continuously sending usage diagnostics and behavioral telemetry to Microsoft servers.",
                        Recommendation = "Disable Diagnostic Data in Privacy tab.",
                        TweakId = "privacy_telemetry"
                    });
                    deductions += 15;
                }
                else
                {
                    report.Findings.Add(new HealthFinding
                    {
                        Category = "Privacy",
                        Severity = HealthSeverity.Ok,
                        Title = "Diagnostic Telemetry Restricted",
                        Description = "Telemetry data collection is set to minimal/security level.",
                        Recommendation = "No action needed."
                    });
                }
            }

            // 2. Check Advertising ID
            var adTweak = TweakRegistry.GetTweak("privacy_advertising_id");
            if (adTweak != null && adTweak.GetCurrentState() != TweakState.Enabled)
            {
                report.Findings.Add(new HealthFinding
                {
                    Category = "Privacy",
                    Severity = HealthSeverity.Attention,
                    Title = "Advertising Tracking ID Enabled",
                    Description = "Apps can use your unique advertising ID to track behavior and serve targeted promotions.",
                    Recommendation = "Disable Advertising ID in Privacy tab.",
                    TweakId = "privacy_advertising_id"
                });
                deductions += 8;
            }

            // 3. Check Network Throttling
            var netTweak = TweakRegistry.GetTweak("opt_network_throttling");
            if (netTweak != null && netTweak.GetCurrentState() != TweakState.Enabled)
            {
                report.Findings.Add(new HealthFinding
                {
                    Category = "Performance",
                    Severity = HealthSeverity.Attention,
                    Title = "Network Throttling Index Active",
                    Description = "Windows limits non-multimedia network packets when running multimedia or game applications.",
                    Recommendation = "Disable Network Throttling in Optimizer tab.",
                    TweakId = "opt_network_throttling"
                });
                deductions += 10;
            }

            // 4. Check Game DVR
            var dvrTweak = TweakRegistry.GetTweak("opt_game_dvr");
            if (dvrTweak != null && dvrTweak.GetCurrentState() != TweakState.Enabled)
            {
                report.Findings.Add(new HealthFinding
                {
                    Category = "Gaming",
                    Severity = HealthSeverity.Info,
                    Title = "Game DVR Background Recording Enabled",
                    Description = "Windows background screen recording is enabled, using GPU encoder cycles.",
                    Recommendation = "Disable Game DVR in Gaming tab if you don't use instant replay.",
                    TweakId = "opt_game_dvr"
                });
                deductions += 5;
            }

            // 5. Check Disk Free Space on C:\
            try
            {
                var systemDrive = new DriveInfo(Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\");
                if (systemDrive.IsReady)
                {
                    double freePct = ((double)systemDrive.AvailableFreeSpace / systemDrive.TotalSize) * 100.0;
                    if (freePct < 10.0)
                    {
                        report.Findings.Add(new HealthFinding
                        {
                            Category = "Storage",
                            Severity = HealthSeverity.Critical,
                            Title = $"System Drive C:\\ Low on Space ({freePct:F1}% Free)",
                            Description = "Drive space is critically low. This can cause severe system instability, update failures, and slow swap file paging.",
                            Recommendation = "Run Storage Cleanup to delete temp files and caches."
                        });
                        deductions += 25;
                    }
                    else if (freePct < 20.0)
                    {
                        report.Findings.Add(new HealthFinding
                        {
                            Category = "Storage",
                            Severity = HealthSeverity.Warning,
                            Title = $"System Drive C:\\ Space Warning ({freePct:F1}% Free)",
                            Description = "Drive space is under 20%. Windows performs best with at least 20-25% free disk capacity.",
                            Recommendation = "Clean temporary files and delivery optimization caches."
                        });
                        deductions += 10;
                    }
                    else
                    {
                        report.Findings.Add(new HealthFinding
                        {
                            Category = "Storage",
                            Severity = HealthSeverity.Ok,
                            Title = $"System Drive Capacity Healthy ({freePct:F1}% Free)",
                            Description = "Drive C:\\ has adequate free space.",
                            Recommendation = "No action needed."
                        });
                    }
                }
            }
            catch { }

            // 6. Check Temp Directory Size
            try
            {
                string tempDir = Path.GetTempPath();
                if (Directory.Exists(tempDir))
                {
                    long totalBytes = 0;
                    var di = new DirectoryInfo(tempDir);
                    foreach (var fi in di.EnumerateFiles("*", SearchOption.TopDirectoryOnly))
                    {
                        try { totalBytes += fi.Length; } catch { }
                    }
                    long tempMb = totalBytes / (1024 * 1024);
                    if (tempMb > 500)
                    {
                        report.Findings.Add(new HealthFinding
                        {
                            Category = "Storage",
                            Severity = HealthSeverity.Attention,
                            Title = $"Accumulated Temp Files ({tempMb} MB in user temp)",
                            Description = "Temporary files have accumulated in your local temporary folder.",
                            Recommendation = "Clean user temporary files in Storage tab."
                        });
                        deductions += 5;
                    }
                }
            }
            catch { }

            // 7. Check UAC
            var uacStatus = Security.PrivilegeManager.GetStatus();
            if (!uacStatus.IsUacEnabled)
            {
                report.Findings.Add(new HealthFinding
                {
                    Category = "Security",
                    Severity = HealthSeverity.Warning,
                    Title = "User Account Control (UAC) Disabled",
                    Description = "UAC is turned off. Applications can execute with administrative privileges without confirmation prompts.",
                    Recommendation = "Enable UAC in Windows Security settings."
                });
                deductions += 15;
            }
            else
            {
                report.Findings.Add(new HealthFinding
                {
                    Category = "Security",
                    Severity = HealthSeverity.Ok,
                    Title = "User Account Control (UAC) Enabled",
                    Description = "UAC privilege isolation is active.",
                    Recommendation = "No action needed."
                });
            }

            // Compute score & counts
            report.Score = Math.Max(10, 100 - deductions);

            foreach (var f in report.Findings)
            {
                switch (f.Severity)
                {
                    case HealthSeverity.Ok: report.OkCount++; break;
                    case HealthSeverity.Info: report.InfoCount++; break;
                    case HealthSeverity.Attention: report.AttentionCount++; break;
                    case HealthSeverity.Warning: report.WarningCount++; break;
                    case HealthSeverity.Critical: report.CriticalCount++; break;
                }
            }

            if (report.CriticalCount > 0) report.Status = "CRITICAL";
            else if (report.WarningCount > 0) report.Status = "WARNING";
            else if (report.AttentionCount > 0) report.Status = "ATTENTION REQUIRED";
            else report.Status = "OPTIMIZED";

            return report;
        }
    }
}
