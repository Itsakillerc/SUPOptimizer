using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using SUPOptimizer.Core.Logging;
using SUPOptimizer.Core.Tweaks;

namespace SUPOptimizer.Core.System
{
    public class ConfigProfile
    {
        public string Id { get; set; } = string.Empty;
        public string Version { get; set; } = "2.0";
        public string Name { get; set; } = "Custom Setup";
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<string> EnabledTweakIds { get; set; } = new();
        public List<string> DisabledTweakIds { get; set; } = new();
        public int TweakCount => EnabledTweakIds.Count + DisabledTweakIds.Count;
        public Dictionary<string, bool> TweakStates
        {
            get
            {
                var d = new Dictionary<string, bool>();
                foreach (var id in EnabledTweakIds) d[id] = true;
                foreach (var id in DisabledTweakIds) d[id] = false;
                return d;
            }
        }
        public List<string> Tags { get; set; } = new();
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    public static class ConfigProfileService
    {
        public static ConfigProfile ExportCurrentConfiguration(string profileName = "My Windows Setup", string description = "Exported from SUPOptimizer v1.0.0")
        {
            var profile = new ConfigProfile
            {
                Id = "custom_export",
                Name = profileName,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                Tags = new List<string> { "Custom", "Backup" }
            };

            var tweaks = TweakRegistry.GetAllTweaks();
            foreach (var tweak in tweaks)
            {
                var state = tweak.GetCurrentState();
                if (state == TweakState.Enabled)
                {
                    profile.EnabledTweakIds.Add(tweak.Id);
                }
                else if (state == TweakState.Disabled)
                {
                    profile.DisabledTweakIds.Add(tweak.Id);
                }
            }

            AuditLogger.Log("Profile", "Exported Configuration", $"{profile.EnabledTweakIds.Count} enabled, {profile.DisabledTweakIds.Count} disabled");
            return profile;
        }

        public static List<ConfigProfile> GetBuiltInPresets()
        {
            return new List<ConfigProfile>
            {
                new ConfigProfile
                {
                    Id = "recommended",
                    Name = "Recommended Setup",
                    Description = "The optimal balance of privacy, shell responsiveness, and background decluttering for everyday Windows 10 & 11 use.",
                    Tags = new List<string> { "Recommended", "Safe", "Privacy" },
                    EnabledTweakIds = new List<string>
                    {
                        "privacy_telemetry", "privacy_advertising_id", "privacy_activity_history",
                        "privacy_suggestions", "privacy_bing_search", "privacy_copilot",
                        "opt_network_throttling", "opt_system_responsiveness", "opt_game_mode",
                        "opt_long_paths", "opt_delivery_opt_p2p", "opt_menu_delay",
                        "win_show_extensions", "win_show_hidden", "win_open_this_pc",
                        "win_classic_context_menu", "win_error_reporting", "win_disable_widgets",
                        "win_end_task_right_click", "win_dark_mode", "win_drive_letters_first"
                    }
                },
                new ConfigProfile
                {
                    Id = "gaming",
                    Name = "Competitive Gaming Mode",
                    Description = "Ultra-low input latency, raw mouse tracking, no background recording, disabled MPO, and prioritized foreground CPU scheduling.",
                    Tags = new List<string> { "Low Latency", "Gaming", "GPU" },
                    EnabledTweakIds = new List<string>
                    {
                        "opt_game_dvr", "opt_game_mode", "opt_mouse_accel", "perf_disable_hpet",
                        "perf_mpo_disable", "opt_network_throttling", "opt_system_responsiveness",
                        "opt_background_apps", "opt_visual_fx", "win_disable_visual_effects",
                        "perf_prefer_ipv4", "perf_disable_teredo", "perf_disable_gamebar_popups"
                    }
                },
                new ConfigProfile
                {
                    Id = "privacy",
                    Name = "Maximum Privacy & Debloat",
                    Description = "Comprehensive privacy lockdown: disables diagnostic telemetry, Copilot, Recall, location sensors, Find My Device, and app tracking.",
                    Tags = new List<string> { "Max Privacy", "Recall Off", "Debloat" },
                    EnabledTweakIds = new List<string>
                    {
                        "privacy_telemetry", "privacy_advertising_id", "privacy_activity_history",
                        "privacy_suggestions", "privacy_bing_search", "privacy_feedback",
                        "privacy_location", "privacy_copilot", "privacy_cortana",
                        "privacy_app_launch_tracking", "privacy_find_my_device", "privacy_app_location",
                        "privacy_windows_recall", "privacy_click_to_do", "privacy_notepad_paint_ai",
                        "privacy_edge_ads_recommendations", "privacy_consumer_features",
                        "privacy_start_phone_link", "privacy_start_recommendations",
                        "priv_disable_office_telemetry", "priv_disable_edge_copilot", "win_disable_onedrive_sync",
                        "perf_brave_debloat", "perf_disable_wsaifabric"
                    }
                },
                new ConfigProfile
                {
                    Id = "minimal",
                    Name = "Clean Minimal Workstation",
                    Description = "Distraction-free desktop: classic Start/taskbar alignment, clean File Explorer without Home/Gallery clutter, and full system dark mode.",
                    Tags = new List<string> { "Clean UI", "Workstation", "Explorer" },
                    EnabledTweakIds = new List<string>
                    {
                        "win_taskbar_align_left", "win_hide_taskbar_search", "win_hide_task_view",
                        "win_hide_home_gallery", "win_hide_duplicate_drives", "win_restore_this_pc_folders",
                        "win_drive_letters_first", "win_disable_folder_discovery", "win_disable_window_snapping",
                        "win_alt_tab_windows_only", "win_dark_mode", "win_show_extensions",
                        "win_show_hidden", "win_open_this_pc", "win_classic_context_menu",
                        "win_hide_spotlight_icon", "win_always_show_scrollbars", "win_sticky_keys_shortcut",
                        "win_hide_settings_home"
                    }
                }
            };
        }

        public static (int Applied, int Restored, int Failed, List<string> Messages) ApplyProfile(ConfigProfile profile, bool dryRun)
        {
            var msgs = new List<string>();
            int applied = 0;
            int restored = 0;
            int failed = 0;

            // Apply enabled tweaks
            foreach (var tid in profile.EnabledTweakIds)
            {
                var tweak = TweakRegistry.GetTweak(tid);
                if (tweak != null)
                {
                    var res = tweak.Apply(dryRun);
                    msgs.Add($"{tid} [ENABLE]: {res.Message}");
                    if (res.Success) applied++;
                    else failed++;
                }
            }

            // Restore disabled tweaks
            foreach (var tid in profile.DisabledTweakIds)
            {
                var tweak = TweakRegistry.GetTweak(tid);
                if (tweak != null)
                {
                    var res = tweak.Restore(dryRun);
                    msgs.Add($"{tid} [RESTORE]: {res.Message}");
                    if (res.Success) restored++;
                    else failed++;
                }
            }

            AuditLogger.Log("Profile", $"Applied Profile {profile.Name}", $"Applied: {applied}, Restored: {restored}, Failed: {failed}", success: failed == 0);
            return (applied, restored, failed, msgs);
        }
    }
}
