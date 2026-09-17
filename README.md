# SUPOptimizer

<div align="center">
  <h1><span style="color:#00f2fe;font-weight:900;">SUPO</span><span style="color:#f0f6fc;font-weight:700;">ptimizer</span></h1>
  <p><strong>Next-Generation Native System Administration, Debloater & Performance Tuning Suite for Windows 10 & Windows 11</strong></p>
  <p><em>Distributed as a Zero-Install, Portable Single-File Executable with Embedded Local Web Engine & Tray Host</em></p>
</div>

---

## Overview

**SUPOptimizer** is an ultra-modern, high-performance maintenance, debloat, configuration, and administration platform designed for **Windows 10 and Windows 11 (x64)**. It combines native Win32/C# architecture with an embedded, state-of-the-art Web UI inspired by modern design systems (Linear, Raycast, and Apple Sonoma).

The application is distributed as a single portable `.exe` that runs completely offline with zero installation, zero external database setup, and zero cloud dependencies.

---

## Key Highlights & Innovations

### 1. Dual-Tone Branding & Modern Design System
- **Dual-Tone Typography**: Consistently formatted as **SUPO** (vibrant cyan/emerald gradient) + **ptimizer** (crisp frosted white) attached seamlessly across the UI header, cards, and metadata.
- **Clustered Navigation**: Organized into 5 logical clusters across the top bar:
  - **Core**: Dashboard, Health Scan, Logs, Settings
  - **Windows**: Windows Tweaks, Privacy & Telemetry, Debloat, Apps Manager
  - **Performance**: System Optimizer, Gaming Mode, Network Engine
  - **Maintenance**: Storage Cleaner, Repair Center, Backup & Restore
  - **System**: Services Manager, Startup Manager, Hardware Specs, Security
- **Command Palette (`Ctrl+K`)**: Instant keyboard-driven global search indexing all 19 module views, registry tweaks, and quick maintenance actions.
- **Micro-Animations & Glassmorphism**: Ambient pulse dots for live telemetry, responsive toggle switches (`.sup-switch`), and high-contrast dark mode.

### 2. Dual Portable Binary Editions
The automated build pipeline (`build-release.ps1`) produces two specialized editions in the `dist/` directory:
1. **Standalone Edition (`dist\SUPOptimizer.exe`) — ~68.8 MB**
   - **Self-contained single portable file** with the complete .NET 8 runtime embedded.
   - **Zero dependencies**: runs out-of-the-box on clean Windows 10 / Windows 11 virtual machines, fresh installations, or recovery USB drives.
2. **Lite Edition (`dist\SUPOptimizer-Lite.exe`) — ~1.82 MB**
   - **Ultra-compact single portable file** for environments where the .NET 8 Desktop Runtime is already installed.
   - Ideal for instant transfers and minimal footprint.

### 3. Native Tray Host & Local Web Hosting
- Runs in the Windows System Tray with a high-resolution icon and contextual menu.
- **Dual Display Modes**:
  - Embedded native hardware-accelerated **WebView2** window with close-to-tray lifecycle.
  - **"Open in Web Browser (Chrome / Edge)"**: 1-click button in the header and tray context menu to launch or access the dashboard directly in your favorite browser at `http://127.0.0.1:<port>/`.
  - Non-blocking fallback: if WebView2 runtime is absent (e.g. minimal VMs), SUPOptimizer seamlessly routes to your default web browser without blocking.

### 4. Enterprise-Grade Security & Reversibility
- **No Arbitrary Shell Injection**: Zero arbitrary command execution from the frontend; all actions execute through typed, validated C# domain services.
- **Cryptographic Token Protection**: REST API protected by a per-session ephemeral security token (`X-SUP-Token`).
- **Dry-Run & Preview Engine**: Inspect the exact registry paths, current values, and proposed modifications before applying any system tweak.
- **Instant Rollback (ChangeSets)**: Pre-modification states are automatically snapshotted into JSON records (`data/snapshots/`), allowing 1-click reversal.
- **VSS System Restore Point**: Native volume shadow copy restore point creation.

---

## Module Directory (19 Dedicated Workspaces)

| Cluster | Section | Description |
| :--- | :--- | :--- |
| **Core** | **Dashboard** | Real-time CPU, RAM gauge, Disk space, Uptime, VM detection, and Quick Actions. |
| **Core** | **Health Scan** | Instant 6-point system diagnostic categorized by severity (OK, Info, Attention, Critical). |
| **Core** | **Audit Logs** | Structured audit history tracking old vs new values, timestamps, and execution status. |
| **Core** | **Settings** | Safe test mode, tray minimization behavior, and graceful exit controls. |
| **Windows** | **Windows Tweaks** | File extensions, hidden files, classic Win11 context menu, error reporting, widgets, lockscreen tips. |
| **Windows** | **Privacy & Telemetry** | Disable DiagTrack, Advertising ID, Activity Timeline, Bing in Start, Cortana, and Copilot AI. |
| **Windows** | **Debloater** | Safe removal of provisioned and user UWP bloatware with Safe, Balanced, and Aggressive presets. |
| **Windows** | **Apps Manager** | Inspect, launch, or trigger official uninstaller for 32-bit and 64-bit desktop applications. |
| **Performance** | **Optimizer** | Multimedia scheduling, network throttling index, NTFS last access, and Win32 long path support. |
| **Performance** | **Gaming Mode** | Auto Game Mode, Game DVR background capture disabling, and raw 1:1 mouse input precision. |
| **Performance** | **Network Engine** | Active adapter metrics, IP/DNS details, 1-click Flush DNS, Winsock Reset, TCP/IP Reset, and Ping tool. |
| **Maintenance** | **Storage Cleaner** | Preview and clean User Temp, Windows Temp, Thumbnail cache, Chrome/Edge caches, and WER crash dumps. |
| **Maintenance** | **Repair Center** | Guided Win32 repair console with live streaming terminal output for `SFC`, `DISM`, and `CHKDSK`. |
| **Maintenance** | **Backup & Restore**| Volume Shadow Copy (VSS) restore points and granular JSON ChangeSet rollback history. |
| **System** | **Services Manager**| Full Windows service controller with filtering, start/stop/restart, and startup type modification. |
| **System** | **Startup Manager** | Non-destructive startup program manager inspecting `HKCU Run`, `HKLM Run`, and Startup folders. |
| **System** | **App Installer** | Curated catalog of essential post-installation software deployed via native silent package managers. |
| **System** | **Hardware Specs** | Instant sub-millisecond hardware profiling for CPU, GPU, Motherboard, BIOS, and physical RAM. |
| **System** | **Security Overview**| Windows Defender real-time protection monitor, Windows Firewall profiles, and UAC status. |
| **System** | **Windows Features** | Toggle Windows Sandbox, WSL, Hyper-V, Virtual Machine Platform, DirectPlay, Telnet, and legacy components via DISM. |
| **System** | **Setup Profiles** | Save, export, and restore complete machine setups in JSON, plus 4 curated presets (Gaming, Privacy, Workstation). |
| **System** | **Unattended ISO** | Automated `autounattend.xml` answer file generator for Win 10 & 11 with TPM 2.0 / Secure Boot bypasses and offline local accounts. |

---

## Competitor Feature Matrix & Enhancements

SUPOptimizer incorporates and outclasses features from top tools (Winhance, Chris Titus Tool, Win-Debloat, Sophia Script, Optimizer):

1. **80+ Native Registry & Kernel Tweaks**:
   - **Privacy & AI**: Disable Copilot, Windows Recall snapshots, Click-to-Do, WSAIFabricSvc auto-start, Paint/Notepad AI, Find My Device, and app location tracking.
   - **Windows 11 Shell**: Taskbar Left/Center alignment, Taskbar End Task right-click menu, Last Active Click window switcher, hide Search bar/Task View, restore classic Windows 10 context menu, hide Home & Gallery from File Explorer, hide duplicate removable drives, show drive letters first.
   - **Performance & Gaming**: Multiplane Overlay (MPO) disable to fix GPU stuttering, HPET disable, mouse acceleration precision disable, S0 modern standby networking disable (battery life fix), prefer IPv4 over IPv6, Brave browser AI/Crypto debloat.
   - **System Servicing**: Stop automatic updates (notify-only), prevent auto-restart when signed in, prevent device companion app downloads (Razer, Alienware, LG), verbose BSOD and logon modes, hardware clock UTC time.

2. **Advanced UWP App Debloater & Package Purge**:
   - 38 curated AppX/Provisioned packages with Safe, Minimal, and Aggressive presets.
   - Multi-select batch uninstaller with one-click purge.
   - Standalone deep-clean uninstallers for Microsoft OneDrive and Microsoft Edge.

3. **Optional Windows Features Manager**:
   - Live query and toggle for Windows Sandbox, WSL, Hyper-V, Virtual Machine Platform, and DirectPlay via non-blocking DISM.

4. **Autounattend.xml Generator**:
   - Zero-interaction answer file generator for Windows 10 & 11 clean installations.
   - Automated TPM 2.0, Secure Boot, and RAM bypasses (`LabConfig`).
   - Microsoft Account bypass (`BypassNRO`) with instant local user configuration and auto-logon setup.

5. **Security & Repair Center**:
   - Full diagnostic suite: `SFC /scannow`, `DISM /RestoreHealth`, Windows Update service reset, full network stack reset, and system corruption scan.
   - Windows AutoLogon configuration utility.
   - Ultimate Performance power scheme activation and hibernation file management (`powercfg -h`).
   - HOSTS Editor with 1-click Windows Telemetry and Adobe URL tracking block lists.

---

## Build Pipeline

To compile both the Standalone and Lite portable binaries, execute:

```powershell
powershell -ExecutionPolicy Bypass -File .\build-release.ps1
```

The build artifacts will be saved to `dist/`:
- `dist\SUPOptimizer.exe` (Standalone Portable Single-File, ~68.9 MB)
- `dist\SUPOptimizer-Lite.exe` (Lite Portable Single-File, ~2.1 MB)

---

## Verification & Quality Assurance Suite

Run the end-to-end automated verification script:

```powershell
# Test Standalone Edition
powershell -ExecutionPolicy Bypass -File .\test-verification.ps1 -Edition Standalone

# Test Lite Edition
powershell -ExecutionPolicy Bypass -File .\test-verification.ps1 -Edition Lite
```

The test suite validates:
- Binary existence and file integrity
- Headless process startup with dynamic port binding
- Dual-tone brand rendering (`SUPO` + `ptimizer`)
- Clustered navigation structure
- Session token extraction and API authentication
- Sub-millisecond system metrics and virtualization detection
- 80 registered system tweaks including all 22 competitor-requested tweaks
- 38 bloatware packages with IsInstalled detection
- 12 storage cleanup targets
- Live socket inspector (100+ connections)
- System tools (HOSTS, DNS presets, Environment Variables, Run Aliases, Shodan, Safe Boost)
- Optional Windows Features (DISM inspection)
- Autounattend.xml Generator with valid XML output
- Configuration Profiles (Presets & Live Export)
- AutoLogon query and Adobe HOSTS block
- Graceful API shutdown signal
