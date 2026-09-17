using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace SUPOptimizer.Core.System
{
    public class SystemMetrics
    {
        public double CpuUsagePercent { get; set; }

        // RAM in MB and Bytes
        public long TotalRamMb { get; set; }
        public long UsedRamMb { get; set; }
        public long FreeRamMb { get; set; }
        public double RamUsagePercent { get; set; }

        public long TotalRamBytes { get; set; }
        public long UsedRamBytes { get; set; }
        public long FreeRamBytes { get; set; }

        // Aliases for frontend flexibility
        public long RamTotalBytes => TotalRamBytes > 0 ? TotalRamBytes : TotalRamMb * 1024L * 1024L;
        public long RamUsedBytes => UsedRamBytes > 0 ? UsedRamBytes : UsedRamMb * 1024L * 1024L;
        public long RamFreeBytes => FreeRamBytes > 0 ? FreeRamBytes : FreeRamMb * 1024L * 1024L;

        // System Drive summary
        public double DiskUsagePercent { get; set; }
        public long DiskTotalBytes { get; set; }
        public long DiskFreeBytes { get; set; }
        public long DiskUsedBytes { get; set; }

        // Multi-disk support
        public List<DriveMetric> Drives { get; set; } = new();

        // System Specs & Metadata
        public string OsName { get; set; } = string.Empty;
        public string OsBuild { get; set; } = string.Empty;
        public string OsVersion => string.IsNullOrEmpty(OsBuild) ? OsName : $"{OsName} (Build {OsBuild})";
        public string Architecture { get; set; } = string.Empty;
        public string Uptime { get; set; } = string.Empty;
        public string SystemUptime => Uptime;
        public string MachineName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public bool IsAdministrator => IsAdmin;

        public string CpuModel { get; set; } = "CPU";
        public string GpuModel { get; set; } = "GPU";
        public string MotherboardModel { get; set; } = "Motherboard";

        public string VirtualizationEnvironment { get; set; } = "Physical Machine";
        public bool IsVirtualMachine { get; set; }
    }

    public class DriveMetric
    {
        public string Name { get; set; } = string.Empty;
        public string VolumeLabel { get; set; } = string.Empty;
        public string DriveFormat { get; set; } = string.Empty;
        public long TotalGb { get; set; }
        public long FreeGb { get; set; }
        public long UsedGb { get; set; }
        public double UsedPercent { get; set; }
        public long TotalBytes { get; set; }
        public long FreeBytes { get; set; }
        public long UsedBytes { get; set; }
        public bool IsSystemDrive { get; set; }
    }

    public static class SystemInfoService
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public MEMORYSTATUSEX()
            {
                dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetSystemTimes(out global::System.Runtime.InteropServices.ComTypes.FILETIME lpIdleTime,
                                                  out global::System.Runtime.InteropServices.ComTypes.FILETIME lpKernelTime,
                                                  out global::System.Runtime.InteropServices.ComTypes.FILETIME lpUserTime);

        private static ulong _prevIdleTime;
        private static ulong _prevKernelTime;
        private static ulong _prevUserTime;
        private static bool _firstSample = true;

        private static string? _cachedVmInfo;
        private static bool? _cachedIsVm;
        private static string? _cachedCpuModel;
        private static string? _cachedGpuModel;
        private static string? _cachedMotherboardModel;
        private static string? _cachedOsName;
        private static string? _cachedOsBuild;

        public static SystemMetrics GetMetrics()
        {
            var metrics = new SystemMetrics
            {
                CpuUsagePercent = Math.Round(GetCpuUsage(), 1),
                MachineName = Environment.MachineName,
                UserName = Environment.UserName,
                Architecture = RuntimeInformation.OSArchitecture.ToString(),
                IsAdmin = Security.PrivilegeManager.IsAdministrator()
            };

            // 1. RAM via GlobalMemoryStatusEx
            var memStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memStatus))
            {
                metrics.TotalRamBytes = (long)memStatus.ullTotalPhys;
                metrics.FreeRamBytes = (long)memStatus.ullAvailPhys;
                metrics.UsedRamBytes = metrics.TotalRamBytes - metrics.FreeRamBytes;

                metrics.TotalRamMb = metrics.TotalRamBytes / (1024 * 1024);
                metrics.FreeRamMb = metrics.FreeRamBytes / (1024 * 1024);
                metrics.UsedRamMb = metrics.TotalRamMb - metrics.FreeRamMb;

                metrics.RamUsagePercent = metrics.TotalRamBytes > 0
                    ? Math.Round(((double)metrics.UsedRamBytes / metrics.TotalRamBytes) * 100.0, 1)
                    : 0;
            }

            // 2. Drives (Multi-Disk Support)
            string sysDriveRoot = Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\";
            try
            {
                foreach (var drive in DriveInfo.GetDrives())
                {
                    if (drive.IsReady && (drive.DriveType == DriveType.Fixed || drive.DriveType == DriveType.Removable))
                    {
                        long totalBytes = drive.TotalSize;
                        long freeBytes = drive.AvailableFreeSpace;
                        long usedBytes = totalBytes - freeBytes;

                        long totalGb = totalBytes / (1024 * 1024 * 1024);
                        long freeGb = freeBytes / (1024 * 1024 * 1024);
                        long usedGb = totalGb - freeGb;
                        double usedPct = totalBytes > 0 ? Math.Round(((double)usedBytes / totalBytes) * 100.0, 1) : 0;
                        bool isSys = string.Equals(drive.Name, sysDriveRoot, StringComparison.OrdinalIgnoreCase);

                        metrics.Drives.Add(new DriveMetric
                        {
                            Name = drive.Name.TrimEnd('\\'),
                            VolumeLabel = string.IsNullOrEmpty(drive.VolumeLabel) ? (isSys ? "System Disk" : "Local Disk") : drive.VolumeLabel,
                            DriveFormat = drive.DriveFormat,
                            TotalGb = totalGb,
                            FreeGb = freeGb,
                            UsedGb = usedGb,
                            UsedPercent = usedPct,
                            TotalBytes = totalBytes,
                            FreeBytes = freeBytes,
                            UsedBytes = usedBytes,
                            IsSystemDrive = isSys
                        });
                    }
                }
            }
            catch { }

            // Compute primary/system drive metrics for dashboard summary card
            var primaryDrive = metrics.Drives.FirstOrDefault(d => d.IsSystemDrive) ?? metrics.Drives.FirstOrDefault();
            if (primaryDrive != null)
            {
                metrics.DiskUsagePercent = primaryDrive.UsedPercent;
                metrics.DiskTotalBytes = primaryDrive.TotalBytes;
                metrics.DiskFreeBytes = primaryDrive.FreeBytes;
                metrics.DiskUsedBytes = primaryDrive.UsedBytes;
            }

            // 3. OS Information
            if (_cachedOsName == null)
            {
                try
                {
                    using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                    if (key != null)
                    {
                        string productName = key.GetValue("ProductName")?.ToString() ?? "Windows";
                        string displayVersion = key.GetValue("DisplayVersion")?.ToString() ?? "";
                        string currentBuild = key.GetValue("CurrentBuild")?.ToString() ?? "";
                        string ubr = key.GetValue("UBR")?.ToString() ?? "";

                        _cachedOsName = $"{productName} {displayVersion}".Trim();
                        _cachedOsBuild = string.IsNullOrEmpty(ubr) ? currentBuild : $"{currentBuild}.{ubr}";
                    }
                }
                catch
                {
                    _cachedOsName = RuntimeInformation.OSDescription;
                    _cachedOsBuild = "";
                }
            }
            metrics.OsName = _cachedOsName ?? "Windows";
            metrics.OsBuild = _cachedOsBuild ?? "";

            // 4. Uptime
            var uptimeSpan = TimeSpan.FromMilliseconds(Environment.TickCount64);
            metrics.Uptime = $"{(int)uptimeSpan.TotalHours}h {uptimeSpan.Minutes}m";

            // 5. Virtualization Environment
            if (_cachedVmInfo == null)
            {
                DetectVirtualization(out string vmName, out bool isVm);
                _cachedVmInfo = vmName;
                _cachedIsVm = isVm;
            }
            metrics.VirtualizationEnvironment = _cachedVmInfo;
            metrics.IsVirtualMachine = _cachedIsVm.GetValueOrDefault();

            // 6. CPU, GPU & Motherboard Models (cached for instant sub-millisecond retrieval)
            if (_cachedCpuModel == null)
            {
                _cachedCpuModel = ReadCpuModel();
            }
            metrics.CpuModel = _cachedCpuModel;

            if (_cachedGpuModel == null)
            {
                _cachedGpuModel = ReadGpuModel();
            }
            metrics.GpuModel = _cachedGpuModel;

            if (_cachedMotherboardModel == null)
            {
                _cachedMotherboardModel = ReadMotherboardModel();
            }
            metrics.MotherboardModel = _cachedMotherboardModel;

            return metrics;
        }

        private static string ReadCpuModel()
        {
            try
            {
                using var cpuKey = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
                string? name = cpuKey?.GetValue("ProcessorNameString")?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(name)) return name;
            }
            catch { }
            return Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "Processor";
        }

        private static string ReadGpuModel()
        {
            // Try Registry Display Drivers first (fastest, zero overhead)
            try
            {
                using var displayKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}");
                if (displayKey != null)
                {
                    foreach (var subName in displayKey.GetSubKeyNames())
                    {
                        if (subName.StartsWith("000", StringComparison.OrdinalIgnoreCase))
                        {
                            using var adapterKey = displayKey.OpenSubKey(subName);
                            string? desc = adapterKey?.GetValue("DriverDesc")?.ToString();
                            if (!string.IsNullOrWhiteSpace(desc) &&
                                !desc.Contains("Basic Render", StringComparison.OrdinalIgnoreCase))
                            {
                                return desc.Trim();
                            }
                        }
                    }
                }
            }
            catch { }

            // Fallback to WMI if registry didn't produce a specific name
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController");
                foreach (ManagementObject mo in searcher.Get())
                {
                    string? name = mo["Name"]?.ToString()?.Trim();
                    if (!string.IsNullOrWhiteSpace(name))
                        return name;
                }
            }
            catch { }

            return "Display Adapter";
        }

        private static string ReadMotherboardModel()
        {
            try
            {
                using var biosKey = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\BIOS");
                if (biosKey != null)
                {
                    string mfg = biosKey.GetValue("BaseBoardManufacturer")?.ToString()?.Trim() ?? "";
                    string prod = biosKey.GetValue("BaseBoardProduct")?.ToString()?.Trim() ?? "";
                    if (!string.IsNullOrEmpty(mfg) || !string.IsNullOrEmpty(prod))
                    {
                        return $"{mfg} {prod}".Trim();
                    }
                }
            }
            catch { }
            return "System Motherboard";
        }

        private static double GetCpuUsage()
        {
            try
            {
                if (!GetSystemTimes(out var idle, out var kernel, out var user))
                    return 0;

                ulong idleTime = ((ulong)idle.dwHighDateTime << 32) | (uint)idle.dwLowDateTime;
                ulong kernelTime = ((ulong)kernel.dwHighDateTime << 32) | (uint)kernel.dwLowDateTime;
                ulong userTime = ((ulong)user.dwHighDateTime << 32) | (uint)user.dwLowDateTime;

                if (_firstSample)
                {
                    _prevIdleTime = idleTime;
                    _prevKernelTime = kernelTime;
                    _prevUserTime = userTime;
                    _firstSample = false;
                    return 0;
                }

                ulong usrDiff = userTime - _prevUserTime;
                ulong kerDiff = kernelTime - _prevKernelTime;
                ulong idlDiff = idleTime - _prevIdleTime;

                _prevIdleTime = idleTime;
                _prevKernelTime = kernelTime;
                _prevUserTime = userTime;

                ulong sysTime = usrDiff + kerDiff;
                if (sysTime == 0) return 0;

                double cpu = ((double)(sysTime - idlDiff) / sysTime) * 100.0;
                return Math.Clamp(cpu, 0.0, 100.0);
            }
            catch
            {
                return 0;
            }
        }

        private static void DetectVirtualization(out string envName, out bool isVm)
        {
            envName = "Physical Machine";
            isVm = false;

            try
            {
                using var biosKey = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\BIOS");
                if (biosKey != null)
                {
                    string mfg = biosKey.GetValue("SystemManufacturer")?.ToString() ?? "";
                    string product = biosKey.GetValue("SystemProductName")?.ToString() ?? "";
                    string biosVer = biosKey.GetValue("BIOSVersion")?.ToString() ?? "";
                    string baseBoardMfg = biosKey.GetValue("BaseBoardManufacturer")?.ToString() ?? "";
                    string baseBoardProd = biosKey.GetValue("BaseBoardProduct")?.ToString() ?? "";

                    string combined = $"{mfg} {product} {biosVer} {baseBoardMfg} {baseBoardProd}".ToUpperInvariant();

                    if (combined.Contains("HYPER-V") || (combined.Contains("MICROSOFT") && (combined.Contains("VIRTUAL") || combined.Contains("VM"))))
                    {
                        envName = "Microsoft Hyper-V Virtual Machine";
                        isVm = true;
                        return;
                    }
                    if (combined.Contains("VMWARE"))
                    {
                        envName = "VMware Virtual Platform";
                        isVm = true;
                        return;
                    }
                    if (combined.Contains("VBOX") || combined.Contains("VIRTUALBOX") || combined.Contains("INNOTEK"))
                    {
                        envName = "Oracle VirtualBox";
                        isVm = true;
                        return;
                    }
                    if (combined.Contains("QEMU") || combined.Contains("BOCHS"))
                    {
                        envName = "QEMU / KVM Virtual Machine";
                        isVm = true;
                        return;
                    }
                    if (combined.Contains("KVM") || combined.Contains("RED HAT"))
                    {
                        envName = "KVM Hypervisor";
                        isVm = true;
                        return;
                    }
                }
            }
            catch { }
        }
    }
}
