using Microsoft.Win32;

namespace SUPOptimizer.Core.Tweaks.Implementations
{
    public class NetworkThrottlingTweak : RegistryTweakBase
    {
        public override string Id => "opt_network_throttling";
        public override string Name => "Disable Network Throttling Index";
        public override string Description => "Disables network packet throttling mechanism, improving throughput in multiplayer games and streaming.";
        public override string TechnicalDetails => "Sets HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile\\NetworkThrottlingIndex to 0xFFFFFFFF (-1).";
        public override TweakCategory Category => TweakCategory.Cpu;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => true;
        public override bool RequiresReboot => false;

        protected override RegistryHive Hive => RegistryHive.LocalMachine;
        protected override string SubKeyPath => @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile";
        protected override string ValueName => "NetworkThrottlingIndex";
        protected override object TargetValue => -1; // 0xFFFFFFFF
        protected override object? RestoreValue => 10; // Default Windows 10/11 value
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class SystemResponsivenessTweak : RegistryTweakBase
    {
        public override string Id => "opt_system_responsiveness";
        public override string Name => "Prioritize Active Process System Responsiveness";
        public override string Description => "Allocates 100% of GPU/CPU multimedia scheduling priority to foreground tasks, eliminating background reserve delay.";
        public override string TechnicalDetails => "Sets HKLM\\SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\Multimedia\\SystemProfile\\SystemResponsiveness to 0 (DWORD). Default is 20 (20% reserved for background).";
        public override TweakCategory Category => TweakCategory.Cpu;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => true;

        protected override RegistryHive Hive => RegistryHive.LocalMachine;
        protected override string SubKeyPath => @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile";
        protected override string ValueName => "SystemResponsiveness";
        protected override object TargetValue => 0;
        protected override object? RestoreValue => 20;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class DisableGameDVRRecordingTweak : RegistryTweakBase
    {
        public override string Id => "opt_game_dvr";
        public override string Name => "Disable Game DVR Background Recording";
        public override string Description => "Disables continuous background video capture of games, freeing up GPU encoder and VRAM.";
        public override string TechnicalDetails => "Sets HKCU\\System\\GameConfigStore\\GameDVR_Enabled to 0 (DWORD).";
        public override TweakCategory Category => TweakCategory.Gaming;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"System\GameConfigStore";
        protected override string ValueName => "GameDVR_Enabled";
        protected override object TargetValue => 0;
        protected override object? RestoreValue => 1;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class EnableAutoGameModeTweak : RegistryTweakBase
    {
        public override string Id => "opt_game_mode";
        public override string Name => "Enable Windows Auto Game Mode";
        public override string Description => "Ensures Windows prioritizes system resources for games and minimizes background notifications.";
        public override string TechnicalDetails => "Sets HKCU\\Software\\Microsoft\\GameBar\\AutoGameModeEnabled to 1 (DWORD).";
        public override TweakCategory Category => TweakCategory.Gaming;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Software\Microsoft\GameBar";
        protected override string ValueName => "AutoGameModeEnabled";
        protected override object TargetValue => 1;
        protected override object? RestoreValue => 0;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class DisableBackgroundAppsTweak : RegistryTweakBase
    {
        public override string Id => "opt_background_apps";
        public override string Name => "Disable Background App Execution";
        public override string Description => "Stops UWP applications from idling, pinging, and consuming RAM in the background.";
        public override string TechnicalDetails => "Sets HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\BackgroundAccessApplications\\GlobalUserDisabled to 1 (DWORD).";
        public override TweakCategory Category => TweakCategory.Ram;
        public override RiskLevel Risk => RiskLevel.Low;
        public override bool RequiresAdmin => false;

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications";
        protected override string ValueName => "GlobalUserDisabled";
        protected override object TargetValue => 1;
        protected override object? RestoreValue => 0;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class OptimizeMenuShowDelayTweak : RegistryTweakBase
    {
        public override string Id => "opt_menu_delay";
        public override string Name => "Reduce Desktop Menu Show Delay (100ms)";
        public override string Description => "Accelerates flyout and context menu hover rendering from the default 400ms delay to 100ms for a snappier feel.";
        public override string TechnicalDetails => "Sets HKCU\\Control Panel\\Desktop\\MenuShowDelay to '100' (String).";
        public override TweakCategory Category => TweakCategory.Windows;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Control Panel\Desktop";
        protected override string ValueName => "MenuShowDelay";
        protected override object TargetValue => "100";
        protected override object? RestoreValue => "400";
        protected override RegistryValueKind ValueKind => RegistryValueKind.String;
    }

    public class NtfsDisableLastAccessTweak : RegistryTweakBase
    {
        public override string Id => "opt_ntfs_last_access";
        public override string Name => "Disable NTFS Last Access Timestamps";
        public override string Description => "Eliminates redundant disk write cycles on every single file read, significantly boosting SSD longevity and I/O speed.";
        public override string TechnicalDetails => "Sets HKLM\\SYSTEM\\CurrentControlSet\\Control\\FileSystem\\NtfsDisableLastAccessUpdate to 1 (DWORD).";
        public override TweakCategory Category => TweakCategory.FileSystem;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => true;
        public override bool RequiresReboot => true;

        protected override RegistryHive Hive => RegistryHive.LocalMachine;
        protected override string SubKeyPath => @"SYSTEM\CurrentControlSet\Control\FileSystem";
        protected override string ValueName => "NtfsDisableLastAccessUpdate";
        protected override object TargetValue => 1;
        protected override object? RestoreValue => 0;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class EnableLongPathsTweak : RegistryTweakBase
    {
        public override string Id => "opt_long_paths";
        public override string Name => "Enable Win32 Long Paths (>260 Characters)";
        public override string Description => "Removes the legacy MAX_PATH 260-character limitation for files and deep folder trees.";
        public override string TechnicalDetails => "Sets HKLM\\SYSTEM\\CurrentControlSet\\Control\\FileSystem\\LongPathsEnabled to 1 (DWORD).";
        public override TweakCategory Category => TweakCategory.FileSystem;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => true;

        protected override RegistryHive Hive => RegistryHive.LocalMachine;
        protected override string SubKeyPath => @"SYSTEM\CurrentControlSet\Control\FileSystem";
        protected override string ValueName => "LongPathsEnabled";
        protected override object TargetValue => 1;
        protected override object? RestoreValue => 0;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class DisableDeliveryOptimizationP2PTweak : RegistryTweakBase
    {
        public override string Id => "opt_delivery_opt_p2p";
        public override string Name => "Disable Windows Update P2P Delivery Optimization";
        public override string Description => "Stops Windows from using your internet upload bandwidth to distribute updates to other computers on the internet or local network.";
        public override string TechnicalDetails => @"Sets HKLM\SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization\DODownloadMode to 0 (DWORD) [0 = HTTP only, no P2P sharing].";
        public override TweakCategory Category => TweakCategory.Network;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => true;

        protected override RegistryHive Hive => RegistryHive.LocalMachine;
        protected override string SubKeyPath => @"SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization";
        protected override string ValueName => "DODownloadMode";
        protected override object TargetValue => 0;
        protected override object? RestoreValue => 1;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class DisableMouseAccelerationTweak : RegistryTweakBase
    {
        public override string Id => "opt_mouse_accel";
        public override string Name => "Disable Mouse Pointer Precision (Raw 1:1 Input)";
        public override string Description => "Enforces a linear 1:1 mouse input curve by eliminating artificial cursor acceleration, essential for gaming precision and consistent muscle memory.";
        public override string TechnicalDetails => @"Sets HKCU\Control Panel\Mouse\MouseSpeed to '0' (String).";
        public override TweakCategory Category => TweakCategory.Gaming;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Control Panel\Mouse";
        protected override string ValueName => "MouseSpeed";
        protected override object TargetValue => "0";
        protected override object? RestoreValue => "1";
        protected override RegistryValueKind ValueKind => RegistryValueKind.String;
    }

    public class VisualPerformancePresetTweak : RegistryTweakBase
    {
        public override string Id => "opt_visual_fx";
        public override string Name => "Disable Window Minimize/Maximize Animation Delay";
        public override string Description => "Disables slow window minimize and maximize animations for immediate desktop UI transitions and snappier window switching.";
        public override string TechnicalDetails => @"Sets HKCU\Control Panel\Desktop\WindowMetrics\MinAnimate to '0' (String).";
        public override TweakCategory Category => TweakCategory.Windows;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Control Panel\Desktop\WindowMetrics";
        protected override string ValueName => "MinAnimate";
        protected override object TargetValue => "0";
        protected override object? RestoreValue => "1";
        protected override RegistryValueKind ValueKind => RegistryValueKind.String;
    }

    public class DisableStorageSenseTweak : RegistryTweakBase
    {
        public override string Id => "perf_disable_storage_sense";
        public override string Name => "Disable Storage Sense Automatic Disk Cleanup";
        public override string Description => "Prevents Windows Storage Sense from silently running in the background and purging temporary files or downloads without notice.";
        public override string TechnicalDetails => @"Sets HKCU\Software\Microsoft\Windows\CurrentVersion\StorageSense\Parameters\StoragePolicy\01 to 0 (DWORD).";
        public override TweakCategory Category => TweakCategory.Storage;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Software\Microsoft\Windows\CurrentVersion\StorageSense\Parameters\StoragePolicy";
        protected override string ValueName => "01";
        protected override object TargetValue => 0;
        protected override object? RestoreValue => 1;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class DisableBitLockerAutoTweak : RegistryTweakBase
    {
        public override string Id => "perf_disable_bitlocker_auto";
        public override string Name => "Disable Automatic BitLocker Device Encryption";
        public override string Description => "Prevents Windows 11 from automatically encrypting new drives or fresh installations with BitLocker, preventing unexpected BitLocker recovery lockouts.";
        public override string TechnicalDetails => @"Sets HKLM\SYSTEM\CurrentControlSet\Control\BitLocker\PreventDeviceEncryption to 1 (DWORD).";
        public override TweakCategory Category => TweakCategory.Storage;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => true;

        protected override RegistryHive Hive => RegistryHive.LocalMachine;
        protected override string SubKeyPath => @"SYSTEM\CurrentControlSet\Control\BitLocker";
        protected override string ValueName => "PreventDeviceEncryption";
        protected override object TargetValue => 1;
        protected override object? RestoreValue => 0;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class ModernStandbyNetworkTweak : RegistryTweakBase
    {
        public override string Id => "perf_modern_standby_net";
        public override string Name => "Disable Network in Modern Standby (Save Battery & Reduce Heat)";
        public override string Description => "Disconnects Wi-Fi and network adapters while in S0 Modern Standby sleep, completely preventing battery drain and laptop bag overheating.";
        public override string TechnicalDetails => @"Sets DCSettingIndex and ACSettingIndex to 0 under HKLM\SOFTWARE\Policies\Microsoft\Power\PowerSettings\f15cbf53-e384-4002-a5dd-85c7773290b9.";
        public override TweakCategory Category => TweakCategory.Power;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => true;

        protected override RegistryHive Hive => RegistryHive.LocalMachine;
        protected override string SubKeyPath => @"SOFTWARE\Policies\Microsoft\Power\PowerSettings\f15cbf53-e384-4002-a5dd-85c7773290b9";
        protected override string ValueName => "DCSettingIndex";
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
                    using var key = Registry.LocalMachine.CreateSubKey(SubKeyPath);
                    key.SetValue("ACSettingIndex", 0, RegistryValueKind.DWord);
                }
                catch { }
            }
            return res;
        }

        public override TweakResult Restore(bool dryRun)
        {
            var res = base.Restore(dryRun);
            if (res.Success && !dryRun)
            {
                try
                {
                    using var key = Registry.LocalMachine.CreateSubKey(SubKeyPath);
                    key.SetValue("ACSettingIndex", 1, RegistryValueKind.DWord);
                }
                catch { }
            }
            return res;
        }
    }

    public class DisableMultiplaneOverlayTweak : RegistryTweakBase
    {
        public override string Id => "perf_mpo_disable";
        public override string Name => "Disable Multiplane Overlay (MPO) - Fix GPU Stutter";
        public override string Description => "Disables Desktop Windows Manager MPO plane composition. Cures black screen flickers, micro-stutters, and hardware acceleration bugs on NVIDIA & AMD graphics cards.";
        public override string TechnicalDetails => @"Sets HKLM\SOFTWARE\Microsoft\Windows\Dwm\OverlayTestMode to 5 (DWORD).";
        public override TweakCategory Category => TweakCategory.Gaming;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => true;
        public override bool RequiresReboot => true;

        protected override RegistryHive Hive => RegistryHive.LocalMachine;
        protected override string SubKeyPath => @"SOFTWARE\Microsoft\Windows\Dwm";
        protected override string ValueName => "OverlayTestMode";
        protected override object TargetValue => 5;
        protected override object? RestoreValue => 0;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class EnableNumLockStartupTweak : RegistryTweakBase
    {
        public override string Id => "perf_numlock_startup";
        public override string Name => "Turn on Num Lock Automatically on Startup";
        public override string Description => "Ensures the numeric keypad is activated automatically when booting Windows and at the Windows logon prompt.";
        public override string TechnicalDetails => @"Sets InitialKeyboardIndicators to '2' in HKCU and HKU\.DEFAULT\Control Panel\Keyboard.";
        public override TweakCategory Category => TweakCategory.Input;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Control Panel\Keyboard";
        protected override string ValueName => "InitialKeyboardIndicators";
        protected override object TargetValue => "2";
        protected override object? RestoreValue => "0";
        protected override RegistryValueKind ValueKind => RegistryValueKind.String;

        public override TweakResult Apply(bool dryRun)
        {
            var res = base.Apply(dryRun);
            if (res.Success && !dryRun)
            {
                try
                {
                    using var defKey = Registry.Users.CreateSubKey(@".DEFAULT\Control Panel\Keyboard");
                    defKey.SetValue("InitialKeyboardIndicators", "2", RegistryValueKind.String);
                }
                catch { }
            }
            return res;
        }

        public override TweakResult Restore(bool dryRun)
        {
            var res = base.Restore(dryRun);
            if (res.Success && !dryRun)
            {
                try
                {
                    using var defKey = Registry.Users.CreateSubKey(@".DEFAULT\Control Panel\Keyboard");
                    defKey.SetValue("InitialKeyboardIndicators", "0", RegistryValueKind.String);
                }
                catch { }
            }
            return res;
        }
    }

    public class EnableVerboseBsodTweak : RegistryTweakBase
    {
        public override string Id => "perf_bsod_verbose";
        public override string Name => "Enable Detailed Verbose BSoD Crash Parameters";
        public override string Description => "Configures the Windows Blue Screen of Death to display precise hexadecimal bugcheck parameters, memory addresses, and driver names instead of just a smiley face.";
        public override string TechnicalDetails => @"Sets HKLM\SYSTEM\CurrentControlSet\Control\CrashControl\DisplayParameters to 1 (DWORD).";
        public override TweakCategory Category => TweakCategory.System;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => true;

        protected override RegistryHive Hive => RegistryHive.LocalMachine;
        protected override string SubKeyPath => @"SYSTEM\CurrentControlSet\Control\CrashControl";
        protected override string ValueName => "DisplayParameters";
        protected override object TargetValue => 1;
        protected override object? RestoreValue => 0;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class EnableVerboseLogonTweak : RegistryTweakBase
    {
        public override string Id => "perf_logon_verbose";
        public override string Name => "Enable Verbose Status Messages During Logon & Boot";
        public override string Description => "Displays step-by-step diagnostic information (Applying computer settings, Loading user profile, Starting services) during startup and shutdown.";
        public override string TechnicalDetails => @"Sets HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System\verbosestatus to 1 (DWORD).";
        public override TweakCategory Category => TweakCategory.System;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => true;

        protected override RegistryHive Hive => RegistryHive.LocalMachine;
        protected override string SubKeyPath => @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System";
        protected override string ValueName => "verbosestatus";
        protected override object TargetValue => 1;
        protected override object? RestoreValue => 0;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class PreferIpv4Tweak : RegistryTweakBase
    {
        public override string Id => "perf_prefer_ipv4";
        public override string Name => "Prefer IPv4 over IPv6 (Fix Network Timeouts)";
        public override string Description => "Configures Windows DNS and network adapter protocol stack to prioritize IPv4 connections, preventing DNS lookup lag and routing timeouts on IPv6-unsupported networks.";
        public override string TechnicalDetails => @"Sets HKLM\SYSTEM\CurrentControlSet\Services\Tcpip6\Parameters\DisabledComponents to 0x20 (DWORD).";
        public override TweakCategory Category => TweakCategory.Network;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => true;
        public override bool RequiresReboot => true;

        protected override RegistryHive Hive => RegistryHive.LocalMachine;
        protected override string SubKeyPath => @"SYSTEM\CurrentControlSet\Services\Tcpip6\Parameters";
        protected override string ValueName => "DisabledComponents";
        protected override object TargetValue => 0x20;
        protected override object? RestoreValue => 0x0;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class DisableTeredoTweak : ITweak
    {
        public string Id => "perf_disable_teredo";
        public string Name => "Disable Teredo IPv6 Tunneling";
        public string Description => "Disables Microsoft Teredo IPv6-over-UDP transition technology, closing listening UDP ports and preventing network interface leaks.";
        public string TechnicalDetails => "Runs netsh interface teredo set state disabled.";
        public TweakCategory Category => TweakCategory.Network;
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
                var psi = new global::System.Diagnostics.ProcessStartInfo
                {
                    FileName = "netsh.exe",
                    Arguments = "interface teredo show state",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
                using var p = global::System.Diagnostics.Process.Start(psi);
                string outStr = p?.StandardOutput.ReadToEnd() ?? "";
                p?.WaitForExit(3000);
                if (outStr.IndexOf("disabled", StringComparison.OrdinalIgnoreCase) >= 0) return TweakState.Enabled;
                return TweakState.Disabled;
            }
            catch { return TweakState.Unknown; }
        }

        public TweakResult Apply(bool dryRun)
        {
            if (dryRun) return new TweakResult { Success = true, Message = "[PREVIEW] Would run netsh interface teredo set state disabled", DryRun = true };
            try
            {
                RunNetsh("interface teredo set state disabled");
                return new TweakResult { Success = true, Message = "Teredo tunneling interface disabled." };
            }
            catch (Exception ex) { return new TweakResult { Success = false, Message = ex.Message }; }
        }

        public TweakResult Restore(bool dryRun)
        {
            if (dryRun) return new TweakResult { Success = true, Message = "[PREVIEW] Would run netsh interface teredo set state default", DryRun = true };
            try
            {
                RunNetsh("interface teredo set state default");
                return new TweakResult { Success = true, Message = "Teredo state restored to default." };
            }
            catch (Exception ex) { return new TweakResult { Success = false, Message = ex.Message }; }
        }

        private static void RunNetsh(string args)
        {
            var psi = new global::System.Diagnostics.ProcessStartInfo
            {
                FileName = "netsh.exe",
                Arguments = args,
                CreateNoWindow = true,
                UseShellExecute = false
            };
            using var p = global::System.Diagnostics.Process.Start(psi);
            p?.WaitForExit(4000);
        }
    }

    public class BraveBrowserDebloatTweak : ITweak
    {
        public string Id => "perf_brave_debloat";
        public string Name => "Brave Browser Debloat (Disable AI Leo, Crypto, News & VPN)";
        public string Description => "Applies enterprise Group Policy settings to Brave Browser, completely removing Brave Leo AI sidebar, Crypto Wallet popups, Brave Rewards, and News feeds.";
        public string TechnicalDetails => @"Configures policies in HKLM\SOFTWARE\Policies\BraveSoftware\Brave: BraveAIChatEnabled=0, BraveRewardsDisabled=1, BraveWalletDisabled=1, BraveVPNDisabled=1, IPFSEnabled=0.";
        public TweakCategory Category => TweakCategory.Privacy;
        public RiskLevel Risk => RiskLevel.Safe;
        public bool RequiresAdmin => true;
        public bool IsReversible => true;
        public bool RequiresReboot => false;
        public string SupportedWindows => "10+";
        public TweakState RecommendedState => TweakState.Enabled;
        public TweakState DefaultState => TweakState.Disabled;

        private const string BravePolicyPath = @"SOFTWARE\Policies\BraveSoftware\Brave";

        public TweakState GetCurrentState()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(BravePolicyPath);
                if (key != null && key.GetValue("BraveAIChatEnabled") is int val && val == 0)
                    return TweakState.Enabled;
                return TweakState.Disabled;
            }
            catch { return TweakState.Unknown; }
        }

        public TweakResult Apply(bool dryRun)
        {
            if (dryRun) return new TweakResult { Success = true, Message = "[PREVIEW] Would configure Brave policies to disable Leo AI, Crypto, VPN, and News.", DryRun = true };
            try
            {
                using var key = Registry.LocalMachine.CreateSubKey(BravePolicyPath);
                key.SetValue("BraveAIChatEnabled", 0, RegistryValueKind.DWord);
                key.SetValue("BraveRewardsDisabled", 1, RegistryValueKind.DWord);
                key.SetValue("BraveWalletDisabled", 1, RegistryValueKind.DWord);
                key.SetValue("BraveVPNDisabled", 1, RegistryValueKind.DWord);
                key.SetValue("IPFSEnabled", 0, RegistryValueKind.DWord);
                return new TweakResult { Success = true, Message = "Brave Browser debloated successfully: Leo AI, Crypto, Rewards, VPN, and IPFS disabled." };
            }
            catch (Exception ex) { return new TweakResult { Success = false, Message = ex.Message }; }
        }

        public TweakResult Restore(bool dryRun)
        {
            if (dryRun) return new TweakResult { Success = true, Message = "[PREVIEW] Would remove Brave debloat policies.", DryRun = true };
            try
            {
                Registry.LocalMachine.DeleteSubKeyTree(BravePolicyPath, false);
                return new TweakResult { Success = true, Message = "Brave Browser policies removed." };
            }
            catch (Exception ex) { return new TweakResult { Success = false, Message = ex.Message }; }
        }
    }

    public class DisableGameBarPopupsTweak : ITweak
    {
        public string Id => "perf_disable_gamebar_popups";
        public string Name => "Disable ms-gamingoverlay / Game Bar Popups";
        public string Description => "Disables 'You'll need a new app to open this ms-gamingoverlay link' error popups when pressing controller Guide or Xbox shortcuts.";
        public string TechnicalDetails => @"Sets AppCaptureEnabled=0 and disables ms-gamingoverlay protocol handling in HKCU and HKLM.";
        public TweakCategory Category => TweakCategory.Gaming;
        public RiskLevel Risk => RiskLevel.Safe;
        public bool RequiresAdmin => false;
        public bool IsReversible => true;
        public bool RequiresReboot => false;
        public string SupportedWindows => "10+";
        public TweakState RecommendedState => TweakState.Enabled;
        public TweakState DefaultState => TweakState.Disabled;

        public TweakState GetCurrentState()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\GameDVR");
                if (key != null && key.GetValue("AppCaptureEnabled") is int val && val == 0)
                    return TweakState.Enabled;
                return TweakState.Disabled;
            }
            catch { return TweakState.Unknown; }
        }

        public TweakResult Apply(bool dryRun)
        {
            if (dryRun) return new TweakResult { Success = true, Message = "[PREVIEW] Would disable Game Bar capture and ms-gamingoverlay popups.", DryRun = true };
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\GameDVR"))
                    key.SetValue("AppCaptureEnabled", 0, RegistryValueKind.DWord);
                using (var gcs = Registry.CurrentUser.CreateSubKey(@"System\GameConfigStore"))
                    gcs.SetValue("GameDVR_Enabled", 0, RegistryValueKind.DWord);
                return new TweakResult { Success = true, Message = "Game Bar popups and screen capture disabled." };
            }
            catch (Exception ex) { return new TweakResult { Success = false, Message = ex.Message }; }
        }

        public TweakResult Restore(bool dryRun)
        {
            if (dryRun) return new TweakResult { Success = true, Message = "[PREVIEW] Would restore Game Bar capture.", DryRun = true };
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\GameDVR"))
                    key.SetValue("AppCaptureEnabled", 1, RegistryValueKind.DWord);
                using (var gcs = Registry.CurrentUser.CreateSubKey(@"System\GameConfigStore"))
                    gcs.SetValue("GameDVR_Enabled", 1, RegistryValueKind.DWord);
                return new TweakResult { Success = true, Message = "Game Bar settings restored." };
            }
            catch (Exception ex) { return new TweakResult { Success = false, Message = ex.Message }; }
        }
    }

    public class DisableAiServiceTweak : ITweak
    {
        public string Id => "perf_disable_wsaifabric";
        public string Name => "Prevent AI Service (WSAIFabricSvc) from Starting Automatically";
        public string Description => "Configures the Windows AI Fabric Service (WSAIFabricSvc) to Manual startup, preventing background NPU and AI telemetry consumption.";
        public string TechnicalDetails => "Sets HKLM\\SYSTEM\\CurrentControlSet\\Services\\WSAIFabricSvc\\Start to 3 (Manual).";
        public TweakCategory Category => TweakCategory.System;
        public RiskLevel Risk => RiskLevel.Safe;
        public bool RequiresAdmin => true;
        public bool IsReversible => true;
        public bool RequiresReboot => false;
        public string SupportedWindows => "11+";
        public TweakState RecommendedState => TweakState.Enabled;
        public TweakState DefaultState => TweakState.Disabled;

        private const string SvcPath = @"SYSTEM\CurrentControlSet\Services\WSAIFabricSvc";

        public TweakState GetCurrentState()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(SvcPath);
                if (key == null) return TweakState.NotConfigured;
                if (key.GetValue("Start") is int val && (val == 3 || val == 4)) return TweakState.Enabled;
                return TweakState.Disabled;
            }
            catch { return TweakState.Unknown; }
        }

        public TweakResult Apply(bool dryRun)
        {
            if (dryRun) return new TweakResult { Success = true, Message = "[PREVIEW] Would set WSAIFabricSvc service startup to Manual (3).", DryRun = true };
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(SvcPath, true);
                if (key != null)
                {
                    key.SetValue("Start", 3, RegistryValueKind.DWord);
                    return new TweakResult { Success = true, Message = "Windows AI Fabric Service (WSAIFabricSvc) set to Manual startup." };
                }
                return new TweakResult { Success = true, Message = "WSAIFabricSvc service is not present on this Windows build." };
            }
            catch (Exception ex) { return new TweakResult { Success = false, Message = ex.Message }; }
        }

        public TweakResult Restore(bool dryRun)
        {
            if (dryRun) return new TweakResult { Success = true, Message = "[PREVIEW] Would restore WSAIFabricSvc startup to Automatic (2).", DryRun = true };
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(SvcPath, true);
                if (key != null)
                {
                    key.SetValue("Start", 2, RegistryValueKind.DWord);
                    return new TweakResult { Success = true, Message = "Windows AI Fabric Service (WSAIFabricSvc) restored to Automatic startup." };
                }
                return new TweakResult { Success = true, Message = "WSAIFabricSvc service is not present on this Windows build." };
            }
            catch (Exception ex) { return new TweakResult { Success = false, Message = ex.Message }; }
        }
    }
}


