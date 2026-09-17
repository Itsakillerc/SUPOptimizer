using System;
using System.Collections.Generic;
using System.Linq;
using SUPOptimizer.Core.Tweaks.Implementations;

namespace SUPOptimizer.Core.Tweaks
{
    public static class TweakRegistry
    {
        private static readonly Dictionary<string, ITweak> _tweaks = new(StringComparer.OrdinalIgnoreCase);

        static TweakRegistry()
        {
            RegisterAll();
        }

        private static void Register(ITweak tweak)
        {
            _tweaks[tweak.Id] = tweak;
        }

        private static void RegisterAll()
        {
            // Privacy Tweaks
            Register(new DisableTelemetryTweak());
            Register(new DisableAdvertisingIdTweak());
            Register(new DisableActivityHistoryTweak());
            Register(new DisableWindowsSuggestionsTweak());
            Register(new DisableBingSearchInStartTweak());
            Register(new DisableFeedbackFrequencyTweak());
            Register(new DisableLocationTrackingTweak());
            Register(new DisableCopilotTweak());
            Register(new DisableCortanaTweak());
            Register(new DisableAppLaunchTrackingTweak());
            Register(new DisableFindMyDeviceTweak());
            Register(new DisableAppLocationAccessTweak());
            Register(new DisableWindowsRecallTweak());
            Register(new DisableClickToDoTweak());
            Register(new DisableNotepadPaintAITweak());
            Register(new DisableEdgeAdsAndPromotionsTweak());
            Register(new DisableConsumerCompanionAppsTweak());
            Register(new DisableStartPhoneLinkTweak());
            Register(new DisableStartRecommendationsTweak());

            // Optimizer Tweaks
            Register(new NetworkThrottlingTweak());
            Register(new SystemResponsivenessTweak());
            Register(new DisableGameDVRRecordingTweak());
            Register(new EnableAutoGameModeTweak());
            Register(new DisableBackgroundAppsTweak());
            Register(new OptimizeMenuShowDelayTweak());
            Register(new NtfsDisableLastAccessTweak());
            Register(new EnableLongPathsTweak());
            Register(new DisableDeliveryOptimizationP2PTweak());
            Register(new DisableMouseAccelerationTweak());
            Register(new VisualPerformancePresetTweak());
            Register(new DisableStorageSenseTweak());
            Register(new DisableBitLockerAutoTweak());
            Register(new ModernStandbyNetworkTweak());
            Register(new DisableMultiplaneOverlayTweak());
            Register(new EnableNumLockStartupTweak());
            Register(new EnableVerboseBsodTweak());
            Register(new EnableVerboseLogonTweak());
            Register(new PreferIpv4Tweak());
            Register(new DisableTeredoTweak());
            Register(new BraveBrowserDebloatTweak());
            Register(new DisableGameBarPopupsTweak());
            Register(new DisableAiServiceTweak());

            // Windows & Shell Tweaks
            Register(new ShowFileExtensionsTweak());
            Register(new ShowHiddenFilesTweak());
            Register(new OpenThisPCTweak());
            Register(new Win11ClassicContextMenuTweak());
            Register(new DisableErrorReportingTweak());
            Register(new DisableFastStartupTweak());
            Register(new DisableLockScreenTipsTweak());
            Register(new DisableWidgetsTaskbarTweak());
            Register(new EndTaskRightClickTweak());
            Register(new LastActiveClickTweak());
            Register(new TaskbarAlignLeftTweak());
            Register(new HideTaskbarSearchTweak());
            Register(new HideTaskViewTweak());
            Register(new HideHomeGalleryTweak());
            Register(new HideDuplicateRemovableDrivesTweak());
            Register(new RestoreThisPCFoldersTweak());
            Register(new DriveLettersFirstTweak());
            Register(new DisableFolderDiscoveryTweak());
            Register(new DisableWindowSnappingTweak());
            Register(new AltTabWindowsOnlyTweak());
            Register(new EnableDarkModeTweak());
            Register(new DisableVisualEffectsTweak());
            Register(new HideDesktopSpotlightIconTweak());
            Register(new DisableLockScreenTweak());
            Register(new DisableAcrylicLogonTweak());
            Register(new AlwaysShowScrollbarsTweak());
            Register(new DisableStickyKeysShortcutTweak());
            Register(new HideSettingsHomePageTweak());
            Register(new PreventUpdateRebootTweak());
            Register(new PreventFastUpdatesTweak());

            // Advanced & System Tweaks
            Register(new DisableOfficeTelemetryTweak());
            Register(new DisableWindowsUpdateAutoTweak());
            Register(new DisableEdgeCopilotTweak());
            Register(new EnableUtcTimeTweak());
            Register(new DisableOneDriveSyncTweak());
            Register(new DisableHpetTweak());
            Register(new TakeOwnershipContextMenuTweak());
            Register(new OpenWithNotepadContextMenuTweak());
        }

        public static ITweak? GetTweak(string id)
        {
            _tweaks.TryGetValue(id, out var tweak);
            return tweak;
        }

        public static List<TweakDefinition> GetAllDefinitions()
        {
            return _tweaks.Values.Select(t => new TweakDefinition
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                TechnicalDetails = t.TechnicalDetails,
                Category = t.Category.ToString(),
                Risk = t.Risk.ToString(),
                RequiresAdmin = t.RequiresAdmin,
                IsReversible = t.IsReversible,
                RequiresReboot = t.RequiresReboot,
                SupportedWindows = t.SupportedWindows,
                CurrentState = t.GetCurrentState().ToString(),
                RecommendedState = t.RecommendedState.ToString(),
                DefaultState = t.DefaultState.ToString()
            }).ToList();
        }

        public static List<ITweak> GetAllTweaks()
        {
            return _tweaks.Values.ToList();
        }
    }
}
