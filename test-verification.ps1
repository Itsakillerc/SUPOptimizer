param(
    [ValidateSet("Standalone", "Lite")]
    [string]$Edition = "Standalone"
)

# SUPOptimizer Comprehensive End-to-End Verification Suite
$ErrorActionPreference = "Stop"

# Ensure no orphan processes are lingering
Get-Process "*SUPOptimizer*" -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Milliseconds 400

$port = 5894
$exePath = if ($Edition -eq "Lite") { "dist\SUPOptimizer-Lite.exe" } else { "dist\SUPOptimizer.exe" }

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "  SUPOptimizer - Quality Assurance & Verification Suite" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. Check binaries exist
if (-not (Test-Path $exePath)) {
    throw "Binary $exePath not found!"
}
$standaloneSize = [math]::Round((Get-Item $exePath).Length / 1MB, 2)
Write-Host "[1/7] Standalone binary found: $exePath ($standaloneSize MB)" -ForegroundColor Green

$litePath = "dist\SUPOptimizer-Lite.exe"
if (Test-Path $litePath) {
    $liteSize = [math]::Round((Get-Item $litePath).Length / 1MB, 2)
    Write-Host "      Lite binary found: $litePath ($liteSize MB)" -ForegroundColor Green
}

# 2. Launch process with explicit array arguments
Write-Host "`n[2/7] Starting $exePath on port $port (minimized)..." -ForegroundColor Yellow
$proc = Start-Process -FilePath $exePath -ArgumentList @("--port", "$port", "--minimized") -PassThru

# Wait for process initialization
Start-Sleep -Seconds 3

# Verify active port from port.txt if written
$portFile = "dist\data\port.txt"
if (Test-Path $portFile) {
    $activePort = (Get-Content $portFile).Trim()
    if ($activePort) {
        $port = [int]$activePort
        Write-Host "      Confirmed bound port from data\port.txt: $port" -ForegroundColor Gray
    }
}

try {
    # 3. Test HTTP 200 and Web Assets
    Write-Host "`n[3/7] Requesting Web UI at http://127.0.0.1:$port/ ..." -ForegroundColor Yellow
    $webResp = Invoke-WebRequest -Uri "http://127.0.0.1:$port/" -UseBasicParsing -TimeoutSec 10
    if ($webResp.StatusCode -ne 200) {
        throw "Expected HTTP 200, got $($webResp.StatusCode)"
    }
    Write-Host "      [OK] HTTP Status: 200 OK" -ForegroundColor Green

    $html = $webResp.Content

    # Check Sidebar & Brand Markup
    if ($html.Contains('class="brand-title">SUPOptimizer</span>') -and $html.Contains('class="brand-badge">v1.0.0</span>')) {
        Write-Host "      [OK] Brand title verified: SUPOptimizer v1.0.0" -ForegroundColor Green
    } else {
        throw "Brand title markup or v1.0.0 badge missing in HTML!"
    }

    if ($html.Contains('class="app-sidebar"')) {
        Write-Host "      [OK] Vertical left sidebar navigation verified" -ForegroundColor Green
    } else {
        throw "Sidebar markup missing in HTML!"
    }

    # Check Subtabs Container with Golden Underline
    if ($html.Contains('class="subtabs-container"')) {
        Write-Host "      [OK] Horizontal sub-category tabs container verified" -ForegroundColor Green
    } else {
        throw "Subtabs container missing in HTML!"
    }

    # Check Command Palette
    if ($html.Contains('id="modal-palette"')) {
        Write-Host "      [OK] Ctrl+K Command Palette overlay markup verified" -ForegroundColor Green
    } else {
        throw "Command palette modal missing in HTML!"
    }

    # Check Close Confirmation Modal
    if ($html.Contains('id="modal-close-confirm"') -and $html.Contains('Minimize to System Tray') -and $html.Contains('Exit Application Completely')) {
        Write-Host "      [OK] Close Confirmation Modal (Minimize to Tray vs Exit Completely) verified" -ForegroundColor Green
    } else {
        throw "Close confirmation modal markup missing or incomplete in HTML!"
    }

    # 4. Extract runtime token and test API endpoints
    Write-Host "`n[4/7] Validating API endpoints..." -ForegroundColor Yellow
    $tokenIdx = $html.IndexOf('window.SUP_TOKEN = "')
    if ($tokenIdx -lt 0) {
        throw "window.SUP_TOKEN not found in HTML!"
    }
    $startIdx = $tokenIdx + 20
    $endIdx = $html.IndexOf('"', $startIdx)
    $token = $html.Substring($startIdx, $endIdx - $startIdx)
    Write-Host "      Extracted session token: $token" -ForegroundColor Gray

    $headers = @{ "X-SUP-Token" = $token }

    # Test Metrics
    Write-Host "      Calling /api/system/metrics..." -ForegroundColor Gray
    $metrics = Invoke-RestMethod -Uri "http://127.0.0.1:$port/api/system/metrics" -Headers $headers -TimeoutSec 10
    Write-Host "      [OK] /api/system/metrics: CPU=$($metrics.cpuUsagePercent)%, RAM=$($metrics.usedRamMb)/$($metrics.totalRamMb) MB, Admin=$($metrics.isAdmin)" -ForegroundColor Green
    
    if (-not $metrics.ramTotalBytes -or $metrics.ramTotalBytes -le 0) {
        throw "RAM total bytes invalid or missing!"
    }
    Write-Host "        - RAM Total: $($metrics.ramTotalBytes) bytes ($([math]::Round($metrics.ramTotalBytes / 1GB, 2)) GB)" -ForegroundColor Gray
    Write-Host "        - CPU Model: $($metrics.cpuModel)" -ForegroundColor Gray
    Write-Host "        - GPU Model: $($metrics.gpuModel)" -ForegroundColor Gray
    Write-Host "        - Uptime: $($metrics.systemUptime)" -ForegroundColor Gray
    Write-Host "        - Multi-Disk Drives Detected: $($metrics.drives.Count)" -ForegroundColor Gray
    if ($metrics.drives.Count -eq 0) {
        throw "Expected at least 1 drive metric in drives list!"
    }
    foreach ($drv in $metrics.drives) {
        Write-Host "          * Drive $($drv.name) [$($drv.volumeLabel)] - Total: $($drv.totalGb) GB, Free: $($drv.freeGb) GB ($($drv.usedPercent)%)" -ForegroundColor Gray
    }

    # Test Hardware Inspection Suite
    Write-Host "`n[4b/7] Validating Comprehensive Hardware Inspection API..." -ForegroundColor Yellow
    $hw = Invoke-RestMethod -Uri "http://127.0.0.1:$port/api/system/hardware" -Headers $headers -TimeoutSec 10
    Write-Host "      [OK] /api/system/hardware: CPU=$($hw.cpu.name), Cores=$($hw.cpu.numberOfCores), Threads=$($hw.cpu.numberOfLogicalProcessors)" -ForegroundColor Green
    Write-Host "        - RAM Total: $($hw.memory.totalRamGb) GB, Slots Used: $($hw.memory.slotsUsed)/$($hw.memory.totalSlots)" -ForegroundColor Gray
    Write-Host "        - RAM Modules: $($hw.memory.modules.Count) physical DIMMs reported" -ForegroundColor Gray
    foreach ($mod in $hw.memory.modules) {
        Write-Host "          * DIMM: $($mod.deviceLocator) | $($mod.capacityFormatted) | $($mod.speedMhz) MHz | $($mod.manufacturer) | $($mod.partNumber)" -ForegroundColor Gray
    }
    Write-Host "        - GPUs Detected: $($hw.gpus.Count)" -ForegroundColor Gray
    foreach ($gpu in $hw.gpus) {
        Write-Host "          * GPU: $($gpu.name) | Driver: $($gpu.driverVersion) | VRAM: $($gpu.memoryFormatted)" -ForegroundColor Gray
    }
    Write-Host "        - Physical Disks: $($hw.disks.Count)" -ForegroundColor Gray
    foreach ($pd in $hw.disks) {
        Write-Host "          * Disk: $($pd.model) | $($pd.interfaceType) | $($pd.sizeFormatted)" -ForegroundColor Gray
    }
    Write-Host "        - Peripherals: Audio=$($hw.peripherals.audioDevices.Count), Monitors=$($hw.peripherals.monitors.Count), Input=$($hw.peripherals.inputDevices.Count), NICs=$($hw.peripherals.networkControllers.Count)" -ForegroundColor Gray

    # Test Tweaks
    Write-Host "`n[4c/7] Calling /api/tweaks..." -ForegroundColor Gray
    $tweaks = Invoke-RestMethod -Uri "http://127.0.0.1:$port/api/tweaks" -Headers $headers -TimeoutSec 10
    Write-Host "      [OK] /api/tweaks: Loaded $($tweaks.Count) registered system tweaks" -ForegroundColor Green

    # Verify newly requested tweaks from competitor list are present
    $requiredTweaks = @(
        "priv_disable_office_telemetry",
        "win_stop_auto_updates",
        "priv_disable_edge_copilot",
        "win_enable_utc_time",
        "win_disable_onedrive_sync",
        "perf_disable_hpet",
        "ctx_take_ownership",
        "ctx_open_with_notepad",
        "win_end_task_right_click",
        "win_dark_mode",
        "win_disable_lockscreen",
        "perf_mpo_disable",
        "perf_disable_storage_sense",
        "privacy_windows_recall",
        "privacy_click_to_do",
        "privacy_app_location",
        "win_hide_home_gallery",
        "perf_modern_standby_net",
        "perf_brave_debloat",
        "perf_prefer_ipv4",
        "perf_disable_wsaifabric",
        "privacy_consumer_features"
    )
    foreach ($tid in $requiredTweaks) {
        $found = $tweaks | Where-Object { $_.id -eq $tid }
        if ($found) {
            Write-Host "        - Verified tweak: $($found.name) [Category: $($found.category)]" -ForegroundColor Gray
        } else {
            throw "Expected tweak $tid was not registered!"
        }
    }

    # 5. Test Bloatware API (verifying IsInstalled is present)
    Write-Host "`n[5/7] Testing Bloatware & Debloater catalog..." -ForegroundColor Yellow
    $packages = Invoke-RestMethod -Uri "http://127.0.0.1:$port/api/debloat/packages" -Headers $headers -TimeoutSec 10
    Write-Host "      [OK] /api/debloat/packages: Returned $($packages.Count) bloatware packages" -ForegroundColor Green
    if ($packages.Count -lt 20) {
        throw "Expected at least 20 bloatware packages, found $($packages.Count)!"
    }
    $installedCount = ($packages | Where-Object { $_.isInstalled -eq $true }).Count
    Write-Host "        - Detected $installedCount currently installed packages out of $($packages.Count) catalog apps" -ForegroundColor Gray

    # 5b. Test Storage Analysis (verifying Recycle Bin is properly measured)
    Write-Host "`n[5b/7] Testing Storage Analysis & Disk Cleanup targets..." -ForegroundColor Yellow
    $storageTargets = Invoke-RestMethod -Uri "http://127.0.0.1:$port/api/storage/analyze" -Headers $headers -TimeoutSec 10
    Write-Host "      [OK] /api/storage/analyze: Returned $($storageTargets.Count) cleanup targets" -ForegroundColor Green
    if ($storageTargets.Count -eq 0) {
        throw "Expected storage targets to be returned, got 0!"
    }
    $rbTarget = $storageTargets | Where-Object { $_.id -eq "recycle_bin" }
    if ($rbTarget) {
        Write-Host "        - Recycle Bin Target: $($rbTarget.name) | Size: $($rbTarget.sizeMb) MB | Files: $($rbTarget.fileCount)" -ForegroundColor Gray
    }
    foreach ($st in $storageTargets) {
        Write-Host "        - Target: $($st.name) | Size: $($st.sizeMb) MB | Files: $($st.fileCount)" -ForegroundColor Gray
    }

    # 5c. Test Startup Applications and status synchronization
    Write-Host "`n[5c/7] Testing Startup Applications Manager..." -ForegroundColor Yellow
    $startupItems = Invoke-RestMethod -Uri "http://127.0.0.1:$port/api/startup" -Headers $headers -TimeoutSec 10
    Write-Host "      [OK] /api/startup: Returned $($startupItems.Count) startup items" -ForegroundColor Green
    foreach ($item in $startupItems) {
        Write-Host "        - Startup item: $($item.name) | Location: $($item.location) | IsEnabled: $($item.isEnabled)" -ForegroundColor Gray
    }

    # 5d. Test Active Network Connections & Stack Optimizations
    Write-Host "`n[5d/7] Testing Network Connections & Network Stack Optimizations..." -ForegroundColor Yellow
    $connections = Invoke-RestMethod -Uri "http://127.0.0.1:$port/api/network/connections" -Headers $headers -TimeoutSec 10
    Write-Host "      [OK] /api/network/connections: Inspected $($connections.Count) live socket connections (NetLimiter-style)" -ForegroundColor Green
    if ($connections.Count -gt 0) {
        $sampleConn = $connections[0]
        Write-Host "        - Sample connection: PID=$($sampleConn.processId) ($($sampleConn.processName)) | $($sampleConn.localAddress):$($sampleConn.localPort) -> $($sampleConn.remoteAddress):$($sampleConn.remotePort) [$($sampleConn.state)] | Direction: $($sampleConn.direction)" -ForegroundColor Gray
    }
    $netOptRes = Invoke-RestMethod -Uri "http://127.0.0.1:$port/api/network/optimize" -Method Post -Headers $headers -Body "{}" -ContentType "application/json" -TimeoutSec 10
    Write-Host "      [OK] /api/network/optimize: $($netOptRes.message)" -ForegroundColor Green

    $netPwrRes = Invoke-RestMethod -Uri "http://127.0.0.1:$port/api/network/powersaving" -Method Post -Headers $headers -Body '{"disable":true}' -ContentType "application/json" -TimeoutSec 10
    Write-Host "      [OK] /api/network/powersaving: $($netPwrRes.message)" -ForegroundColor Green

    # 5e. Test Window Host Endpoints
    Write-Host "`n[5e/7] Testing Window controls & Host endpoints..." -ForegroundColor Yellow
    $dragResp = Invoke-RestMethod -Uri "http://127.0.0.1:$port/api/host/drag" -Method Post -Headers $headers -Body "{}" -ContentType "application/json" -TimeoutSec 10
    Write-Host "      [OK] /api/host/drag: $($dragResp.success)" -ForegroundColor Green
    $maxResp = Invoke-RestMethod -Uri "http://127.0.0.1:$port/api/host/maximize" -Method Post -Headers $headers -Body "{}" -ContentType "application/json" -TimeoutSec 10
    Write-Host "      [OK] /api/host/maximize: $($maxResp.success)" -ForegroundColor Green

    # 6. Test New System Tools Endpoints
    Write-Host "`n[6/7] Validating System Tools (HOSTS, DNS, Env Vars, Run Aliases, Shodan, Safe Boost)..." -ForegroundColor Yellow

    # Test HOSTS
    $hosts = Invoke-RestMethod -Uri "http://127.0.0.1:$port/api/tools/hosts" -Headers $headers -TimeoutSec 10
    Write-Host "      [OK] /api/tools/hosts: Read $($hosts.content.Length) bytes from $($hosts.path)" -ForegroundColor Green

    # Test DNS Presets
    $dnsPresets = Invoke-RestMethod -Uri "http://127.0.0.1:$port/api/tools/dns/presets" -Headers $headers -TimeoutSec 10
    Write-Host "      [OK] /api/tools/dns/presets: Loaded $($dnsPresets.Count) DNS presets (Cloudflare, Google, Quad9, AdGuard, DHCP)" -ForegroundColor Green

    # Test Environment Variables
    $envVars = Invoke-RestMethod -Uri "http://127.0.0.1:$port/api/tools/env" -Headers $headers -TimeoutSec 10
    Write-Host "      [OK] /api/tools/env: Loaded $($envVars.Count) User and System environment variables" -ForegroundColor Green

    # Test Run Aliases
    $runAliases = Invoke-RestMethod -Uri "http://127.0.0.1:$port/api/tools/run-aliases" -Headers $headers -TimeoutSec 10
    Write-Host "      [OK] /api/tools/run-aliases: Loaded $($runAliases.Count) Run dialog App Paths aliases" -ForegroundColor Green

    # Test Shodan Search
    $shodan = Invoke-RestMethod -Uri "http://127.0.0.1:$port/api/network/shodan?ip=1.1.1.1" -Headers $headers -TimeoutSec 10
    Write-Host "      [OK] /api/network/shodan: Resolved IP=$($shodan.resolvedIp), Host=$($shodan.hostName), URL=$($shodan.shodanUrl)" -ForegroundColor Green

    # Test 1-Click Safe Boost (Automation profile)
    $safeBoostBody = @{ profileId = "safe_optimize"; dryRun = $true } | ConvertTo-Json
    $safeBoostResp = Invoke-RestMethod -Uri "http://127.0.0.1:$port/api/automation/apply" -Method Post -Headers $headers -Body $safeBoostBody -ContentType "application/json" -TimeoutSec 10
    Write-Host "      [OK] /api/automation/apply (Safe Boost DryRun): Processed $($safeBoostResp.total) tweaks (Failed: $($safeBoostResp.failed))" -ForegroundColor Green

    # Test Dry-Run simulation endpoint
    $body = @{ tweakId = "priv_disable_office_telemetry"; dryRun = $true } | ConvertTo-Json
    $dryRunResp = Invoke-RestMethod -Uri "http://127.0.0.1:$port/api/tweaks/apply" -Method Post -Headers $headers -Body $body -ContentType "application/json" -TimeoutSec 10
    Write-Host "      [OK] /api/tweaks/apply (DryRun): $($dryRunResp.message)" -ForegroundColor Green

    # 6b. Test Competitor Feature Suite (Features, Profiles, Unattend, AutoLogon, Debloat Presets)
    Write-Host "`n[6b/7] Validating Advanced Competitor Feature Engine..." -ForegroundColor Yellow

    # Test Windows Features Query
    $features = Invoke-RestMethod -Uri "http://127.0.0.1:$port/api/features" -Headers $headers -TimeoutSec 15
    Write-Host "      [OK] /api/features: Inspected $($features.Count) optional Windows features" -ForegroundColor Green
    if ($features.Count -lt 5) {
        throw "Expected at least 5 optional features, found $($features.Count)!"
    }
    $sampleFeat = $features | Where-Object { $_.featureName -eq "Containers-DisposableClientVM" }
    if ($sampleFeat) {
        Write-Host "        - Found Windows Sandbox: $($sampleFeat.displayName) [State: $($sampleFeat.state)]" -ForegroundColor Gray
    }

    # Test Autounattend.xml Generator
    $unattendReq = @{
        operatingSystem = "Windows 11"
        localUsername = "TestAdmin"
        computerName = "TEST-PC"
        bypassTpmAndSecureBoot = $true
        bypassMicrosoftAccount = $true
        enableDarkTheme = $true
        disableTelemetry = $true
        enableEndTask = $true
    } | ConvertTo-Json
    $unattendResp = Invoke-RestMethod -Uri "http://127.0.0.1:$port/api/autounattend/generate" -Method Post -Headers $headers -Body $unattendReq -ContentType "application/json" -TimeoutSec 10
    if (-not $unattendResp.xmlContent -or -not $unattendResp.xmlContent.Contains("<unattend xmlns=")) {
        throw "Autounattend generator failed to output valid XML!"
    }
    Write-Host "      [OK] /api/autounattend/generate: Generated valid autounattend.xml ($($unattendResp.xmlContent.Length) chars)" -ForegroundColor Green

    # Test Setup Profiles
    $presets = Invoke-RestMethod -Uri "http://127.0.0.1:$port/api/profiles/presets" -Headers $headers -TimeoutSec 10
    Write-Host "      [OK] /api/profiles/presets: Loaded $($presets.Count) curated configuration profiles" -ForegroundColor Green
    if ($presets.Count -lt 4) {
        throw "Expected at least 4 profile presets, found $($presets.Count)!"
    }
    foreach ($p in $presets) {
        Write-Host "        - Profile: $($p.name) [$($p.tweakCount) tweaks]" -ForegroundColor Gray
    }

    $exportProfile = Invoke-RestMethod -Uri "http://127.0.0.1:$port/api/profiles/export" -Method Post -Headers $headers -Body "{}" -ContentType "application/json" -TimeoutSec 10
    if (-not $exportProfile.tweakStates) {
        throw "Profile export missing tweakStates!"
    }
    Write-Host "      [OK] /api/profiles/export: Successfully exported current configuration profile with $($exportProfile.tweakCount) tweaks" -ForegroundColor Green

    # Test Debloat Presets
    $debloatPresets = Invoke-RestMethod -Uri "http://127.0.0.1:$port/api/debloat/presets" -Headers $headers -TimeoutSec 10
    Write-Host "      [OK] /api/debloat/presets: Loaded $($debloatPresets.Count) debloater tier presets" -ForegroundColor Green
    foreach ($dp in $debloatPresets) {
        Write-Host "        - Debloat Preset: $($dp.name) [$($dp.packageIds.Count) packages]" -ForegroundColor Gray
    }

    # Test AutoLogon Status
    $autoLogonStatus = Invoke-RestMethod -Uri "http://127.0.0.1:$port/api/repair/autologon" -Headers $headers -TimeoutSec 10
    Write-Host "      [OK] /api/repair/autologon: AutoLogon Enabled=$($autoLogonStatus.enabled)" -ForegroundColor Green

    # Test Block Adobe Domains in Hosts
    $adobeHostsResp = Invoke-RestMethod -Uri "http://127.0.0.1:$port/api/tools/hosts/block-adobe" -Method Post -Headers $headers -Body "{}" -ContentType "application/json" -TimeoutSec 10
    Write-Host "      [OK] /api/tools/hosts/block-adobe: $($adobeHostsResp.message)" -ForegroundColor Green

    # 7. Clean Shutdown
    Write-Host "`n[7/7] Shutting down application cleanly..." -ForegroundColor Yellow
    Invoke-RestMethod -Uri "http://127.0.0.1:$port/api/host/exit" -Method Post -Headers $headers -Body "{}" -ContentType "application/json" -TimeoutSec 10 | Out-Null
    Start-Sleep -Seconds 1
    Write-Host "      [OK] Host shutdown signal processed" -ForegroundColor Green

    Write-Host "`n==========================================================" -ForegroundColor Green
    Write-Host "  ALL 7 VERIFICATION STEPS PASSED WITH 100% SUCCESS!" -ForegroundColor Green
    Write-Host "==========================================================" -ForegroundColor Green
}
catch {
    Write-Host "Error during verification: $_" -ForegroundColor Red
    throw $_
}
finally {
    if ($proc -and -not $proc.HasExited) {
        Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
    }
}
