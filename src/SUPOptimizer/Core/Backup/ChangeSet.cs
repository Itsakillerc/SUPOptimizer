using System;
using System.Collections.Generic;

namespace SUPOptimizer.Core.Backup
{
    public enum OperationType
    {
        Registry,
        Service,
        File,
        Setting
    }

    public class ChangeOperation
    {
        public OperationType Type { get; set; } = OperationType.Registry;
        public string Target { get; set; } = string.Empty;       // E.g. HKLM\SOFTWARE\... or ServiceName
        public string PropertyName { get; set; } = string.Empty; // E.g. ValueName or StartupType
        public object? PreviousValue { get; set; }
        public object? NewValue { get; set; }
        public string? ValueKind { get; set; }                    // DWord, String, etc.
    }

    public class ChangeSet
    {
        public string Id { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<ChangeOperation> Operations { get; set; } = new();
        public bool IsReversible { get; set; } = true;
        public bool IsRolledBack { get; set; } = false;
        public DateTime? RolledBackAt { get; set; }
    }
}
