using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using Microsoft.Win32;
using SUPOptimizer.Core.Logging;
using SUPOptimizer.Core.Security;

namespace SUPOptimizer.Core.System
{
    public class ServiceItem
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Status { get; set; } = "Stopped"; // Running, Stopped, Paused
        public string StartupType { get; set; } = "Manual"; // Automatic, Manual, Disabled
        public string Description { get; set; } = string.Empty;
        public bool IsMicrosoft { get; set; } = true;
    }

    public static class ServiceManager
    {
        public static List<ServiceItem> GetServices()
        {
            var list = new List<ServiceItem>();

            try
            {
                var controllers = ServiceController.GetServices();
                foreach (var sc in controllers)
                {
                    string startupType = "Manual";
                    string desc = string.Empty;
                    try
                    {
                        using var key = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Services\{sc.ServiceName}");
                        if (key != null)
                        {
                            object? startVal = key.GetValue("Start");
                            if (startVal is int s)
                            {
                                startupType = s switch
                                {
                                    2 => "Automatic",
                                    3 => "Manual",
                                    4 => "Disabled",
                                    _ => "Unknown"
                                };
                            }
                            desc = key.GetValue("Description")?.ToString() ?? string.Empty;
                        }
                    }
                    catch { }

                    list.Add(new ServiceItem
                    {
                        Name = sc.ServiceName,
                        DisplayName = string.IsNullOrEmpty(sc.DisplayName) ? sc.ServiceName : sc.DisplayName,
                        Status = sc.Status.ToString(),
                        StartupType = startupType,
                        Description = desc,
                        IsMicrosoft = sc.ServiceName.StartsWith("Win", StringComparison.OrdinalIgnoreCase) ||
                                      sc.ServiceName.StartsWith("App", StringComparison.OrdinalIgnoreCase) ||
                                      sc.ServiceName.StartsWith("W", StringComparison.OrdinalIgnoreCase)
                    });
                }
            }
            catch (Exception ex)
            {
                AuditLogger.Log("Services", "GetServices Error", ex.Message, success: false, errorMessage: ex.Message);
            }

            return list.OrderBy(s => s.DisplayName).ToList();
        }

        public static (bool Success, string Message) StartService(string serviceName)
        {
            if (!PrivilegeManager.IsAdministrator())
                return (false, "Administrator privileges are required to manage services.");

            try
            {
                using var sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running)
                    return (true, $"Service '{serviceName}' is already running.");

                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                AuditLogger.Log("Services", "Started Service", serviceName);
                return (true, $"Service '{serviceName}' started successfully.");
            }
            catch (Exception ex)
            {
                AuditLogger.Log("Services", "Start Service Failed", $"{serviceName}: {ex.Message}", success: false, errorMessage: ex.Message);
                return (false, $"Failed to start service: {ex.Message}");
            }
        }

        public static (bool Success, string Message) StopService(string serviceName)
        {
            if (!PrivilegeManager.IsAdministrator())
                return (false, "Administrator privileges are required to manage services.");

            try
            {
                using var sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Stopped)
                    return (true, $"Service '{serviceName}' is already stopped.");

                sc.Stop();
                sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                AuditLogger.Log("Services", "Stopped Service", serviceName);
                return (true, $"Service '{serviceName}' stopped successfully.");
            }
            catch (Exception ex)
            {
                AuditLogger.Log("Services", "Stop Service Failed", $"{serviceName}: {ex.Message}", success: false, errorMessage: ex.Message);
                return (false, $"Failed to stop service: {ex.Message}");
            }
        }

        public static (bool Success, string Message) RestartService(string serviceName)
        {
            var stopRes = StopService(serviceName);
            if (!stopRes.Success && !stopRes.Message.Contains("already stopped"))
                return stopRes;

            return StartService(serviceName);
        }

        public static (bool Success, string Message) SetStartupType(string serviceName, string startupType)
        {
            if (!PrivilegeManager.IsAdministrator())
                return (false, "Administrator privileges are required to change service startup type.");

            int startVal = startupType.ToLowerInvariant() switch
            {
                "automatic" => 2,
                "manual" => 3,
                "disabled" => 4,
                _ => 3
            };

            try
            {
                using var key = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Services\{serviceName}", true);
                if (key == null)
                    return (false, $"Service registry key for '{serviceName}' not found.");

                object? oldVal = key.GetValue("Start");
                key.SetValue("Start", startVal, RegistryValueKind.DWord);

                AuditLogger.Log("Services", "Changed Startup Type", $"{serviceName} -> {startupType}", oldVal?.ToString(), startVal.ToString());
                return (true, $"Service '{serviceName}' startup type set to {startupType}.");
            }
            catch (Exception ex)
            {
                AuditLogger.Log("Services", "Set Startup Type Failed", $"{serviceName}: {ex.Message}", success: false, errorMessage: ex.Message);
                return (false, $"Failed to set startup type: {ex.Message}");
            }
        }
    }
}
