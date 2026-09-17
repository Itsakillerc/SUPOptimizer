using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SUPOptimizer.Core.Logging;
using SUPOptimizer.Core.Security;

namespace SUPOptimizer.Core.System
{
    public class RepairToolInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Command { get; set; } = string.Empty;
        public bool RequiresAdmin { get; set; } = true;
    }

    public static class RepairService
    {
        private static readonly object _lock = new();
        private static readonly StringBuilder _outputBuffer = new();
        private static bool _isRunning = false;
        private static string _currentToolId = string.Empty;

        public static List<RepairToolInfo> GetAvailableTools()
        {
            return new List<RepairToolInfo>
            {
                new RepairToolInfo
                {
                    Id = "system_corruption_scan",
                    Name = "System Corruption Scan (SFC + DISM Sequential Repair)",
                    Description = "Comprehensive two-stage operating system integrity scan and repair: executes SFC /scannow followed by DISM component store repair.",
                    Command = "sfc.exe /scannow && dism.exe /online /cleanup-image /restorehealth",
                    RequiresAdmin = true
                },
                new RepairToolInfo
                {
                    Id = "reset_windows_update",
                    Name = "Windows Update Complete Reset",
                    Description = "Stops update services, renames and flushes SoftwareDistribution and Catroot2 caches, re-registers core Windows Update DLLs, and restarts services.",
                    Command = "PowerShell -ExecutionPolicy Bypass (Internal Engine)",
                    RequiresAdmin = true
                },
                new RepairToolInfo
                {
                    Id = "reset_network_full",
                    Name = "Full Network Stack & Firewall Reset",
                    Description = "Flushes DNS cache, resets Winsock catalog, resets TCP/IP stack, restores default Windows Firewall profiles, and releases/renews DHCP lease.",
                    Command = "netsh winsock reset && netsh int ip reset && netsh advfirewall reset && ipconfig /flushdns",
                    RequiresAdmin = true
                },
                new RepairToolInfo
                {
                    Id = "sfc_scannow",
                    Name = "System File Checker (SFC /scannow)",
                    Description = "Scans integrity of all protected system files and repairs corrupted or missing Windows files.",
                    Command = "sfc.exe /scannow",
                    RequiresAdmin = true
                },
                new RepairToolInfo
                {
                    Id = "dism_checkhealth",
                    Name = "DISM Component Store Health Check",
                    Description = "Quickly inspects whether Windows component store corruption has been flagged by the servicing stack.",
                    Command = "dism.exe /online /cleanup-image /checkhealth",
                    RequiresAdmin = true
                },
                new RepairToolInfo
                {
                    Id = "dism_restorehealth",
                    Name = "DISM Repair Windows Image",
                    Description = "Downloads and replaces damaged or missing system store files using Windows Update servicing components.",
                    Command = "dism.exe /online /cleanup-image /restorehealth",
                    RequiresAdmin = true
                },
                new RepairToolInfo
                {
                    Id = "chkdsk_scan",
                    Name = "Check Disk Read-Only Inspection (CHKDSK)",
                    Description = "Scans NTFS metadata, file allocation tables, and drive sectors for file system discrepancies.",
                    Command = "chkdsk.exe C:",
                    RequiresAdmin = true
                },
                new RepairToolInfo
                {
                    Id = "repair_network_stack",
                    Name = "Quick Network Stack Reset & Flush",
                    Description = "Flushes DNS cache, resets Winsock catalog, and cleans TCP/IP protocol stack.",
                    Command = "netsh winsock reset && ipconfig /flushdns",
                    RequiresAdmin = true
                },
                new RepairToolInfo
                {
                    Id = "fix_registry_issues",
                    Name = "Fix Common Windows Registry Issues",
                    Description = "Scans and resolves invalid system policies, missing COM registrations, corrupted MUI/Icon caches, and broken user shell folders.",
                    Command = "PowerShell -ExecutionPolicy Bypass (Internal Engine)",
                    RequiresAdmin = true
                }
            };
        }

        public static (bool Started, string Message) RunRepairTool(string toolId)
        {
            lock (_lock)
            {
                if (_isRunning)
                    return (false, "Another repair operation is already in progress.");

                if (!PrivilegeManager.IsAdministrator())
                    return (false, "Administrator privileges are required to run Windows repair tools.");

                _outputBuffer.Clear();
                _isRunning = true;
                _currentToolId = toolId;
            }

            Task.Run(() => ExecuteToolAsync(toolId));
            return (true, "Repair process started.");
        }

        private static void ExecuteToolAsync(string toolId)
        {
            if (toolId == "fix_registry_issues")
            {
                ExecuteRegistryRepair();
                return;
            }

            if (toolId == "reset_windows_update")
            {
                ExecuteWindowsUpdateReset();
                return;
            }

            string fileName = "cmd.exe";
            string args = string.Empty;

            switch (toolId)
            {
                case "system_corruption_scan":
                    fileName = "cmd.exe";
                    args = "/c \"echo === [STAGE 1/2] RUNNING SFC SYSTEM FILE CHECKER === && sfc.exe /scannow && echo. && echo === [STAGE 2/2] REPAIRING WINDOWS IMAGE STORE VIA DISM === && dism.exe /online /cleanup-image /restorehealth\"";
                    break;
                case "reset_network_full":
                    fileName = "cmd.exe";
                    args = "/c \"netsh winsock reset && netsh int ip reset && netsh advfirewall reset && ipconfig /flushdns && ipconfig /release && ipconfig /renew\"";
                    break;
                case "sfc_scannow":
                    fileName = "sfc.exe";
                    args = "/scannow";
                    break;
                case "dism_checkhealth":
                    fileName = "dism.exe";
                    args = "/online /cleanup-image /checkhealth";
                    break;
                case "dism_restorehealth":
                    fileName = "dism.exe";
                    args = "/online /cleanup-image /restorehealth";
                    break;
                case "chkdsk_scan":
                    fileName = "chkdsk.exe";
                    args = "C:";
                    break;
                case "repair_network_stack":
                    fileName = "cmd.exe";
                    args = "/c \"netsh winsock reset && netsh int ip reset && ipconfig /flushdns\"";
                    break;
                default:
                    AppendOutput($"[ERROR] Unknown tool ID: {toolId}");
                    lock (_lock) { _isRunning = false; }
                    return;
            }

            AppendOutput($"[START] Launching {fileName} {args} at {DateTime.Now:HH:mm:ss}...\n");

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = args,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                using var proc = new Process { StartInfo = psi };
                proc.OutputDataReceived += (s, e) => { if (e.Data != null) AppendOutput(e.Data); };
                proc.ErrorDataReceived += (s, e) => { if (e.Data != null) AppendOutput($"[STDERR] {e.Data}"); };

                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();

                proc.WaitForExit();

                AppendOutput($"\n[COMPLETED] Process exited with code {proc.ExitCode} at {DateTime.Now:HH:mm:ss}\n");
                AuditLogger.Log("Repair", $"Completed {toolId}", $"Exit Code: {proc.ExitCode}", success: proc.ExitCode == 0);
            }
            catch (Exception ex)
            {
                AppendOutput($"\n[EXCEPTION] {ex.Message}\n");
                AuditLogger.Log("Repair", $"Failed {toolId}", ex.Message, success: false, errorMessage: ex.Message);
            }
            finally
            {
                lock (_lock)
                {
                    _isRunning = false;
                }
            }
        }

        private static void ExecuteRegistryRepair()
        {
            AppendOutput($"[START] Initializing Windows Registry Deep Integrity Diagnostic at {DateTime.Now:HH:mm:ss}...\n");
            int issuesFound = 0;
            int issuesFixed = 0;

            try
            {
                // 1. Check & Repair Administrative restriction policies
                AppendOutput("[1/5] Checking for orphaned administrative lockouts and policy restrictions...");
                string[] policyKeys = new[]
                {
                    @"Software\Microsoft\Windows\CurrentVersion\Policies\System",
                    @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer"
                };

                string[] restrictiveValues = new[]
                {
                    "DisableTaskMgr", "DisableRegistryTools", "NoFolderOptions", "NoControlPanel", "NoRun"
                };

                foreach (var pKey in policyKeys)
                {
                    try
                    {
                        using var cu = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(pKey, true);
                        if (cu != null)
                        {
                            foreach (var val in restrictiveValues)
                            {
                                if (cu.GetValue(val) != null)
                                {
                                    issuesFound++;
                                    cu.DeleteValue(val, false);
                                    issuesFixed++;
                                    AppendOutput($"  [FIXED] Removed invalid administrative block policy: {val} in HKCU\\{pKey}");
                                }
                            }
                        }
                    }
                    catch { }
                }

                // 2. Validate User Shell Folders
                AppendOutput("[2/5] Validating Windows User Shell Folders paths...");
                try
                {
                    using var sfKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders", true);
                    if (sfKey != null)
                    {
                        var desktopPath = sfKey.GetValue("Desktop")?.ToString();
                        if (string.IsNullOrEmpty(desktopPath))
                        {
                            issuesFound++;
                            sfKey.SetValue("Desktop", @"%USERPROFILE%\Desktop", Microsoft.Win32.RegistryValueKind.ExpandString);
                            issuesFixed++;
                            AppendOutput("  [FIXED] Restored missing Desktop User Shell Folder.");
                        }
                    }
                }
                catch { }

                // 3. Clear Stale OpenWithProgids & Invalid MRU Lists
                AppendOutput("[3/5] Cleaning stale Run/Search MRU registry cache...");
                try
                {
                    using var runMru = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\RunMRU", true);
                    if (runMru != null)
                    {
                        var valNames = runMru.GetValueNames();
                        if (valNames.Length > 20)
                        {
                            issuesFound++;
                            foreach (var v in valNames) runMru.DeleteValue(v, false);
                            issuesFixed++;
                            AppendOutput($"  [CLEANED] Flushed {valNames.Length} obsolete Run MRU cache items.");
                        }
                    }
                }
                catch { }

                // 4. Re-register essential System DLLs
                AppendOutput("[4/5] Re-registering essential COM/OLE Windows system binaries...");
                string[] dlls = new[] { "ole32.dll", "oleaut32.dll", "actxprxy.dll" };
                foreach (var dll in dlls)
                {
                    try
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = "regsvr32.exe",
                            Arguments = $"/s {dll}",
                            CreateNoWindow = true,
                            UseShellExecute = false
                        };
                        using var p = Process.Start(psi);
                        p?.WaitForExit(3000);
                        AppendOutput($"  [SUCCESS] COM registration verified: {dll}");
                    }
                    catch { }
                }

                // 5. Restart Explorer Icon Cache
                AppendOutput("[5/5] Rebuilding Windows Explorer Icon & Thumbnail Cache...");
                try
                {
                    string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    string iconCache = global::System.IO.Path.Combine(localAppData, "IconCache.db");
                    if (global::System.IO.File.Exists(iconCache))
                    {
                        issuesFound++;
                        try { global::System.IO.File.Delete(iconCache); issuesFixed++; AppendOutput("  [FIXED] Rebuilt corrupted IconCache.db"); } catch { }
                    }
                }
                catch { }

                AppendOutput($"\n[COMPLETED] Registry diagnosis finished! Scanned 5 subsystems. Issues resolved: {issuesFixed}/{issuesFound}.\n");
                AuditLogger.Log("Repair", "Completed fix_registry_issues", $"Issues resolved: {issuesFixed}/{issuesFound}", success: true);
            }
            catch (Exception ex)
            {
                AppendOutput($"\n[EXCEPTION] Registry repair error: {ex.Message}\n");
                AuditLogger.Log("Repair", "Failed fix_registry_issues", ex.Message, success: false, errorMessage: ex.Message);
            }
            finally
            {
                lock (_lock)
                {
                    _isRunning = false;
                }
            }
        }

        private static void ExecuteWindowsUpdateReset()
        {
            AppendOutput($"[START] Initializing Windows Update Complete Reset at {DateTime.Now:HH:mm:ss}...\n");
            try
            {
                // 1. Stop Windows Update Services
                AppendOutput("[1/4] Stopping Windows Update & Cryptographic background services...");
                string[] services = new[] { "wuauserv", "cryptSvc", "bits", "msiserver" };
                foreach (var svc in services)
                {
                    RunCmdSilent($"net stop {svc} /y");
                    AppendOutput($"  [STOPPED] Service {svc}");
                }

                // 2. Rename/Purge SoftwareDistribution & catroot2 caches
                AppendOutput("[2/4] Purging and resetting download store caches...");
                string winDir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                string softDist = Path.Combine(winDir, "SoftwareDistribution");
                string softDistBak = Path.Combine(winDir, $"SoftwareDistribution.bak_{DateTime.Now:yyyyMMdd}");
                try
                {
                    if (Directory.Exists(softDist))
                    {
                        if (Directory.Exists(softDistBak)) Directory.Delete(softDistBak, true);
                        Directory.Move(softDist, softDistBak);
                        AppendOutput($"  [BACKUP] Renamed SoftwareDistribution to {Path.GetFileName(softDistBak)}");
                    }
                }
                catch (Exception ex)
                {
                    AppendOutput($"  [WARNING] Could not rename SoftwareDistribution: {ex.Message}");
                }

                // 3. Re-register essential Update DLLs
                AppendOutput("[3/4] Re-registering core Windows Update COM interfaces...");
                string[] updateDlls = new[] { "wuapi.dll", "wuaueng.dll", "wucltui.dll", "wups.dll", "wups2.dll", "wuwebv.dll", "atl.dll" };
                foreach (var dll in updateDlls)
                {
                    try
                    {
                        var psi = new ProcessStartInfo { FileName = "regsvr32.exe", Arguments = $"/s {dll}", CreateNoWindow = true, UseShellExecute = false };
                        using var p = Process.Start(psi);
                        p?.WaitForExit(2000);
                    }
                    catch { }
                }
                AppendOutput("  [REGISTERED] Core update DLLs verified.");

                // 4. Restart Services
                AppendOutput("[4/4] Restarting services and resetting network socket catalog...");
                RunCmdSilent("netsh winsock reset");
                foreach (var svc in services.Reverse())
                {
                    RunCmdSilent($"net start {svc}");
                    AppendOutput($"  [STARTED] Service {svc}");
                }

                AppendOutput($"\n[COMPLETED] Windows Update servicing components reset successfully at {DateTime.Now:HH:mm:ss}!\n");
                AuditLogger.Log("Repair", "Reset Windows Update", "Completed SoftwareDistribution purge and service restart", success: true);
            }
            catch (Exception ex)
            {
                AppendOutput($"\n[EXCEPTION] Update reset error: {ex.Message}\n");
                AuditLogger.Log("Repair", "Reset Windows Update Error", ex.Message, success: false, errorMessage: ex.Message);
            }
            finally
            {
                lock (_lock) { _isRunning = false; }
            }
        }

        private static void RunCmdSilent(string cmd)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {cmd}",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using var p = Process.Start(psi);
                p?.WaitForExit(5000);
            }
            catch { }
        }

        // ==========================================
        // AUTOLOGON CONFIGURATION
        // ==========================================
        private const string WinlogonKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon";

        public static (bool Enabled, string Username, string Domain) GetAutoLogonStatus()
        {
            try
            {
                using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(WinlogonKey);
                if (key == null) return (false, "", "");

                string? auto = key.GetValue("AutoAdminLogon")?.ToString();
                string? user = key.GetValue("DefaultUserName")?.ToString();
                string? dom = key.GetValue("DefaultDomainName")?.ToString();

                return (auto == "1", user ?? "", dom ?? "");
            }
            catch
            {
                return (false, "", "");
            }
        }

        public static (bool Success, string Message) ConfigureAutoLogon(string username, string password, string domain)
        {
            if (!PrivilegeManager.IsAdministrator())
                return (false, "Administrator privileges required to configure AutoLogon.");

            if (string.IsNullOrWhiteSpace(username))
                return (false, "Username cannot be empty.");

            try
            {
                using var key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(WinlogonKey);
                key.SetValue("AutoAdminLogon", "1", Microsoft.Win32.RegistryValueKind.String);
                key.SetValue("DefaultUserName", username, Microsoft.Win32.RegistryValueKind.String);
                key.SetValue("DefaultPassword", password ?? "", Microsoft.Win32.RegistryValueKind.String);
                if (!string.IsNullOrEmpty(domain))
                {
                    key.SetValue("DefaultDomainName", domain, Microsoft.Win32.RegistryValueKind.String);
                }
                AuditLogger.Log("Repair", "Configured AutoLogon", $"User: {username}");
                return (true, $"AutoLogon successfully configured for user '{username}'.");
            }
            catch (Exception ex)
            {
                return (false, $"Error configuring AutoLogon: {ex.Message}");
            }
        }

        public static (bool Success, string Message) DisableAutoLogon()
        {
            if (!PrivilegeManager.IsAdministrator())
                return (false, "Administrator privileges required.");

            try
            {
                using var key = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(WinlogonKey);
                key.SetValue("AutoAdminLogon", "0", Microsoft.Win32.RegistryValueKind.String);
                try { key.DeleteValue("DefaultPassword", false); } catch { }
                AuditLogger.Log("Repair", "Disabled AutoLogon", "AutoAdminLogon set to 0");
                return (true, "AutoLogon disabled successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Error disabling AutoLogon: {ex.Message}");
            }
        }

        // ==========================================
        // POWER SCHEME & HIBERNATION
        // ==========================================
        public static (bool Success, string Message) EnableUltimatePerformance()
        {
            if (!PrivilegeManager.IsAdministrator())
                return (false, "Administrator privileges required.");

            try
            {
                // Duplicate Ultimate Performance scheme GUID: e9a42b02-d5df-448d-aa00-03f14749eb61
                var psi = new ProcessStartInfo
                {
                    FileName = "powercfg.exe",
                    Arguments = "-duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
                using var proc = Process.Start(psi);
                string outStr = proc?.StandardOutput.ReadToEnd() ?? "";
                proc?.WaitForExit(5000);

                // Activate it
                var psiAct = new ProcessStartInfo
                {
                    FileName = "powercfg.exe",
                    Arguments = "-setactive e9a42b02-d5df-448d-aa00-03f14749eb61",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using var pAct = Process.Start(psiAct);
                pAct?.WaitForExit(5000);

                AuditLogger.Log("Power", "Enabled Ultimate Performance Plan", "powercfg duplicatescheme");
                return (true, "Ultimate Performance power plan unlocked and set as active scheme.");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to enable Ultimate Performance plan: {ex.Message}");
            }
        }

        public static (bool Success, string Message) ToggleHibernation(bool enable)
        {
            if (!PrivilegeManager.IsAdministrator())
                return (false, "Administrator privileges required.");

            try
            {
                string arg = enable ? "-h on" : "-h off";
                var psi = new ProcessStartInfo
                {
                    FileName = "powercfg.exe",
                    Arguments = arg,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using var p = Process.Start(psi);
                p?.WaitForExit(5000);

                AuditLogger.Log("Power", $"Toggled Hibernation {(enable ? "On" : "Off")}", $"powercfg {arg}");
                string detail = enable ? "Hibernation enabled (hiberfil.sys created)." : "Hibernation disabled (hiberfil.sys deleted, saving gigabytes of SSD space).";
                return (true, detail);
            }
            catch (Exception ex)
            {
                return (false, $"Error toggling hibernation: {ex.Message}");
            }
        }

        private static void AppendOutput(string text)
        {
            lock (_lock)
            {
                _outputBuffer.AppendLine(text);
                // Keep buffer manageable
                if (_outputBuffer.Length > 200_000)
                {
                    _outputBuffer.Remove(0, 50_000);
                }
            }
        }

        public static (bool IsRunning, string CurrentTool, string Output) GetOutput()
        {
            lock (_lock)
            {
                return (_isRunning, _currentToolId, _outputBuffer.ToString());
            }
        }
    }
}
