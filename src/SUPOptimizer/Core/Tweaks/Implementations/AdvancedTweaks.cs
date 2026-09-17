using System;
using System.Diagnostics;
using Microsoft.Win32;
using SUPOptimizer.Core.Logging;
using SUPOptimizer.Core.Security;

namespace SUPOptimizer.Core.Tweaks.Implementations
{
    // ==========================================
    // 1. OFFICE TELEMETRY (2016 / 2019 / 2021 / 365)
    // ==========================================
    public class DisableOfficeTelemetryTweak : RegistryTweakBase
    {
        public override string Id => "priv_disable_office_telemetry";
        public override string Name => "Disable Microsoft Office Telemetry & Logging";
        public override string Description => "Prevents Office (2016, 2019, 2021, M365) from uploading document telemetry, diagnostic data, and background user usage statistics.";
        public override string TechnicalDetails => @"Sets HKCU\Software\Policies\Microsoft\Office\16.0\osm\enablelogging to 0 and common\privacy\disconnectedstate to 2.";
        public override TweakCategory Category => TweakCategory.Privacy;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Software\Policies\Microsoft\Office\16.0\osm";
        protected override string ValueName => "enablelogging";
        protected override object TargetValue => 0;
        protected override object? RestoreValue => 1;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;

        public override TweakResult Apply(bool dryRun)
        {
            var res = base.Apply(dryRun);
            if (res.Success && !dryRun)
            {
                try
                {
                    using var commonKey = Registry.CurrentUser.CreateSubKey(@"Software\Policies\Microsoft\Office\16.0\common\privacy");
                    commonKey.SetValue("disconnectedstate", 2, RegistryValueKind.DWord);
                    using var osmKey = Registry.CurrentUser.CreateSubKey(@"Software\Policies\Microsoft\Office\16.0\osm");
                    osmKey.SetValue("enableupload", 0, RegistryValueKind.DWord);
                }
                catch { }
            }
            return res;
        }
    }

    // ==========================================
    // 2. DISABLE AUTOMATIC WINDOWS UPDATES
    // ==========================================
    public class DisableWindowsUpdateAutoTweak : RegistryTweakBase
    {
        public override string Id => "win_stop_auto_updates";
        public override string Name => "Stop Automatic Windows Updates (Notify Only)";
        public override string Description => "Prevents Windows 10/11 from automatically rebooting your PC or downloading heavy feature updates without manual user approval.";
        public override string TechnicalDetails => @"Sets HKLM\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU\NoAutoUpdate to 1 (DWORD).";
        public override TweakCategory Category => TweakCategory.Windows;
        public override RiskLevel Risk => RiskLevel.Low;
        public override bool RequiresAdmin => true;
        public override bool RequiresReboot => false;

        protected override RegistryHive Hive => RegistryHive.LocalMachine;
        protected override string SubKeyPath => @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU";
        protected override string ValueName => "NoAutoUpdate";
        protected override object TargetValue => 1;
        protected override object? RestoreValue => 0;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;

        public override TweakResult Apply(bool dryRun)
        {
            var res = base.Apply(dryRun);
            if (res.Success && !dryRun)
            {
                try
                {
                    using var au = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU");
                    au.SetValue("AUOptions", 2, RegistryValueKind.DWord); // 2 = Notify before download and install
                }
                catch { }
            }
            return res;
        }
    }

    // ==========================================
    // 3. DISABLE EDGE COPILOT
    // ==========================================
    public class DisableEdgeCopilotTweak : RegistryTweakBase
    {
        public override string Id => "priv_disable_edge_copilot";
        public override string Name => "Disable Microsoft Edge Copilot & Sidebar AI";
        public override string Description => "Completely disables Copilot AI and promotional Bing sidebar widgets in Microsoft Edge browser.";
        public override string TechnicalDetails => @"Sets HKLM\SOFTWARE\Policies\Microsoft\Edge\HubsSidebarEnabled to 0 (DWORD).";
        public override TweakCategory Category => TweakCategory.Privacy;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => true;

        protected override RegistryHive Hive => RegistryHive.LocalMachine;
        protected override string SubKeyPath => @"SOFTWARE\Policies\Microsoft\Edge";
        protected override string ValueName => "HubsSidebarEnabled";
        protected override object TargetValue => 0;
        protected override object? RestoreValue => 1;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    // ==========================================
    // 4. ENABLE UTC TIME GLOBALLY (RealTimeIsUniversal)
    // ==========================================
    public class EnableUtcTimeTweak : RegistryTweakBase
    {
        public override string Id => "win_enable_utc_time";
        public override string Name => "Enable Hardware Clock UTC Time (Dual-Boot Fix)";
        public override string Description => "Configures Windows hardware CMOS clock to use Universal Time (UTC). Resolves clock time shifting when dual-booting with Linux, macOS, or WSL.";
        public override string TechnicalDetails => @"Sets HKLM\SYSTEM\CurrentControlSet\Control\TimeZoneInformation\RealTimeIsUniversal to 1 (DWORD).";
        public override TweakCategory Category => TweakCategory.System;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => true;
        public override bool RequiresReboot => true;

        protected override RegistryHive Hive => RegistryHive.LocalMachine;
        protected override string SubKeyPath => @"SYSTEM\CurrentControlSet\Control\TimeZoneInformation";
        protected override string ValueName => "RealTimeIsUniversal";
        protected override object TargetValue => 1;
        protected override object? RestoreValue => 0;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    // ==========================================
    // 5. DISABLE ONEDRIVE SYNC
    // ==========================================
    public class DisableOneDriveSyncTweak : RegistryTweakBase
    {
        public override string Id => "win_disable_onedrive_sync";
        public override string Name => "Disable OneDrive Cloud File Syncing";
        public override string Description => "Prevents OneDrive from syncing files in the background, conserving network bandwidth, CPU usage, and battery life.";
        public override string TechnicalDetails => @"Sets HKLM\SOFTWARE\Policies\Microsoft\Windows\OneDrive\DisableFileSyncNGSC to 1 (DWORD).";
        public override TweakCategory Category => TweakCategory.System;
        public override RiskLevel Risk => RiskLevel.Low;
        public override bool RequiresAdmin => true;

        protected override RegistryHive Hive => RegistryHive.LocalMachine;
        protected override string SubKeyPath => @"SOFTWARE\Policies\Microsoft\Windows\OneDrive";
        protected override string ValueName => "DisableFileSyncNGSC";
        protected override object TargetValue => 1;
        protected override object? RestoreValue => 0;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    // ==========================================
    // 6. DISABLE HPET (HIGH PRECISION EVENT TIMER)
    // ==========================================
    public class DisableHpetTweak : ITweak
    {
        public string Id => "perf_disable_hpet";
        public string Name => "Disable HPET (Lower Input Latency in Games)";
        public string Description => "Configures the Windows boot loader to use low-overhead TSC/invariant hardware timers instead of HPET, reducing micro-stutter in competitive gaming.";
        public string TechnicalDetails => "Runs bcdedit.exe /deletevalue useplatformclock and bcdedit.exe /set disabledynamictick yes.";
        public TweakCategory Category => TweakCategory.Gaming;
        public RiskLevel Risk => RiskLevel.Low;
        public bool RequiresAdmin => true;
        public bool IsReversible => true;
        public bool RequiresReboot => true;
        public string SupportedWindows => "10+";
        public TweakState RecommendedState => TweakState.Enabled;
        public TweakState DefaultState => TweakState.Disabled;

        public TweakState GetCurrentState()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "bcdedit.exe",
                    Arguments = "/enum {current}",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
                using var p = Process.Start(psi);
                string outStr = p?.StandardOutput.ReadToEnd() ?? "";
                p?.WaitForExit(3000);

                if (outStr.IndexOf("useplatformclock        Yes", StringComparison.OrdinalIgnoreCase) >= 0)
                    return TweakState.Disabled;

                return TweakState.Enabled;
            }
            catch
            {
                return TweakState.Unknown;
            }
        }

        public TweakResult Apply(bool dryRun)
        {
            if (!PrivilegeManager.IsAdministrator())
                return new TweakResult { Success = false, Message = "Administrator privileges required.", DryRun = dryRun };

            if (dryRun)
                return new TweakResult { Success = true, Message = "[DRY-RUN] Would execute bcdedit /set useplatformclock false", DryRun = true };

            try
            {
                RunBcdedit("/set useplatformclock false");
                RunBcdedit("/set disabledynamictick yes");
                AuditLogger.Log("Tweaks", "Applied Disable HPET", "useplatformclock false");
                return new TweakResult { Success = true, Message = "HPET disabled. System will use fast hardware TSC timers after reboot.", RequiresReboot = true };
            }
            catch (Exception ex)
            {
                return new TweakResult { Success = false, Message = $"Error configuring HPET: {ex.Message}" };
            }
        }

        public TweakResult Restore(bool dryRun)
        {
            if (!PrivilegeManager.IsAdministrator())
                return new TweakResult { Success = false, Message = "Administrator privileges required.", DryRun = dryRun };

            if (dryRun)
                return new TweakResult { Success = true, Message = "[DRY-RUN] Would execute bcdedit /deletevalue useplatformclock", DryRun = true };

            try
            {
                RunBcdedit("/deletevalue useplatformclock");
                RunBcdedit("/deletevalue disabledynamictick");
                AuditLogger.Log("Tweaks", "Restored HPET", "useplatformclock deleted");
                return new TweakResult { Success = true, Message = "HPET settings restored to default.", RequiresReboot = true };
            }
            catch (Exception ex)
            {
                return new TweakResult { Success = false, Message = $"Error restoring HPET: {ex.Message}" };
            }
        }

        private static void RunBcdedit(string args)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "bcdedit.exe",
                Arguments = args,
                CreateNoWindow = true,
                UseShellExecute = false
            };
            using var p = Process.Start(psi);
            p?.WaitForExit(5000);
        }
    }

    // ==========================================
    // 7. CONTEXT MENU: TAKE OWNERSHIP
    // ==========================================
    public class TakeOwnershipContextMenuTweak : ITweak
    {
        public string Id => "ctx_take_ownership";
        public string Name => "Add 'Take Ownership' to Context Menu";
        public string Description => "Adds a one-click 'Take Ownership' option to the Windows Explorer right-click menu for locked files and folders.";
        public string TechnicalDetails => @"Registers HKCR\*\shell\TakeOwnership and HKCR\Directory\shell\TakeOwnership.";
        public TweakCategory Category => TweakCategory.Explorer;
        public RiskLevel Risk => RiskLevel.Safe;
        public bool RequiresAdmin => true;
        public bool IsReversible => true;
        public bool RequiresReboot => false;
        public string SupportedWindows => "10+";
        public TweakState RecommendedState => TweakState.Enabled;
        public TweakState DefaultState => TweakState.Disabled;

        public TweakState GetCurrentState()
        {
            try
            {
                using var key = Registry.ClassesRoot.OpenSubKey(@"*\shell\TakeOwnership");
                return key != null ? TweakState.Enabled : TweakState.Disabled;
            }
            catch { return TweakState.Unknown; }
        }

        public TweakResult Apply(bool dryRun)
        {
            if (!PrivilegeManager.IsAdministrator())
                return new TweakResult { Success = false, Message = "Administrator privileges required.", DryRun = dryRun };

            if (dryRun)
                return new TweakResult { Success = true, Message = "[DRY-RUN] Would add Take Ownership to HKCR.", DryRun = true };

            try
            {
                // Files
                using (var key = Registry.ClassesRoot.CreateSubKey(@"*\shell\TakeOwnership"))
                {
                    key.SetValue("", "Take Ownership");
                    key.SetValue("HasLUAShield", "");
                    key.SetValue("NoWorkingDirectory", "");
                    using var cmd = key.CreateSubKey("command");
                    cmd.SetValue("", "cmd.exe /c takeown /f \"%1\" && icacls \"%1\" /grant administrators:F");
                }

                // Directories
                using (var dirKey = Registry.ClassesRoot.CreateSubKey(@"Directory\shell\TakeOwnership"))
                {
                    dirKey.SetValue("", "Take Ownership");
                    dirKey.SetValue("HasLUAShield", "");
                    dirKey.SetValue("NoWorkingDirectory", "");
                    using var cmd = dirKey.CreateSubKey("command");
                    cmd.SetValue("", "cmd.exe /c takeown /f \"%1\" /r /d y && icacls \"%1\" /grant administrators:F /t");
                }

                AuditLogger.Log("Tweaks", "Added Take Ownership Context Menu", "HKCR\\*\\shell\\TakeOwnership");
                return new TweakResult { Success = true, Message = "'Take Ownership' added to Windows right-click menu." };
            }
            catch (Exception ex)
            {
                return new TweakResult { Success = false, Message = $"Error: {ex.Message}" };
            }
        }

        public TweakResult Restore(bool dryRun)
        {
            if (!PrivilegeManager.IsAdministrator())
                return new TweakResult { Success = false, Message = "Administrator privileges required.", DryRun = dryRun };

            if (dryRun)
                return new TweakResult { Success = true, Message = "[DRY-RUN] Would remove Take Ownership from HKCR.", DryRun = true };

            try
            {
                Registry.ClassesRoot.DeleteSubKeyTree(@"*\shell\TakeOwnership", false);
                Registry.ClassesRoot.DeleteSubKeyTree(@"Directory\shell\TakeOwnership", false);
                AuditLogger.Log("Tweaks", "Removed Take Ownership Context Menu", "HKCR");
                return new TweakResult { Success = true, Message = "'Take Ownership' removed from right-click menu." };
            }
            catch (Exception ex)
            {
                return new TweakResult { Success = false, Message = $"Error: {ex.Message}" };
            }
        }
    }

    // ==========================================
    // 8. CONTEXT MENU: OPEN WITH NOTEPAD
    // ==========================================
    public class OpenWithNotepadContextMenuTweak : ITweak
    {
        public string Id => "ctx_open_with_notepad";
        public string Name => "Add 'Open with Notepad' to Context Menu";
        public string Description => "Adds an instant 'Open with Notepad' entry to right-click menu for all file types (.log, .cfg, .ini, unknown files).";
        public string TechnicalDetails => @"Registers HKCR\*\shell\OpenWithNotepad with notepad.exe %1.";
        public TweakCategory Category => TweakCategory.Explorer;
        public RiskLevel Risk => RiskLevel.Safe;
        public bool RequiresAdmin => true;
        public bool IsReversible => true;
        public bool RequiresReboot => false;
        public string SupportedWindows => "10+";
        public TweakState RecommendedState => TweakState.Enabled;
        public TweakState DefaultState => TweakState.Disabled;

        public TweakState GetCurrentState()
        {
            try
            {
                using var key = Registry.ClassesRoot.OpenSubKey(@"*\shell\OpenWithNotepad");
                return key != null ? TweakState.Enabled : TweakState.Disabled;
            }
            catch { return TweakState.Unknown; }
        }

        public TweakResult Apply(bool dryRun)
        {
            if (!PrivilegeManager.IsAdministrator())
                return new TweakResult { Success = false, Message = "Administrator privileges required.", DryRun = dryRun };

            if (dryRun)
                return new TweakResult { Success = true, Message = "[DRY-RUN] Would add Open with Notepad to HKCR.", DryRun = true };

            try
            {
                using var key = Registry.ClassesRoot.CreateSubKey(@"*\shell\OpenWithNotepad");
                key.SetValue("", "Open with Notepad");
                key.SetValue("Icon", "notepad.exe,0");
                using var cmd = key.CreateSubKey("command");
                cmd.SetValue("", "notepad.exe \"%1\"");

                AuditLogger.Log("Tweaks", "Added Open with Notepad", "HKCR\\*\\shell\\OpenWithNotepad");
                return new TweakResult { Success = true, Message = "'Open with Notepad' added to context menu." };
            }
            catch (Exception ex)
            {
                return new TweakResult { Success = false, Message = $"Error: {ex.Message}" };
            }
        }

        public TweakResult Restore(bool dryRun)
        {
            if (!PrivilegeManager.IsAdministrator())
                return new TweakResult { Success = false, Message = "Administrator privileges required.", DryRun = dryRun };

            if (dryRun)
                return new TweakResult { Success = true, Message = "[DRY-RUN] Would remove Open with Notepad.", DryRun = true };

            try
            {
                Registry.ClassesRoot.DeleteSubKeyTree(@"*\shell\OpenWithNotepad", false);
                return new TweakResult { Success = true, Message = "'Open with Notepad' removed." };
            }
            catch (Exception ex)
            {
                return new TweakResult { Success = false, Message = $"Error: {ex.Message}" };
            }
        }
    }
}
