using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using SUPOptimizer.Core.Logging;
using SUPOptimizer.Core.Security;

namespace SUPOptimizer.Core.System
{
    public class DnsPreset
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string PrimaryDns { get; set; } = string.Empty;
        public string SecondaryDns { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsDhcp { get; set; }
    }

    public class EnvVariableItem
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Scope { get; set; } = "User"; // "User" or "System"
    }

    public class LockedFileInfo
    {
        public string FilePath { get; set; } = string.Empty;
        public List<LockingProcessInfo> LockingProcesses { get; set; } = new();
    }

    public class LockingProcessInfo
    {
        public int ProcessId { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public string AppName { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
    }

    public class RunAliasItem
    {
        public string CommandName { get; set; } = string.Empty;
        public string TargetPath { get; set; } = string.Empty;
        public string Scope { get; set; } = "HKCU";
    }

    public static class SystemToolsService
    {
        // ==========================================
        // 1. HOSTS FILE MANAGER
        // ==========================================
        private static string HostsFilePath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), @"drivers\etc\hosts");

        public static (bool Success, string Content, string Path) ReadHostsFile()
        {
            try
            {
                if (!File.Exists(HostsFilePath))
                {
                    return (false, "Hosts file not found.", HostsFilePath);
                }

                string content = File.ReadAllText(HostsFilePath);
                return (true, content, HostsFilePath);
            }
            catch (Exception ex)
            {
                return (false, $"Error reading hosts file: {ex.Message}", HostsFilePath);
            }
        }

        public static (bool Success, string Message) SaveHostsFile(string newContent)
        {
            if (!PrivilegeManager.IsAdministrator())
                return (false, "Administrator privileges required to edit the HOSTS file.");

            try
            {
                // Create timestamped backup first
                string backupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backups", "hosts");
                Directory.CreateDirectory(backupDir);
                string backupFile = Path.Combine(backupDir, $"hosts_{DateTime.Now:yyyyMMdd_HHmmss}.bak");
                if (File.Exists(HostsFilePath))
                {
                    File.Copy(HostsFilePath, backupFile, true);
                }

                // Remove read-only attribute if present
                var attrs = File.GetAttributes(HostsFilePath);
                if (attrs.HasFlag(FileAttributes.ReadOnly))
                {
                    File.SetAttributes(HostsFilePath, attrs & ~FileAttributes.ReadOnly);
                }

                File.WriteAllText(HostsFilePath, newContent);
                AuditLogger.Log("SystemTools", "Edited HOSTS file", $"Backup saved to {backupFile}");
                return (true, $"HOSTS file saved successfully. Backup created: {Path.GetFileName(backupFile)}");
            }
            catch (Exception ex)
            {
                AuditLogger.Log("SystemTools", "Hosts Save Error", ex.Message, success: false, errorMessage: ex.Message);
                return (false, $"Error writing hosts file: {ex.Message}");
            }
        }

        public static (bool Success, string Message) BlockTelemetryDomainsInHosts()
        {
            if (!PrivilegeManager.IsAdministrator())
                return (false, "Administrator privileges required.");

            var domains = new[]
            {
                "v10.events.data.microsoft.com",
                "v20.events.data.microsoft.com",
                "telemetry.microsoft.com",
                "watson.telemetry.microsoft.com",
                "watson.ppe.telemetry.microsoft.com",
                "settings-win.data.microsoft.com",
                "feedback.windows.com",
                "diagnostics.support.microsoft.com",
                "corp.sts.microsoft.com",
                "msftncsi.com"
            };

            var (readSuccess, content, _) = ReadHostsFile();
            if (!readSuccess) return (false, content);

            var lines = content.Split('\n').Select(l => l.TrimEnd('\r')).ToList();
            int addedCount = 0;

            foreach (var domain in domains)
            {
                bool exists = lines.Any(l => !l.TrimStart().StartsWith("#") && l.Contains(domain));
                if (!exists)
                {
                    lines.Add($"0.0.0.0 {domain} # Added by SUPOptimizer");
                    addedCount++;
                }
            }

            if (addedCount == 0)
                return (true, "All telemetry domains are already blocked in your HOSTS file.");

            string newContent = string.Join(Environment.NewLine, lines);
            return SaveHostsFile(newContent);
        }

        public static (bool Success, string Message) BlockAdobeDomainsInHosts()
        {
            if (!PrivilegeManager.IsAdministrator())
                return (false, "Administrator privileges required to edit HOSTS.");

            var adobeDomains = new[]
            {
                "genuine.adobe.com",
                "prod.adobegenuine.com",
                "lmlicenses.wip4.adobe.com",
                "lm.licenses.adobe.com",
                "na1r.services.adobe.com",
                "hlrcv.stage.adobe.com",
                "uds.licenses.adobe.com",
                "cc-api-data.adobe.com",
                "ic.adobe.io",
                "ims-na1.adobelogin.com",
                "adobe-dns.adobe.com",
                "adobe-dns-2.adobe.com",
                "adobe-dns-3.adobe.com",
                "workflow-sub.adobe.io",
                "armmf.adobe.com",
                "arrm.adobe.com",
                "c5.pat.adobe.com",
                "activate.adobe.com",
                "practivate.adobe.com",
                "ereg.adobe.com",
                "wip.adobe.com",
                "ans.oobesaas.adobe.com"
            };

            var (readSuccess, content, _) = ReadHostsFile();
            if (!readSuccess) return (false, content);

            var lines = content.Split('\n').Select(l => l.TrimEnd('\r')).ToList();
            int addedCount = 0;

            foreach (var domain in adobeDomains)
            {
                bool exists = lines.Any(l => !l.TrimStart().StartsWith("#") && l.Contains(domain));
                if (!exists)
                {
                    lines.Add($"0.0.0.0 {domain} # Added by SUPOptimizer Adobe Block");
                    addedCount++;
                }
            }

            if (addedCount == 0)
                return (true, "All Adobe tracking/licensing domains are already blocked in your HOSTS file.");

            string newContent = string.Join(Environment.NewLine, lines);
            return SaveHostsFile(newContent);
        }

        // ==========================================
        // 2. QUICK DNS SWITCHER
        // ==========================================
        public static List<DnsPreset> GetDnsPresets()
        {
            return new List<DnsPreset>
            {
                new DnsPreset
                {
                    Id = "cloudflare",
                    Name = "Cloudflare DNS (1.1.1.1)",
                    Provider = "Cloudflare",
                    PrimaryDns = "1.1.1.1",
                    SecondaryDns = "1.0.0.1",
                    Description = "Fastest consumer DNS, privacy-respecting, logs deleted after 24h."
                },
                new DnsPreset
                {
                    Id = "google",
                    Name = "Google Public DNS",
                    Provider = "Google",
                    PrimaryDns = "8.8.8.8",
                    SecondaryDns = "8.8.4.4",
                    Description = "Global, robust, fast resolution with worldwide Anycast routing."
                },
                new DnsPreset
                {
                    Id = "quad9",
                    Name = "Quad9 Secure DNS",
                    Provider = "Quad9",
                    PrimaryDns = "9.9.9.9",
                    SecondaryDns = "149.112.112.112",
                    Description = "Blocks malicious domains, phishing, and malware based on threat intelligence."
                },
                new DnsPreset
                {
                    Id = "adguard",
                    Name = "AdGuard Default Ad-Blocking",
                    Provider = "AdGuard",
                    PrimaryDns = "94.140.14.14",
                    SecondaryDns = "94.140.15.15",
                    Description = "Blocks ads, trackers, and malicious domains across your entire system."
                },
                new DnsPreset
                {
                    Id = "opendns",
                    Name = "Cisco OpenDNS",
                    Provider = "Cisco",
                    PrimaryDns = "208.67.222.222",
                    SecondaryDns = "208.67.220.220",
                    Description = "Enterprise-grade reliability, phishing protection, and web filtering."
                },
                new DnsPreset
                {
                    Id = "dhcp",
                    Name = "Automatic (DHCP / Router Default)",
                    Provider = "Router / ISP",
                    PrimaryDns = "",
                    SecondaryDns = "",
                    Description = "Restores DNS settings assigned by your local router or network provider.",
                    IsDhcp = true
                }
            };
        }

        public static (bool Success, string Message) ApplyDns(string adapterName, string primaryDns, string secondaryDns, bool isDhcp)
        {
            if (!PrivilegeManager.IsAdministrator())
                return (false, "Administrator privileges required to change DNS servers.");

            try
            {
                if (string.IsNullOrWhiteSpace(adapterName))
                {
                    // Fallback to finding active interface name
                    var adapters = NetworkService.GetAdapters();
                    var active = adapters.FirstOrDefault(a => a.Status.Equals("Up", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(a.Ipv4Address));
                    if (active != null)
                        adapterName = active.Name;
                    else
                        return (false, "No active network adapter found.");
                }

                if (isDhcp || string.IsNullOrWhiteSpace(primaryDns))
                {
                    // Set DHCP
                    var psi = new ProcessStartInfo
                    {
                        FileName = "netsh.exe",
                        Arguments = $"interface ip set dns name=\"{adapterName}\" source=dhcp",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };
                    using var proc = Process.Start(psi);
                    proc?.WaitForExit(5000);

                    NetworkService.FlushDns();
                    AuditLogger.Log("Network", "DNS reset to DHCP", adapterName);
                    return (true, $"DNS reset to automatic DHCP for adapter '{adapterName}'.");
                }
                else
                {
                    // Set Static Primary
                    var psi1 = new ProcessStartInfo
                    {
                        FileName = "netsh.exe",
                        Arguments = $"interface ip set dns name=\"{adapterName}\" static {primaryDns}",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };
                    using var p1 = Process.Start(psi1);
                    p1?.WaitForExit(5000);

                    // Add Secondary if present
                    if (!string.IsNullOrWhiteSpace(secondaryDns))
                    {
                        var psi2 = new ProcessStartInfo
                        {
                            FileName = "netsh.exe",
                            Arguments = $"interface ip add dns name=\"{adapterName}\" {secondaryDns} index=2",
                            CreateNoWindow = true,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true
                        };
                        using var p2 = Process.Start(psi2);
                        p2?.WaitForExit(5000);
                    }

                    NetworkService.FlushDns();
                    AuditLogger.Log("Network", "DNS changed", $"{adapterName} -> {primaryDns}, {secondaryDns}");
                    return (true, $"DNS configured to {primaryDns} / {secondaryDns} on '{adapterName}'.");
                }
            }
            catch (Exception ex)
            {
                AuditLogger.Log("Network", "DNS Change Error", ex.Message, success: false, errorMessage: ex.Message);
                return (false, $"Failed to configure DNS: {ex.Message}");
            }
        }

        // ==========================================
        // 3. SYSTEM & USER ENVIRONMENT VARIABLES
        // ==========================================
        public static List<EnvVariableItem> GetEnvironmentVariables()
        {
            var list = new List<EnvVariableItem>();

            try
            {
                // User variables
                var userVars = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User);
                foreach (global::System.Collections.DictionaryEntry entry in userVars)
                {
                    list.Add(new EnvVariableItem
                    {
                        Name = entry.Key?.ToString() ?? "",
                        Value = entry.Value?.ToString() ?? "",
                        Scope = "User"
                    });
                }

                // System variables
                var machineVars = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Machine);
                foreach (global::System.Collections.DictionaryEntry entry in machineVars)
                {
                    list.Add(new EnvVariableItem
                    {
                        Name = entry.Key?.ToString() ?? "",
                        Value = entry.Value?.ToString() ?? "",
                        Scope = "System"
                    });
                }
            }
            catch (Exception ex)
            {
                AuditLogger.Log("SystemTools", "GetEnvVars Error", ex.Message, success: false, errorMessage: ex.Message);
            }

            return list.OrderBy(v => v.Scope).ThenBy(v => v.Name).ToList();
        }

        public static (bool Success, string Message) SetEnvironmentVariable(string name, string value, string scope)
        {
            if (string.IsNullOrWhiteSpace(name))
                return (false, "Variable name cannot be empty.");

            var target = scope.Equals("System", StringComparison.OrdinalIgnoreCase)
                ? EnvironmentVariableTarget.Machine
                : EnvironmentVariableTarget.User;

            if (target == EnvironmentVariableTarget.Machine && !PrivilegeManager.IsAdministrator())
                return (false, "Administrator privileges required to edit System environment variables.");

            try
            {
                Environment.SetEnvironmentVariable(name, string.IsNullOrEmpty(value) ? null : value, target);
                AuditLogger.Log("SystemTools", $"Set Env Var [{target}]", $"{name}={(value.Length > 50 ? value.Substring(0, 50) + "..." : value)}");
                return (true, $"Variable '{name}' updated successfully in {target} scope.");
            }
            catch (Exception ex)
            {
                return (false, $"Error setting variable: {ex.Message}");
            }
        }

        // ==========================================
        // 4. FILE UNLOCKER (RESTART MANAGER API)
        // ==========================================
        [StructLayout(LayoutKind.Sequential)]
        private struct RM_UNIQUE_PROCESS
        {
            public int dwProcessId;
            public global::System.Runtime.InteropServices.ComTypes.FILETIME ProcessStartTime;
        }

        private const int CCH_RM_MAX_APP_NAME = 255;
        private const int CCH_RM_MAX_SVC_NAME = 63;

        private enum RM_APP_TYPE
        {
            RmUnknownApp = 0,
            RmMainWindow = 1,
            RmOtherWindow = 2,
            RmService = 3,
            RmExplorer = 4,
            RmConsole = 5,
            RmCritical = 1000
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct RM_PROCESS_INFO
        {
            public RM_UNIQUE_PROCESS Process;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH_RM_MAX_APP_NAME + 1)]
            public string strAppName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH_RM_MAX_SVC_NAME + 1)]
            public string strServiceShortName;
            public RM_APP_TYPE ApplicationType;
            public uint AppStatus;
            public uint TSSessionId;
            [MarshalAs(UnmanagedType.Bool)]
            public bool bRestartable;
        }

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        private static extern int RmStartSession(out uint pSessionHandle, int dwSessionFlags, string strSessionKey);

        [DllImport("rstrtmgr.dll")]
        private static extern int RmEndSession(uint pSessionHandle);

        [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
        private static extern int RmRegisterResources(uint pSessionHandle, uint nFiles, string[] rgsFilenames,
            uint nApplications, [In] RM_UNIQUE_PROCESS[]? rgApplications, uint nServices, string[]? rgsServiceNames);

        [DllImport("rstrtmgr.dll")]
        private static extern int RmGetList(uint pSessionHandle, out uint pnProcInfoNeeded, ref uint pnProcInfo,
            [In, Out] RM_PROCESS_INFO[]? rgAffectedApps, ref uint lpdwRebootReasons);

        public static LockedFileInfo FindFileLocks(string filePath)
        {
            var result = new LockedFileInfo { FilePath = filePath };

            if (!File.Exists(filePath) && !Directory.Exists(filePath))
            {
                return result;
            }

            int res = RmStartSession(out uint sessionHandle, 0, Guid.NewGuid().ToString("N"));
            if (res != 0) return result;

            try
            {
                string[] resources = new[] { filePath };
                res = RmRegisterResources(sessionHandle, (uint)resources.Length, resources, 0, null, 0, null);
                if (res != 0) return result;

                uint nProcInfoNeeded = 0;
                uint nProcInfo = 0;
                uint rebootReasons = 0;

                res = RmGetList(sessionHandle, out nProcInfoNeeded, ref nProcInfo, null, ref rebootReasons);
                if (res == 234) // ERROR_MORE_DATA
                {
                    var processInfo = new RM_PROCESS_INFO[nProcInfoNeeded];
                    nProcInfo = nProcInfoNeeded;
                    res = RmGetList(sessionHandle, out nProcInfoNeeded, ref nProcInfo, processInfo, ref rebootReasons);

                    if (res == 0)
                    {
                        for (int i = 0; i < nProcInfo; i++)
                        {
                            try
                            {
                                int pid = processInfo[i].Process.dwProcessId;
                                var proc = Process.GetProcessById(pid);
                                result.LockingProcesses.Add(new LockingProcessInfo
                                {
                                    ProcessId = pid,
                                    ProcessName = proc.ProcessName,
                                    AppName = processInfo[i].strAppName,
                                    FullPath = proc.MainModule?.FileName ?? ""
                                });
                            }
                            catch
                            {
                                result.LockingProcesses.Add(new LockingProcessInfo
                                {
                                    ProcessId = processInfo[i].Process.dwProcessId,
                                    ProcessName = processInfo[i].strAppName,
                                    AppName = processInfo[i].strAppName
                                });
                            }
                        }
                    }
                }
            }
            finally
            {
                RmEndSession(sessionHandle);
            }

            return result;
        }

        public static (bool Success, string Message) TerminateLockingProcess(int processId)
        {
            try
            {
                var proc = Process.GetProcessById(processId);
                string name = proc.ProcessName;
                proc.Kill();
                AuditLogger.Log("SystemTools", "Terminated Locking Process", $"Killed PID {processId} ({name})");
                return (true, $"Process '{name}' (PID {processId}) terminated successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to terminate process {processId}: {ex.Message}");
            }
        }

        // ==========================================
        // 5. RUN DIALOG CUSTOM COMMAND ALIASES (App Paths)
        // ==========================================
        private const string AppPathsKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths";

        public static List<RunAliasItem> GetRunAliases()
        {
            var list = new List<RunAliasItem>();

            // Inspect HKCU
            try
            {
                using var cuKey = Registry.CurrentUser.OpenSubKey(AppPathsKey);
                if (cuKey != null)
                {
                    foreach (var sub in cuKey.GetSubKeyNames())
                    {
                        using var itemKey = cuKey.OpenSubKey(sub);
                        var target = itemKey?.GetValue("")?.ToString();
                        if (!string.IsNullOrEmpty(target))
                        {
                            list.Add(new RunAliasItem
                            {
                                CommandName = sub,
                                TargetPath = target,
                                Scope = "HKCU"
                            });
                        }
                    }
                }
            }
            catch { }

            // Inspect HKLM
            try
            {
                using var lmKey = Registry.LocalMachine.OpenSubKey(AppPathsKey);
                if (lmKey != null)
                {
                    foreach (var sub in lmKey.GetSubKeyNames())
                    {
                        using var itemKey = lmKey.OpenSubKey(sub);
                        var target = itemKey?.GetValue("")?.ToString();
                        if (!string.IsNullOrEmpty(target))
                        {
                            // Avoid duplicates if also in HKCU
                            if (!list.Any(x => x.CommandName.Equals(sub, StringComparison.OrdinalIgnoreCase)))
                            {
                                list.Add(new RunAliasItem
                                {
                                    CommandName = sub,
                                    TargetPath = target,
                                    Scope = "HKLM"
                                });
                            }
                        }
                    }
                }
            }
            catch { }

            return list.OrderBy(a => a.CommandName).ToList();
        }

        public static (bool Success, string Message) AddRunAlias(string aliasName, string targetPath)
        {
            if (string.IsNullOrWhiteSpace(aliasName) || string.IsNullOrWhiteSpace(targetPath))
                return (false, "Alias and Target Path cannot be empty.");

            if (!aliasName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                aliasName += ".exe";

            try
            {
                using var key = Registry.CurrentUser.CreateSubKey($@"{AppPathsKey}\{aliasName}");
                key.SetValue("", targetPath);
                AuditLogger.Log("SystemTools", "Added Run Alias", $"{aliasName} -> {targetPath}");
                return (true, $"Run alias '{aliasName}' registered. You can now press Win+R and type '{aliasName.Replace(".exe", "")}'!");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to add run alias: {ex.Message}");
            }
        }

        public static (bool Success, string Message) RemoveRunAlias(string aliasName)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(AppPathsKey, true);
                if (key != null)
                {
                    key.DeleteSubKeyTree(aliasName, false);
                    AuditLogger.Log("SystemTools", "Removed Run Alias", aliasName);
                    return (true, $"Run alias '{aliasName}' removed.");
                }
                return (false, "Run aliases key not found.");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to remove run alias: {ex.Message}");
            }
        }
    }
}
