using System;
using System.Diagnostics;
using System.Security.Principal;
using Microsoft.Win32;

namespace SUPOptimizer.Core.Security
{
    public class PrivilegeStatus
    {
        public bool IsAdministrator { get; set; }
        public bool IsUacEnabled { get; set; }
        public string IntegrityLevel { get; set; } = "Medium";
        public string UserName { get; set; } = string.Empty;
    }

    public static class PrivilegeManager
    {
        private static bool? _isAdminCache;

        public static bool IsAdministrator()
        {
            if (_isAdminCache.HasValue)
                return _isAdminCache.Value;

            try
            {
                using var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                _isAdminCache = principal.IsInRole(WindowsBuiltInRole.Administrator);
                return _isAdminCache.Value;
            }
            catch
            {
                return false;
            }
        }

        public static PrivilegeStatus GetStatus()
        {
            bool isAdmin = IsAdministrator();
            bool uacEnabled = true;

            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System");
                if (key != null)
                {
                    object? val = key.GetValue("EnableLUA");
                    if (val is int intVal)
                        uacEnabled = (intVal == 1);
                }
            }
            catch
            {
                // Non-admin may not always be able to read some keys or key might not exist
            }

            return new PrivilegeStatus
            {
                IsAdministrator = isAdmin,
                IsUacEnabled = uacEnabled,
                IntegrityLevel = isAdmin ? "High" : "Medium",
                UserName = Environment.UserName
            };
        }

        public static bool RestartElevated(string? additionalArgs = null)
        {
            try
            {
                var processInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName ?? "SUPOptimizer.exe",
                    Verb = "runas",
                    Arguments = additionalArgs ?? string.Empty
                };

                Process.Start(processInfo);
                return true;
            }
            catch (Exception)
            {
                // User clicked "No" on UAC prompt or canceled
                return false;
            }
        }
    }
}
