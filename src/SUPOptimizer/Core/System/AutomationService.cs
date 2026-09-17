using System;
using System.Collections.Generic;
using System.Linq;
using SUPOptimizer.Core.Logging;
using SUPOptimizer.Core.Tweaks;

namespace SUPOptimizer.Core.System
{
    public class AutomationProfile
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = "zap";
        public List<string> TweakIds { get; set; } = new();
    }

    public static class AutomationService
    {
        private static readonly List<AutomationProfile> _profiles = new()
        {
            new AutomationProfile
            {
                Id = "fresh_install",
                Name = "Fresh Install Essentials",
                Description = "Optimal baseline immediately after a clean Windows install: show file extensions, open This PC, enable long paths, disable diagnostic data, and optimize menu delay.",
                Icon = "sparkles",
                TweakIds = new List<string>
                {
                    "win_show_extensions",
                    "win_open_this_pc",
                    "opt_long_paths",
                    "privacy_telemetry",
                    "privacy_suggestions",
                    "opt_menu_delay"
                }
            },
            new AutomationProfile
            {
                Id = "safe_optimize",
                Name = "Safe Daily Optimization",
                Description = "Non-invasive performance improvements: active process responsiveness, disable network packet throttling, disable advertising ID, and SSD write reductions.",
                Icon = "shield-check",
                TweakIds = new List<string>
                {
                    "opt_network_throttling",
                    "opt_system_responsiveness",
                    "opt_ntfs_last_access",
                    "privacy_advertising_id",
                    "privacy_suggestions",
                    "opt_menu_delay"
                }
            },
            new AutomationProfile
            {
                Id = "gaming_mode",
                Name = "Ultimate Gaming Preset",
                Description = "Maximizes FPS and lowest input lag: enables Game Mode, disables Game DVR background capture, disables network throttling, and dedicates full scheduling priority to foreground game.",
                Icon = "gamepad-2",
                TweakIds = new List<string>
                {
                    "opt_game_mode",
                    "opt_game_dvr",
                    "opt_network_throttling",
                    "opt_system_responsiveness"
                }
            },
            new AutomationProfile
            {
                Id = "privacy_hardening",
                Name = "Privacy Hardening",
                Description = "Aggressively minimizes Windows diagnostic telemetry, activity history, tailored ads, Bing start menu integration, and survey feedback pings.",
                Icon = "lock",
                TweakIds = new List<string>
                {
                    "privacy_telemetry",
                    "privacy_advertising_id",
                    "privacy_activity_history",
                    "privacy_suggestions",
                    "privacy_bing_search",
                    "privacy_feedback",
                    "privacy_location"
                }
            },
            new AutomationProfile
            {
                Id = "developer_workstation",
                Name = "Developer Workstation",
                Description = "Tailored for developers: enables 260+ character long paths, shows all hidden files and extensions, disables Windows Error Reporting dumps, and disables background apps.",
                Icon = "code",
                TweakIds = new List<string>
                {
                    "opt_long_paths",
                    "win_show_extensions",
                    "win_show_hidden",
                    "win_error_reporting",
                    "opt_background_apps",
                    "opt_network_throttling"
                }
            }
        };

        public static List<AutomationProfile> GetProfiles()
        {
            return _profiles;
        }

        public static BatchApplyResult ApplyProfile(string profileId, bool dryRun)
        {
            var profile = _profiles.FirstOrDefault(p => p.Id.Equals(profileId, StringComparison.OrdinalIgnoreCase));
            if (profile == null)
            {
                return new BatchApplyResult
                {
                    DryRun = dryRun,
                    Failed = 1,
                    Results = new List<TweakResult> { new TweakResult { Success = false, Message = $"Profile '{profileId}' not found." } }
                };
            }

            AuditLogger.Log("Automation", $"Applied Profile: {profile.Name}", $"{(dryRun ? "Dry Run Preview" : "Applied")} {profile.TweakIds.Count} tweaks");
            return TweakEngine.ApplyBatch(profile.TweakIds, dryRun);
        }
    }
}
