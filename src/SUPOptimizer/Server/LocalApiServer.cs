using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SUPOptimizer.Core.Backup;
using SUPOptimizer.Core.Logging;
using SUPOptimizer.Core.Security;
using SUPOptimizer.Core.System;
using SUPOptimizer.Core.Tweaks;

namespace SUPOptimizer.Server
{
    public class LocalApiServer
    {
        private readonly HttpListener _listener = new();
        private readonly int _port;
        private readonly string _sessionToken;
        private bool _isRunning = false;

        public static event Action? RequestMinimize;
        public static event Action? RequestMaximize;
        public static event Action? RequestExit;
        public static event Action? RequestDrag;

        public int Port => _port;
        public string SessionToken => _sessionToken;
        public string Url => $"http://127.0.0.1:{_port}/";

        public LocalApiServer(int? preferredPort = null)
        {
            _port = preferredPort ?? FindFreePort();
            _sessionToken = Guid.NewGuid().ToString("N");
            _listener.Prefixes.Add($"http://127.0.0.1:{_port}/");
        }

        private static int FindFreePort()
        {
            using var socket = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
            socket.Start();
            int port = ((IPEndPoint)socket.LocalEndpoint).Port;
            socket.Stop();
            return port;
        }

        public void Start()
        {
            _listener.Start();
            _isRunning = true;
            Task.Run(ListenLoop);
            AuditLogger.Log("Server", "API Server Started", $"Bound to {Url}");
        }

        public void Stop()
        {
            _isRunning = false;
            try { _listener.Stop(); } catch { }
        }

        private async Task ListenLoop()
        {
            while (_isRunning)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    _ = Task.Run(() => HandleRequest(context));
                }
                catch when (!_isRunning)
                {
                    break;
                }
                catch (Exception ex)
                {
                    AuditLogger.Log("Server", "Listen Error", ex.Message, success: false, errorMessage: ex.Message);
                }
            }
        }

        private void HandleRequest(HttpListenerContext context)
        {
            try
            {
                var req = context.Request;
                var resp = context.Response;

                // Enable CORS for localhost
                resp.AddHeader("Access-Control-Allow-Origin", "*");
                resp.AddHeader("Access-Control-Allow-Headers", "Content-Type, X-SUP-Token");
                resp.AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");

                if (req.HttpMethod.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
                {
                    resp.StatusCode = 200;
                    resp.Close();
                    return;
                }

                string rawPath = req.Url?.AbsolutePath ?? "/";

                if (rawPath.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
                {
                    // Validate Session Token
                    string? headerToken = req.Headers["X-SUP-Token"];
                    string? queryToken = req.QueryString["token"];

                    bool validHeader = !string.IsNullOrEmpty(headerToken) && headerToken == _sessionToken;
                    bool validQuery = !string.IsNullOrEmpty(queryToken) && queryToken == _sessionToken;

                    if (!validHeader && !validQuery)
                    {
                        resp.StatusCode = 403;
                        SendJson(resp, new { success = false, message = "Invalid or missing X-SUP-Token." });
                        return;
                    }

                    HandleApiRoute(req, resp, rawPath);
                }
                else
                {
                    ServeStaticAsset(req, resp, rawPath);
                }
            }
            catch (Exception ex)
            {
                AuditLogger.Log("Server", "HandleRequest Exception", ex.Message, success: false, errorMessage: ex.Message);
                try { context.Response.Close(); } catch { }
            }
        }

        private void HandleApiRoute(HttpListenerRequest req, HttpListenerResponse resp, string path)
        {
            try
            {
                string method = req.HttpMethod.ToUpperInvariant();

                if (method == "GET")
                {
                    switch (path.ToLowerInvariant())
                    {
                        case "/api/system/metrics":
                            SendJson(resp, SystemInfoService.GetMetrics());
                            return;
                        case "/api/system/health":
                            SendJson(resp, HealthScanService.RunScan());
                            return;
                        case "/api/system/hardware":
                            SendJson(resp, HardwareService.GetSummary());
                            return;
                        case "/api/system/security":
                            SendJson(resp, SecurityService.GetOverview());
                            return;
                        case "/api/tweaks":
                            SendJson(resp, TweakRegistry.GetAllDefinitions());
                            return;
                        case "/api/services":
                            SendJson(resp, ServiceManager.GetServices());
                            return;
                        case "/api/startup":
                            SendJson(resp, StartupManager.GetStartupItems());
                            return;
                        case "/api/apps":
                            SendJson(resp, AppManager.GetInstalledApps());
                            return;
                        case "/api/debloat/packages":
                            SendJson(resp, DebloaterService.GetPackages());
                            return;
                        case "/api/installer/catalog":
                            SendJson(resp, AppInstallerService.GetCatalog());
                            return;
                        case "/api/network/adapters":
                            SendJson(resp, NetworkService.GetAdapters());
                            return;
                        case "/api/storage/analyze":
                            SendJson(resp, StorageService.Analyze());
                            return;
                        case "/api/repair/tools":
                            SendJson(resp, RepairService.GetAvailableTools());
                            return;
                        case "/api/repair/output":
                            var (isRunning, tool, output) = RepairService.GetOutput();
                            SendJson(resp, new { isRunning, currentTool = tool, output });
                            return;
                        case "/api/backups":
                            SendJson(resp, BackupManager.GetChangeSets());
                            return;
                        case "/api/logs":
                            string? cat = req.QueryString["category"];
                            string? search = req.QueryString["search"];
                            int limit = int.TryParse(req.QueryString["limit"], out int l) ? l : 200;
                            SendJson(resp, AuditLogger.GetEntries(cat, search, limit));
                            return;
                        case "/api/automation/profiles":
                            SendJson(resp, AutomationService.GetProfiles());
                            return;
                        case "/api/tools/hosts":
                            var (hSuccess, hContent, hPath) = SystemToolsService.ReadHostsFile();
                            SendJson(resp, new { success = hSuccess, content = hContent, path = hPath });
                            return;
                        case "/api/tools/dns/presets":
                            SendJson(resp, SystemToolsService.GetDnsPresets());
                            return;
                        case "/api/tools/env":
                            SendJson(resp, SystemToolsService.GetEnvironmentVariables());
                            return;
                        case "/api/tools/run-aliases":
                            SendJson(resp, SystemToolsService.GetRunAliases());
                            return;
                        case "/api/network/shodan":
                            string targetIp = req.QueryString["ip"] ?? "1.1.1.1";
                            SendJson(resp, NetworkService.LookupShodan(targetIp));
                            return;
                        case "/api/network/connections":
                            SendJson(resp, NetworkService.GetActiveConnections());
                            return;
                        case "/api/features":
                            SendJson(resp, FeaturesService.GetFeatures());
                            return;
                        case "/api/debloat/presets":
                            SendJson(resp, DebloaterService.GetPresets());
                            return;
                        case "/api/profiles/presets":
                            SendJson(resp, ConfigProfileService.GetBuiltInPresets());
                            return;
                        case "/api/repair/autologon":
                            var (alEnabled, alUser, alDom) = RepairService.GetAutoLogonStatus();
                            SendJson(resp, new { enabled = alEnabled, username = alUser, domain = alDom });
                            return;
                    }
                }
                else if (method == "POST")
                {
                    string body = ReadBody(req);
                    using var doc = !string.IsNullOrEmpty(body) ? JsonDocument.Parse(body) : null;
                    var root = doc?.RootElement;

                    switch (path.ToLowerInvariant())
                    {
                        case "/api/tweaks/apply":
                            {
                                string tweakId = root?.GetProperty("tweakId").GetString() ?? "";
                                bool dryRun = root?.TryGetProperty("dryRun", out var d) == true && d.GetBoolean();
                                SendJson(resp, TweakEngine.Apply(tweakId, dryRun));
                                return;
                            }
                        case "/api/tweaks/restore":
                            {
                                string tweakId = root?.GetProperty("tweakId").GetString() ?? "";
                                bool dryRun = root?.TryGetProperty("dryRun", out var d) == true && d.GetBoolean();
                                SendJson(resp, TweakEngine.Restore(tweakId, dryRun));
                                return;
                            }
                        case "/api/tweaks/batch":
                            {
                                var ids = new List<string>();
                                if (root?.TryGetProperty("tweakIds", out var listEl) == true)
                                {
                                    foreach (var item in listEl.EnumerateArray())
                                    {
                                        var s = item.GetString();
                                        if (!string.IsNullOrEmpty(s)) ids.Add(s);
                                    }
                                }
                                bool dryRun = root?.TryGetProperty("dryRun", out var d) == true && d.GetBoolean();
                                SendJson(resp, TweakEngine.ApplyBatch(ids, dryRun));
                                return;
                            }
                        case "/api/services/action":
                            {
                                string svcName = root?.GetProperty("serviceName").GetString() ?? "";
                                string action = root?.GetProperty("action").GetString()?.ToLowerInvariant() ?? "";
                                (bool success, string message) = action switch
                                {
                                    "start" => ServiceManager.StartService(svcName),
                                    "stop" => ServiceManager.StopService(svcName),
                                    "restart" => ServiceManager.RestartService(svcName),
                                    "set_startup" => ServiceManager.SetStartupType(svcName, root?.GetProperty("startupType").GetString() ?? "Manual"),
                                    _ => (false, "Unknown action")
                                };
                                SendJson(resp, new { success, message });
                                return;
                            }
                        case "/api/startup/toggle":
                            {
                                string loc = root?.GetProperty("location").GetString() ?? "";
                                string name = root?.GetProperty("name").GetString() ?? "";
                                bool enable = root?.GetProperty("enable").GetBoolean() ?? true;
                                var res = StartupManager.ToggleStartup(loc, name, enable);
                                SendJson(resp, new { success = res.Success, message = res.Message });
                                return;
                            }
                        case "/api/apps/uninstall":
                            {
                                string uninst = root?.GetProperty("uninstallString").GetString() ?? "";
                                var res = AppManager.UninstallApp(uninst);
                                SendJson(resp, new { success = res.Success, message = res.Message });
                                return;
                            }
                        case "/api/apps/open":
                            {
                                string loc = root?.GetProperty("location").GetString() ?? "";
                                var res = AppManager.OpenLocation(loc);
                                SendJson(resp, new { success = res.Success, message = res.Message });
                                return;
                            }
                        case "/api/debloat/remove":
                            {
                                string pkgId = root?.GetProperty("packageId").GetString() ?? "";
                                bool dryRun = root?.TryGetProperty("dryRun", out var d) == true && d.GetBoolean();
                                var res = DebloaterService.RemovePackage(pkgId, dryRun);
                                SendJson(resp, new { success = res.Success, message = res.Message });
                                return;
                            }
                        case "/api/installer/install":
                            {
                                string pkgId = root?.GetProperty("packageId").GetString() ?? "";
                                var res = AppInstallerService.InstallApp(pkgId);
                                SendJson(resp, new { success = res.Success, message = res.Message });
                                return;
                            }
                        case "/api/network/action":
                            {
                                string act = root?.GetProperty("action").GetString()?.ToLowerInvariant() ?? "";
                                (bool s, string m) = act switch
                                {
                                    "flush_dns" => NetworkService.FlushDns(),
                                    "reset_winsock" => NetworkService.ResetWinsock(),
                                    "reset_tcpip" => NetworkService.ResetTcpIp(),
                                    "renew_dhcp" => NetworkService.RenewDhcp(),
                                    _ => (false, "Unknown network action")
                                };
                                SendJson(resp, new { success = s, message = m });
                                return;
                            }
                        case "/api/network/ping":
                            {
                                string host = root?.GetProperty("host").GetString() ?? "8.8.8.8";
                                SendJson(resp, NetworkService.PingHost(host));
                                return;
                            }
                        case "/api/network/optimize":
                            {
                                var (s, m) = NetworkService.OptimizeNetworkStack();
                                SendJson(resp, new { success = s, message = m });
                                return;
                            }
                        case "/api/network/powersaving":
                            {
                                bool disable = root?.TryGetProperty("disablePowerSaving", out var dp) == true ? dp.GetBoolean() : true;
                                var (s, m) = NetworkService.ToggleAdapterPowerSaving(disable);
                                SendJson(resp, new { success = s, message = m });
                                return;
                            }
                                case "/api/storage/clean":
                            {
                                var targets = new List<string>();
                                if (root?.TryGetProperty("targetIds", out var listEl) == true)
                                {
                                    foreach (var item in listEl.EnumerateArray())
                                    {
                                        var targetId = item.GetString();
                                        if (!string.IsNullOrEmpty(targetId)) targets.Add(targetId);
                                    }
                                }
                                bool dryRun = root?.TryGetProperty("dryRun", out var d) == true && d.GetBoolean();
                                var (sClean, mClean, mbClean) = StorageService.Clean(targets, dryRun);
                                SendJson(resp, new { success = sClean, message = mClean, cleanedMb = mbClean });
                                return;
                            }
                        case "/api/repair/run":
                            {
                                string toolId = root?.GetProperty("toolId").GetString() ?? "";
                                var (started, msg) = RepairService.RunRepairTool(toolId);
                                SendJson(resp, new { success = started, message = msg });
                                return;
                            }
                        case "/api/backups/create_restore_point":
                            {
                                string desc = root?.GetProperty("description").GetString() ?? "SUP Snapshot";
                                var (s, m) = BackupManager.CreateSystemRestorePoint(desc);
                                SendJson(resp, new { success = s, message = m });
                                return;
                            }
                        case "/api/backups/rollback":
                            {
                                string csId = root?.GetProperty("changeSetId").GetString() ?? "";
                                var (s, m) = BackupManager.RollbackChangeSet(csId);
                                SendJson(resp, new { success = s, message = m });
                                return;
                            }
                        case "/api/logs/clear":
                            {
                                AuditLogger.Clear();
                                SendJson(resp, new { success = true, message = "Audit logs cleared." });
                                return;
                            }
                        case "/api/automation/apply":
                            {
                                string profId = root?.GetProperty("profileId").GetString() ?? "";
                                bool dryRun = root?.TryGetProperty("dryRun", out var d) == true && d.GetBoolean();
                                SendJson(resp, AutomationService.ApplyProfile(profId, dryRun));
                                return;
                            }
                        case "/api/host/elevate":
                            {
                                if (SUPOptimizer.Core.Security.PrivilegeManager.IsAdministrator())
                                {
                                    SendJson(resp, new { success = true, alreadyAdmin = true });
                                    return;
                                }
                                bool started = PrivilegeManager.RestartElevated();
                                SendJson(resp, new { success = started, alreadyAdmin = false });
                                if (started)
                                {
                                    RequestExit?.Invoke();
                                }
                                return;
                            }
                        case "/api/host/drag":
                            {
                                RequestDrag?.Invoke();
                                SendJson(resp, new { success = true });
                                return;
                            }
                        case "/api/host/maximize":
                            {
                                RequestMaximize?.Invoke();
                                SendJson(resp, new { success = true });
                                return;
                            }
                        case "/api/host/minimize":
                            {
                                RequestMinimize?.Invoke();
                                SendJson(resp, new { success = true });
                                return;
                            }
                        case "/api/host/exit":
                            {
                                SendJson(resp, new { success = true });
                                RequestExit?.Invoke();
                                return;
                            }
                        case "/api/host/open-browser":
                            {
                                try
                                {
                                    Process.Start(new ProcessStartInfo
                                    {
                                        FileName = Url,
                                        UseShellExecute = true
                                    });
                                    SendJson(resp, new { success = true, url = Url });
                                }
                                catch (Exception ex)
                                {
                                    SendJson(resp, new { success = false, message = ex.Message });
                                }
                                return;
                            }
                        case "/api/tools/hosts/save":
                            {
                                string content = root?.GetProperty("content").GetString() ?? "";
                                var (hS, hM) = SystemToolsService.SaveHostsFile(content);
                                SendJson(resp, new { success = hS, message = hM });
                                return;
                            }
                        case "/api/tools/hosts/block-telemetry":
                            {
                                var (hS, hM) = SystemToolsService.BlockTelemetryDomainsInHosts();
                                SendJson(resp, new { success = hS, message = hM });
                                return;
                            }
                        case "/api/tools/dns/apply":
                            {
                                string adapter = root?.TryGetProperty("adapterName", out var aEl) == true ? aEl.GetString() ?? "" : "";
                                string primary = root?.TryGetProperty("primaryDns", out var pEl) == true ? pEl.GetString() ?? "" : "";
                                string secondary = root?.TryGetProperty("secondaryDns", out var sEl) == true ? sEl.GetString() ?? "" : "";
                                bool isDhcp = root?.TryGetProperty("isDhcp", out var dEl) == true && dEl.GetBoolean();
                                var (dS, dM) = SystemToolsService.ApplyDns(adapter, primary, secondary, isDhcp);
                                SendJson(resp, new { success = dS, message = dM });
                                return;
                            }
                        case "/api/tools/env/set":
                            {
                                string name = root?.GetProperty("name").GetString() ?? "";
                                string val = root?.GetProperty("value").GetString() ?? "";
                                string scope = root?.TryGetProperty("scope", out var scEl) == true ? scEl.GetString() ?? "User" : "User";
                                var (eS, eM) = SystemToolsService.SetEnvironmentVariable(name, val, scope);
                                SendJson(resp, new { success = eS, message = eM });
                                return;
                            }
                        case "/api/tools/unlock/find":
                            {
                                string filePath = root?.GetProperty("filePath").GetString() ?? "";
                                var locks = SystemToolsService.FindFileLocks(filePath);
                                SendJson(resp, locks);
                                return;
                            }
                        case "/api/tools/unlock/kill":
                            {
                                int pid = root?.GetProperty("processId").GetInt32() ?? 0;
                                var (kS, kM) = SystemToolsService.TerminateLockingProcess(pid);
                                SendJson(resp, new { success = kS, message = kM });
                                return;
                            }
                        case "/api/tools/run-aliases/add":
                            {
                                string aliasName = root?.GetProperty("aliasName").GetString() ?? "";
                                string targetPath = root?.GetProperty("targetPath").GetString() ?? "";
                                var (aS, aM) = SystemToolsService.AddRunAlias(aliasName, targetPath);
                                SendJson(resp, new { success = aS, message = aM });
                                return;
                            }
                        case "/api/tools/run-aliases/remove":
                            {
                                string aliasName = root?.GetProperty("aliasName").GetString() ?? "";
                                var (rS, rM) = SystemToolsService.RemoveRunAlias(aliasName);
                                SendJson(resp, new { success = rS, message = rM });
                                return;
                            }
                        case "/api/features/set":
                            {
                                string featId = root?.GetProperty("featureId").GetString() ?? "";
                                bool enable = root?.GetProperty("enable").GetBoolean() ?? true;
                                var (s, m) = FeaturesService.SetFeature(featId, enable);
                                SendJson(resp, new { success = s, message = m });
                                return;
                            }
                        case "/api/debloat/remove-batch":
                            {
                                var ids = new List<string>();
                                if (root?.TryGetProperty("packageIds", out var listEl) == true)
                                {
                                    foreach (var item in listEl.EnumerateArray())
                                    {
                                        var s = item.GetString();
                                        if (!string.IsNullOrEmpty(s)) ids.Add(s);
                                    }
                                }
                                bool dryRun = root?.TryGetProperty("dryRun", out var d) == true && d.GetBoolean();
                                var (tot, rem, fld, msgs) = DebloaterService.RemovePackages(ids, dryRun);
                                SendJson(resp, new { success = fld == 0, total = tot, removed = rem, failed = fld, messages = msgs });
                                return;
                            }
                        case "/api/debloat/remove-onedrive":
                            {
                                bool dryRun = root?.TryGetProperty("dryRun", out var d) == true && d.GetBoolean();
                                var (s, m) = DebloaterService.RemoveOneDrive(dryRun);
                                SendJson(resp, new { success = s, message = m });
                                return;
                            }
                        case "/api/debloat/remove-edge":
                            {
                                bool dryRun = root?.TryGetProperty("dryRun", out var d) == true && d.GetBoolean();
                                var (s, m) = DebloaterService.RemoveMicrosoftEdge(dryRun);
                                SendJson(resp, new { success = s, message = m });
                                return;
                            }
                        case "/api/profiles/export":
                            {
                                string name = root?.TryGetProperty("name", out var nEl) == true ? nEl.GetString() ?? "Custom Setup" : "Custom Setup";
                                string desc = root?.TryGetProperty("description", out var dsEl) == true ? dsEl.GetString() ?? "" : "";
                                SendJson(resp, ConfigProfileService.ExportCurrentConfiguration(name, desc));
                                return;
                            }
                        case "/api/profiles/apply":
                            {
                                bool dryRun = root?.TryGetProperty("dryRun", out var d) == true && d.GetBoolean();
                                ConfigProfile? profile = null;
                                if (root?.TryGetProperty("profile", out var pEl) == true)
                                {
                                    profile = JsonSerializer.Deserialize<ConfigProfile>(pEl.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                                }
                                if (profile == null)
                                {
                                    SendJson(resp, new { success = false, message = "Invalid profile payload." });
                                    return;
                                }
                                var (app, rest, fld, msgs) = ConfigProfileService.ApplyProfile(profile, dryRun);
                                SendJson(resp, new { success = fld == 0, applied = app, restored = rest, failed = fld, messages = msgs });
                                return;
                            }
                        case "/api/autounattend/generate":
                            {
                                AutounattendOptions opts = new();
                                if (root.HasValue)
                                {
                                    if (root.Value.TryGetProperty("options", out var oEl))
                                    {
                                        opts = JsonSerializer.Deserialize<AutounattendOptions>(oEl.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                                    }
                                    else
                                    {
                                        opts = JsonSerializer.Deserialize<AutounattendOptions>(root.Value.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
                                    }
                                }
                                string xml = AutounattendService.GenerateXml(opts);
                                SendJson(resp, new { success = true, xmlContent = xml, xml });
                                return;
                            }
                        case "/api/repair/autologon/set":
                            {
                                string u = root?.GetProperty("username").GetString() ?? "";
                                string p = root?.TryGetProperty("password", out var pEl) == true ? pEl.GetString() ?? "" : "";
                                string dom = root?.TryGetProperty("domain", out var domEl) == true ? domEl.GetString() ?? "" : "";
                                var (s, m) = RepairService.ConfigureAutoLogon(u, p, dom);
                                SendJson(resp, new { success = s, message = m });
                                return;
                            }
                        case "/api/repair/autologon/disable":
                            {
                                var (s, m) = RepairService.DisableAutoLogon();
                                SendJson(resp, new { success = s, message = m });
                                return;
                            }
                        case "/api/repair/power/ultimate":
                            {
                                var (s, m) = RepairService.EnableUltimatePerformance();
                                SendJson(resp, new { success = s, message = m });
                                return;
                            }
                        case "/api/repair/power/hibernation":
                            {
                                bool enable = root?.TryGetProperty("enable", out var enEl) == true && enEl.GetBoolean();
                                var (s, m) = RepairService.ToggleHibernation(enable);
                                SendJson(resp, new { success = s, message = m });
                                return;
                            }
                        case "/api/tools/hosts/block-adobe":
                            {
                                var (hS, hM) = SystemToolsService.BlockAdobeDomainsInHosts();
                                SendJson(resp, new { success = hS, message = hM });
                                return;
                            }
                    }
                }

                resp.StatusCode = 404;
                SendJson(resp, new { success = false, message = $"Endpoint {method} {path} not found." });
            }
            catch (Exception ex)
            {
                resp.StatusCode = 500;
                SendJson(resp, new { success = false, message = ex.Message });
            }
        }

        private void ServeStaticAsset(HttpListenerRequest req, HttpListenerResponse resp, string path)
        {
            if (path == "/" || string.IsNullOrEmpty(path))
                path = "/index.html";

            string resourcePath = "SUPOptimizer.WebAssets" + path.Replace('/', '.');
            var assembly = Assembly.GetExecutingAssembly();

            using var stream = assembly.GetManifestResourceStream(resourcePath);
            if (stream == null)
            {
                // Try fallback to index.html for SPA routing
                using var indexStream = assembly.GetManifestResourceStream("SUPOptimizer.WebAssets.index.html");
                if (indexStream != null)
                {
                    ServeHtmlStream(indexStream, resp);
                    return;
                }

                resp.StatusCode = 404;
                byte[] notFound = Encoding.UTF8.GetBytes("404 Not Found");
                resp.OutputStream.Write(notFound, 0, notFound.Length);
                resp.Close();
                return;
            }

            string ext = Path.GetExtension(path).ToLowerInvariant();
            resp.ContentType = ext switch
            {
                ".html" => "text/html; charset=utf-8",
                ".css" => "text/css; charset=utf-8",
                ".js" => "application/javascript; charset=utf-8",
                ".svg" => "image/svg+xml",
                ".json" => "application/json",
                ".png" => "image/png",
                ".ico" => "image/x-icon",
                _ => "application/octet-stream"
            };

            if (ext == ".html")
            {
                ServeHtmlStream(stream, resp);
            }
            else
            {
                resp.StatusCode = 200;
                resp.ContentLength64 = stream.Length;
                stream.CopyTo(resp.OutputStream);
                resp.Close();
            }
        }

        private void ServeHtmlStream(Stream stream, HttpListenerResponse resp)
        {
            using var reader = new StreamReader(stream, Encoding.UTF8);
            string html = reader.ReadToEnd();

            // Inject runtime session token and port into index.html
            html = html.Replace("{{SUP_TOKEN}}", _sessionToken)
                       .Replace("{{SUP_PORT}}", _port.ToString());

            byte[] bytes = Encoding.UTF8.GetBytes(html);
            resp.ContentType = "text/html; charset=utf-8";
            resp.StatusCode = 200;
            resp.ContentLength64 = bytes.Length;
            resp.OutputStream.Write(bytes, 0, bytes.Length);
            resp.Close();
        }

        private static string ReadBody(HttpListenerRequest req)
        {
            if (!req.HasEntityBody) return string.Empty;
            using var reader = new StreamReader(req.InputStream, req.ContentEncoding);
            return reader.ReadToEnd();
        }

        private static void SendJson(HttpListenerResponse resp, object data)
        {
            try
            {
                resp.ContentType = "application/json; charset=utf-8";
                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                resp.ContentLength64 = bytes.Length;
                resp.OutputStream.Write(bytes, 0, bytes.Length);
            }
            catch (Exception ex)
            {
                AuditLogger.Log("Server", "SendJson Exception", ex.Message, success: false, errorMessage: ex.Message);
            }
            finally
            {
                try { resp.Close(); } catch { }
            }
        }
    }
}
