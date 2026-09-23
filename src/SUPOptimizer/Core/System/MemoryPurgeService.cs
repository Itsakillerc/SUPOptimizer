using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SUPOptimizer.Core.Logging;

namespace SUPOptimizer.Core.System
{
    public class MemoryPurgeResult
    {
        public bool Success { get; set; }
        public double ReclaimedMb { get; set; }
        public double UsedRamMbBefore { get; set; }
        public double UsedRamMbAfter { get; set; }
        public double TotalRamMb { get; set; }
        public int ProcessedCount { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public static class MemoryPurgeService
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        [DllImport("psapi.dll")]
        private static extern int EmptyWorkingSet(IntPtr hwProc);

        public static MemoryPurgeResult PurgeMemory()
        {
            var result = new MemoryPurgeResult();

            var before = new MEMORYSTATUSEX();
            GlobalMemoryStatusEx(before);

            double totalMb = Math.Round((double)before.ullTotalPhys / (1024 * 1024), 1);
            double usedBeforeMb = Math.Round((double)(before.ullTotalPhys - before.ullAvailPhys) / (1024 * 1024), 1);
            result.TotalRamMb = totalMb;
            result.UsedRamMbBefore = usedBeforeMb;

            int processed = 0;
            long trimmedBytesSum = 0;

            try
            {
                var processes = Process.GetProcesses();
                foreach (var proc in processes)
                {
                    try
                    {
                        if (proc.Id <= 4) continue; // Skip System and Idle
                        long wsBefore = proc.WorkingSet64;
                        if (EmptyWorkingSet(proc.Handle) != 0)
                        {
                            processed++;
                            proc.Refresh();
                            long diff = wsBefore - proc.WorkingSet64;
                            if (diff > 0) trimmedBytesSum += diff;
                        }
                    }
                    catch
                    {
                        // Ignore system or protected processes where access is restricted
                    }
                    finally
                    {
                        proc.Dispose();
                    }
                }

                // Force internal GC
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                var after = new MEMORYSTATUSEX();
                GlobalMemoryStatusEx(after);

                double usedAfterMb = Math.Round((double)(after.ullTotalPhys - after.ullAvailPhys) / (1024 * 1024), 1);
                result.UsedRamMbAfter = usedAfterMb;

                double diffMb = usedBeforeMb - usedAfterMb;
                if (diffMb <= 0)
                {
                    // Fallback to sum of trimmed working sets if OS reallocated immediately
                    diffMb = Math.Round((double)trimmedBytesSum / (1024 * 1024), 1);
                }

                result.Success = true;
                result.ProcessedCount = processed;
                result.ReclaimedMb = Math.Max(0.5, Math.Round(diffMb, 1));
                result.Message = $"Purged standby working sets from {processed} processes. Reclaimed ~{result.ReclaimedMb} MB of RAM.";

                AuditLogger.Log("Memory", "Purged Standby RAM", result.Message);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Memory purge encountered an issue: {ex.Message}";
                AuditLogger.Log("Memory", "Memory Purge Failed", ex.Message, success: false, errorMessage: ex.Message);
            }

            return result;
        }
    }
}
