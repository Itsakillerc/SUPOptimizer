using Microsoft.Win32;

namespace SUPOptimizer.Core.Tweaks.Implementations
{
    public class ShowFileExtensionsTweak : RegistryTweakBase
    {
        public override string Id => "win_show_extensions";
        public override string Name => "Show File Extensions in Explorer";
        public override string Description => "Makes file extensions (.exe, .txt, .zip, etc.) visible, enhancing security against spoofed files.";
        public override string TechnicalDetails => "Sets HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced\\HideFileExt to 0 (DWORD).";
        public override TweakCategory Category => TweakCategory.Explorer;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        protected override string ValueName => "HideFileExt";
        protected override object TargetValue => 0;
        protected override object? RestoreValue => 1;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class ShowHiddenFilesTweak : RegistryTweakBase
    {
        public override string Id => "win_show_hidden";
        public override string Name => "Show Hidden Files and Folders";
        public override string Description => "Displays hidden files and directories in File Explorer.";
        public override string TechnicalDetails => "Sets HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced\\Hidden to 1 (DWORD).";
        public override TweakCategory Category => TweakCategory.Explorer;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        protected override string ValueName => "Hidden";
        protected override object TargetValue => 1;
        protected override object? RestoreValue => 2;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class OpenThisPCTweak : RegistryTweakBase
    {
        public override string Id => "win_open_this_pc";
        public override string Name => "Open File Explorer to 'This PC' Instead of Quick Access";
        public override string Description => "Configures Explorer to open directly to drive devices and PC drives instead of recent files.";
        public override string TechnicalDetails => "Sets HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced\\LaunchTo to 1 (DWORD).";
        public override TweakCategory Category => TweakCategory.Explorer;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        protected override string ValueName => "LaunchTo";
        protected override object TargetValue => 1;
        protected override object? RestoreValue => 2;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class Win11ClassicContextMenuTweak : RegistryTweakBase
    {
        public override string Id => "win_classic_context_menu";
        public override string Name => "Windows 11 Classic Full Context Menu";
        public override string Description => "Restores the traditional full right-click context menu in Windows 11, eliminating the sluggish 'Show more options' sub-menu.";
        public override string TechnicalDetails => "Registers HKCU\\Software\\Classes\\CLSID\\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\\InprocServer32 with empty default string.";
        public override TweakCategory Category => TweakCategory.Explorer;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;
        public override string SupportedWindows => "11+";

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32";
        protected override string ValueName => "";
        protected override object TargetValue => "";
        protected override object? RestoreValue => null; // Deleting key restores modern Win11 menu
        protected override RegistryValueKind ValueKind => RegistryValueKind.String;
    }

    public class DisableErrorReportingTweak : RegistryTweakBase
    {
        public override string Id => "win_error_reporting";
        public override string Name => "Disable Windows Error Reporting (WerFault)";
        public override string Description => "Prevents Windows from freezing crashed apps to collect memory dumps and send telemetry reports.";
        public override string TechnicalDetails => "Sets HKLM\\SOFTWARE\\Microsoft\\Windows\\Windows Error Reporting\\Disabled to 1 (DWORD).";
        public override TweakCategory Category => TweakCategory.System;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => true;

        protected override RegistryHive Hive => RegistryHive.LocalMachine;
        protected override string SubKeyPath => @"SOFTWARE\Microsoft\Windows\Windows Error Reporting";
        protected override string ValueName => "Disabled";
        protected override object TargetValue => 1;
        protected override object? RestoreValue => 0;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class DisableFastStartupTweak : RegistryTweakBase
    {
        public override string Id => "win_fast_startup";
        public override string Name => "Disable Windows Fast Startup (Hybrid Boot)";
        public override string Description => "Performs clean system shutdowns, preventing NTFS partition locks, dual-boot driver corruption, and SSD wake bugs.";
        public override string TechnicalDetails => "Sets HKLM\\SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Power\\HiberbootEnabled to 0 (DWORD).";
        public override TweakCategory Category => TweakCategory.Power;
        public override RiskLevel Risk => RiskLevel.Low;
        public override bool RequiresAdmin => true;

        protected override RegistryHive Hive => RegistryHive.LocalMachine;
        protected override string SubKeyPath => @"SYSTEM\CurrentControlSet\Control\Session Manager\Power";
        protected override string ValueName => "HiberbootEnabled";
        protected override object TargetValue => 0;
        protected override object? RestoreValue => 1;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class DisableLockScreenTipsTweak : RegistryTweakBase
    {
        public override string Id => "win_lockscreen_tips";
        public override string Name => "Disable Lock Screen Spotlight Ads & Tips";
        public override string Description => "Prevents Windows from displaying trivia, advertisements, and web suggestions on the Windows lock screen.";
        public override string TechnicalDetails => @"Sets HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager\RotatingInfo_Enabled to 0 (DWORD).";
        public override TweakCategory Category => TweakCategory.Explorer;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager";
        protected override string ValueName => "RotatingInfo_Enabled";
        protected override object TargetValue => 0;
        protected override object? RestoreValue => 1;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class DisableWidgetsTaskbarTweak : RegistryTweakBase
    {
        public override string Id => "win_disable_widgets";
        public override string Name => "Disable Windows 11 Widgets Taskbar Icon";
        public override string Description => "Removes the Windows 11 Widgets news and weather icon from the taskbar, saving CPU cycles and idle RAM.";
        public override string TechnicalDetails => @"Sets HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarDa to 0 (DWORD).";
        public override TweakCategory Category => TweakCategory.Explorer;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;
        public override string SupportedWindows => "11+";

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        protected override string ValueName => "TaskbarDa";
        protected override object TargetValue => 0;
        protected override object? RestoreValue => 1;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class EndTaskRightClickTweak : RegistryTweakBase
    {
        public override string Id => "win_end_task_right_click";
        public override string Name => "Enable 'End Task' in Taskbar Right-Click Menu";
        public override string Description => "Adds an 'End Task' option directly to taskbar window previews, allowing instant forced termination of frozen apps without opening Task Manager.";
        public override string TechnicalDetails => @"Sets HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarDeveloperSettings\TaskbarEndTask to 1 (DWORD).";
        public override TweakCategory Category => TweakCategory.Explorer;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;
        public override string SupportedWindows => "11+";

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarDeveloperSettings";
        protected override string ValueName => "TaskbarEndTask";
        protected override object TargetValue => 1;
        protected override object? RestoreValue => 0;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class LastActiveClickTweak : RegistryTweakBase
    {
        public override string Id => "win_last_active_click";
        public override string Name => "Enable Taskbar 'Last Active Click' Switching";
        public override string Description => "Clicking an application's icon on the taskbar immediately switches between open windows of that application instead of showing thumbnail previews.";
        public override string TechnicalDetails => @"Sets HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\LastActiveClick to 1 (DWORD).";
        public override TweakCategory Category => TweakCategory.Explorer;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        protected override string ValueName => "LastActiveClick";
        protected override object TargetValue => 1;
        protected override object? RestoreValue => 0;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class TaskbarAlignLeftTweak : RegistryTweakBase
    {
        public override string Id => "win_taskbar_align_left";
        public override string Name => "Align Taskbar Icons to Left";
        public override string Description => "Moves the Windows 11 Start button and application icons to the left side of the taskbar, restoring the classic Windows layout.";
        public override string TechnicalDetails => @"Sets HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarAl to 0 (DWORD).";
        public override TweakCategory Category => TweakCategory.Explorer;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;
        public override string SupportedWindows => "11+";

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        protected override string ValueName => "TaskbarAl";
        protected override object TargetValue => 0;
        protected override object? RestoreValue => 1; // 1 = Center
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class HideTaskbarSearchTweak : RegistryTweakBase
    {
        public override string Id => "win_hide_taskbar_search";
        public override string Name => "Hide Search Bar from Taskbar";
        public override string Description => "Hides the search box on the taskbar to save space. Search remains instantly accessible by opening the Start Menu.";
        public override string TechnicalDetails => @"Sets HKCU\Software\Microsoft\Windows\CurrentVersion\Search\SearchboxTaskbarMode to 0 (DWORD).";
        public override TweakCategory Category => TweakCategory.Explorer;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Software\Microsoft\Windows\CurrentVersion\Search";
        protected override string ValueName => "SearchboxTaskbarMode";
        protected override object TargetValue => 0;
        protected override object? RestoreValue => 1; // 1 = Icon, 2 = Box
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class HideTaskViewTweak : RegistryTweakBase
    {
        public override string Id => "win_hide_task_view";
        public override string Name => "Hide Task View Button from Taskbar";
        public override string Description => "Removes the Task View virtual desktop button from the taskbar. Virtual desktops remain accessible via Win+Tab.";
        public override string TechnicalDetails => @"Sets HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\ShowTaskViewButton to 0 (DWORD).";
        public override TweakCategory Category => TweakCategory.Explorer;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        protected override string ValueName => "ShowTaskViewButton";
        protected override object TargetValue => 0;
        protected override object? RestoreValue => 1;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class HideHomeGalleryTweak : ITweak
    {
        public string Id => "win_hide_home_gallery";
        public string Name => "Hide Home & Gallery from File Explorer Navigation Pane";
        public string Description => "Removes the redundant 'Home' and 'Gallery' cloud-oriented entries from the File Explorer sidebar tree.";
        public string TechnicalDetails => @"Sets System.IsPinnedToNameSpaceTree to 0 for Gallery and Home CLSIDs in HKCU\Software\Classes\CLSID.";
        public TweakCategory Category => TweakCategory.Explorer;
        public RiskLevel Risk => RiskLevel.Safe;
        public bool RequiresAdmin => false;
        public bool IsReversible => true;
        public bool RequiresReboot => false;
        public string SupportedWindows => "11+";
        public TweakState RecommendedState => TweakState.Enabled;
        public TweakState DefaultState => TweakState.Disabled;

        private const string GalleryClsid = @"{e88865ea-0e1c-4e20-9aa6-ed353b7c4777}";
        private const string HomeClsid = @"{f874310e-b6b7-47dc-bc84-b9e6b38f5903}";

        public TweakState GetCurrentState()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey($@"Software\Classes\CLSID\{GalleryClsid}");
                if (key != null)
                {
                    var val = key.GetValue("System.IsPinnedToNameSpaceTree");
                    if (val is int i && i == 0) return TweakState.Enabled;
                }
                return TweakState.Disabled;
            }
            catch { return TweakState.Unknown; }
        }

        public TweakResult Apply(bool dryRun)
        {
            if (dryRun) return new TweakResult { Success = true, Message = "[PREVIEW] Would hide Home and Gallery from Explorer navigation pane.", DryRun = true };
            try
            {
                using (var gKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\CLSID\{GalleryClsid}"))
                    gKey.SetValue("System.IsPinnedToNameSpaceTree", 0, RegistryValueKind.DWord);
                using (var hKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\CLSID\{HomeClsid}"))
                    hKey.SetValue("System.IsPinnedToNameSpaceTree", 0, RegistryValueKind.DWord);
                return new TweakResult { Success = true, Message = "Home and Gallery hidden from File Explorer navigation pane." };
            }
            catch (Exception ex) { return new TweakResult { Success = false, Message = ex.Message }; }
        }

        public TweakResult Restore(bool dryRun)
        {
            if (dryRun) return new TweakResult { Success = true, Message = "[PREVIEW] Would restore Home and Gallery in Explorer navigation pane.", DryRun = true };
            try
            {
                using (var gKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\CLSID\{GalleryClsid}"))
                    gKey.SetValue("System.IsPinnedToNameSpaceTree", 1, RegistryValueKind.DWord);
                using (var hKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\CLSID\{HomeClsid}"))
                    hKey.SetValue("System.IsPinnedToNameSpaceTree", 1, RegistryValueKind.DWord);
                return new TweakResult { Success = true, Message = "Home and Gallery restored in File Explorer navigation pane." };
            }
            catch (Exception ex) { return new TweakResult { Success = false, Message = ex.Message }; }
        }
    }

    public class HideDuplicateRemovableDrivesTweak : RegistryTweakBase
    {
        public override string Id => "win_hide_duplicate_drives";
        public override string Name => "Hide Duplicate Removable Drives in Explorer Navigation Pane";
        public override string Description => "Eliminates duplicate USB flash drives and external hard drives from appearing separately outside of 'This PC' in the File Explorer sidebar.";
        public override string TechnicalDetails => @"Removes or renames HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\DelegateFolders\{F5FB2C77-0E2F-4A16-A381-3E560C68BC83}.";
        public override TweakCategory Category => TweakCategory.Explorer;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => true;

        protected override RegistryHive Hive => RegistryHive.LocalMachine;
        protected override string SubKeyPath => @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\DelegateFolders\{F5FB2C77-0E2F-4A16-A381-3E560C68BC83}";
        protected override string ValueName => "";
        protected override object TargetValue => "";
        protected override object? RestoreValue => "Removable Drives";
        protected override RegistryValueKind ValueKind => RegistryValueKind.String;

        public override TweakState GetCurrentState()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(SubKeyPath);
                return key == null ? TweakState.Enabled : TweakState.Disabled;
            }
            catch { return TweakState.Unknown; }
        }

        public override TweakResult Apply(bool dryRun)
        {
            if (dryRun) return new TweakResult { Success = true, Message = "[PREVIEW] Would remove duplicate removable drives from navigation pane.", DryRun = true };
            try
            {
                Registry.LocalMachine.DeleteSubKeyTree(SubKeyPath, false);
                return new TweakResult { Success = true, Message = "Duplicate removable drives entry removed from File Explorer sidebar." };
            }
            catch (Exception ex) { return new TweakResult { Success = false, Message = ex.Message }; }
        }

        public override TweakResult Restore(bool dryRun)
        {
            if (dryRun) return new TweakResult { Success = true, Message = "[PREVIEW] Would restore duplicate removable drives in navigation pane.", DryRun = true };
            try
            {
                using var key = Registry.LocalMachine.CreateSubKey(SubKeyPath);
                key.SetValue("", "Removable Drives");
                return new TweakResult { Success = true, Message = "Duplicate removable drives entry restored." };
            }
            catch (Exception ex) { return new TweakResult { Success = false, Message = ex.Message }; }
        }
    }

    public class RestoreThisPCFoldersTweak : ITweak
    {
        public string Id => "win_restore_this_pc_folders";
        public string Name => "Add Common Folders (Downloads, Documents, Desktop) back to 'This PC'";
        public string Description => "Restores classic Desktop, Documents, Downloads, Music, Pictures, and Videos folders directly under 'This PC' in File Explorer.";
        public string TechnicalDetails => @"Registers known folder CLSIDs under HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace.";
        public TweakCategory Category => TweakCategory.Explorer;
        public RiskLevel Risk => RiskLevel.Safe;
        public bool RequiresAdmin => true;
        public bool IsReversible => true;
        public bool RequiresReboot => false;
        public string SupportedWindows => "11+";
        public TweakState RecommendedState => TweakState.Enabled;
        public TweakState DefaultState => TweakState.Disabled;

        private static readonly Dictionary<string, string> FolderClsids = new()
        {
            { "{B4BFCC3A-DB2C-424C-B029-7FE99A87C641}", "Desktop" },
            { "{d3162b92-9365-467a-956b-92703aca08af}", "Documents" },
            { "{088e3905-0323-4b02-9826-5d99428e115f}", "Downloads" },
            { "{3dfdf296-dbec-4fb4-81d1-6a3438bcf4de}", "Music" },
            { "{24ad3ad4-a569-4530-9952-d6888c63f501}", "Pictures" },
            { "{f86fa3ab-70d2-4424-ae79-8ab222e80970}", "Videos" }
        };

        private const string NameSpacePath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace";

        public TweakState GetCurrentState()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(NameSpacePath);
                if (key == null) return TweakState.Disabled;
                var subKeys = key.GetSubKeyNames();
                bool allPresent = FolderClsids.Keys.All(k => subKeys.Contains(k, StringComparer.OrdinalIgnoreCase));
                return allPresent ? TweakState.Enabled : TweakState.Disabled;
            }
            catch { return TweakState.Unknown; }
        }

        public TweakResult Apply(bool dryRun)
        {
            if (dryRun) return new TweakResult { Success = true, Message = "[PREVIEW] Would register all 6 common user folders under 'This PC'.", DryRun = true };
            try
            {
                foreach (var kvp in FolderClsids)
                {
                    using var k = Registry.LocalMachine.CreateSubKey($@"{NameSpacePath}\{kvp.Key}");
                    k.SetValue("", kvp.Value);
                }
                return new TweakResult { Success = true, Message = "Desktop, Documents, Downloads, Music, Pictures, and Videos restored to 'This PC'." };
            }
            catch (Exception ex) { return new TweakResult { Success = false, Message = ex.Message }; }
        }

        public TweakResult Restore(bool dryRun)
        {
            if (dryRun) return new TweakResult { Success = true, Message = "[PREVIEW] Would remove common user folders from 'This PC'.", DryRun = true };
            try
            {
                foreach (var kvp in FolderClsids)
                {
                    Registry.LocalMachine.DeleteSubKeyTree($@"{NameSpacePath}\{kvp.Key}", false);
                }
                return new TweakResult { Success = true, Message = "Common user folders hidden from 'This PC'." };
            }
            catch (Exception ex) { return new TweakResult { Success = false, Message = ex.Message }; }
        }
    }

    public class DriveLettersFirstTweak : RegistryTweakBase
    {
        public override string Id => "win_drive_letters_first";
        public override string Name => "Show Drive Letters Before Drive Names in Explorer";
        public override string Description => "Displays drive letters first (e.g. '(C:) Local Disk' instead of 'Local Disk (C:)'), improving navigation speed.";
        public override string TechnicalDetails => @"Sets HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\ShowDriveLettersFirst to 4 (DWORD).";
        public override TweakCategory Category => TweakCategory.Explorer;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Software\Microsoft\Windows\CurrentVersion\Explorer";
        protected override string ValueName => "ShowDriveLettersFirst";
        protected override object TargetValue => 4;
        protected override object? RestoreValue => 0;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class DisableFolderDiscoveryTweak : RegistryTweakBase
    {
        public override string Id => "win_disable_folder_discovery";
        public override string Name => "Disable File Explorer Automatic Folder Template Sniffing";
        public override string Description => "Prevents Windows from lagging while inspecting huge directories to guess whether to display Music, Video, or Document columns.";
        public override string TechnicalDetails => @"Sets HKCU\Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags\AllFolders\Shell\FolderType to 'NotSpecified'.";
        public override TweakCategory Category => TweakCategory.Explorer;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags\AllFolders\Shell";
        protected override string ValueName => "FolderType";
        protected override object TargetValue => "NotSpecified";
        protected override object? RestoreValue => null;
        protected override RegistryValueKind ValueKind => RegistryValueKind.String;
    }

    public class DisableWindowSnappingTweak : RegistryTweakBase
    {
        public override string Id => "win_disable_window_snapping";
        public override string Name => "Disable Window Snapping & Snap Assist";
        public override string Description => "Prevents automatic resizing or rearranging of windows when dragging them to screen edges or top.";
        public override string TechnicalDetails => @"Sets HKCU\Control Panel\Desktop\WindowArrangementActive to '0' (String).";
        public override TweakCategory Category => TweakCategory.Explorer;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Control Panel\Desktop";
        protected override string ValueName => "WindowArrangementActive";
        protected override object TargetValue => "0";
        protected override object? RestoreValue => "1";
        protected override RegistryValueKind ValueKind => RegistryValueKind.String;

        public override TweakResult Apply(bool dryRun)
        {
            var res = base.Apply(dryRun);
            if (res.Success && !dryRun)
            {
                try
                {
                    using var adv = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced");
                    adv.SetValue("SnapAssist", 0, RegistryValueKind.DWord);
                    adv.SetValue("EnableSnapBar", 0, RegistryValueKind.DWord);
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
                    using var adv = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced");
                    adv.SetValue("SnapAssist", 1, RegistryValueKind.DWord);
                    adv.SetValue("EnableSnapBar", 1, RegistryValueKind.DWord);
                }
                catch { }
            }
            return res;
        }
    }

    public class AltTabWindowsOnlyTweak : RegistryTweakBase
    {
        public override string Id => "win_alt_tab_windows_only";
        public override string Name => "Exclude Edge/Browser Tabs from Alt+Tab Switcher";
        public override string Description => "Forces Alt+Tab to only switch between actual desktop application windows, removing cluttered individual browser tabs.";
        public override string TechnicalDetails => @"Sets HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\MultiTaskingAltTabFilter to 3 (DWORD).";
        public override TweakCategory Category => TweakCategory.Explorer;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        protected override string ValueName => "MultiTaskingAltTabFilter";
        protected override object TargetValue => 3;
        protected override object? RestoreValue => 0; // 0 = Open windows and all tabs
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class EnableDarkModeTweak : RegistryTweakBase
    {
        public override string Id => "win_dark_mode";
        public override string Name => "Enable Windows System & Apps Dark Theme";
        public override string Description => "Applies high-contrast, eye-strain-reducing dark mode across Windows 10/11 system shell, Explorer, and modern applications.";
        public override string TechnicalDetails => @"Sets AppsUseLightTheme and SystemUsesLightTheme to 0 (DWORD) under HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize.";
        public override TweakCategory Category => TweakCategory.Explorer;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        protected override string ValueName => "AppsUseLightTheme";
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
                    using var key = Registry.CurrentUser.CreateSubKey(SubKeyPath);
                    key.SetValue("SystemUsesLightTheme", 0, RegistryValueKind.DWord);
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
                    using var key = Registry.CurrentUser.CreateSubKey(SubKeyPath);
                    key.SetValue("SystemUsesLightTheme", 1, RegistryValueKind.DWord);
                }
                catch { }
            }
            return res;
        }
    }

    public class DisableVisualEffectsTweak : ITweak
    {
        public string Id => "win_disable_visual_effects";
        public string Name => "Disable Transparency & Window Animations (Best Performance)";
        public string Description => "Disables UI animations, fade effects, and transparency acrylics for instant, zero-lag window opening and maximum GPU responsiveness.";
        public string TechnicalDetails => @"Sets MinAnimate to 0, EnableTransparency to 0, and VisualFXSetting to 2.";
        public TweakCategory Category => TweakCategory.Explorer;
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
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                if (key != null)
                {
                    var tr = key.GetValue("EnableTransparency");
                    if (tr is int i && i == 0) return TweakState.Enabled;
                }
                return TweakState.Disabled;
            }
            catch { return TweakState.Unknown; }
        }

        public TweakResult Apply(bool dryRun)
        {
            if (dryRun) return new TweakResult { Success = true, Message = "[PREVIEW] Would disable UI animations and window transparency.", DryRun = true };
            try
            {
                using (var wm = Registry.CurrentUser.CreateSubKey(@"Control Panel\Desktop\WindowMetrics"))
                    wm.SetValue("MinAnimate", "0", RegistryValueKind.String);
                using (var th = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                    th.SetValue("EnableTransparency", 0, RegistryValueKind.DWord);
                using (var fx = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects"))
                    fx.SetValue("VisualFXSetting", 2, RegistryValueKind.DWord);
                return new TweakResult { Success = true, Message = "Transparency and UI animations disabled for maximum visual responsiveness." };
            }
            catch (Exception ex) { return new TweakResult { Success = false, Message = ex.Message }; }
        }

        public TweakResult Restore(bool dryRun)
        {
            if (dryRun) return new TweakResult { Success = true, Message = "[PREVIEW] Would restore UI animations and transparency.", DryRun = true };
            try
            {
                using (var wm = Registry.CurrentUser.CreateSubKey(@"Control Panel\Desktop\WindowMetrics"))
                    wm.SetValue("MinAnimate", "1", RegistryValueKind.String);
                using (var th = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                    th.SetValue("EnableTransparency", 1, RegistryValueKind.DWord);
                using (var fx = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects"))
                    fx.SetValue("VisualFXSetting", 0, RegistryValueKind.DWord);
                return new TweakResult { Success = true, Message = "Transparency and animations restored to Windows defaults." };
            }
            catch (Exception ex) { return new TweakResult { Success = false, Message = ex.Message }; }
        }
    }

    public class HideDesktopSpotlightIconTweak : RegistryTweakBase
    {
        public override string Id => "win_hide_spotlight_icon";
        public override string Name => "Hide 'Learn about this picture' Desktop Spotlight Icon";
        public override string Description => "Removes the unremovable Desktop Spotlight information shortcut icon from your desktop.";
        public override string TechnicalDetails => @"Sets HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel\{2cc5ca98-6485-4e9a-920e-418299616c6ced} to 1 (DWORD).";
        public override TweakCategory Category => TweakCategory.Explorer;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;
        public override string SupportedWindows => "11+";

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Software\Microsoft\Windows\CurrentVersion\Explorer\HideDesktopIcons\NewStartPanel";
        protected override string ValueName => "{2cc5ca98-6485-4e9a-920e-418299616c6ced}";
        protected override object TargetValue => 1;
        protected override object? RestoreValue => 0;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class DisableLockScreenTweak : RegistryTweakBase
    {
        public override string Id => "win_disable_lockscreen";
        public override string Name => "Disable Windows Lock Screen (Direct Login)";
        public override string Description => "Bypasses the swipe-up lock screen image and goes straight to the password/PIN entry screen on boot or wake.";
        public override string TechnicalDetails => @"Sets HKLM\SOFTWARE\Policies\Microsoft\Windows\Personalization\NoLockScreen to 1 (DWORD).";
        public override TweakCategory Category => TweakCategory.Windows;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => true;

        protected override RegistryHive Hive => RegistryHive.LocalMachine;
        protected override string SubKeyPath => @"SOFTWARE\Policies\Microsoft\Windows\Personalization";
        protected override string ValueName => "NoLockScreen";
        protected override object TargetValue => 1;
        protected override object? RestoreValue => 0;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class DisableAcrylicLogonTweak : RegistryTweakBase
    {
        public override string Id => "win_disable_acrylic_logon";
        public override string Name => "Disable Acrylic Blur on Sign-in Screen";
        public override string Description => "Displays clean, crisp logon wallpaper without the GPU-heavy acrylic background blur filter.";
        public override string TechnicalDetails => @"Sets HKLM\SOFTWARE\Policies\Microsoft\Windows\System\DisableAcrylicBackgroundOnLogon to 1 (DWORD).";
        public override TweakCategory Category => TweakCategory.Windows;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => true;

        protected override RegistryHive Hive => RegistryHive.LocalMachine;
        protected override string SubKeyPath => @"SOFTWARE\Policies\Microsoft\Windows\System";
        protected override string ValueName => "DisableAcrylicBackgroundOnLogon";
        protected override object TargetValue => 1;
        protected override object? RestoreValue => 0;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class AlwaysShowScrollbarsTweak : RegistryTweakBase
    {
        public override string Id => "win_always_show_scrollbars";
        public override string Name => "Always Show Scrollbars (Prevent Auto-Hide)";
        public override string Description => "Keeps scrollbars always visible in Settings and UWP apps, preventing them from disappearing while browsing content.";
        public override string TechnicalDetails => @"Sets HKCU\Control Panel\Accessibility\DynamicScrollbars to 0 (DWORD).";
        public override TweakCategory Category => TweakCategory.Explorer;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Control Panel\Accessibility";
        protected override string ValueName => "DynamicScrollbars";
        protected override object TargetValue => 0;
        protected override object? RestoreValue => 1;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class DisableStickyKeysShortcutTweak : RegistryTweakBase
    {
        public override string Id => "win_sticky_keys_shortcut";
        public override string Name => "Disable 5x Shift Sticky Keys Keyboard Shortcut";
        public override string Description => "Prevents accidental activation of the Sticky Keys dialog when pressing the Shift key 5 times in rapid succession during gaming or typing.";
        public override string TechnicalDetails => @"Sets HKCU\Control Panel\Accessibility\StickyKeys\Flags to '506' (String).";
        public override TweakCategory Category => TweakCategory.Explorer;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Control Panel\Accessibility\StickyKeys";
        protected override string ValueName => "Flags";
        protected override object TargetValue => "506";
        protected override object? RestoreValue => "510";
        protected override RegistryValueKind ValueKind => RegistryValueKind.String;
    }

    public class HideSettingsHomePageTweak : RegistryTweakBase
    {
        public override string Id => "win_hide_settings_home";
        public override string Name => "Hide Settings 'Home' Page and Microsoft 365 Ads";
        public override string Description => "Removes the promotional 'Home' tab from the Windows 11 Settings app, opening directly to System settings and blocking cloud subscription banners.";
        public override string TechnicalDetails => @"Sets HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer\SettingsPageVisibility to 'hide:home' (String).";
        public override TweakCategory Category => TweakCategory.Explorer;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => true;
        public override string SupportedWindows => "11+";

        protected override RegistryHive Hive => RegistryHive.LocalMachine;
        protected override string SubKeyPath => @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer";
        protected override string ValueName => "SettingsPageVisibility";
        protected override object TargetValue => "hide:home";
        protected override object? RestoreValue => null;
        protected override RegistryValueKind ValueKind => RegistryValueKind.String;
    }

    public class PreventUpdateRebootTweak : RegistryTweakBase
    {
        public override string Id => "win_prevent_update_reboot";
        public override string Name => "Prevent Automatic Restarts After Updates While Signed In";
        public override string Description => "Stops Windows Update from forcibly restarting your computer while user accounts are logged in, preventing loss of unsaved work.";
        public override string TechnicalDetails => @"Sets HKLM\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU\NoAutoRebootWithLoggedOnUsers to 1 (DWORD).";
        public override TweakCategory Category => TweakCategory.Windows;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => true;

        protected override RegistryHive Hive => RegistryHive.LocalMachine;
        protected override string SubKeyPath => @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU";
        protected override string ValueName => "NoAutoRebootWithLoggedOnUsers";
        protected override object TargetValue => 1;
        protected override object? RestoreValue => 0;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class PreventFastUpdatesTweak : RegistryTweakBase
    {
        public override string Id => "win_prevent_fast_updates";
        public override string Name => "Prevent Windows from Getting Experimental Updates Immediately";
        public override string Description => "Disables 'Get the latest updates as soon as they're available', ensuring you only receive fully tested, stable update waves.";
        public override string TechnicalDetails => @"Sets HKLM\SOFTWARE\Microsoft\WindowsUpdate\UX\Settings\IsContinuousInnovationOptedIn to 0 (DWORD).";
        public override TweakCategory Category => TweakCategory.Windows;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => true;

        protected override RegistryHive Hive => RegistryHive.LocalMachine;
        protected override string SubKeyPath => @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings";
        protected override string ValueName => "IsContinuousInnovationOptedIn";
        protected override object TargetValue => 0;
        protected override object? RestoreValue => 1;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }
}


