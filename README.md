# SUPOptimizer

<div align="center">
  <h1><span style="color:#00f2fe;font-weight:900;">SUPO</span><span style="color:#f0f6fc;font-weight:700;">ptimizer</span></h1>
  <p><strong>Professional Open-Source Administration, Optimization, Debloating, and Maintenance Suite for Windows 10 & Windows 11</strong></p>
  <p><em>Single portable executable (Zero-Install), embedded local web engine, ultra-modern UI, and native system tray host.</em></p>

  <p>
    <img src="https://img.shields.io/badge/Version-1.0.1-brightgreen?style=flat-square" alt="Version 1.0.1">
    <img src="https://img.shields.io/badge/Platform-Windows%2010%20%7C%2011%20(x64)-0078d4?style=flat-square&logo=windows" alt="Platform">
    <img src="https://img.shields.io/badge/.NET-8.0-512bd4?style=flat-square&logo=dotnet" alt=".NET 8">
    <img src="https://img.shields.io/badge/Architecture-Single--File%20Portable-00c853?style=flat-square" alt="Portable">
    <img src="https://img.shields.io/badge/UI-WebView2%20%2B%20Browser%20Fallback-ff6d00?style=flat-square" alt="UI">
    <img src="https://img.shields.io/badge/License-MIT-blue?style=flat-square" alt="License">
  </p>
</div>

---

## Table of Contents

1. [What is SUPOptimizer](#what-is-supoptimizer)
2. [Key Features](#key-features)
3. [Available Editions (Standalone vs Lite)](#available-editions-standalone-vs-lite)
4. [Requirements and Quick Start](#requirements-and-quick-start)
5. [Detailed Module Guide (19 Sections)](#detailed-module-guide-19-sections)
   - [Core Cluster](#1-core-cluster)
   - [Windows Cluster](#2-windows-cluster)
   - [Performance Cluster](#3-performance-cluster)
   - [Maintenance Cluster](#4-maintenance-cluster)
   - [System Cluster](#5-system-cluster)
6. [Security, Reversibility, and Rollback](#security-reversibility-and-rollback)
7. [Keyboard Shortcuts](#keyboard-shortcuts)
8. [Compilation and Build Pipeline](#compilation-and-build-pipeline)
9. [Testing and Automated Verification Suite](#testing-and-automated-verification-suite)
10. [FAQ and Troubleshooting](#faq-and-troubleshooting)
11. [Version History and Changelog](#version-history-and-changelog)

---

## What is SUPOptimizer

**SUPOptimizer** is a comprehensive, state-of-the-art suite for the administration, cleanup, and advanced tuning of **Microsoft Windows 10 and Windows 11 (64-bit)** operating systems.

It combines the performance, safety, and responsiveness of a native **C# / .NET 8** backend with a modern, elegant, and responsive UI inspired by the design languages of *Linear*, *Raycast*, and *Apple macOS Sonoma*.

The application is engineered to be **completely portable**: zero installation required, no low-level third-party drivers installed, leaves no registry clutter, and can be run straight from an IT technician's USB drive.

---

## Key Features

- **Dual-Engine Graphical Interface**:
  - Native hardware-accelerated window powered by **Microsoft Edge WebView2**.
  - **"Web View"** button: instantly open the dashboard in your default browser (Chrome, Edge, Brave, Firefox) connected to the local endpoint `http://127.0.0.1:<port>/`.
  - Non-blocking fallback: even in environments lacking the WebView2 runtime (such as minimal VMs or Windows PE environments), the application automatically redirects to your default web browser.
- **Integrated System Tray Host**:
  - Discreetly minimizes to the Windows System Tray with a context menu for quick access, management, and clean exit.
- **Over 80 Registry & Kernel Tweaks**:
  - UI customization, privacy hardening, telemetry deactivation, network tuning, and audio/video latency optimization.
- **Selective & Profiled UWP Debloater**:
  - Removal of preinstalled sponsored apps and system bloatware with guided presets (*Safe*, *Balanced*, *Aggressive*).
- **Live Diagnostics & Repair Console**:
  - Transparent, guided execution of `SFC /scannow`, `DISM /RestoreHealth`, and `CHKDSK` with real-time log streaming.
- **Autounattend.xml Generator**:
  - Step-by-step creation of automated answer files for unattended clean installations of Windows 10 & 11, including automatic bypass of TPM 2.0 / Secure Boot requirements and offline local account setup.
- **Global Command Palette (`Ctrl+K`)**:
  - Instant keyboard search to navigate across all 19 modules and execute quick actions.

---

## Available Editions (Standalone vs Lite)

The automated build pipeline (`build-release.ps1`) produces two portable variants in the `dist/` directory:

| Feature | Standalone (`SUPOptimizer.exe`) | Lite (`SUPOptimizer-Lite.exe`) |
| :--- | :--- | :--- |
| **Size** | ~68.9 MB | ~2.1 MB |
| **.NET 8 Runtime** | **Embedded / Included** (Self-contained) | Requires .NET 8 Desktop Runtime installed on the PC |
| **Installation** | None (Zero-Install) | None (Zero-Install) |
| **Ideal Use Case** | Fresh installs, technician USB drives, offline VMs, clean PCs | Ultra-fast download for machines with .NET 8 already installed |

---

## Requirements and Quick Start

### System Requirements
- **Operating System**: Windows 10 (version 1809 or higher) or Windows 11 (all editions, x64).
- **Privileges**: Running as Administrator (*Right click > Run as administrator*) is strongly recommended to allow management of system registry keys (`HKLM`), Windows services, and DISM features.

### How to Run
1. Download `SUPOptimizer.exe` from the `dist/` folder or official releases.
2. Double-click the file to launch it.
3. If you prefer viewing the dashboard fullscreen in your web browser (e.g., Google Chrome or Microsoft Edge), click the **"Web View"** button in the top-right corner of the application bar.
4. To minimize or close the app, use standard window controls or right-click the icon in the Windows System Tray.

---

## Detailed Module Guide (19 Sections)

The application modules are grouped into 5 logical clusters in the navigation sidebar:

```
┌────────────────────────────────────────────────────────────────────────┐
│                              SUPOptimizer                              │
├──────────────┬──────────────┬──────────────┬─────────────┬─────────────┤
│     CORE     │   WINDOWS    │ PERFORMANCE  │ MAINTENANCE │   SYSTEM    │
├──────────────┼──────────────┼──────────────┼─────────────┼─────────────┤
│ • Dashboard  │ • Tweaks     │ • Optimizer  │ • Storage   │ • Services  │
│ • HealthScan │ • Privacy    │ • Gaming     │ • Repair    │ • Startup   │
│ • Logs       │ • Debloat    │ • Network    │ • Backup    │ • Hardware  │
│ • Settings   │ • Apps       │              │             │ • Features  │
│              │              │              │             │ • Profiles  │
│              │              │              │             │ • ISO Setup │
│              │              │              │             │ • Tools     │
└──────────────┴──────────────┴──────────────┴─────────────┴─────────────┘
```

---

### 1. Core Cluster

- **Dashboard**:
  - Real-time telemetry monitoring CPU usage percentage, RAM consumption (used/total), available disk space, and system uptime.
  - **⚡ Purge RAM**: instant purging of working sets and non-essential standby memory with real-time reclaimed MB computation.
  - Automated detection of virtualized environments (Hyper-V, VMware, VirtualBox, QEMU) and Windows license status.
  - One-click quick actions for routine maintenance and administrator privilege indicators.
- **Health Scan**:
  - Rapid 6-point diagnostic assessment: disk health, critical services, antivirus status, system file integrity, drive capacity, and restore point readiness.
  - Severity-graded visual indicators: *OK (Green)*, *Info (Blue)*, *Warning (Yellow)*, *Critical (Red)*.
- **Audit Logs**:
  - Full transparent history of every system modification (prior value, new value applied, timestamp, and success status).
- **Settings**:
  - Safe mode toggle (Safe Mode / Dry Run to preview operations without modifying disk or registry).
  - Authentication token management for local REST APIs.
  - Window close behavior options (minimize to system tray or exit completely).

---

### 2. Windows Cluster

- **Windows Tweaks**:
  - **User Interface & File Explorer**: show file extensions for known types, show hidden files and folders, restore classic Windows 10 context menu on Windows 11, display drive letters before drive names, remove Home/Gallery clutter from Explorer.
  - **Taskbar (Windows 11)**: enable "End Task" on right-click for running apps, taskbar alignment (left vs. center), fast switching to last active window (*LastActiveClick*).
  - **System Behavior**: disable Windows error reporting prompts, turn off lock screen tips and ads, suppress disruptive notifications, prevent forced automatic restarts after updates.
- **Privacy & Telemetry**:
  - Disable Microsoft diagnostic and telemetry service (`DiagTrack`).
  - Turn off Advertising ID used for cross-app advertising profiling.
  - Disable Cortana, Bing search integration in Start Menu, and Activity History (Timeline).
  - Deactivate Microsoft Copilot AI, Windows Recall (screen snapshots), and intrusive AI features in Notepad and Paint.
- **Debloater (UWP AppX)**:
  - Curated catalog of 38 preinstalled packages that are often unnecessary (sponsored apps, promotional games, duplicate utilities).
  - Preset filters: **Safe** (risk-free removal of sponsored software), **Balanced**, and **Aggressive**.
  - Deep-uninstall capabilities for Microsoft OneDrive and Microsoft Edge.
- **Apps Manager**:
  - Full inventory of installed Win32 / x64 desktop software.
  - Quick search, install path inspection, and one-click launch of official uninstallers.

---

### 3. Performance Cluster

- **System Optimizer**:
  - Multimedia Class Scheduler Service (`MMCSS`) tuning and network throttling removal for high-throughput streaming and downloads.
  - Disable NTFS last access timestamp updates (`NtfsDisableLastAccessUpdate`) to reduce unnecessary SSD/NVMe writes.
  - Enable Win32 Long Path support (>260 characters).
- **Gaming Mode**:
  - Activate native Windows Game Mode.
  - Disable Game DVR and background gameplay video capture to free up GPU cycles.
  - Disable mouse pointer acceleration (Enhanced Pointer Precision) for true 1:1 raw input in gaming.
  - Disable Multiplane Overlay (MPO) to prevent micro-stuttering and flickering on NVIDIA and AMD GPUs.
- **Network Engine**:
  - Complete inspection of active network adapters, local IP addresses, default gateways, and configured DNS servers.
  - One-click network repair tools: **Flush DNS**, **Reset Winsock**, **Reset TCP/IP Stack**.
  - Fast, secure DNS presets with instant application: *Cloudflare (1.1.1.1)*, *Google (8.8.8.8)*, *Quad9 (9.9.9.9)*, and automatic *DHCP restore*.
  - Integrated diagnostic Ping utility with latency statistics.

---

### 4. Maintenance Cluster

- **Storage Cleaner**:
  - Safe scanning and cleaning across 12 temporary and unneeded file categories:
    - User temporary files (`%TEMP%`) and system temp files (`C:\Windows\Temp`).
    - File Explorer thumbnail cache (Thumbnails).
    - Chromium browser caches and temporary data (Google Chrome, Microsoft Edge, Brave).
    - Legacy log files, Windows Error Reporting (WER) crash dumps, and Windows setup logs.
  - Preview mode to review reclaimable disk space before confirming deletion.
- **Repair Center**:
  - Interactive guided console with real-time streaming output for Windows maintenance utilities:
    - **System File Checker**: `sfc /scannow` to detect and repair corrupt operating system files.
    - **Windows Component Store Repair**: `dism /online /cleanup-image /restorehealth` to restore Windows image integrity.
    - **Disk Check**: `chkdsk` in read-only mode or scheduled for the next system reboot.
    - **Windows Update Reset**: stops services, purges `SoftwareDistribution` cache, and restarts update subsystems cleanly.
- **Backup & Restore**:
  - Instant creation of a **System Restore Point (VSS - Volume Shadow Copy)** prior to major adjustments.
  - **ChangeSet Management**: every modified tweak records its previous state as JSON (`data/snapshots/`), enabling targeted rollbacks at any time.

---

### 5. System Cluster

- **Services Manager**:
  - Comprehensive listing of Windows services with search filters by name and execution status.
  - Start, stop, restart, and change startup type (Automatic, Manual, Disabled).
- **Startup Manager**:
  - Transparent control over applications configured to launch on computer startup.
  - Inspects `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`, `HKLM\...`, and Start Menu Startup folders, supporting safe deactivation without destructive deletion.
- **App Installer**:
  - Curated catalog of essential open-source and freeware software (7-Zip, Notepad++, Git, VLC, Visual Studio Code, Firefox, Chrome, Brave, Revo Uninstaller).
  - Silent automated installation powered by native package managers (free of adware or bundled toolbars).
  - **⚡ Winget Software Updates**: rapid scanning of installed programs with pending upgrades and one-click bulk upgrade capability.
- **Hardware Specs**:
  - Sub-millisecond hardware profiling: CPU (model, physical cores, logical threads), motherboard, BIOS vendor and revision, installed physical RAM, graphics cards (GPU), and storage drives.
  - **🔋 Battery Diagnostics**: detects battery health and wear level, cycle count, active power plan, and generates the official HTML diagnostic report (`powercfg /batteryreport`).
- **Security Overview**:
  - Real-time status of Microsoft Defender (antivirus service active, real-time protection enabled).
  - Windows Firewall profiles status (Domain, Private, Public).
  - User Account Control (UAC) notification level.
- **Windows Features**:
  - Query and toggle optional system components via DISM:
    - Windows Sandbox
    - Windows Subsystem for Linux (WSL)
    - Hyper-V and Virtual Machine Platform
    - Legacy components such as DirectPlay and Telnet Client.
- **Setup Profiles**:
  - Save, export, and import complete system configuration profiles in JSON format.
  - Built-in presets: *Gaming*, *Privacy Focus*, *Minimal*, *Standard Workstation*.
- **Unattended ISO Generator**:
  - Guided generator for `autounattend.xml` answer files placed in the root of Windows 10/11 USB installation media.
  - Includes options for:
    - Automatic bypass of TPM 2.0, Secure Boot, and minimum RAM checks (`LabConfig`).
    - Bypass of mandatory Microsoft account requirement (`BypassNRO`) to immediately create an offline local account.
    - Timezone, language, local username configuration, and optional auto-logon.
- **System Tools**:
  - **HOSTS File Editor**: view, safely edit, and apply one-click blocking rules for Microsoft telemetry and tracking endpoints.
  - **Port & Socket Inspector**: live inspector of active TCP/UDP connections, remote addresses, and associated process IDs (PID).
  - **Environment Variables**: quick inspection and modification of user and system environment variables.
  - **Run Aliases**: configure shortcut commands executable directly from the `Win + R` Run dialog.
  - **Official License & Activation Center**: safe WMI `SoftwareLicensingProduct` inspection to verify genuine Windows activation status, license channel (Retail, OEM, Volume KMS), partial product key, and direct shortcut to Windows Settings.

---

## Security, Reversibility, and Rollback

SUPOptimizer is built with rigorous safety safeguards to prevent unintended system instability:

1. **No Arbitrary Shell Execution from Frontend**:
   - The HTML/JS UI cannot execute arbitrary shell commands. Every operation is routed through strongly typed, validated C# service methods.
2. **Ephemeral Session Token (`X-SUP-Token`)**:
   - All REST requests sent to the local `127.0.0.1` server require a cryptographically random token generated on each application startup.
3. **Dry-Run / Preview Mode**:
   - You can inspect exact registry keys, paths, and values before applying any changes.
4. **Restore Points and JSON Snapshots**:
   - Prior to applying bulk tweaks, SUPOptimizer can trigger a Windows System Restore Point and saves rollback snapshots to `data/snapshots/`.

---

## Keyboard Shortcuts

| Key / Shortcut | Function |
| :--- | :--- |
| `Ctrl + K` | Opens the **Command Palette** to quickly search modules, tweaks, and settings |
| `Esc` | Closes active modals, dialogs, or the Command Palette |
| `F5` / `Ctrl + R` | Reloads current view and refreshes telemetry data |
| `Alt + ←` | Navigates back in view history |
| `Alt + →` | Navigates forward in view history |

---

## Compilation and Build Pipeline

The project targets .NET 8 SDK and provides an automated PowerShell script that cleans, builds, optimizes, and publishes single-file binaries.

To compile both editions (*Standalone* and *Lite*):

```powershell
# From the repository root:
powershell -ExecutionPolicy Bypass -File .\build-release.ps1
```

The resulting binaries will be placed in the `dist/` directory:
- `dist\SUPOptimizer.exe` (Standalone, ~68.9 MB)
- `dist\SUPOptimizer-Lite.exe` (Lite, ~2.1 MB)

---

## Testing and Automated Verification Suite

A comprehensive end-to-end verification suite (`test-verification.ps1`) validates over 20 functional checkpoints on the compiled binary (headless launch, port binding, security token verification, REST API responses, DISM modules, XML generation, cleanup, and graceful shutdown).

To run the verification suite:

```powershell
# Verify the Standalone edition:
powershell -ExecutionPolicy Bypass -File .\test-verification.ps1 -Edition Standalone

# Verify the Lite edition:
powershell -ExecutionPolicy Bypass -File .\test-verification.ps1 -Edition Lite
```

---

## FAQ and Troubleshooting

### Why might antivirus software flag the binary?
Certain antivirus engines apply generic heuristic flags to newly compiled single-file executables or software that adjusts system registry keys (such as telemetry policies). SUPOptimizer source code is 100% transparent, free of malicious payloads, and can be inspected and compiled directly from source.

### How can I undo a tweak I applied?
Under **Maintenance > Backup & Restore**, you can view the ChangeSet history and restore original values with a single click. Alternatively, you can use the Windows System Restore Points created prior to your changes.

### Can I run this tool on multiple PCs from a USB drive?
Yes. The **Standalone** edition (`SUPOptimizer.exe`) bundles the complete runtime within a single `.exe` file: simply copy it to any USB thumb drive and run it on any Windows 10 or Windows 11 computer.

---

## Version History and Changelog

### Current Version: `1.0.1` (Official Release)
*Release date: September 2026*

#### Release Notes & New Features (v1.0.1)
- **🪪 Official Windows License & Activation Center**:
  - Native WMI/CIM querying via the `SoftwareLicensingProduct` system class (no external scripts or security risks).
  - Real-time detection of genuine activation status (*Licensed / Permanent*, *Grace Period*, *Unlicensed*), distribution channel (*Retail*, *OEM:DM*, *Volume KMS/MAK*), partial product key (`PartialProductKey`), and Windows edition.
  - One-click shortcut to launch the official Windows Activation Settings (`ms-settings:activation`).
- **⚡ Standby Memory & RAM Cache Purger**:
  - High-performance C# engine utilizing Win32 `EmptyWorkingSet` and `GlobalMemoryStatusEx` to purge process working sets and non-essential standby RAM.
  - One-click quick purge button in the Dashboard RAM telemetry card with instant toast notification and reclaimed MB counter (over 600 MB freed in testing).
- **📦 Winget Bulk Package Upgrader**:
  - Integrated software update scanner powered by native `winget upgrade`.
  - Added *"⚡ Software Updates"* tab in App Store / Installer with side-by-side version comparison (Installed vs Available) and one-click bulk upgrade button.
- **🔋 Battery Health Diagnostics & Power Plans**:
  - Comprehensive battery hardware analysis via `Win32_Battery` and `root\wmi` (design capacity vs current full capacity, wear level percentage, total charge cycles, and active power plan).
  - Automated generation and browser preview of the official Windows HTML battery report (`powercfg /batteryreport`).
- **📚 Comprehensive User Guide & Documentation**:
  - Completely rewritten `README.md` with in-depth documentation for all 19 suite modules, step-by-step user guidance, automated compilation instructions, and security FAQs.
  - Synchronized application title and version badges across the UI, WebView2 window, and system tray context menu (`SUPOptimizer v1.0.1`).

---

<div align="center">
  <sub>SUPOptimizer v1.0.1 — Built with dedication to Windows transparency, privacy, and performance.</sub>
</div>
