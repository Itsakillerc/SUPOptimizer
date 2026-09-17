using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace SUPOptimizer.Core.System
{
    public class HardwareSummary
    {
        public CpuInfo Cpu { get; set; } = new();
        public MemoryOverview Memory { get; set; } = new();
        public List<GpuInfo> Gpus { get; set; } = new();
        public MotherboardInfo Motherboard { get; set; } = new();
        public BiosInfo Bios { get; set; } = new();
        public List<PhysicalDiskInfo> Disks { get; set; } = new();
        public List<DriveMetric> Volumes { get; set; } = new();
        public PeripheralsInfo Peripherals { get; set; } = new();

        // Backward compatibility
        public List<MemoryModuleInfo> MemoryModules => Memory.Modules;
    }

    public class CpuInfo
    {
        public string Name { get; set; } = "Unknown CPU";
        public string Manufacturer { get; set; } = "Unknown";
        public uint NumberOfCores { get; set; } = 1;
        public uint NumberOfLogicalProcessors { get; set; } = 1;
        public uint MaxClockSpeedMhz { get; set; }
        public uint CurrentClockSpeedMhz { get; set; }
        public double CurrentUsagePercent { get; set; }
        public double L2CacheMb { get; set; }
        public double L3CacheMb { get; set; }
        public string Architecture { get; set; } = "x64";
        public string Socket { get; set; } = "CPU Socket";
    }

    public class MemoryOverview
    {
        public double TotalRamGb { get; set; }
        public double UsedRamGb { get; set; }
        public double FreeRamGb { get; set; }
        public double RamLoadPercent { get; set; }
        public int SlotsUsed { get; set; }
        public int TotalSlots { get; set; }
        public string SlotsSummary { get; set; } = string.Empty;
        public string SpeedSummary { get; set; } = string.Empty;
        public string TypeSummary { get; set; } = string.Empty;
        public long PagedPoolMb { get; set; }
        public long NonPagedPoolMb { get; set; }
        public List<MemoryModuleInfo> Modules { get; set; } = new();
    }

    public class MemoryModuleInfo
    {
        public string BankLabel { get; set; } = string.Empty;
        public string DeviceLocator { get; set; } = string.Empty;
        public double CapacityGb { get; set; }
        public string CapacityFormatted { get; set; } = string.Empty;
        public uint SpeedMhz { get; set; }
        public uint ConfiguredClockSpeedMhz { get; set; }
        public string Manufacturer { get; set; } = "Unknown";
        public string PartNumber { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string MemoryType { get; set; } = "DDR4 / DDR5";
        public string FormFactor { get; set; } = "DIMM";
    }

    public class GpuInfo
    {
        public string Name { get; set; } = "Unknown GPU";
        public string Manufacturer { get; set; } = "Unknown";
        public string DriverVersion { get; set; } = string.Empty;
        public string DriverDate { get; set; } = string.Empty;
        public string VideoProcessor { get; set; } = string.Empty;
        public long MemoryMb { get; set; }
        public string MemoryFormatted { get; set; } = string.Empty;
        public string CurrentResolution { get; set; } = string.Empty;
        public string CurrentRefreshRate { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
    }

    public class MotherboardInfo
    {
        public string Manufacturer { get; set; } = "Unknown";
        public string Product { get; set; } = "Unknown";
        public string SerialNumber { get; set; } = "Unknown";
        public string Version { get; set; } = string.Empty;
    }

    public class BiosInfo
    {
        public string Manufacturer { get; set; } = "Unknown";
        public string Version { get; set; } = "Unknown";
        public string ReleaseDate { get; set; } = "Unknown";
        public string SmbiosVersion { get; set; } = string.Empty;
    }

    public class PhysicalDiskInfo
    {
        public string Model { get; set; } = string.Empty;
        public string InterfaceType { get; set; } = "NVMe/SATA";
        public string MediaType { get; set; } = "Fixed Hard Disk";
        public long SizeGb { get; set; }
        public string SizeFormatted { get; set; } = string.Empty;
        public string Status { get; set; } = "OK";
    }

    public class PeripheralsInfo
    {
        public List<AudioDeviceInfo> AudioDevices { get; set; } = new();
        public List<MonitorInfo> Monitors { get; set; } = new();
        public List<InputDeviceInfo> InputDevices { get; set; } = new();
        public List<NetworkControllerInfo> NetworkControllers { get; set; } = new();
    }

    public class AudioDeviceInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string Status { get; set; } = "OK";
    }

    public class MonitorInfo
    {
        public string Name { get; set; } = "Display Monitor";
        public string Resolution { get; set; } = string.Empty;
        public string RefreshRate { get; set; } = string.Empty;
    }

    public class InputDeviceInfo
    {
        public string Name { get; set; } = string.Empty;
        public string DeviceType { get; set; } = "Input";
        public string Status { get; set; } = "Connected";
    }

    public class NetworkControllerInfo
    {
        public string Name { get; set; } = string.Empty;
        public string MacAddress { get; set; } = string.Empty;
        public string Speed { get; set; } = string.Empty;
        public string Status { get; set; } = "Up";
    }

    public static class HardwareService
    {
        private static HardwareSummary? _cachedSummary;
        private static DateTime _lastFetchTime = DateTime.MinValue;
        private static readonly object _cacheLock = new();

        public static HardwareSummary GetSummary()
        {
            lock (_cacheLock)
            {
                if (_cachedSummary != null && (DateTime.UtcNow - _lastFetchTime).TotalSeconds < 8)
                {
                    // Update dynamic real-time metrics
                    var m = SystemInfoService.GetMetrics();
                    _cachedSummary.Cpu.CurrentUsagePercent = m.CpuUsagePercent;
                    _cachedSummary.Memory.UsedRamGb = Math.Round(m.UsedRamMb / 1024.0, 1);
                    _cachedSummary.Memory.FreeRamGb = Math.Round(m.FreeRamMb / 1024.0, 1);
                    _cachedSummary.Memory.RamLoadPercent = m.RamUsagePercent;
                    _cachedSummary.Volumes = m.Drives;
                    return _cachedSummary;
                }

                var summary = new HardwareSummary();
                var metrics = SystemInfoService.GetMetrics();

                // 1. CPU
                summary.Cpu = PopulateCpuInfo(metrics);

                // 2. RAM & Physical Memory Modules
                summary.Memory = PopulateMemoryInfo(metrics);

                // 3. Motherboard & BIOS
                PopulateMotherboardAndBios(summary);

                // 4. GPUs
                summary.Gpus = PopulateGpuInfo();

                // 5. Physical Storage Disks & Volumes
                summary.Disks = PopulateDiskInfo();
                summary.Volumes = metrics.Drives;

                // 6. Peripherals & Connected Devices
                summary.Peripherals = PopulatePeripherals();

                _cachedSummary = summary;
                _lastFetchTime = DateTime.UtcNow;
                return summary;
            }
        }

        private static CpuInfo PopulateCpuInfo(SystemMetrics metrics)
        {
            var cpu = new CpuInfo
            {
                Name = metrics.CpuModel,
                NumberOfLogicalProcessors = (uint)Environment.ProcessorCount,
                NumberOfCores = (uint)Math.Max(1, Environment.ProcessorCount / 2),
                CurrentUsagePercent = metrics.CpuUsagePercent,
                Architecture = RuntimeInformation.OSArchitecture.ToString()
            };

            // Registry CPU info (instant)
            try
            {
                using var cpuKey = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
                if (cpuKey != null)
                {
                    string? name = cpuKey.GetValue("ProcessorNameString")?.ToString()?.Trim();
                    if (!string.IsNullOrEmpty(name)) cpu.Name = name;

                    cpu.MaxClockSpeedMhz = Convert.ToUInt32(cpuKey.GetValue("~MHz") ?? 0);
                    cpu.CurrentClockSpeedMhz = cpu.MaxClockSpeedMhz;
                    cpu.Manufacturer = cpuKey.GetValue("VendorIdentifier")?.ToString() ?? "CPU Vendor";
                }
            }
            catch { }

            // WMI CPU Details for exact physical cores, cache & socket
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT NumberOfCores, NumberOfLogicalProcessors, MaxClockSpeed, CurrentClockSpeed, L2CacheSize, L3CacheSize, SocketDesignation, Manufacturer FROM Win32_Processor");
                foreach (ManagementObject mo in searcher.Get())
                {
                    if (mo["NumberOfCores"] != null)
                        cpu.NumberOfCores = Convert.ToUInt32(mo["NumberOfCores"]);
                    if (mo["NumberOfLogicalProcessors"] != null)
                        cpu.NumberOfLogicalProcessors = Convert.ToUInt32(mo["NumberOfLogicalProcessors"]);
                    if (mo["MaxClockSpeed"] != null)
                        cpu.MaxClockSpeedMhz = Convert.ToUInt32(mo["MaxClockSpeed"]);
                    if (mo["CurrentClockSpeed"] != null)
                        cpu.CurrentClockSpeedMhz = Convert.ToUInt32(mo["CurrentClockSpeed"]);
                    if (mo["L2CacheSize"] != null)
                        cpu.L2CacheMb = Math.Round(Convert.ToDouble(mo["L2CacheSize"]) / 1024.0, 2);
                    if (mo["L3CacheSize"] != null)
                        cpu.L3CacheMb = Math.Round(Convert.ToDouble(mo["L3CacheSize"]) / 1024.0, 2);
                    if (mo["SocketDesignation"] != null)
                        cpu.Socket = mo["SocketDesignation"]?.ToString() ?? cpu.Socket;
                    if (mo["Manufacturer"] != null)
                        cpu.Manufacturer = mo["Manufacturer"]?.ToString() ?? cpu.Manufacturer;
                    break;
                }
            }
            catch { }

            return cpu;
        }

        private static MemoryOverview PopulateMemoryInfo(SystemMetrics metrics)
        {
            var mem = new MemoryOverview
            {
                TotalRamGb = Math.Round(metrics.TotalRamMb / 1024.0, 1),
                UsedRamGb = Math.Round(metrics.UsedRamMb / 1024.0, 1),
                FreeRamGb = Math.Round(metrics.FreeRamMb / 1024.0, 1),
                RamLoadPercent = metrics.RamUsagePercent
            };

            // Total slots via Win32_PhysicalMemoryArray
            try
            {
                using var arraySearcher = new ManagementObjectSearcher("SELECT MemoryDevices FROM Win32_PhysicalMemoryArray");
                foreach (ManagementObject mo in arraySearcher.Get())
                {
                    if (mo["MemoryDevices"] != null)
                    {
                        mem.TotalSlots = Convert.ToInt32(mo["MemoryDevices"]);
                        break;
                    }
                }
            }
            catch { }

            // Physical Memory Modules via Win32_PhysicalMemory
            try
            {
                using var memSearcher = new ManagementObjectSearcher("SELECT BankLabel, DeviceLocator, Capacity, Speed, ConfiguredClockSpeed, Manufacturer, PartNumber, SerialNumber, MemoryType, SMBIOSMemoryType, FormFactor FROM Win32_PhysicalMemory");
                foreach (ManagementObject mo in memSearcher.Get())
                {
                    ulong capacityBytes = 0;
                    if (mo["Capacity"] != null)
                        ulong.TryParse(mo["Capacity"].ToString(), out capacityBytes);

                    double capGb = Math.Round(capacityBytes / (1024.0 * 1024.0 * 1024.0), 1);
                    uint speed = mo["Speed"] != null ? Convert.ToUInt32(mo["Speed"]) : 0;
                    uint cfgSpeed = mo["ConfiguredClockSpeed"] != null ? Convert.ToUInt32(mo["ConfiguredClockSpeed"]) : speed;

                    int memTypeVal = mo["SMBIOSMemoryType"] != null ? Convert.ToInt32(mo["SMBIOSMemoryType"])
                                  : (mo["MemoryType"] != null ? Convert.ToInt32(mo["MemoryType"]) : 0);

                    string typeStr = DecodeMemoryType(memTypeVal);

                    int formVal = mo["FormFactor"] != null ? Convert.ToInt32(mo["FormFactor"]) : 0;
                    string formStr = formVal == 12 ? "SODIMM" : "DIMM";

                    string mfg = mo["Manufacturer"]?.ToString()?.Trim() ?? "Standard";
                    if (string.IsNullOrEmpty(mfg) || mfg.Equals("None", StringComparison.OrdinalIgnoreCase))
                        mfg = "OEM Memory";

                    string part = mo["PartNumber"]?.ToString()?.Trim() ?? "";
                    if (part.Equals("None", StringComparison.OrdinalIgnoreCase)) part = "";

                    string serial = mo["SerialNumber"]?.ToString()?.Trim() ?? "";
                    if (serial.Equals("None", StringComparison.OrdinalIgnoreCase)) serial = "";

                    string locator = mo["DeviceLocator"]?.ToString()?.Trim() ?? "";
                    string bank = mo["BankLabel"]?.ToString()?.Trim() ?? "";

                    mem.Modules.Add(new MemoryModuleInfo
                    {
                        BankLabel = string.IsNullOrEmpty(bank) || bank.Equals("None", StringComparison.OrdinalIgnoreCase) ? (string.IsNullOrEmpty(locator) ? $"Slot {mem.Modules.Count + 1}" : locator) : bank,
                        DeviceLocator = locator,
                        CapacityGb = capGb,
                        CapacityFormatted = capGb >= 1.0 ? $"{capGb:0.#} GB" : $"{capacityBytes / (1024 * 1024)} MB",
                        SpeedMhz = speed > 0 ? speed : cfgSpeed,
                        ConfiguredClockSpeedMhz = cfgSpeed > 0 ? cfgSpeed : speed,
                        Manufacturer = mfg,
                        PartNumber = part,
                        SerialNumber = serial,
                        MemoryType = typeStr,
                        FormFactor = formStr
                    });
                }
            }
            catch { }

            mem.SlotsUsed = mem.Modules.Count;
            if (mem.TotalSlots < mem.SlotsUsed)
                mem.TotalSlots = Math.Max(mem.SlotsUsed, 2);

            mem.SlotsSummary = $"{mem.SlotsUsed} of {mem.TotalSlots} Slots Populated";

            if (mem.Modules.Count > 0)
            {
                uint maxSpeed = mem.Modules.Max(m => Math.Max(m.SpeedMhz, m.ConfiguredClockSpeedMhz));
                mem.SpeedSummary = maxSpeed > 0 ? $"{maxSpeed} MHz" : "Standard Speed";
                mem.TypeSummary = mem.Modules[0].MemoryType;
            }
            else
            {
                mem.SlotsSummary = "Physical Memory Subsystem";
                mem.SpeedSummary = "Standard";
                mem.TypeSummary = "Unified RAM";
            }

            return mem;
        }

        private static string DecodeMemoryType(int smbiosType)
        {
            return smbiosType switch
            {
                20 => "DDR",
                21 => "DDR2",
                24 => "DDR3",
                26 => "DDR4",
                30 => "LPDDR4",
                34 => "DDR5",
                35 => "LPDDR5",
                _ => "DDR4 / DDR5"
            };
        }

        private static void PopulateMotherboardAndBios(HardwareSummary summary)
        {
            // Registry (instant)
            try
            {
                using var biosKey = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\BIOS");
                if (biosKey != null)
                {
                    summary.Motherboard.Manufacturer = biosKey.GetValue("BaseBoardManufacturer")?.ToString()?.Trim() ?? "System Board";
                    summary.Motherboard.Product = biosKey.GetValue("BaseBoardProduct")?.ToString()?.Trim() ?? "Motherboard";
                    summary.Motherboard.SerialNumber = biosKey.GetValue("BaseBoardSerialNumber")?.ToString()?.Trim() ?? "N/A";
                    summary.Motherboard.Version = biosKey.GetValue("BaseBoardVersion")?.ToString()?.Trim() ?? "1.0";

                    summary.Bios.Manufacturer = biosKey.GetValue("BIOSVendor")?.ToString()?.Trim() ?? "UEFI Vendor";
                    summary.Bios.Version = biosKey.GetValue("BIOSVersion")?.ToString()?.Trim() ?? "1.0";
                    summary.Bios.ReleaseDate = biosKey.GetValue("BIOSReleaseDate")?.ToString()?.Trim() ?? "";
                }
            }
            catch { }
        }

        private static List<GpuInfo> PopulateGpuInfo()
        {
            var gpus = new List<GpuInfo>();

            // 1. WMI Query for Video Controllers (returns real GPU name, VRAM bytes, driver date/version, resolution)
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Name, DriverVersion, DriverDate, AdapterRAM, VideoProcessor, CurrentHorizontalResolution, CurrentVerticalResolution, CurrentRefreshRate, Status FROM Win32_VideoController");
                foreach (ManagementObject mo in searcher.Get())
                {
                    string name = mo["Name"]?.ToString()?.Trim() ?? "";
                    if (string.IsNullOrEmpty(name)) continue;

                    long vramBytes = 0;
                    if (mo["AdapterRAM"] != null)
                        long.TryParse(mo["AdapterRAM"].ToString(), out vramBytes);

                    long vramMb = vramBytes / (1024 * 1024);
                    string vramFormatted = vramMb > 0
                        ? (vramMb >= 1024 ? $"{vramMb / 1024.0:F1} GB ({vramMb} MB)" : $"{vramMb} MB")
                        : "Shared Dynamic VRAM";

                    string res = string.Empty;
                    if (mo["CurrentHorizontalResolution"] != null && mo["CurrentVerticalResolution"] != null)
                    {
                        res = $"{mo["CurrentHorizontalResolution"]} x {mo["CurrentVerticalResolution"]}";
                    }

                    string refresh = mo["CurrentRefreshRate"] != null ? $"{mo["CurrentRefreshRate"]} Hz" : "";

                    string driverDate = string.Empty;
                    if (mo["DriverDate"] != null)
                    {
                        string rawDate = mo["DriverDate"].ToString() ?? "";
                        if (rawDate.Length >= 8)
                        {
                            driverDate = $"{rawDate.Substring(0, 4)}-{rawDate.Substring(4, 2)}-{rawDate.Substring(6, 2)}";
                        }
                    }

                    gpus.Add(new GpuInfo
                    {
                        Name = name,
                        Manufacturer = InferGpuManufacturer(name),
                        DriverVersion = mo["DriverVersion"]?.ToString()?.Trim() ?? "N/A",
                        DriverDate = driverDate,
                        VideoProcessor = mo["VideoProcessor"]?.ToString()?.Trim() ?? name,
                        MemoryMb = vramMb,
                        MemoryFormatted = vramFormatted,
                        CurrentResolution = string.IsNullOrEmpty(res) ? "Display Connected" : res,
                        CurrentRefreshRate = refresh,
                        Status = mo["Status"]?.ToString() ?? "Active"
                    });
                }
            }
            catch { }

            // 2. Registry fallback if WMI returned nothing
            if (gpus.Count == 0)
            {
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
                                string driverDesc = adapterKey?.GetValue("DriverDesc")?.ToString() ?? "";
                                if (!string.IsNullOrEmpty(driverDesc))
                                {
                                    gpus.Add(new GpuInfo
                                    {
                                        Name = driverDesc,
                                        Manufacturer = InferGpuManufacturer(driverDesc),
                                        DriverVersion = adapterKey?.GetValue("DriverVersion")?.ToString() ?? "N/A",
                                        VideoProcessor = driverDesc,
                                        MemoryFormatted = "System Assigned",
                                        CurrentResolution = "Default Display",
                                        Status = "Active"
                                    });
                                }
                            }
                        }
                    }
                }
                catch { }
            }

            if (gpus.Count == 0)
            {
                gpus.Add(new GpuInfo
                {
                    Name = "Standard Display Adapter",
                    Manufacturer = "Microsoft",
                    Status = "Active"
                });
            }

            return gpus;
        }

        private static string InferGpuManufacturer(string name)
        {
            if (name.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase) || name.Contains("GeForce", StringComparison.OrdinalIgnoreCase))
                return "NVIDIA";
            if (name.Contains("AMD", StringComparison.OrdinalIgnoreCase) || name.Contains("Radeon", StringComparison.OrdinalIgnoreCase))
                return "AMD";
            if (name.Contains("Intel", StringComparison.OrdinalIgnoreCase) || name.Contains("Arc", StringComparison.OrdinalIgnoreCase))
                return "Intel";
            if (name.Contains("Microsoft", StringComparison.OrdinalIgnoreCase) || name.Contains("Hyper-V", StringComparison.OrdinalIgnoreCase))
                return "Microsoft";
            return "Display Adapter";
        }

        private static List<PhysicalDiskInfo> PopulateDiskInfo()
        {
            var disks = new List<PhysicalDiskInfo>();

            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Model, InterfaceType, MediaType, Size, Status FROM Win32_DiskDrive");
                foreach (ManagementObject mo in searcher.Get())
                {
                    string model = mo["Model"]?.ToString()?.Trim() ?? "Physical Disk";
                    string iface = mo["InterfaceType"]?.ToString()?.Trim() ?? "SCSI/NVMe";
                    string media = mo["MediaType"]?.ToString()?.Trim() ?? "Fixed Disk";

                    long sizeBytes = 0;
                    if (mo["Size"] != null)
                        long.TryParse(mo["Size"].ToString(), out sizeBytes);

                    long sizeGb = sizeBytes / (1024 * 1024 * 1024);
                    string sizeFormatted = sizeGb >= 1000
                        ? $"{sizeGb / 1024.0:F1} TB ({sizeGb:N0} GB)"
                        : $"{sizeGb:N0} GB";

                    disks.Add(new PhysicalDiskInfo
                    {
                        Model = model,
                        InterfaceType = iface,
                        MediaType = media,
                        SizeGb = sizeGb,
                        SizeFormatted = sizeFormatted,
                        Status = mo["Status"]?.ToString() ?? "OK"
                    });
                }
            }
            catch { }

            return disks;
        }

        private static PeripheralsInfo PopulatePeripherals()
        {
            var p = new PeripheralsInfo();

            // Audio devices
            try
            {
                using var soundSearcher = new ManagementObjectSearcher("SELECT Name, Manufacturer, Status FROM Win32_SoundDevice");
                foreach (ManagementObject mo in soundSearcher.Get())
                {
                    string name = mo["Name"]?.ToString()?.Trim() ?? "";
                    if (!string.IsNullOrEmpty(name))
                    {
                        p.AudioDevices.Add(new AudioDeviceInfo
                        {
                            Name = name,
                            Manufacturer = mo["Manufacturer"]?.ToString()?.Trim() ?? "Audio OEM",
                            Status = mo["Status"]?.ToString() ?? "OK"
                        });
                    }
                }
            }
            catch { }

            // Monitors
            try
            {
                using var monSearcher = new ManagementObjectSearcher("SELECT MonitorType, ScreenHeight, ScreenWidth, Status FROM Win32_DesktopMonitor");
                foreach (ManagementObject mo in monSearcher.Get())
                {
                    string type = mo["MonitorType"]?.ToString()?.Trim() ?? "Generic Monitor";
                    string res = (mo["ScreenWidth"] != null && mo["ScreenHeight"] != null)
                        ? $"{mo["ScreenWidth"]} x {mo["ScreenHeight"]}"
                        : "Active";

                    p.Monitors.Add(new MonitorInfo
                    {
                        Name = type,
                        Resolution = res,
                        RefreshRate = "Standard"
                    });
                }
            }
            catch { }

            // Input devices: Keyboards and Pointing Devices
            try
            {
                using var keySearcher = new ManagementObjectSearcher("SELECT Description FROM Win32_Keyboard");
                foreach (ManagementObject mo in keySearcher.Get())
                {
                    string desc = mo["Description"]?.ToString()?.Trim() ?? "";
                    if (!string.IsNullOrEmpty(desc) && !p.InputDevices.Any(d => d.Name == desc))
                    {
                        p.InputDevices.Add(new InputDeviceInfo { Name = desc, DeviceType = "Keyboard" });
                    }
                }

                using var mouseSearcher = new ManagementObjectSearcher("SELECT Description FROM Win32_PointingDevice");
                foreach (ManagementObject mo in mouseSearcher.Get())
                {
                    string desc = mo["Description"]?.ToString()?.Trim() ?? "";
                    if (!string.IsNullOrEmpty(desc) && !p.InputDevices.Any(d => d.Name == desc))
                    {
                        p.InputDevices.Add(new InputDeviceInfo { Name = desc, DeviceType = "Mouse / Pointing" });
                    }
                }
            }
            catch { }

            // Network controllers
            try
            {
                using var netSearcher = new ManagementObjectSearcher("SELECT Name, MACAddress, Speed, NetConnectionStatus FROM Win32_NetworkAdapter WHERE NetConnectionStatus = 2 OR PhysicalAdapter = TRUE");
                foreach (ManagementObject mo in netSearcher.Get())
                {
                    string name = mo["Name"]?.ToString()?.Trim() ?? "";
                    string mac = mo["MACAddress"]?.ToString()?.Trim() ?? "";
                    if (!string.IsNullOrEmpty(name))
                    {
                        p.NetworkControllers.Add(new NetworkControllerInfo
                        {
                            Name = name,
                            MacAddress = mac,
                            Speed = mo["Speed"] != null ? $"{Convert.ToInt64(mo["Speed"]) / 1_000_000} Mbps" : "Auto",
                            Status = "Connected"
                        });
                    }
                }
            }
            catch { }

            return p;
        }
    }
}
