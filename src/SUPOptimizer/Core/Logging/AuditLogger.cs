using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SUPOptimizer.Core.Logging
{
    public class AuditEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Category { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string? PreviousValue { get; set; }
        public string? NewValue { get; set; }
        public bool Success { get; set; } = true;
        public string? ErrorMessage { get; set; }
        public bool IsAdmin { get; set; }
    }

    public static class AuditLogger
    {
        private static readonly object _lock = new();
        private static readonly List<AuditEntry> _entries = new();
        private static string _logDirectory = string.Empty;
        private static string _logFilePath = string.Empty;

        public static void Initialize(string baseDataDir)
        {
            try
            {
                _logDirectory = Path.Combine(baseDataDir, "logs");
                if (!Directory.Exists(_logDirectory))
                    Directory.CreateDirectory(_logDirectory);

                _logFilePath = Path.Combine(_logDirectory, "audit.log");

                // Pre-populate with startup entry
                Log("System", "SUPOptimizer Initialized", $"Data directory: {baseDataDir}", null, null, true, null);
            }
            catch (Exception ex)
            {
                global::System.Diagnostics.Debug.WriteLine($"Failed to initialize logger: {ex.Message}");
            }
        }

        public static void Log(
            string category,
            string action,
            string details,
            string? previousValue = null,
            string? newValue = null,
            bool success = true,
            string? errorMessage = null)
        {
            var entry = new AuditEntry
            {
                Category = category,
                Action = action,
                Details = details,
                PreviousValue = previousValue,
                NewValue = newValue,
                Success = success,
                ErrorMessage = errorMessage,
                IsAdmin = Security.PrivilegeManager.IsAdministrator()
            };

            lock (_lock)
            {
                _entries.Insert(0, entry);
                if (_entries.Count > 1000)
                    _entries.RemoveAt(_entries.Count - 1);

                try
                {
                    if (!string.IsNullOrEmpty(_logFilePath))
                    {
                        string line = $"{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{entry.Category}] [{(entry.Success ? "OK" : "FAIL")}] {entry.Action}: {entry.Details}" +
                                      (string.IsNullOrEmpty(entry.ErrorMessage) ? "" : $" (Error: {entry.ErrorMessage})");
                        File.AppendAllText(_logFilePath, line + Environment.NewLine);
                    }
                }
                catch
                {
                    // Avoid throwing from logger
                }
            }
        }

        public static List<AuditEntry> GetEntries(string? category = null, string? search = null, int limit = 200)
        {
            lock (_lock)
            {
                IEnumerable<AuditEntry> query = _entries;

                if (!string.IsNullOrWhiteSpace(category) && !category.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(e => e.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(e =>
                        e.Action.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        e.Details.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        (e.ErrorMessage != null && e.ErrorMessage.Contains(search, StringComparison.OrdinalIgnoreCase)));
                }

                return query.Take(limit).ToList();
            }
        }

        public static void Clear()
        {
            lock (_lock)
            {
                _entries.Clear();
                try
                {
                    if (!string.IsNullOrEmpty(_logFilePath) && File.Exists(_logFilePath))
                        File.Delete(_logFilePath);
                }
                catch { }
                Log("Audit", "Logs Cleared", "Audit log history cleared by user");
            }
        }

        public static string ExportJson()
        {
            lock (_lock)
            {
                return JsonSerializer.Serialize(_entries, new JsonSerializerOptions { WriteIndented = true });
            }
        }
    }
}
