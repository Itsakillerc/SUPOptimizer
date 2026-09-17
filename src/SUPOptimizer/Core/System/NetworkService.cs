using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using SUPOptimizer.Core.Logging;
using SUPOptimizer.Core.Security;

namespace SUPOptimizer.Core.System
{
    public class NetworkAdapterInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Up";
        public string Type { get; set; } = "Ethernet";
        public string Speed { get; set; } = "1 Gbps";
        public string MacAddress { get; set; } = string.Empty;
        public string Ipv4Address { get; set; } = string.Empty;
        public string SubnetMask { get; set; } = string.Empty;
        public string Gateway { get; set; } = string.Empty;
        public List<string> DnsServers { get; set; } = new();
        public bool DhcpEnabled { get; set; }
    }

    public class PingResult
    {
        public string Host { get; set; } = string.Empty;
        public bool Success { get; set; }
        public long RoundtripTimeMs { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class NetworkConnectionEntry
    {
        public int ProcessId { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public string Protocol { get; set; } = "TCP";
        public string LocalAddress { get; set; } = string.Empty;
        public int LocalPort { get; set; }
        public string RemoteAddress { get; set; } = string.Empty;
        public int RemotePort { get; set; }
        public string State { get; set; } = "ESTABLISHED";
        public string Direction { get; set; } = "Outbound";
    }

    public static class NetworkService
    {
        private const int AF_INET = 2; // IPv4
        private const int TCP_TABLE_OWNER_PID_ALL = 5;

        [DllImport("iphlpapi.dll", SetLastError = true)]
        private static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int pdwOutBufLen, bool bOrder, int ulAf, int tableClass, uint reserved = 0);

        [StructLayout(LayoutKind.Sequential)]
        private struct MIB_TCPROW_OWNER_PID
        {
            public uint state;
            public uint localAddr;
            public byte localPort1;
            public byte localPort2;
            public byte localPort3;
            public byte localPort4;
            public uint remoteAddr;
            public byte remotePort1;
            public byte remotePort2;
            public byte remotePort3;
            public byte remotePort4;
            public uint owningPid;

            public ushort LocalPort => (ushort)((localPort1 << 8) | localPort2);
            public ushort RemotePort => (ushort)((remotePort1 << 8) | remotePort2);
        }

        public static List<NetworkAdapterInfo> GetAdapters()
        {
            var list = new List<NetworkAdapterInfo>();

            try
            {
                var interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (var ni in interfaces)
                {
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                        continue;

                    var ipProps = ni.GetIPProperties();
                    string ipv4 = string.Empty;
                    string subnet = string.Empty;

                    foreach (var addr in ipProps.UnicastAddresses)
                    {
                        if (addr.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipv4 = addr.Address.ToString();
                            subnet = addr.IPv4Mask?.ToString() ?? string.Empty;
                            break;
                        }
                    }

                    string gateway = string.Empty;
                    foreach (var gw in ipProps.GatewayAddresses)
                    {
                        if (gw.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            gateway = gw.Address.ToString();
                            break;
                        }
                    }

                    var dnsList = new List<string>();
                    foreach (var dns in ipProps.DnsAddresses)
                    {
                        if (dns.AddressFamily == AddressFamily.InterNetwork)
                        {
                            dnsList.Add(dns.ToString());
                        }
                    }

                    string mac = string.Join(":", ni.GetPhysicalAddress().GetAddressBytes().Select(b => b.ToString("X2")));

                    list.Add(new NetworkAdapterInfo
                    {
                        Id = ni.Id,
                        Name = ni.Name,
                        Description = ni.Description,
                        Status = ni.OperationalStatus.ToString(),
                        Type = ni.NetworkInterfaceType.ToString(),
                        Speed = FormatSpeed(ni.Speed),
                        MacAddress = mac,
                        Ipv4Address = ipv4,
                        SubnetMask = subnet,
                        Gateway = gateway,
                        DnsServers = dnsList,
                        DhcpEnabled = ipProps.GetIPv4Properties()?.IsDhcpEnabled ?? false
                    });
                }
            }
            catch (Exception ex)
            {
                AuditLogger.Log("Network", "GetAdapters Error", ex.Message, success: false, errorMessage: ex.Message);
            }

            return list;
        }

        private static string FormatSpeed(long speedBitsPerSec)
        {
            if (speedBitsPerSec <= 0) return "N/A";
            if (speedBitsPerSec >= 1_000_000_000) return $"{speedBitsPerSec / 1_000_000_000.0:F1} Gbps";
            if (speedBitsPerSec >= 1_000_000) return $"{speedBitsPerSec / 1_000_000.0:F1} Mbps";
            return $"{speedBitsPerSec / 1000.0:F1} Kbps";
        }

        public static List<NetworkConnectionEntry> GetActiveConnections()
        {
            var list = new List<NetworkConnectionEntry>();
            var procCache = new Dictionary<int, string>();

            int bufferSize = 0;
            GetExtendedTcpTable(IntPtr.Zero, ref bufferSize, true, AF_INET, TCP_TABLE_OWNER_PID_ALL, 0);

            if (bufferSize > 0)
            {
                IntPtr tcpTablePtr = Marshal.AllocHGlobal(bufferSize);
                try
                {
                    uint result = GetExtendedTcpTable(tcpTablePtr, ref bufferSize, true, AF_INET, TCP_TABLE_OWNER_PID_ALL, 0);
                    if (result == 0)
                    {
                        int numEntries = Marshal.ReadInt32(tcpTablePtr);
                        IntPtr rowPtr = IntPtr.Add(tcpTablePtr, 4);

                        for (int i = 0; i < numEntries; i++)
                        {
                            var row = Marshal.PtrToStructure<MIB_TCPROW_OWNER_PID>(rowPtr);
                            string stateStr = ResolveTcpState(row.state);

                            string localIp = new IPAddress(row.localAddr).ToString();
                            string remoteIp = new IPAddress(row.remoteAddr).ToString();
                            int localPort = row.LocalPort;
                            int remotePort = row.RemotePort;
                            int pid = (int)row.owningPid;

                            string direction = "Outbound";
                            if (stateStr == "LISTEN" || stateStr == "LISTENING")
                            {
                                direction = "Inbound (Listening)";
                            }
                            else if (remoteIp == "0.0.0.0" || remoteIp == "127.0.0.1" || remoteIp == "::1")
                            {
                                direction = "Local Loopback";
                            }
                            else if (localPort == 80 || localPort == 443 || localPort == 5000 || localPort == 8080 || localPort == 3389)
                            {
                                direction = "Inbound";
                            }

                            list.Add(new NetworkConnectionEntry
                            {
                                ProcessId = pid,
                                ProcessName = GetProcessName(pid, procCache),
                                Protocol = "TCP",
                                LocalAddress = $"{localIp}:{localPort}",
                                LocalPort = localPort,
                                RemoteAddress = $"{remoteIp}:{remotePort}",
                                RemotePort = remotePort,
                                State = stateStr,
                                Direction = direction
                            });

                            rowPtr = IntPtr.Add(rowPtr, Marshal.SizeOf<MIB_TCPROW_OWNER_PID>());
                        }
                    }
                }
                catch { }
                finally
                {
                    Marshal.FreeHGlobal(tcpTablePtr);
                }
            }

            // Fallback to IPGlobalProperties if list is empty
            if (list.Count == 0)
            {
                try
                {
                    var props = IPGlobalProperties.GetIPGlobalProperties();
                    foreach (var conn in props.GetActiveTcpConnections())
                    {
                        list.Add(new NetworkConnectionEntry
                        {
                            ProcessId = 0,
                            ProcessName = "System Network Stack",
                            Protocol = "TCP",
                            LocalAddress = conn.LocalEndPoint.ToString(),
                            LocalPort = conn.LocalEndPoint.Port,
                            RemoteAddress = conn.RemoteEndPoint.ToString(),
                            RemotePort = conn.RemoteEndPoint.Port,
                            State = conn.State.ToString().ToUpperInvariant(),
                            Direction = conn.RemoteEndPoint.Address.ToString() == "127.0.0.1" ? "Local Loopback" : "Outbound"
                        });
                    }
                }
                catch { }
            }

            return list;
        }

        private static string ResolveTcpState(uint state)
        {
            return state switch
            {
                1 => "CLOSED",
                2 => "LISTEN",
                3 => "SYN_SENT",
                4 => "SYN_RCVD",
                5 => "ESTABLISHED",
                6 => "FIN_WAIT_1",
                7 => "FIN_WAIT_2",
                8 => "CLOSE_WAIT",
                9 => "CLOSING",
                10 => "LAST_ACK",
                11 => "TIME_WAIT",
                12 => "DELETE_TCB",
                _ => "UNKNOWN"
            };
        }

        private static string GetProcessName(int pid, Dictionary<int, string> cache)
        {
            if (pid == 0) return "System Idle";
            if (pid == 4) return "System Kernel";
            if (cache.TryGetValue(pid, out var cachedName)) return cachedName;

            try
            {
                using var p = Process.GetProcessById(pid);
                string name = $"{p.ProcessName}.exe";
                cache[pid] = name;
                return name;
            }
            catch
            {
                string fallback = $"PID {pid}";
                cache[pid] = fallback;
                return fallback;
            }
        }

        public static (bool Success, string Message) OptimizeNetworkStack()
        {
            if (!PrivilegeManager.IsAdministrator())
                return (false, "Administrator privileges required to optimize network properties.");

            var actions = new List<string>();

            try
            {
                // 1. TCP Window Auto-Tuning
                RunSystemCommand("netsh.exe", "int tcp set global autotuninglevel=normal", out _, out _);
                actions.Add("TCP Window Auto-Tuning set to 'Normal' to maximize throughput");

                // 2. Receive Side Scaling (RSS)
                RunSystemCommand("netsh.exe", "int tcp set global rss=enabled", out _, out _);
                actions.Add("Receive Side Scaling (RSS) enabled across all CPU cores");

                // 3. Receive Segment Coalescing (RSC)
                RunSystemCommand("netsh.exe", "int tcp set global rsc=enabled", out _, out _);
                actions.Add("Receive Segment Coalescing (RSC) enabled to minimize CPU overhead");

                // 4. Disable TCP Heuristics & ECN
                RunSystemCommand("netsh.exe", "int tcp set heuristics disabled", out _, out _);
                RunSystemCommand("netsh.exe", "int tcp set global ecncapability=disabled", out _, out _);
                actions.Add("TCP Heuristics disabled and ECN optimized");

                // 5. Network Throttling Index & System Responsiveness
                using (var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile"))
                {
                    if (key != null)
                    {
                        key.SetValue("NetworkThrottlingIndex", unchecked((int)0xFFFFFFFF), RegistryValueKind.DWord);
                        key.SetValue("SystemResponsiveness", 0, RegistryValueKind.DWord);
                        actions.Add("Network Throttling Index disabled (unrestricted packet throughput)");
                        actions.Add("System Responsiveness set to 0% (maximum network and gaming scheduling priority)");
                    }
                }

                // 6. Disable Adapter Power Saving (Green Ethernet / EEE)
                ToggleAdapterPowerSaving(disablePowerSaving: true);
                actions.Add("Network adapter Energy Efficient Ethernet (EEE) power saving disabled");

                // 7. Flush DNS
                FlushDns();
                actions.Add("DNS resolver cache flushed");

                AuditLogger.Log("Network", "Stack Optimized", string.Join("; ", actions));
                return (true, "Network stack optimization completed:\n• " + string.Join("\n• ", actions));
            }
            catch (Exception ex)
            {
                AuditLogger.Log("Network", "Optimize Error", ex.Message, success: false, errorMessage: ex.Message);
                return (false, $"Error optimizing network stack: {ex.Message}");
            }
        }

        public static (bool Success, string Message) ToggleAdapterPowerSaving(bool disablePowerSaving)
        {
            try
            {
                using var netClassKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e972-e325-11ce-bfc1-08002be10318}", true);
                if (netClassKey != null)
                {
                    foreach (var sub in netClassKey.GetSubKeyNames())
                    {
                        if (sub.StartsWith("000", StringComparison.OrdinalIgnoreCase))
                        {
                            using var adapterKey = netClassKey.OpenSubKey(sub, true);
                            if (adapterKey != null)
                            {
                                string val = disablePowerSaving ? "0" : "1";
                                if (adapterKey.GetValue("*EEE") != null) adapterKey.SetValue("*EEE", val);
                                if (adapterKey.GetValue("*EEELinkAdvertisement") != null) adapterKey.SetValue("*EEELinkAdvertisement", val);
                                if (adapterKey.GetValue("ReduceSpeedOnPowerDown") != null) adapterKey.SetValue("ReduceSpeedOnPowerDown", val);
                                if (adapterKey.GetValue("AutoPowerSaveModeEnabled") != null) adapterKey.SetValue("AutoPowerSaveModeEnabled", val);
                                if (adapterKey.GetValue("EnergyEfficientEthernet") != null) adapterKey.SetValue("EnergyEfficientEthernet", val);
                            }
                        }
                    }
                }

                string msg = disablePowerSaving
                    ? "Network adapter Energy Efficient Ethernet (EEE) disabled for sustained low latency."
                    : "Network adapter power saving enabled.";
                AuditLogger.Log("Network", "Adapter Power Saving", msg);
                return (true, msg);
            }
            catch (Exception ex)
            {
                return (false, $"Error configuring network adapter power saving: {ex.Message}");
            }
        }

        public static (bool Success, string Message) FlushDns()
        {
            try
            {
                RunSystemCommand("ipconfig.exe", "/flushdns", out string output, out int exitCode);
                AuditLogger.Log("Network", "Flushed DNS Resolver Cache", output);
                return (exitCode == 0, "DNS resolver cache flushed successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Error flushing DNS cache: {ex.Message}");
            }
        }

        public static (bool Success, string Message) ResetWinsock()
        {
            if (!PrivilegeManager.IsAdministrator())
                return (false, "Administrator privileges are required to reset the Winsock catalog.");

            try
            {
                RunSystemCommand("netsh.exe", "winsock reset", out string output, out int exitCode);
                AuditLogger.Log("Network", "Reset Winsock Catalog", output);
                return (exitCode == 0, "Winsock catalog reset successfully. Please reboot your PC to finalize.");
            }
            catch (Exception ex)
            {
                return (false, $"Error resetting Winsock: {ex.Message}");
            }
        }

        public static (bool Success, string Message) ResetTcpIp()
        {
            if (!PrivilegeManager.IsAdministrator())
                return (false, "Administrator privileges are required to reset the TCP/IP stack.");

            try
            {
                RunSystemCommand("netsh.exe", "int ip reset", out string output, out int exitCode);
                AuditLogger.Log("Network", "Reset TCP/IP Stack", output);
                return (exitCode == 0, "TCP/IP stack reset successfully. Please reboot your PC.");
            }
            catch (Exception ex)
            {
                return (false, $"Error resetting TCP/IP: {ex.Message}");
            }
        }

        public static (bool Success, string Message) ReleaseRenewIp()
        {
            try
            {
                RunSystemCommand("ipconfig.exe", "/release", out _, out _);
                RunSystemCommand("ipconfig.exe", "/renew", out string output, out int exitCode);
                AuditLogger.Log("Network", "Released and Renewed IP Lease", output);
                return (exitCode == 0, "DHCP IP lease renewed successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Error renewing IP lease: {ex.Message}");
            }
        }

        public static (bool Success, string Message) RenewDhcp() => ReleaseRenewIp();
        public static PingResult PingHost(string host) => Ping(host);

        public static PingResult Ping(string host, int timeoutMs = 2000)
        {
            try
            {
                using var pinger = new Ping();
                var reply = pinger.Send(host, timeoutMs);
                return new PingResult
                {
                    Host = host,
                    Success = reply.Status == IPStatus.Success,
                    RoundtripTimeMs = reply.RoundtripTime,
                    Status = reply.Status.ToString()
                };
            }
            catch (Exception ex)
            {
                return new PingResult
                {
                    Host = host,
                    Success = false,
                    RoundtripTimeMs = -1,
                    Status = ex.Message
                };
            }
        }

        public static ShodanIpInfo LookupShodan(string ipOrHost)
        {
            var info = new ShodanIpInfo
            {
                Target = ipOrHost,
                ShodanUrl = $"https://www.shodan.io/host/{ipOrHost}"
            };

            try
            {
                var entry = global::System.Net.Dns.GetHostEntry(ipOrHost);
                info.HostName = entry.HostName;
                if (entry.AddressList.Length > 0)
                {
                    info.ResolvedIp = entry.AddressList[0].ToString();
                    info.ShodanUrl = $"https://www.shodan.io/host/{info.ResolvedIp}";
                }
            }
            catch
            {
                info.ResolvedIp = ipOrHost;
            }

            // Quick probe of standard service ports
            int[] commonPorts = new[] { 80, 443, 22, 53, 3389, 8080 };
            foreach (var port in commonPorts)
            {
                try
                {
                    using var tcp = new global::System.Net.Sockets.TcpClient();
                    var connectTask = tcp.ConnectAsync(info.ResolvedIp, port);
                    if (global::System.Threading.Tasks.Task.WhenAny(connectTask, global::System.Threading.Tasks.Task.Delay(400)).Result == connectTask && tcp.Connected)
                    {
                        info.OpenPorts.Add(port);
                    }
                }
                catch { }
            }

            return info;
        }

        private static void RunSystemCommand(string fileName, string args, out string output, out int exitCode)
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = args,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var proc = Process.Start(psi);
            if (proc != null)
            {
                output = proc.StandardOutput.ReadToEnd();
                proc.WaitForExit(10000);
                exitCode = proc.ExitCode;
            }
            else
            {
                output = string.Empty;
                exitCode = -1;
            }
        }
    }

    public class ShodanIpInfo
    {
        public string Target { get; set; } = string.Empty;
        public string ResolvedIp { get; set; } = string.Empty;
        public string HostName { get; set; } = string.Empty;
        public string ShodanUrl { get; set; } = string.Empty;
        public List<int> OpenPorts { get; set; } = new();
    }
}
