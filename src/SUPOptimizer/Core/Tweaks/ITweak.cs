using System;
using SUPOptimizer.Core.Backup;

namespace SUPOptimizer.Core.Tweaks
{
    public enum RiskLevel
    {
        Safe,
        Low,
        Medium,
        High,
        VeryHigh
    }

    public enum TweakCategory
    {
        Cpu,
        Ram,
        Storage,
        Network,
        Windows,
        Power,
        FileSystem,
        Privacy,
        Gaming,
        Explorer,
        Desktop,
        System,
        Input
    }

    public enum TweakState
    {
        Enabled,
        Disabled,
        NotConfigured,
        Unknown,
        Unsupported
    }

    public class TweakResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool RequiresReboot { get; set; }
        public bool DryRun { get; set; }
        public string? ChangeSetId { get; set; }
        public string? TargetKey { get; set; }
        public object? OldValue { get; set; }
        public object? NewValue { get; set; }
    }

    public class TweakDefinition
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TechnicalDetails { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Risk { get; set; } = "Safe";
        public bool RequiresAdmin { get; set; }
        public bool IsReversible { get; set; } = true;
        public bool RequiresReboot { get; set; }
        public string SupportedWindows { get; set; } = "10+";
        public string CurrentState { get; set; } = "Unknown";
        public string RecommendedState { get; set; } = "Enabled";
        public string DefaultState { get; set; } = "Disabled";
    }

    public interface ITweak
    {
        string Id { get; }
        string Name { get; }
        string Description { get; }
        string TechnicalDetails { get; }
        TweakCategory Category { get; }
        RiskLevel Risk { get; }
        bool RequiresAdmin { get; }
        bool IsReversible { get; }
        bool RequiresReboot { get; }
        string SupportedWindows { get; }

        TweakState GetCurrentState();
        TweakState RecommendedState { get; }
        TweakState DefaultState { get; }

        TweakResult Apply(bool dryRun);
        TweakResult Restore(bool dryRun);
    }
}
