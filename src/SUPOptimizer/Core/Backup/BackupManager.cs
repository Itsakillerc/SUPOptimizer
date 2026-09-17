using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text.Json;
using Microsoft.Win32;
using SUPOptimizer.Core.Logging;

namespace SUPOptimizer.Core.Backup
{
    public static class BackupManager
    {
        private static string _snapshotsDirectory = string.Empty;
        private static readonly object _lock = new();

        public static void Initialize(string baseDataDir)
        {
            try
            {
                _snapshotsDirectory = Path.Combine(baseDataDir, "snapshots");
                if (!Directory.Exists(_snapshotsDirectory))
                    Directory.CreateDirectory(_snapshotsDirectory);
            }
            catch (Exception ex)
            {
                AuditLogger.Log("Backup", "Init Error", ex.Message, success: false, errorMessage: ex.Message);
            }
        }

        public static (bool Success, string Message) CreateSystemRestorePoint(string description)
        {
            if (!Security.PrivilegeManager.IsAdministrator())
            {
                return (false, "Administrator privileges are required to create a System Restore Point.");
            }

            try
            {
                // Try WMI SystemRestore class
                var scope = new ManagementScope(@"\\localhost\root\default");
                scope.Connect();

                var path = new ManagementPath("SystemRestore");
                var options = new ObjectGetOptions();
                using var processClass = new ManagementClass(scope, path, options);

                using var inParams = processClass.GetMethodParameters("CreateRestorePoint");
                inParams["Description"] = string.IsNullOrWhiteSpace(description) ? "SUP Optimizer Restore Point" : description;
                inParams["RestorePointType"] = 12; // MODIFY_SETTINGS
                inParams["EventType"] = 100;        // BEGIN_SYSTEM_CHANGE

                using var outParams = processClass.InvokeMethod("CreateRestorePoint", inParams, null);
                uint returnCode = (uint)outParams["ReturnValue"];

                if (returnCode == 0)
                {
                    AuditLogger.Log("Backup", "Restore Point Created", description);
                    return (true, "System Restore Point created successfully.");
                }
                else
                {
                    // Fallback to PowerShell
                    return CreateRestorePointPowerShell(description);
                }
            }
            catch (Exception)
            {
                // Attempt PowerShell fallback
                return CreateRestorePointPowerShell(description);
            }
        }

        private static (bool Success, string Message) CreateRestorePointPowerShell(string description)
        {
            try
            {
                string desc = string.IsNullOrWhiteSpace(description) ? "SUP Optimizer Snapshot" : description.Replace("\"", "'");
                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"Checkpoint-Computer -Description '{desc}' -RestorePointType MODIFY_SETTINGS -ErrorAction Stop\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var proc = Process.Start(psi);
                proc?.WaitForExit(30000);

                if (proc != null && proc.ExitCode == 0)
                {
                    AuditLogger.Log("Backup", "Restore Point Created (PowerShell)", desc);
                    return (true, "System Restore Point created via PowerShell.");
                }
                else
                {
                    string err = proc?.StandardError.ReadToEnd() ?? "Unknown error or timeout";
                    AuditLogger.Log("Backup", "Restore Point Failed", err, success: false, errorMessage: err);
                    return (false, $"Could not create System Restore Point: {err}");
                }
            }
            catch (Exception ex)
            {
                AuditLogger.Log("Backup", "Restore Point Exception", ex.Message, success: false, errorMessage: ex.Message);
                return (false, $"System Restore error: {ex.Message}. (Note: System Protection must be enabled on C:\\).");
            }
        }

        public static string SaveChangeSet(ChangeSet changeSet)
        {
            lock (_lock)
            {
                if (string.IsNullOrEmpty(changeSet.Id))
                {
                    changeSet.Id = $"{DateTime.Now:yyyyMMdd_HHmmss}_{changeSet.Category}";
                }

                try
                {
                    string filePath = Path.Combine(_snapshotsDirectory, $"{changeSet.Id}.json");
                    string json = JsonSerializer.Serialize(changeSet, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(filePath, json);

                    AuditLogger.Log("Backup", "ChangeSet Saved", $"ChangeSet ID: {changeSet.Id} ({changeSet.Operations.Count} operations)");
                    return changeSet.Id;
                }
                catch (Exception ex)
                {
                    AuditLogger.Log("Backup", "Save ChangeSet Failed", ex.Message, success: false, errorMessage: ex.Message);
                    return string.Empty;
                }
            }
        }

        public static List<ChangeSet> GetChangeSets()
        {
            lock (_lock)
            {
                var list = new List<ChangeSet>();
                if (!Directory.Exists(_snapshotsDirectory))
                    return list;

                foreach (var file in Directory.GetFiles(_snapshotsDirectory, "*.json").OrderByDescending(f => File.GetCreationTime(f)))
                {
                    try
                    {
                        string json = File.ReadAllText(file);
                        var cs = JsonSerializer.Deserialize<ChangeSet>(json);
                        if (cs != null)
                            list.Add(cs);
                    }
                    catch { }
                }

                return list;
            }
        }

        public static (bool Success, string Message) RollbackChangeSet(string changeSetId)
        {
            lock (_lock)
            {
                string filePath = Path.Combine(_snapshotsDirectory, $"{changeSetId}.json");
                if (!File.Exists(filePath))
                    return (false, $"ChangeSet {changeSetId} not found.");

                try
                {
                    string json = File.ReadAllText(filePath);
                    var cs = JsonSerializer.Deserialize<ChangeSet>(json);
                    if (cs == null)
                        return (false, "Invalid ChangeSet format.");

                    if (cs.IsRolledBack)
                        return (false, "This ChangeSet has already been rolled back.");

                    int revertedCount = 0;
                    // Revert in reverse order
                    for (int i = cs.Operations.Count - 1; i >= 0; i--)
                    {
                        var op = cs.Operations[i];
                        if (op.Type == OperationType.Registry)
                        {
                            RevertRegistryOperation(op);
                            revertedCount++;
                        }
                    }

                    cs.IsRolledBack = true;
                    cs.RolledBackAt = DateTime.Now;

                    string updatedJson = JsonSerializer.Serialize(cs, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(filePath, updatedJson);

                    AuditLogger.Log("Backup", "ChangeSet Rolled Back", $"Rolled back {revertedCount} operations for ChangeSet {changeSetId}");
                    return (true, $"ChangeSet {changeSetId} successfully rolled back ({revertedCount} operations restored).");
                }
                catch (Exception ex)
                {
                    AuditLogger.Log("Backup", "Rollback Failed", ex.Message, success: false, errorMessage: ex.Message);
                    return (false, $"Rollback failed: {ex.Message}");
                }
            }
        }

        private static void RevertRegistryOperation(ChangeOperation op)
        {
            try
            {
                // Parse hive
                string target = op.Target;
                RegistryKey? rootKey = null;
                string subKeyPath = target;

                if (target.StartsWith("HKLM\\", StringComparison.OrdinalIgnoreCase) || target.StartsWith("HKEY_LOCAL_MACHINE\\", StringComparison.OrdinalIgnoreCase))
                {
                    rootKey = Registry.LocalMachine;
                    subKeyPath = target.Substring(target.IndexOf('\\') + 1);
                }
                else if (target.StartsWith("HKCU\\", StringComparison.OrdinalIgnoreCase) || target.StartsWith("HKEY_CURRENT_USER\\", StringComparison.OrdinalIgnoreCase))
                {
                    rootKey = Registry.CurrentUser;
                    subKeyPath = target.Substring(target.IndexOf('\\') + 1);
                }

                if (rootKey == null) return;

                if (op.PreviousValue == null)
                {
                    // Previous value did not exist -> delete it
                    using var key = rootKey.OpenSubKey(subKeyPath, true);
                    if (key != null && !string.IsNullOrEmpty(op.PropertyName))
                    {
                        key.DeleteValue(op.PropertyName, false);
                    }
                }
                else
                {
                    // Restore previous value
                    using var key = rootKey.CreateSubKey(subKeyPath, true);
                    if (key != null && !string.IsNullOrEmpty(op.PropertyName))
                    {
                        if (op.PreviousValue is JsonElement je)
                        {
                            if (je.ValueKind == JsonValueKind.Number && je.TryGetInt32(out int intVal))
                                key.SetValue(op.PropertyName, intVal, RegistryValueKind.DWord);
                            else if (je.ValueKind == JsonValueKind.String)
                                key.SetValue(op.PropertyName, je.GetString() ?? "", RegistryValueKind.String);
                            else
                                key.SetValue(op.PropertyName, je.ToString());
                        }
                        else
                        {
                            key.SetValue(op.PropertyName, op.PreviousValue);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AuditLogger.Log("Backup", "Registry Revert Error", $"{op.Target}: {ex.Message}", success: false, errorMessage: ex.Message);
            }
        }
    }
}
