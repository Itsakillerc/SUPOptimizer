using System;
using Microsoft.Win32;
using SUPOptimizer.Core.Backup;
using SUPOptimizer.Core.Logging;
using SUPOptimizer.Core.Security;

namespace SUPOptimizer.Core.Tweaks
{
    public abstract class RegistryTweakBase : ITweak
    {
        public abstract string Id { get; }
        public abstract string Name { get; }
        public abstract string Description { get; }
        public abstract string TechnicalDetails { get; }
        public abstract TweakCategory Category { get; }
        public abstract RiskLevel Risk { get; }
        public abstract bool RequiresAdmin { get; }
        public virtual bool IsReversible => true;
        public virtual bool RequiresReboot => false;
        public virtual string SupportedWindows => "10+";

        public virtual TweakState RecommendedState => TweakState.Enabled;
        public virtual TweakState DefaultState => TweakState.Disabled;

        protected abstract RegistryHive Hive { get; }
        protected abstract string SubKeyPath { get; }
        protected abstract string ValueName { get; }
        protected abstract object TargetValue { get; }
        protected abstract object? RestoreValue { get; }
        protected abstract RegistryValueKind ValueKind { get; }

        protected virtual RegistryKey GetRootKey()
        {
            return Hive switch
            {
                RegistryHive.LocalMachine => Registry.LocalMachine,
                RegistryHive.CurrentUser => Registry.CurrentUser,
                _ => throw new NotSupportedException($"Hive {Hive} is not supported")
            };
        }

        public virtual TweakState GetCurrentState()
        {
            try
            {
                using var key = GetRootKey().OpenSubKey(SubKeyPath, false);
                if (key == null)
                    return TweakState.NotConfigured;

                var currentVal = key.GetValue(ValueName);
                if (currentVal == null)
                    return TweakState.NotConfigured;

                if (ValuesEqual(currentVal, TargetValue))
                    return TweakState.Enabled;

                return TweakState.Disabled;
            }
            catch
            {
                return TweakState.Unknown;
            }
        }

        public virtual TweakResult Apply(bool dryRun)
        {
            if (RequiresAdmin && !PrivilegeManager.IsAdministrator())
            {
                return new TweakResult
                {
                    Success = false,
                    Message = "Administrator privileges required for this tweak.",
                    DryRun = dryRun
                };
            }

            try
            {
                object? currentVal = null;
                using (var readKey = GetRootKey().OpenSubKey(SubKeyPath, false))
                {
                    currentVal = readKey?.GetValue(ValueName);
                }

                string hivePrefix = Hive == RegistryHive.LocalMachine ? "HKLM" : "HKCU";
                string fullPath = $"{hivePrefix}\\{SubKeyPath}";

                if (dryRun)
                {
                    return new TweakResult
                    {
                        Success = true,
                        Message = $"[PREVIEW] Would set {fullPath}\\{ValueName} from '{(currentVal ?? "<Not Set>")}' to '{TargetValue}'",
                        DryRun = true,
                        TargetKey = $"{fullPath}\\{ValueName}",
                        OldValue = currentVal,
                        NewValue = TargetValue
                    };
                }

                // Create ChangeSet snapshot before applying
                var changeSet = new ChangeSet
                {
                    Category = Category.ToString(),
                    Description = $"Apply {Name}"
                };
                changeSet.Operations.Add(new ChangeOperation
                {
                    Type = OperationType.Registry,
                    Target = fullPath,
                    PropertyName = ValueName,
                    PreviousValue = currentVal,
                    NewValue = TargetValue,
                    ValueKind = ValueKind.ToString()
                });
                string changeSetId = BackupManager.SaveChangeSet(changeSet);

                // Apply to registry
                using (var writeKey = GetRootKey().CreateSubKey(SubKeyPath, true))
                {
                    writeKey.SetValue(ValueName, TargetValue, ValueKind);
                }

                AuditLogger.Log(Category.ToString(), $"Applied Tweak: {Name}", $"Updated {fullPath}\\{ValueName}",
                    currentVal?.ToString(), TargetValue.ToString(), true);

                return new TweakResult
                {
                    Success = true,
                    Message = $"{Name} successfully applied.",
                    RequiresReboot = RequiresReboot,
                    DryRun = false,
                    ChangeSetId = changeSetId,
                    TargetKey = $"{fullPath}\\{ValueName}",
                    OldValue = currentVal,
                    NewValue = TargetValue
                };
            }
            catch (Exception ex)
            {
                AuditLogger.Log(Category.ToString(), $"Apply Failed: {Name}", ex.Message, null, null, false, ex.Message);
                return new TweakResult
                {
                    Success = false,
                    Message = $"Error applying tweak: {ex.Message}",
                    DryRun = dryRun
                };
            }
        }

        public virtual TweakResult Restore(bool dryRun)
        {
            if (RequiresAdmin && !PrivilegeManager.IsAdministrator())
            {
                return new TweakResult
                {
                    Success = false,
                    Message = "Administrator privileges required for this tweak.",
                    DryRun = dryRun
                };
            }

            try
            {
                string hivePrefix = Hive == RegistryHive.LocalMachine ? "HKLM" : "HKCU";
                string fullPath = $"{hivePrefix}\\{SubKeyPath}";

                object? currentVal = null;
                using (var readKey = GetRootKey().OpenSubKey(SubKeyPath, false))
                {
                    currentVal = readKey?.GetValue(ValueName);
                }

                if (dryRun)
                {
                    return new TweakResult
                    {
                        Success = true,
                        Message = $"[PREVIEW] Would restore {fullPath}\\{ValueName} to '{(RestoreValue ?? "<Delete/Default>")}'",
                        DryRun = true,
                        TargetKey = $"{fullPath}\\{ValueName}",
                        OldValue = currentVal,
                        NewValue = RestoreValue
                    };
                }

                using (var key = GetRootKey().OpenSubKey(SubKeyPath, true))
                {
                    if (key != null)
                    {
                        if (RestoreValue == null)
                        {
                            key.DeleteValue(ValueName, false);
                        }
                        else
                        {
                            key.SetValue(ValueName, RestoreValue, ValueKind);
                        }
                    }
                }

                AuditLogger.Log(Category.ToString(), $"Restored Tweak: {Name}", $"Restored {fullPath}\\{ValueName}",
                    currentVal?.ToString(), RestoreValue?.ToString() ?? "<Deleted>", true);

                return new TweakResult
                {
                    Success = true,
                    Message = $"{Name} successfully restored.",
                    RequiresReboot = RequiresReboot,
                    DryRun = false,
                    TargetKey = $"{fullPath}\\{ValueName}",
                    OldValue = currentVal,
                    NewValue = RestoreValue
                };
            }
            catch (Exception ex)
            {
                AuditLogger.Log(Category.ToString(), $"Restore Failed: {Name}", ex.Message, null, null, false, ex.Message);
                return new TweakResult
                {
                    Success = false,
                    Message = $"Error restoring tweak: {ex.Message}",
                    DryRun = dryRun
                };
            }
        }

        protected virtual bool ValuesEqual(object current, object target)
        {
            if (current == null && target == null) return true;
            if (current == null || target == null) return false;

            if (current is int ci && target is int ti) return ci == ti;
            if (current is string cs && target is string ts) return string.Equals(cs, ts, StringComparison.OrdinalIgnoreCase);

            return current.ToString() == target.ToString();
        }
    }
}
