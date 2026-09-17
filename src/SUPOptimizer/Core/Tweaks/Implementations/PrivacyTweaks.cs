using Microsoft.Win32;

namespace SUPOptimizer.Core.Tweaks.Implementations
{
    public class DisableTelemetryTweak : RegistryTweakBase
    {
        public override string Id => "privacy_telemetry";
        public override string Name => "Disable Windows Diagnostic Data & Telemetry";
        public override string Description => "Reduces OS diagnostic data collection sent to Microsoft to Security/Minimum level.";
        public override string TechnicalDetails => "Sets HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\DataCollection\\AllowTelemetry to 0 (DWORD).";
        public override TweakCategory Category => TweakCategory.Privacy;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => true;
        public override bool RequiresReboot => false;

        protected override RegistryHive Hive => RegistryHive.LocalMachine;
        protected override string SubKeyPath => @"SOFTWARE\Policies\Microsoft\Windows\DataCollection";
        protected override string ValueName => "AllowTelemetry";
        protected override object TargetValue => 0;
        protected override object? RestoreValue => 1; // Basic/Standard level
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class DisableAdvertisingIdTweak : RegistryTweakBase
    {
        public override string Id => "privacy_advertising_id";
        public override string Name => "Disable Advertising ID for Tailored Ads";
        public override string Description => "Prevents applications from using your diagnostic and advertising ID to track user profile.";
        public override string TechnicalDetails => @"Sets HKCU\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo\Enabled to 0 (DWORD).";
        public override TweakCategory Category => TweakCategory.Privacy;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo";
        protected override string ValueName => "Enabled";
        protected override object TargetValue => 0;
        protected override object? RestoreValue => 1;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class DisableActivityHistoryTweak : RegistryTweakBase
    {
        public override string Id => "privacy_activity_history";
        public override string Name => "Disable Timeline Activity History";
        public override string Description => "Stops Windows from recording tasks, open documents, and timeline history.";
        public override string TechnicalDetails => "Sets HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\System\\EnableActivityFeed to 0 (DWORD).";
        public override TweakCategory Category => TweakCategory.Privacy;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => true;

        protected override RegistryHive Hive => RegistryHive.LocalMachine;
        protected override string SubKeyPath => @"SOFTWARE\Policies\Microsoft\Windows\System";
        protected override string ValueName => "EnableActivityFeed";
        protected override object TargetValue => 0;
        protected override object? RestoreValue => 1;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class DisableWindowsSuggestionsTweak : RegistryTweakBase
    {
        public override string Id => "privacy_suggestions";
        public override string Name => "Disable Windows Start & Settings Suggestions";
        public override string Description => "Blocks recommended apps, suggested store apps, and sponsored tiles in Start and Settings.";
        public override string TechnicalDetails => "Sets HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\ContentDeliveryManager\\SystemPaneSuggestionsEnabled to 0 (DWORD).";
        public override TweakCategory Category => TweakCategory.Privacy;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager";
        protected override string ValueName => "SystemPaneSuggestionsEnabled";
        protected override object TargetValue => 0;
        protected override object? RestoreValue => 1;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class DisableBingSearchInStartTweak : RegistryTweakBase
    {
        public override string Id => "privacy_bing_search";
        public override string Name => "Disable Bing Web Search in Start Menu";
        public override string Description => "Restricts Start Menu search to local files and apps, eliminating Bing internet search delay.";
        public override string TechnicalDetails => "Sets HKCU\\Software\\Policies\\Microsoft\\Windows\\Explorer\\DisableSearchBoxSuggestions to 1 (DWORD).";
        public override TweakCategory Category => TweakCategory.Privacy;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Software\Policies\Microsoft\Windows\Explorer";
        protected override string ValueName => "DisableSearchBoxSuggestions";
        protected override object TargetValue => 1;
        protected override object? RestoreValue => 0;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class DisableFeedbackFrequencyTweak : RegistryTweakBase
    {
        public override string Id => "privacy_feedback";
        public override string Name => "Disable Windows Feedback Prompts";
        public override string Description => "Prevents Windows from nagging you with feedback popups and surveys.";
        public override string TechnicalDetails => "Sets HKCU\\Software\\Microsoft\\Siuf\\Rules\\NumberOfSIUFInPeriod to 0 (DWORD).";
        public override TweakCategory Category => TweakCategory.Privacy;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Software\Microsoft\Siuf\Rules";
        protected override string ValueName => "NumberOfSIUFInPeriod";
        protected override object TargetValue => 0;
        protected override object? RestoreValue => 1;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class DisableCopilotTweak : RegistryTweakBase
    {
        public override string Id => "privacy_copilot";
        public override string Name => "Disable Windows Copilot AI Integration";
        public override string Description => "Completely disables Microsoft Copilot sidebar, taskbar icon, and cloud telemetry integration.";
        public override string TechnicalDetails => @"Sets HKCU\Software\Policies\Microsoft\Windows\WindowsCopilot\TurnOffWindowsCopilot to 1 (DWORD).";
        public override TweakCategory Category => TweakCategory.Privacy;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;
        public override string SupportedWindows => "11+";

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Software\Policies\Microsoft\Windows\WindowsCopilot";
        protected override string ValueName => "TurnOffWindowsCopilot";
        protected override object TargetValue => 1;
        protected override object? RestoreValue => 0;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class DisableCortanaTweak : RegistryTweakBase
    {
        public override string Id => "privacy_cortana";
        public override string Name => "Disable Cortana Voice Assistant & Background Indexing";
        public override string Description => "Disables Cortana background speech engine and cloud search listener.";
        public override string TechnicalDetails => @"Sets HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Search\AllowCortana to 0 (DWORD).";
        public override TweakCategory Category => TweakCategory.Privacy;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => true;

        protected override RegistryHive Hive => RegistryHive.LocalMachine;
        protected override string SubKeyPath => @"SOFTWARE\Policies\Microsoft\Windows\Windows Search";
        protected override string ValueName => "AllowCortana";
        protected override object TargetValue => 0;
        protected override object? RestoreValue => 1;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class DisableLocationTrackingTweak : RegistryTweakBase
    {
        public override string Id => "privacy_location";
        public override string Name => "Disable Windows Device Location Tracking";
        public override string Description => "Prevents Windows system services and installed desktop applications from accessing and logging physical device location coordinates.";
        public override string TechnicalDetails => @"Sets HKLM\SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors\DisableLocation to 1 (DWORD).";
        public override TweakCategory Category => TweakCategory.Privacy;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => true;

        protected override RegistryHive Hive => RegistryHive.LocalMachine;
        protected override string SubKeyPath => @"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors";
        protected override string ValueName => "DisableLocation";
        protected override object TargetValue => 1;
        protected override object? RestoreValue => 0;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class DisableAppLaunchTrackingTweak : RegistryTweakBase
    {
        public override string Id => "privacy_app_launch_tracking";
        public override string Name => "Disable Windows App Launch Tracking";
        public override string Description => "Stops Windows from monitoring and logging when and how frequently you launch applications to improve Start menu ordering.";
        public override string TechnicalDetails => @"Sets HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\Start_TrackProgs to 0 (DWORD).";
        public override TweakCategory Category => TweakCategory.Privacy;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        protected override string ValueName => "Start_TrackProgs";
        protected override object TargetValue => 0;
        protected override object? RestoreValue => 1;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class DisableFindMyDeviceTweak : RegistryTweakBase
    {
        public override string Id => "privacy_find_my_device";
        public override string Name => "Disable Windows 'Find My Device' Background Tracking";
        public override string Description => "Prevents Windows from periodically reporting your PC's geographic coordinates to Microsoft cloud servers.";
        public override string TechnicalDetails => @"Sets HKLM\SOFTWARE\Policies\Microsoft\FindMyDevice\AllowFindMyDevice to 0 (DWORD).";
        public override TweakCategory Category => TweakCategory.Privacy;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => true;

        protected override RegistryHive Hive => RegistryHive.LocalMachine;
        protected override string SubKeyPath => @"SOFTWARE\Policies\Microsoft\FindMyDevice";
        protected override string ValueName => "AllowFindMyDevice";
        protected override object TargetValue => 0;
        protected override object? RestoreValue => 1;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class DisableAppLocationAccessTweak : RegistryTweakBase
    {
        public override string Id => "privacy_app_location";
        public override string Name => "Disable Windows App Location Access Sensor";
        public override string Description => "Blocks Windows Store apps and desktop programs from querying your physical location sensor.";
        public override string TechnicalDetails => @"Sets HKCU\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location\Value to 'Deny'.";
        public override TweakCategory Category => TweakCategory.Privacy;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location";
        protected override string ValueName => "Value";
        protected override object TargetValue => "Deny";
        protected override object? RestoreValue => "Allow";
        protected override RegistryValueKind ValueKind => RegistryValueKind.String;
    }

    public class DisableWindowsRecallTweak : RegistryTweakBase
    {
        public override string Id => "privacy_windows_recall";
        public override string Name => "Disable Windows Recall AI Snapshot & Data Analysis";
        public override string Description => "Completely blocks Windows Recall from capturing periodic desktop snapshots, OCR scanning, and storing AI activity logs.";
        public override string TechnicalDetails => @"Sets DisableAIDataAnalysis to 1 (DWORD) under HKLM\SOFTWARE\Policies\Microsoft\Windows\WindowsAI and HKCU.";
        public override TweakCategory Category => TweakCategory.Privacy;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => true;
        public override string SupportedWindows => "11+";

        protected override RegistryHive Hive => RegistryHive.LocalMachine;
        protected override string SubKeyPath => @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI";
        protected override string ValueName => "DisableAIDataAnalysis";
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
                    using var cuKey = Registry.CurrentUser.CreateSubKey(@"Software\Policies\Microsoft\Windows\WindowsAI");
                    cuKey.SetValue("DisableAIDataAnalysis", 1, RegistryValueKind.DWord);
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
                    using var cuKey = Registry.CurrentUser.CreateSubKey(@"Software\Policies\Microsoft\Windows\WindowsAI");
                    cuKey.SetValue("DisableAIDataAnalysis", 0, RegistryValueKind.DWord);
                }
                catch { }
            }
            return res;
        }
    }

    public class DisableClickToDoTweak : RegistryTweakBase
    {
        public override string Id => "privacy_click_to_do";
        public override string Name => "Disable Windows AI 'Click to Do' Screen Context Actions";
        public override string Description => "Prevents Windows Copilot+ AI from analyzing screen pixels to offer interactive generative actions on text and images.";
        public override string TechnicalDetails => @"Sets HKCU\Software\Policies\Microsoft\Windows\WindowsAI\DisableClickToDo to 1 (DWORD).";
        public override TweakCategory Category => TweakCategory.Privacy;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;
        public override string SupportedWindows => "11+";

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Software\Policies\Microsoft\Windows\WindowsAI";
        protected override string ValueName => "DisableClickToDo";
        protected override object TargetValue => 1;
        protected override object? RestoreValue => 0;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class DisableNotepadPaintAITweak : ITweak
    {
        public string Id => "privacy_notepad_paint_ai";
        public string Name => "Disable AI Features in Paint & Notepad";
        public string Description => "Disables generative Cocreator AI in Paint and AI rewrite/summarization features in Windows Notepad.";
        public string TechnicalDetails => @"Configures Paint Cocreator and Notepad AI policy registry subkeys.";
        public TweakCategory Category => TweakCategory.Privacy;
        public RiskLevel Risk => RiskLevel.Safe;
        public bool RequiresAdmin => false;
        public bool IsReversible => true;
        public bool RequiresReboot => false;
        public string SupportedWindows => "11+";
        public TweakState RecommendedState => TweakState.Enabled;
        public TweakState DefaultState => TweakState.Disabled;

        public TweakState GetCurrentState()
        {
            try
            {
                using var pKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Paint\Cocreator");
                if (pKey != null && pKey.GetValue("Enabled") is int val && val == 0)
                    return TweakState.Enabled;
                return TweakState.Disabled;
            }
            catch { return TweakState.Unknown; }
        }

        public TweakResult Apply(bool dryRun)
        {
            if (dryRun) return new TweakResult { Success = true, Message = "[PREVIEW] Would disable AI features in Paint and Notepad.", DryRun = true };
            try
            {
                using (var pKey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Paint\Cocreator"))
                    pKey.SetValue("Enabled", 0, RegistryValueKind.DWord);
                using (var nKey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Notepad"))
                    nKey.SetValue("AIEnabled", 0, RegistryValueKind.DWord);
                return new TweakResult { Success = true, Message = "Generative AI features disabled in Paint and Notepad." };
            }
            catch (Exception ex) { return new TweakResult { Success = false, Message = ex.Message }; }
        }

        public TweakResult Restore(bool dryRun)
        {
            if (dryRun) return new TweakResult { Success = true, Message = "[PREVIEW] Would re-enable AI features in Paint and Notepad.", DryRun = true };
            try
            {
                using (var pKey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Paint\Cocreator"))
                    pKey.SetValue("Enabled", 1, RegistryValueKind.DWord);
                using (var nKey = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Notepad"))
                    nKey.SetValue("AIEnabled", 1, RegistryValueKind.DWord);
                return new TweakResult { Success = true, Message = "AI features in Paint and Notepad restored." };
            }
            catch (Exception ex) { return new TweakResult { Success = false, Message = ex.Message }; }
        }
    }

    public class DisableEdgeAdsAndPromotionsTweak : RegistryTweakBase
    {
        public override string Id => "privacy_edge_ads_recommendations";
        public override string Name => "Disable Microsoft Edge Ads, Shopping Tips & First-Run Promos";
        public override string Description => "Blocks promotional search recommendations, shopping assistant popups, and splash screen promotions in Microsoft Edge.";
        public override string TechnicalDetails => @"Sets HideFirstRunExperience=1, PersonalizationReportingEnabled=0, and ShowRecommendationsEnabled=0 in HKLM\SOFTWARE\Policies\Microsoft\Edge.";
        public override TweakCategory Category => TweakCategory.Privacy;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => true;

        protected override RegistryHive Hive => RegistryHive.LocalMachine;
        protected override string SubKeyPath => @"SOFTWARE\Policies\Microsoft\Edge";
        protected override string ValueName => "HideFirstRunExperience";
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
                    using var edgeKey = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Edge");
                    edgeKey.SetValue("PersonalizationReportingEnabled", 0, RegistryValueKind.DWord);
                    edgeKey.SetValue("ShowRecommendationsEnabled", 0, RegistryValueKind.DWord);
                    edgeKey.SetValue("EdgeShoppingAssistantEnabled", 0, RegistryValueKind.DWord);
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
                    using var edgeKey = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Policies\Microsoft\Edge");
                    edgeKey.SetValue("PersonalizationReportingEnabled", 1, RegistryValueKind.DWord);
                    edgeKey.SetValue("ShowRecommendationsEnabled", 1, RegistryValueKind.DWord);
                    edgeKey.SetValue("EdgeShoppingAssistantEnabled", 1, RegistryValueKind.DWord);
                }
                catch { }
            }
            return res;
        }
    }

    public class DisableConsumerCompanionAppsTweak : RegistryTweakBase
    {
        public override string Id => "privacy_consumer_features";
        public override string Name => "Prevent Auto-Installing Device Companion & OEM Apps";
        public override string Description => "Blocks Windows from silently downloading sponsored OEM companion software (such as Razer Synapse, Alienware Command Center, LG Monitor App).";
        public override string TechnicalDetails => @"Sets DisableWindowsConsumerFeatures to 1 under HKLM\SOFTWARE\Policies\Microsoft\Windows\CloudContent and blocks device metadata network pulls.";
        public override TweakCategory Category => TweakCategory.Privacy;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => true;

        protected override RegistryHive Hive => RegistryHive.LocalMachine;
        protected override string SubKeyPath => @"SOFTWARE\Policies\Microsoft\Windows\CloudContent";
        protected override string ValueName => "DisableWindowsConsumerFeatures";
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
                    using var meta = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Device Metadata");
                    meta.SetValue("PreventDeviceMetadataFromNetwork", 1, RegistryValueKind.DWord);
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
                    using var meta = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Device Metadata");
                    meta.SetValue("PreventDeviceMetadataFromNetwork", 0, RegistryValueKind.DWord);
                }
                catch { }
            }
            return res;
        }
    }

    public class DisableStartPhoneLinkTweak : RegistryTweakBase
    {
        public override string Id => "privacy_start_phone_link";
        public override string Name => "Disable Phone Link Mobile Devices in Start Menu";
        public override string Description => "Hides the mobile device side-panel companion and battery status widget in the Windows 11 Start Menu.";
        public override string TechnicalDetails => @"Sets HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\ShowPhoneLinkInStart to 0 (DWORD).";
        public override TweakCategory Category => TweakCategory.Privacy;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;
        public override string SupportedWindows => "11+";

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        protected override string ValueName => "ShowPhoneLinkInStart";
        protected override object TargetValue => 0;
        protected override object? RestoreValue => 1;
        protected override RegistryValueKind ValueKind => RegistryValueKind.DWord;
    }

    public class DisableStartRecommendationsTweak : RegistryTweakBase
    {
        public override string Id => "privacy_start_recommendations";
        public override string Name => "Hide Recommended Section & Account Promotions in Start Menu";
        public override string Description => "Cleans up the bottom half of the Windows 11 Start Menu, removing recent file suggestions and Microsoft account upsell badges.";
        public override string TechnicalDetails => @"Sets Start_IrisRecommendations and ShowRecent to 0 under HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced.";
        public override TweakCategory Category => TweakCategory.Privacy;
        public override RiskLevel Risk => RiskLevel.Safe;
        public override bool RequiresAdmin => false;
        public override string SupportedWindows => "11+";

        protected override RegistryHive Hive => RegistryHive.CurrentUser;
        protected override string SubKeyPath => @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
        protected override string ValueName => "Start_IrisRecommendations";
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
                    using var adv = Registry.CurrentUser.CreateSubKey(SubKeyPath);
                    adv.SetValue("ShowRecent", 0, RegistryValueKind.DWord);
                    adv.SetValue("Start_AccountNotifications", 0, RegistryValueKind.DWord);
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
                    using var adv = Registry.CurrentUser.CreateSubKey(SubKeyPath);
                    adv.SetValue("ShowRecent", 1, RegistryValueKind.DWord);
                    adv.SetValue("Start_AccountNotifications", 1, RegistryValueKind.DWord);
                }
                catch { }
            }
            return res;
        }
    }
}


