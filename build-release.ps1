# SUP Optimizer - Automated Release Build Script
# Generates a standalone, portable, single-file Windows executable (SUPOptimizer.exe)

$ErrorActionPreference = "Stop"
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "  SUP OPTIMIZER - PORTABLE SINGLE-FILE BUILD PIPELINE" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. Resolve .NET SDK path
$dotnetCmd = "dotnet"
$userDotnet = "$env:LocalAppData\Microsoft\dotnet\dotnet.exe"
if (Test-Path $userDotnet) {
    $dotnetCmd = $userDotnet
}

Write-Host "`n[1/4] Checking .NET SDK..." -ForegroundColor Yellow
$sdkVersion = & $dotnetCmd --version
Write-Host "Found .NET SDK: $sdkVersion ($dotnetCmd)" -ForegroundColor Green

# 2. Clean and prepare output folder
Get-Process "*SUPOptimizer*" -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Milliseconds 500
$distDir = Join-Path $PSScriptRoot "dist"
Write-Host "`n[2/4] Preparing output directory: $distDir" -ForegroundColor Yellow
if (Test-Path $distDir) {
    Remove-Item -Recurse -Force $distDir
}
New-Item -ItemType Directory -Force -Path $distDir | Out-Null

# 3. Publish Both Editions: Standalone (Self-Contained) & Lite (Framework-Dependent)
$projectPath = Join-Path $PSScriptRoot "src\SUPOptimizer\SUPOptimizer.csproj"

Write-Host "`n[3/5] Publishing Standalone single-file binary (Self-contained, compressed, zero runtimes required)..." -ForegroundColor Yellow
& $dotnetCmd publish $projectPath `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:EnableCompressionInSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:PublishTrimmed=false `
    -p:InvariantGlobalization=true `
    -p:DebugType=none `
    -p:DebugSymbols=false `
    -o $distDir

Write-Host "`n[4/5] Publishing Lite single-file binary (Ultra-lightweight ~1.8 MB)..." -ForegroundColor Yellow
$liteTempDir = Join-Path $distDir "temp_lite"
& $dotnetCmd publish $projectPath `
    -c Release `
    -r win-x64 `
    --self-contained false `
    -p:PublishSingleFile=true `
    -p:PublishTrimmed=false `
    -p:EnableCompressionInSingleFile=false `
    -p:DebugType=none `
    -p:DebugSymbols=false `
    -o $liteTempDir

$liteExeSource = Join-Path $liteTempDir "SUPOptimizer.exe"
$liteExeDest = Join-Path $distDir "SUPOptimizer-Lite.exe"
if (Test-Path $liteExeSource) {
    Move-Item -Force $liteExeSource $liteExeDest
    Remove-Item -Recurse -Force $liteTempDir
}

# 5. Verify Output
$exePath = Join-Path $distDir "SUPOptimizer.exe"
if (Test-Path $exePath) {
    $fileInfo = Get-Item $exePath
    $sizeMb = [Math]::Round($fileInfo.Length / 1MB, 2)

    $liteSizeMb = "N/A"
    if (Test-Path $liteExeDest) {
        $liteInfo = Get-Item $liteExeDest
        $liteSizeMb = [Math]::Round($liteInfo.Length / 1MB, 2)
    }

    Write-Host "`n[5/5] BUILD PIPELINE COMPLETE!" -ForegroundColor Green
    Write-Host "==========================================================" -ForegroundColor Green
    Write-Host " Standalone Edition: $exePath" -ForegroundColor White
    Write-Host " File Size:          $sizeMb MB (Self-contained, trimmed, zero runtimes required)" -ForegroundColor White
    Write-Host " Lite Edition:       $liteExeDest" -ForegroundColor White
    Write-Host " File Size:          $liteSizeMb MB (Requires .NET 8 Desktop Runtime)" -ForegroundColor White
    Write-Host " Target OS:          Windows 10 / 11 (x64)" -ForegroundColor White
    Write-Host "==========================================================" -ForegroundColor Green
    Write-Host "`nReady for testing in clean Windows 10/11 VMs or native hardware." -ForegroundColor Cyan
} else {
    Write-Error "Build failed: SUPOptimizer.exe was not found in $distDir."
}

