/**
 * SUPOptimizer - Native Windows System Engineering & Maintenance Platform
 * Premium Client Application Engine
 */

class SUPApp {
  constructor() {
    this.token = window.SUP_TOKEN || "";
    this.port = window.SUP_PORT || window.location.port || "5000";
    this.baseUrl = window.location.origin;
    this.activeTab = "dashboard";
    this.activeOptimizeSubtab = "All";
    this.activeDebloatSubtab = "All";
    this.activeInstallerSubtab = "All";
    
    this.allTweaks = [];
    this.allPackages = [];
    this.allDnsPresets = [];
    this.allEnvVars = [];
    this.allRunAliases = [];
    this.allStartup = [];
    this.allStorageTargets = [];
    this.allCatalogApps = [];
    this.allNetConnections = [];
    this.currentEditEnv = null;
    this.editEnvRows = [];
    this.editEnvIsRaw = false;
    
    this.selectedDebloatPackages = new Set();
    this.allFeatures = [];
    this.allProfiles = [];
    
    this.safeTestMode = localStorage.getItem("sup_safe_mode") === "true";
    this.repairInterval = null;
    this.paletteItems = [];
    this.historyStack = ["dashboard"];
    this.historyIndex = 0;

    this.init();
  }

  async init() {
    // Expose global navigation for C# WebView2 host caller
    window.navigateToTab = (tabId) => this.switchTab(tabId);

    // Setup Window Controls & Dragging
    this.initWindowControls();

    // Setup Keyboard Shortcuts (Ctrl+K for Command Palette, Esc for Modals)
    window.addEventListener("keydown", (e) => {
      if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === "k") {
        e.preventDefault();
        this.openCommandPalette();
      } else if (e.key === "Escape") {
        this.closeAllModals();
      }
    });

    // Initial Telemetry & System Polling
    await this.fetchMetrics();
    setInterval(() => this.fetchMetrics(), 2500);

    // Initial Data Fetching
    await this.loadTweaks();
    await this.loadDebloat();
    this.loadLicenseInfo();
    this.buildPaletteIndex();

    // Check Safe Test Mode checkbox in settings
    const safeModeBox = document.getElementById("cfg-safe-mode");
    if (safeModeBox) safeModeBox.checked = this.safeTestMode;

    const serverUrlLabel = document.getElementById("cfg-server-url");
    if (serverUrlLabel) {
      serverUrlLabel.textContent = `${this.baseUrl}/ (Accessible via Chrome, Edge, Brave, etc.)`;
    }
  }

  // =========================================================================
  // API CLIENT
  // =========================================================================
  async api(endpoint, method = "GET", body = null) {
    const headers = {
      "X-SUP-Token": this.token,
      "Content-Type": "application/json"
    };

    try {
      const resp = await fetch(`${this.baseUrl}${endpoint}`, {
        method,
        headers,
        body: body ? JSON.stringify(body) : null
      });

      if (!resp.ok) {
        const errJson = await resp.json().catch(() => ({}));
        throw new Error(errJson.message || `HTTP ${resp.status}`);
      }

      return await resp.json();
    } catch (err) {
      console.error(`API Error on ${endpoint}:`, err);
      this.showToast(`API Communication Error: ${err.message}`, "error");
      throw err;
    }
  }

  showToast(message, type = "info") {
    const container = document.getElementById("toast-container");
    if (!container) return;

    const toast = document.createElement("div");
    toast.className = "toast";
    let icon = "ℹ️";
    if (type === "success") icon = "✅";
    if (type === "warning") icon = "⚠️";
    if (type === "error") icon = "❌";

    toast.innerHTML = `<span>${icon}</span><span>${message}</span>`;
    container.appendChild(toast);

    setTimeout(() => {
      toast.style.opacity = "0";
      toast.style.transition = "opacity 0.3s ease";
      setTimeout(() => toast.remove(), 300);
    }, 3500);
  }

  // =========================================================================
  // NAVIGATION & TAB SWITCHING
  // =========================================================================
  switchTab(tabId) {
    this.activeTab = tabId;

    // Update history stack
    if (this.historyStack[this.historyIndex] !== tabId) {
      this.historyStack = this.historyStack.slice(0, this.historyIndex + 1);
      this.historyStack.push(tabId);
      this.historyIndex = this.historyStack.length - 1;
    }

    // Update Sidebar active state
    document.querySelectorAll(".sidebar-nav .nav-item, .sidebar-footer .nav-item").forEach(item => {
      item.classList.toggle("active", item.getAttribute("data-tab") === tabId);
    });

    // Update Tab Panes
    document.querySelectorAll(".tab-pane").forEach(pane => {
      pane.classList.toggle("active", pane.id === `tab-${tabId}`);
    });

    // Update Header Breadcrumb Title
    const titleMap = {
      dashboard: "Dashboard Overview",
      optimize: "Optimize & System Tweaks",
      debloat: "Bloatware & Telemetry Remover",
      startup: "Startup Applications Manager",
      storage: "Disk Cleanup & Drive Health",
      installer: "Application Store & Winget",
      network: "Network Optimizer & DNS Switcher",
      systemtools: "Native Windows System Tools",
      repair: "Security Servicing & Registry Diagnostics",
      features: "Optional Windows Features & Virtualization",
      profiles: "System Setup Profiles & Presets",
      unattend: "Windows Autounattend.xml Generator",
      hardware: "Hardware Specifications & Devices",
      backups: "Transaction History & Restore Points",
      settings: "Preferences & Configuration"
    };

    const headerTitle = document.getElementById("header-page-title");
    if (headerTitle) headerTitle.textContent = titleMap[tabId] || "SUPOptimizer";

    // Lazy load tab data
    switch (tabId) {
      case "dashboard": this.fetchMetrics(); break;
      case "optimize": this.renderOptimizeGrid(); break;
      case "debloat": this.renderDebloatGrid(); break;
      case "startup": this.loadStartup(); break;
      case "storage": this.loadStorage(); break;
      case "installer": this.loadInstaller(); break;
      case "network": this.loadNetwork(); break;
      case "systemtools": this.loadSystemTools(); break;
      case "repair": this.loadRepair(); break;
      case "features": this.loadFeatures(); break;
      case "profiles": this.loadProfiles(); break;
      case "unattend": this.generateAutounattendPreview(); break;
      case "hardware": this.loadHardware(); break;
      case "backups": this.loadBackups(); break;
    }
  }

  navigateHistory(direction) {
    const nextIdx = this.historyIndex + direction;
    if (nextIdx >= 0 && nextIdx < this.historyStack.length) {
      this.historyIndex = nextIdx;
      const targetTab = this.historyStack[this.historyIndex];
      this.switchTab(targetTab);
    }
  }

  // =========================================================================
  // TELEMETRY & METRICS
  // =========================================================================
  async fetchMetrics() {
    try {
      const data = await this.api("/api/system/metrics");
      if (!data) return;

      // Robust RAM calculation preventing NaN
      const ramUsedBytes = Number(data.ramUsedBytes ?? data.usedRamBytes ?? ((data.usedRamMb || 0) * 1048576)) || 0;
      const ramTotalBytes = Number(data.ramTotalBytes ?? data.totalRamBytes ?? ((data.totalRamMb || 0) * 1048576)) || 0;

      // Header telemetry
      const hCpu = document.getElementById("header-cpu");
      if (hCpu) hCpu.textContent = `${(data.cpuUsagePercent || 0).toFixed(1)}%`;

      const hRam = document.getElementById("header-ram");
      if (hRam) {
        const usedGb = (ramUsedBytes / (1024 ** 3)).toFixed(1);
        const totGb = (ramTotalBytes / (1024 ** 3)).toFixed(1);
        hRam.textContent = `${usedGb} / ${totGb} GB`;
      }

      const isAdmin = !!(data.isAdmin || data.isAdministrator);
      const hAdmin = document.getElementById("header-admin");
      if (hAdmin) {
        hAdmin.textContent = isAdmin ? "ELEVATED (ADMIN)" : "STANDARD USER";
        hAdmin.style.color = isAdmin ? "var(--safe-green)" : "var(--caution-amber)";
      }
      const hAdminBadge = document.getElementById("header-admin-badge");
      if (hAdminBadge) {
        hAdminBadge.title = isAdmin
          ? "Application running with full Administrator privileges."
          : "Application running as Standard User. Click to request UAC elevation.";
      }

      // Dashboard cards
      const dCpuVal = document.getElementById("dash-cpu-val");
      const dCpuBar = document.getElementById("dash-cpu-bar");
      if (dCpuVal) dCpuVal.textContent = `${(data.cpuUsagePercent || 0).toFixed(1)}%`;
      if (dCpuBar) dCpuBar.style.width = `${Math.min(100, Math.max(0, data.cpuUsagePercent || 0))}%`;

      const dRamVal = document.getElementById("dash-ram-val");
      const dRamBar = document.getElementById("dash-ram-bar");
      const dRamDetail = document.getElementById("dash-ram-detail");
      const ramPct = (data.ramUsagePercent != null && !isNaN(data.ramUsagePercent))
        ? data.ramUsagePercent
        : (ramTotalBytes > 0 ? (ramUsedBytes / ramTotalBytes * 100) : 0);

      if (dRamVal) dRamVal.textContent = `${ramPct.toFixed(1)}%`;
      if (dRamBar) dRamBar.style.width = `${Math.min(100, Math.max(0, ramPct))}%`;
      if (dRamDetail) {
        const usedMb = Math.round(ramUsedBytes / (1024 * 1024));
        const totalMb = Math.round(ramTotalBytes / (1024 * 1024));
        const freeMb = Math.max(0, totalMb - usedMb);
        dRamDetail.textContent = `${usedMb.toLocaleString()} MB used / ${freeMb.toLocaleString()} MB free (${totalMb.toLocaleString()} MB total)`;
      }

      const dDiskVal = document.getElementById("dash-disk-val");
      const dDiskBar = document.getElementById("dash-disk-bar");
      const dDiskDetail = document.getElementById("dash-disk-detail");
      const diskPct = data.diskUsagePercent != null ? data.diskUsagePercent : 0;
      if (dDiskVal) dDiskVal.textContent = `${diskPct.toFixed(1)}%`;
      if (dDiskBar) dDiskBar.style.width = `${Math.min(100, Math.max(0, diskPct))}%`;
      if (dDiskDetail) {
        const freeGb = data.diskFreeBytes ? (data.diskFreeBytes / (1024 ** 3)).toFixed(1) : "0";
        const totGb = data.diskTotalBytes ? (data.diskTotalBytes / (1024 ** 3)).toFixed(1) : "0";
        dDiskDetail.textContent = `${freeGb} GB available of ${totGb} GB`;
      }

      const dUptime = document.getElementById("dash-uptime");
      if (dUptime) dUptime.textContent = `Uptime: ${data.systemUptime || data.uptime || "Online"}`;

      const dCpuModel = document.getElementById("dash-cpu-model");
      if (dCpuModel && data.cpuModel) dCpuModel.textContent = data.cpuModel;

      // Fill baseline specs table
      const sOs = document.getElementById("dash-spec-os");
      const sCpu = document.getElementById("dash-spec-cpu");
      const sGpu = document.getElementById("dash-spec-gpu");
      const sMb = document.getElementById("dash-spec-mb");
      if (sOs && data.osVersion) sOs.textContent = `${data.osVersion} (${data.architecture || 'x64'})`;
      if (sCpu && data.cpuModel) sCpu.textContent = data.cpuModel;
      if (sGpu && data.gpuModel) sGpu.textContent = data.gpuModel;
      if (sMb && data.motherboardModel) sMb.textContent = data.motherboardModel;

      // Multi-Disk storage volumes rendering
      this.renderDashboardDisks(data.drives || []);

    } catch (err) { }
  }

  renderDashboardDisks(drives) {
    const grid = document.getElementById("dash-disks-grid");
    const countBadge = document.getElementById("dash-disks-count");
    if (!grid) return;

    if (!drives || drives.length === 0) {
      grid.innerHTML = '<div style="color:var(--text-muted); font-size:12px; padding:12px;">No partitions or storage drives detected.</div>';
      if (countBadge) countBadge.textContent = "0 Drives";
      return;
    }

    if (countBadge) countBadge.textContent = `${drives.length} Drives Detected`;

    grid.innerHTML = drives.map(d => {
      const isSys = d.isSystemDrive ? '<span class="pill-risk safe" style="font-size:10px; padding:1px 6px;">OS System</span>' : '';
      const usedPct = Math.min(100, Math.max(0, d.usedPercent || 0)).toFixed(1);
      const barClass = usedPct > 90 ? 'red' : (usedPct > 75 ? 'amber' : 'green');
      const freeGb = (d.freeGb ?? (d.freeBytes ? (d.freeBytes / (1024 ** 3)).toFixed(1) : 0));
      const totalGb = (d.totalGb ?? (d.totalBytes ? (d.totalBytes / (1024 ** 3)).toFixed(1) : 0));
      const usedGb = (d.usedGb ?? (d.usedBytes ? (d.usedBytes / (1024 ** 3)).toFixed(1) : 0));

      return `
        <div class="disk-card">
          <div class="disk-card-head">
            <div>
              <strong style="color:#ffffff; font-size:14px;">${this.escapeHtml(d.name || "Drive")}</strong>
              <span style="font-size:11px; color:var(--text-muted); margin-left:6px;">${this.escapeHtml(d.volumeLabel || "Local Disk")} (${this.escapeHtml(d.driveFormat || "NTFS")})</span>
            </div>
            ${isSys}
          </div>
          <div class="stat-gauge-track" style="margin:10px 0 8px 0;">
            <div class="stat-gauge-bar ${barClass}" style="width:${usedPct}%;"></div>
          </div>
          <div class="disk-card-details">
            <span>${usedGb} GB used (${usedPct}%)</span>
            <strong style="color:var(--text-secondary);">${freeGb} GB available of ${totalGb} GB</strong>
          </div>
        </div>
      `;
    }).join("");
  }

  // =========================================================================
  // 1-CLICK SAFE BOOST (Transparent Execution with Progress Feedback)
  // =========================================================================
  openSafeBoostModal() {
    const area = document.getElementById("safe-boost-progress-area");
    if (area) area.style.display = "none";
    const btn = document.getElementById("btn-execute-safe-boost");
    if (btn) {
      btn.disabled = false;
      btn.innerHTML = `<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5"><polygon points="13 2 3 14 12 14 11 22 21 10 12 10 13 2"></polygon></svg><span>Execute Safe Boost Now</span>`;
    }
    document.getElementById("modal-safe-boost")?.classList.add("active");
  }

  async runSafeBoost() {
    const btn = document.getElementById("btn-execute-safe-boost");
    const progressArea = document.getElementById("safe-boost-progress-area");
    const statusText = document.getElementById("safe-boost-status-text");
    const percentText = document.getElementById("safe-boost-percent");
    const progressBar = document.getElementById("safe-boost-progress-bar");
    const logBox = document.getElementById("safe-boost-log");

    if (btn) btn.disabled = true;
    if (progressArea) progressArea.style.display = "block";
    if (logBox) logBox.innerHTML = "";

    const addLog = (msg) => {
      if (!logBox) return;
      const d = document.createElement("div");
      d.textContent = `[${new Date().toLocaleTimeString()}] ${msg}`;
      logBox.appendChild(d);
      logBox.scrollTop = logBox.scrollHeight;
    };

    const setProgress = (pct, text) => {
      if (percentText) percentText.textContent = `${pct}%`;
      if (progressBar) progressBar.style.width = `${pct}%`;
      if (statusText) statusText.textContent = text;
      addLog(text);
    };

    try {
      setProgress(10, "Initializing restore point and verifying system stability...");
      await new Promise(r => setTimeout(r, 350));

      setProgress(25, "Applying safe, conservative optimization suite (Safe Optimize)...");
      const applyRes = await this.api("/api/automation/apply", "POST", { profileId: "safe_optimize", dryRun: this.safeTestMode });
      if (applyRes) {
        addLog(`Applied ${applyRes.appliedCount || applyRes.AppliedCount || 0} tweaks successfully.`);
      }

      setProgress(55, "Flushing DNS resolver cache (ipconfig /flushdns)...");
      try {
        await this.api("/api/network/flushdns", "POST", {});
        addLog("DNS resolver cache flushed successfully.");
      } catch (e) {
        addLog(`DNS Note: ${e.message}`);
      }

      setProgress(75, "Cleaning temporary system files and volatile cache...");
      try {
        const cleanRes = await this.api("/api/storage/clean", "POST", { targetIds: ["temp_files", "system_cache"], dryRun: this.safeTestMode });
        if (cleanRes && (cleanRes.message || cleanRes.Message)) {
          addLog(`Storage cleanup: ${cleanRes.message || cleanRes.Message}`);
        }
      } catch (e) {
        addLog(`Storage cleanup note: ${e.message}`);
      }

      setProgress(90, "Synchronizing system metrics and telemetry...");
      await this.fetchMetrics();
      await new Promise(r => setTimeout(r, 300));

      setProgress(100, "Safe Boost completed successfully! System optimized.");
      this.showToast("1-Click Safe Boost completed successfully!", "success");

      if (btn) {
        btn.innerHTML = `<span>✓ Optimization Complete</span>`;
      }
    } catch (err) {
      setProgress(100, `Error during optimization: ${err.message}`);
      this.showToast(`Safe Boost Error: ${err.message}`, "error");
      if (btn) btn.disabled = false;
    }
  }

  // =========================================================================
  // OPTIMIZE PANE & 2-COLUMN TWEAK CARDS
  // =========================================================================
  async loadTweaks() {
    try {
      this.allTweaks = await this.api("/api/tweaks");
      const badge = document.getElementById("badge-opt-count");
      if (badge) badge.textContent = this.allTweaks.length;
      this.renderOptimizeGrid();
    } catch (err) { }
  }

  filterOptimizeTab(category, btn) {
    this.activeOptimizeSubtab = category;
    document.querySelectorAll("#tab-optimize .subtab-item").forEach(b => b.classList.remove("active"));
    if (btn) btn.classList.add("active");
    this.renderOptimizeGrid();
  }

  renderOptimizeGrid() {
    const grid = document.getElementById("optimize-tweak-grid");
    if (!grid) return;

    const searchQuery = (document.getElementById("tweak-search-input")?.value || "").toLowerCase().trim();
    const riskFilter = document.getElementById("tweak-risk-filter")?.value || "All";
    const stateFilter = document.getElementById("tweak-state-filter")?.value || "All";

    const filtered = this.allTweaks.filter(t => {
      // Sub-tab category match
      if (this.activeOptimizeSubtab !== "All") {
        const cat = (t.category || "").toLowerCase();
        const sub = this.activeOptimizeSubtab.toLowerCase();

        if (sub === "performance" && !["cpu", "ram", "network", "system"].includes(cat)) return false;
        if (sub === "security" && !["privacy", "system", "windows"].includes(cat)) return false;
        if (sub === "gaming" && !["gaming", "gpu", "input", "network"].includes(cat)) return false;
        if (sub === "power" && !["power", "system"].includes(cat)) return false;
        if (sub === "services" && !["windows", "system", "privacy"].includes(cat)) return false;
        if (sub === "explorer" && !["explorer", "desktop", "filesystem"].includes(cat)) return false;
        if (sub === "advanced" && !["system", "gaming", "privacy"].includes(cat)) return false;
      }

      // Search match
      if (searchQuery) {
        const matchTitle = t.name.toLowerCase().includes(searchQuery);
        const matchDesc = t.description.toLowerCase().includes(searchQuery);
        const matchTech = (t.technicalDetails || "").toLowerCase().includes(searchQuery);
        const matchId = t.id.toLowerCase().includes(searchQuery);
        if (!matchTitle && !matchDesc && !matchTech && !matchId) return false;
      }

      // Risk filter
      if (riskFilter !== "All" && t.risk.toLowerCase() !== riskFilter.toLowerCase()) return false;

      // State filter
      if (stateFilter !== "All") {
        const isEnabled = t.currentState === "Enabled";
        if (stateFilter === "Enabled" && !isEnabled) return false;
        if (stateFilter === "Disabled" && isEnabled) return false;
      }

      return true;
    });

    grid.innerHTML = "";

    if (filtered.length === 0) {
      grid.innerHTML = `
        <div style="grid-column:1/-1; text-align:center; padding:40px; color:var(--text-muted);">
          No optimization tweaks found matching your active filter.
        </div>`;
      return;
    }

    filtered.forEach(tweak => {
      const isEnabled = tweak.currentState === "Enabled";
      const riskClass = tweak.risk.toLowerCase() === "safe" ? "safe" : (tweak.risk.toLowerCase() === "low" ? "safe" : "caution");
      const riskIcon = riskClass === "safe" ? "🛡️" : "⚠️";

      // Monospace pseudo-UUID hash for sleek UI
      const pseudoHash = this.generateHashFromId(tweak.id);

      const card = document.createElement("div");
      card.className = "tweak-card";
      card.innerHTML = `
        <div class="card-top">
          <div class="card-uuid">${pseudoHash}</div>
          <div class="card-title">${this.escapeHtml(tweak.name)}</div>
          <div class="card-micro-tags">
            <span class="micro-tag">
              <svg viewBox="0 0 24 24"><rect x="2" y="3" width="20" height="14" rx="2"></rect><line x1="8" y1="21" x2="16" y2="21"></line><line x1="12" y1="17" x2="12" y2="21"></line></svg>
              ${tweak.category}
            </span>
            ${tweak.requiresAdmin ? `<span class="micro-tag"><svg viewBox="0 0 24 24"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"></path></svg>Admin</span>` : ""}
            ${tweak.requiresReboot ? `<span class="micro-tag"><svg viewBox="0 0 24 24"><polyline points="1 4 1 10 7 10"></polyline><path d="M3.51 15a9 9 0 1 0 2.13-9.36L1 10"></path></svg>Reboot</span>` : ""}
          </div>
          <div class="card-desc">${this.escapeHtml(tweak.description)}</div>
        </div>

        <div class="card-bottom">
          <div class="pill-risk ${riskClass}">
            <span>${riskIcon}</span>
            <span>${tweak.risk}</span>
          </div>

          <div class="card-actions-row">
            <div class="fluid-switch ${isEnabled ? 'active' : ''}" onclick="supApp.toggleTweak('${tweak.id}', ${!isEnabled})" title="${isEnabled ? 'Click to Restore Default' : 'Click to Apply Optimization'}">
              <div class="switch-knob"></div>
            </div>

            <button class="btn-card-util" onclick="supApp.inspectTweak('${tweak.id}')" title="Inspect Registry Keys & Code">&lt; / &gt;</button>
          </div>
        </div>
      `;
      grid.appendChild(card);
    });
  }

  generateHashFromId(id) {
    let hash = 0;
    for (let i = 0; i < id.length; i++) {
      hash = ((hash << 5) - hash) + id.charCodeAt(i);
      hash |= 0;
    }
    const hex = Math.abs(hash).toString(16).padStart(8, '0');
    return `${hex.slice(0,8)}-497d-46c9-a97d-${hex.padEnd(12, 'a').slice(0,12)}`;
  }

  async toggleTweak(tweakId, enable) {
    try {
      const endpoint = enable ? "/api/tweaks/apply" : "/api/tweaks/restore";
      const result = await this.api(endpoint, "POST", { tweakId, dryRun: this.safeTestMode });

      if (result.Success || result.success) {
        this.showToast(`${enable ? "Applied" : "Restored"}: ${result.Message || result.message}`, "success");
        await this.loadTweaks();
      } else {
        this.showToast(`Operation Failed: ${result.Message || result.message}`, "error");
      }
    } catch (err) { }
  }

  async applyAllSafeTweaks() {
    const safeIds = this.allTweaks
      .filter(t => t.risk.toLowerCase() === "safe" && t.currentState !== "Enabled")
      .map(t => t.id);

    if (safeIds.length === 0) {
      this.showToast("All safe optimizations are already applied!", "info");
      return;
    }

    try {
      const res = await this.api("/api/tweaks/batch", "POST", { tweakIds: safeIds, dryRun: this.safeTestMode });
      this.showToast(`Batch Complete: ${res.Succeeded || res.succeeded} applied, ${res.Failed || res.failed} failed.`, "success");
      await this.loadTweaks();
    } catch (err) { }
  }

  async restoreAllTweaks() {
    const enabledIds = this.allTweaks
      .filter(t => t.currentState === "Enabled")
      .map(t => t.id);

    if (enabledIds.length === 0) {
      this.showToast("No active tweaks to restore.", "info");
      return;
    }

    for (const id of enabledIds) {
      await this.api("/api/tweaks/restore", "POST", { tweakId: id, dryRun: this.safeTestMode });
    }

    this.showToast("Restored all tweaks to Windows defaults.", "success");
    await this.loadTweaks();
  }

  inspectTweak(tweakId) {
    const tweak = this.allTweaks.find(t => t.id === tweakId);
    if (!tweak) return;

    const titleEl = document.getElementById("inspect-modal-title");
    const bodyEl = document.getElementById("inspect-modal-body");

    if (titleEl) titleEl.textContent = `Inspect: ${tweak.name}`;
    if (bodyEl) {
      bodyEl.innerHTML = `
        <div style="display:flex; flex-direction:column; gap:14px;">
          <div>
            <label style="color:var(--text-muted); font-size:11px; text-transform:uppercase;">Technical Implementation</label>
            <div style="font-family:var(--font-mono); font-size:12px; color:#38bdf8; background:#111114; padding:12px; border-radius:6px; border:1px solid var(--border-subtle); margin-top:4px;">
              ${this.escapeHtml(tweak.technicalDetails || "Native Windows Registry configuration.")}
            </div>
          </div>
          <div style="display:grid; grid-template-columns: 1fr 1fr; gap:10px;">
            <div>
              <span style="color:var(--text-muted); font-size:11px;">Category:</span>
              <strong style="color:#ffffff; font-size:12px; margin-left:4px;">${tweak.category}</strong>
            </div>
            <div>
              <span style="color:var(--text-muted); font-size:11px;">Safety Level:</span>
              <strong style="color:var(--safe-green); font-size:12px; margin-left:4px;">${tweak.risk}</strong>
            </div>
            <div>
              <span style="color:var(--text-muted); font-size:11px;">Current State:</span>
              <strong style="color:#ffffff; font-size:12px; margin-left:4px;">${tweak.currentState}</strong>
            </div>
            <div>
              <span style="color:var(--text-muted); font-size:11px;">Reversible:</span>
              <strong style="color:#ffffff; font-size:12px; margin-left:4px;">${tweak.isReversible ? "Yes (1-Click)" : "No"}</strong>
            </div>
          </div>
        </div>
      `;
    }

    const modal = document.getElementById("modal-inspect");
    if (modal) modal.classList.add("active");
  }

  // =========================================================================
  // BLOATWARE / DEBLOATER (Fixed with 35+ packages & 2-column cards)
  // =========================================================================
  async loadDebloat() {
    try {
      this.allPackages = await this.api("/api/debloat/packages");
      const badge = document.getElementById("badge-debloat-count");
      if (badge) badge.textContent = this.allPackages.length;
      this.renderDebloatGrid();
    } catch (err) { }
  }

  filterDebloatTab(tier, btn) {
    this.activeDebloatSubtab = tier;
    document.querySelectorAll("#tab-debloat .subtab-item").forEach(b => b.classList.remove("active"));
    if (btn) btn.classList.add("active");
    this.renderDebloatGrid();
  }

  renderDebloatGrid() {
    const list = document.getElementById("debloat-list");
    if (!list) return;

    list.innerHTML = "";

    const filtered = this.allPackages.filter(p => {
      if (this.activeDebloatSubtab === "All") return true;
      const cat = (p.category || p.tier || "").toLowerCase();
      const sub = this.activeDebloatSubtab.toLowerCase();

      if (sub === "safe") return p.safeToRemove || p.recommendedRemove || cat === "safe";
      if (sub === "telemetry") return cat.includes("telemetry") || cat.includes("bing") || p.id.includes("Bing");
      if (sub === "gaming") return cat.includes("gaming") || cat.includes("xbox") || p.id.includes("Xbox");
      if (sub === "apps") return !p.id.includes("Xbox") && !p.id.includes("Bing");

      return cat.includes(sub);
    });

    this.updateDebloatSelectedUi();

    if (filtered.length === 0) {
      list.innerHTML = `
        <div style="grid-column:1/-1; text-align:center; padding:40px; color:var(--text-muted);">
          No bloatware packages detected in this category.
        </div>`;
      return;
    }

    filtered.forEach(pkg => {
      const name = pkg.displayName || pkg.name || pkg.id;
      const isSafe = pkg.safeToRemove ?? pkg.recommendedRemove ?? true;
      const isInstalled = !!(pkg.isInstalled ?? pkg.IsInstalled);
      const isSelected = this.selectedDebloatPackages.has(pkg.id);

      const installBadge = isInstalled
        ? `<span class="pill-risk safe" style="font-size:11px; padding:2px 8px;">● Installed</span>`
        : `<span class="pill-risk" style="background:rgba(255,255,255,0.06); color:var(--text-muted); border-color:var(--border-subtle); font-size:11px; padding:2px 8px;">○ Not Installed</span>`;

      const actionBtn = isInstalled
        ? `<button class="btn-secondary" style="color:#ef4444; border-color:rgba(239, 68, 68, 0.25);" onclick="supApp.removePackage('${this.escapeJs(pkg.id)}')">Uninstall</button>`
        : `<button class="btn-secondary" disabled style="opacity:0.4; cursor:not-allowed; color:var(--text-muted);" title="This package is not installed on this system">Not Installed</button>`;

      const chkBox = isInstalled ? `
        <label style="position:absolute; top:12px; right:12px; cursor:pointer; display:flex; align-items:center; gap:6px;">
          <input type="checkbox" style="transform:scale(1.25); cursor:pointer;" ${isSelected ? 'checked' : ''} onchange="supApp.toggleDebloatSelect('${this.escapeJs(pkg.id)}', this.checked)">
        </label>` : '';

      const card = document.createElement("div");
      card.className = "tweak-card";
      card.style.position = "relative";
      card.innerHTML = `
        <div class="card-top">
          ${chkBox}
          <div class="card-uuid">${this.escapeHtml(pkg.id)}</div>
          <div class="card-title" style="padding-right:26px;">${this.escapeHtml(name)}</div>
          <div class="card-micro-tags">
            <span class="micro-tag">
              <svg viewBox="0 0 24 24"><path d="M3 6h18M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path></svg>
              ${this.escapeHtml(pkg.category || pkg.tier || "UWP")}
            </span>
            <span class="micro-tag">Package</span>
          </div>
          <div class="card-desc">${this.escapeHtml(pkg.description || "Windows built-in provisioned UWP application package.")}</div>
        </div>

        <div class="card-bottom">
          <div style="display:flex; align-items:center; gap:8px;">
            <div class="pill-risk ${isSafe ? 'safe' : 'caution'}">
              <span>${isSafe ? '🛡️ Safe' : '⚠️ Caution'}</span>
            </div>
            ${installBadge}
          </div>

          <div class="card-actions-row">
            ${actionBtn}
          </div>
        </div>
      `;
      list.appendChild(card);
    });
  }

  toggleDebloatSelect(pkgId, isChecked) {
    if (isChecked) {
      this.selectedDebloatPackages.add(pkgId);
    } else {
      this.selectedDebloatPackages.delete(pkgId);
    }
    this.updateDebloatSelectedUi();
  }

  updateDebloatSelectedUi() {
    const countSpan = document.getElementById("selected-debloat-count");
    const removeBtn = document.getElementById("btn-remove-selected");
    const count = this.selectedDebloatPackages.size;

    if (countSpan) countSpan.textContent = count;
    if (removeBtn) removeBtn.style.display = count > 0 ? "inline-flex" : "none";
  }

  selectAllDebloat(select) {
    if (select) {
      this.allPackages.forEach(p => {
        if (p.isInstalled ?? p.IsInstalled) {
          this.selectedDebloatPackages.add(p.id);
        }
      });
    } else {
      this.selectedDebloatPackages.clear();
    }
    this.renderDebloatGrid();
  }

  async applyDebloatPreset(presetName) {
    try {
      const presets = await this.api("/api/debloat/presets");
      const target = presets.find(p => p.id.toLowerCase() === presetName.toLowerCase() || p.name.toLowerCase().includes(presetName.toLowerCase()));
      if (target && target.packageIds) {
        this.selectedDebloatPackages.clear();
        target.packageIds.forEach(id => this.selectedDebloatPackages.add(id));
        this.showToast(`Applied preset '${target.name}' (${this.selectedDebloatPackages.size} packages marked).`, "info");
      } else {
        // Fallback
        this.selectedDebloatPackages.clear();
        this.allPackages.forEach(p => {
          if ((p.isInstalled ?? p.IsInstalled) && (p.safeToRemove || p.recommendedRemove)) {
            this.selectedDebloatPackages.add(p.id);
          }
        });
        this.showToast(`Applied Safe preset (${this.selectedDebloatPackages.size} packages marked).`, "info");
      }
      this.renderDebloatGrid();
    } catch (err) {
      this.showToast(`Failed to load preset: ${err.message}`, "error");
    }
  }

  async removeSelectedBloat() {
    const ids = Array.from(this.selectedDebloatPackages);
    if (ids.length === 0) {
      this.showToast("No bloatware packages selected for removal.", "info");
      return;
    }

    if (!confirm(`Are you sure you want to uninstall ${ids.length} selected bloatware package(s)?`)) return;

    this.showToast(`Batch purging ${ids.length} packages...`, "info");
    try {
      const res = await this.api("/api/debloat/remove-batch", "POST", { ids, dryRun: this.safeTestMode });
      if (res.Success || res.success) {
        this.showToast(res.Message || `Successfully removed ${ids.length} packages.`, "success");
        this.selectedDebloatPackages.clear();
        await this.loadDebloat();
      } else {
        this.showToast(res.Message || "Batch removal failed.", "error");
      }
    } catch (err) { }
  }

  async removeOneDrive() {
    if (!confirm("Are you sure you want to completely uninstall Microsoft OneDrive and remove file sync registry hooks?")) return;
    this.showToast("Purging Microsoft OneDrive...", "info");
    try {
      const res = await this.api("/api/debloat/remove-onedrive", "POST", { dryRun: this.safeTestMode });
      if (res.Success || res.success) {
        this.showToast(res.Message || "Microsoft OneDrive uninstalled.", "success");
      } else {
        this.showToast(res.Message || "OneDrive uninstall failed.", "error");
      }
    } catch (err) { }
  }

  async removeMicrosoftEdge() {
    if (!confirm("Are you sure you want to deep-uninstall Microsoft Edge browser and block automatic reinstallation?")) return;
    this.showToast("Purging Microsoft Edge...", "info");
    try {
      const res = await this.api("/api/debloat/remove-edge", "POST", { dryRun: this.safeTestMode });
      if (res.Success || res.success) {
        this.showToast(res.Message || "Microsoft Edge uninstalled.", "success");
      } else {
        this.showToast(res.Message || "Edge uninstall failed.", "error");
      }
    } catch (err) { }
  }

  async removePackage(packageId) {
    const pkg = this.allPackages.find(p => p.id === packageId);
    if (pkg && !(pkg.isInstalled ?? pkg.IsInstalled)) {
      this.showToast(`Application '${pkg.displayName || packageId}' is not installed.`, "warning");
      return;
    }

    try {
      const res = await this.api("/api/debloat/remove", "POST", { packageId, dryRun: this.safeTestMode });
      if (res.Success || res.success) {
        this.showToast(`Uninstalled: ${packageId}`, "success");
        await this.loadDebloat();
      } else {
        this.showToast(`Uninstall failed: ${res.Message || res.message}`, "error");
      }
    } catch (err) { }
  }

  async removeRecommendedBloat() {
    const recommended = this.allPackages.filter(p => p.safeToRemove || p.recommendedRemove);
    if (recommended.length === 0) {
      this.showToast("No recommended bloatware found to remove.", "info");
      return;
    }

    this.showToast(`Purging ${recommended.length} bloatware packages...`, "info");

    for (const pkg of recommended) {
      await this.api("/api/debloat/remove", "POST", { packageId: pkg.id, dryRun: this.safeTestMode });
    }

    this.showToast("Recommended bloatware packages purged successfully!", "success");
    await this.loadDebloat();
  }

  // =========================================================================
  // SYSTEM TOOLS: HOSTS, DNS, ENV, UNLOCKER, RUN ALIASES
  // =========================================================================
  switchSystemTool(toolId, btn) {
    document.querySelectorAll("#tab-systemtools .subtab-item").forEach(b => b.classList.remove("active"));
    if (btn) btn.classList.add("active");

    document.querySelectorAll(".systool-subpane").forEach(p => p.style.display = "none");
    const targetPane = document.getElementById(`tool-${toolId}`);
    if (targetPane) targetPane.style.display = "block";

    switch (toolId) {
      case "hosts": this.loadHostsFile(); break;
      case "env": this.loadEnvVariables(); break;
      case "unlock": break;
      case "aliases": this.loadRunAliases(); break;
      case "contextmenu": this.loadContextMenuTweaks(); break;
      case "license": this.loadLicenseInfo(); break;
    }
  }

  loadSystemTools() {
    this.loadHostsFile();
  }

  // 1. HOSTS FILE
  async loadHostsFile() {
    try {
      const res = await this.api("/api/tools/hosts");
      const textarea = document.getElementById("hosts-textarea");
      const pathLabel = document.getElementById("hosts-file-path");
      if (textarea && res.content) textarea.value = res.content;
      if (pathLabel && res.path) pathLabel.textContent = `Path: ${res.path}`;
    } catch (err) { }
  }

  async saveHostsFile() {
    const textarea = document.getElementById("hosts-textarea");
    if (!textarea) return;

    try {
      const res = await this.api("/api/tools/hosts/save", "POST", { content: textarea.value });
      if (res.Success || res.success) {
        this.showToast(res.Message || res.message, "success");
      } else {
        this.showToast(`Error: ${res.Message || res.message}`, "error");
      }
    } catch (err) { }
  }

  async blockTelemetryInHosts() {
    try {
      const res = await this.api("/api/tools/hosts/block-telemetry", "POST", {});
      if (res.Success || res.success) {
        this.showToast(res.Message || res.message, "success");
        await this.loadHostsFile();
      } else {
        this.showToast(`Error: ${res.Message || res.message}`, "error");
      }
    } catch (err) { }
  }

  async blockAdobeInHosts() {
    try {
      const res = await this.api("/api/tools/hosts/block-adobe", "POST", {});
      if (res.Success || res.success) {
        this.showToast(res.Message || res.message, "success");
        await this.loadHostsFile();
      } else {
        this.showToast(`Error: ${res.Message || res.message}`, "error");
      }
    } catch (err) { }
  }

  // 2. DNS SWITCHER & ADVANCED NETWORK
  async loadNetwork() {
    try {
      this.allDnsPresets = await this.api("/api/tools/dns/presets");
      const grid = document.getElementById("dns-presets-grid");
      if (grid) {
        grid.innerHTML = "";
        this.allDnsPresets.forEach(preset => {
          const card = document.createElement("div");
          card.className = "dns-card";
          card.innerHTML = `
            <div>
              <div class="dns-header">
                <span class="dns-provider">${this.escapeHtml(preset.name)}</span>
                <span class="pill-risk safe">${this.escapeHtml(preset.provider)}</span>
              </div>
              <div class="dns-ips">${preset.isDhcp ? "Auto DHCP (Predefinito Router)" : `${preset.primaryDns}  /  ${preset.secondaryDns}`}</div>
              <div class="dns-desc">${this.escapeHtml(preset.description)}</div>
            </div>
            <button class="btn-primary-amber" onclick="supApp.applyDnsPreset('${preset.id}')" style="align-self:flex-start;">
              Applica DNS
            </button>
          `;
          grid.appendChild(card);
        });
      }

      // Load active connections table
      await this.loadNetworkConnections();
    } catch (err) { }
  }

  async optimizeNetworkStack() {
    try {
      this.showToast("Optimizing network stack (TCP Autotuning, RSS, RSC, NoThrottling)...", "info");
      const res = await this.api("/api/network/optimize", "POST", {});
      if (res.Success || res.success) {
        this.showToast(res.Message || res.message, "success");
      } else {
        this.showToast(`Network Optimization Error: ${res.Message || res.message}`, "error");
      }
    } catch (err) {
      this.showToast(`Error: ${err.message}`, "error");
    }
  }

  async toggleNetworkPowerSaving(disable = true) {
    try {
      this.showToast("Configuring network adapter power saving...", "info");
      const res = await this.api("/api/network/powersaving", "POST", { disable: disable });
      if (res.Success || res.success) {
        this.showToast(res.Message || res.message, "success");
      } else {
        this.showToast(`Power Saving Error: ${res.Message || res.message}`, "error");
      }
    } catch (err) {
      this.showToast(`Error: ${err.message}`, "error");
    }
  }

  async loadNetworkConnections() {
    const tbody = document.getElementById("net-connections-tbody");
    const countBadge = document.getElementById("net-conn-count");
    if (!tbody) return;

    try {
      const connections = await this.api("/api/network/connections");
      this.allNetConnections = Array.isArray(connections) ? connections : [];
      if (countBadge) countBadge.textContent = `${this.allNetConnections.length} Connessioni`;
      this.filterNetworkConnections();
    } catch (err) {
      tbody.innerHTML = `<tr><td colspan="6" style="text-align:center; padding:20px; color:var(--text-muted);">Impossibile leggere la tabella connessioni: ${err.message}</td></tr>`;
    }
  }

  filterNetworkConnections() {
    const tbody = document.getElementById("net-connections-tbody");
    if (!tbody) return;

    const query = (document.getElementById("net-conn-filter")?.value || "").toLowerCase().trim();
    const filtered = this.allNetConnections.filter(c => {
      if (!query) return true;
      const pName = (c.processName || c.ProcessName || "").toLowerCase();
      const pid = String(c.processId || c.ProcessId || "");
      const local = `${c.localAddress || c.LocalAddress}:${c.localPort || c.LocalPort}`.toLowerCase();
      const remote = `${c.remoteAddress || c.RemoteAddress}:${c.remotePort || c.RemotePort}`.toLowerCase();
      const state = (c.state || c.State || "").toLowerCase();
      return pName.includes(query) || pid.includes(query) || local.includes(query) || remote.includes(query) || state.includes(query);
    });

    if (filtered.length === 0) {
      tbody.innerHTML = `<tr><td colspan="6" style="text-align:center; padding:20px; color:var(--text-muted);">Nessuna connessione di rete corrispondente ai criteri di ricerca.</td></tr>`;
      return;
    }

    tbody.innerHTML = filtered.slice(0, 150).map(c => {
      const pName = c.processName || c.ProcessName || "System";
      const pid = c.processId || c.ProcessId || 0;
      const proto = c.protocol || c.Protocol || "TCP";
      const local = `${c.localAddress || c.LocalAddress || "0.0.0.0"}:${c.localPort || c.LocalPort || 0}`;
      const remote = (c.remotePort || c.RemotePort) > 0 ? `${c.remoteAddress || c.RemoteAddress}:${c.remotePort || c.RemotePort}` : (c.remoteAddress || c.RemoteAddress || "*:*");
      const state = c.state || c.State || "Unknown";
      const dir = c.direction || c.Direction || "Inbound";

      let dirBadgeClass = "conn-badge-in";
      if (dir.toLowerCase() === "outbound") dirBadgeClass = "conn-badge-out";
      else if (dir.toLowerCase() === "listening") dirBadgeClass = "conn-badge-listen";

      return `
        <tr>
          <td>
            <div style="display:flex; align-items:center; gap:8px;">
              <strong style="color:#ffffff;">${this.escapeHtml(pName)}</strong>
              <span class="pill-risk safe" style="font-size:10px; padding:1px 6px;">PID ${pid}</span>
            </div>
          </td>
          <td><span style="font-family:var(--font-mono); color:var(--text-secondary);">${this.escapeHtml(proto)}</span></td>
          <td style="font-family:var(--font-mono); font-size:12px; color:var(--text-secondary);">${this.escapeHtml(local)}</td>
          <td style="font-family:var(--font-mono); font-size:12px; color:#ffffff;">${this.escapeHtml(remote)}</td>
          <td><span style="font-family:var(--font-mono); font-size:11px; color:var(--amber-gold); font-weight:600;">${this.escapeHtml(state)}</span></td>
          <td><span class="conn-badge ${dirBadgeClass}">${this.escapeHtml(dir)}</span></td>
        </tr>
      `;
    }).join("");
  }

  async applyDnsPreset(presetId) {
    const preset = this.allDnsPresets.find(p => p.id === presetId);
    if (!preset) return;

    try {
      const res = await this.api("/api/tools/dns/apply", "POST", {
        adapterName: "",
        primaryDns: preset.primaryDns,
        secondaryDns: preset.secondaryDns,
        isDhcp: preset.isDhcp
      });

      if (res.Success || res.success) {
        this.showToast(res.Message || res.message, "success");
      } else {
        this.showToast(`DNS Error: ${res.Message || res.message}`, "error");
      }
    } catch (err) { }
  }

  async runNetworkAction(action) {
    try {
      const res = await this.api("/api/network/action", "POST", { action });
      if (res.Success || res.success) {
        this.showToast(res.Message || res.message, "success");
      } else {
        this.showToast(`Network Action Failed: ${res.Message || res.message}`, "error");
      }
    } catch (err) { }
  }

  async executePing() {
    const target = document.getElementById("ping-target-input")?.value || "1.1.1.1";
    const resBox = document.getElementById("ping-result-box");
    if (resBox) resBox.textContent = `Pinging ${target}...`;

    try {
      const res = await this.api("/api/network/ping", "POST", { host: target });
      if (resBox) {
        if (res.success || res.Success) {
          resBox.innerHTML = `<span style="color:var(--safe-green);">Reply from ${res.host || res.Host}: Roundtrip = ${res.roundtripTimeMs || res.RoundtripTimeMs}ms (${res.status || res.Status})</span>`;
        } else {
          resBox.innerHTML = `<span style="color:var(--danger-red);">Ping to ${target} failed: ${res.status || res.Status}</span>`;
        }
      }
    } catch (err) { }
  }

  async executeShodanLookup() {
    const target = document.getElementById("shodan-target-input")?.value.trim();
    const resBox = document.getElementById("shodan-result-box");
    if (!target) {
      this.showToast("Please enter an IP or domain.", "warning");
      return;
    }

    if (resBox) resBox.textContent = `Querying SHODAN.io for ${target}...`;

    try {
      const res = await this.api(`/api/network/shodan?ip=${encodeURIComponent(target)}`);
      if (resBox && res) {
        const ports = res.openPorts || res.OpenPorts || [];
        const portStr = ports.length > 0 ? ports.join(", ") : "No standard open ports discovered";
        resBox.innerHTML = `
          <div>Resolved IP: <strong style="color:#ffffff;">${res.resolvedIp || res.ResolvedIp}</strong></div>
          <div>Hostname: <strong style="color:#38bdf8;">${res.hostName || res.HostName || "N/A"}</strong></div>
          <div>Probed Ports: <span style="color:var(--accent-amber);">${portStr}</span></div>
          <div style="margin-top:8px;">
            <a href="${res.shodanUrl || res.ShodanUrl}" target="_blank" style="color:var(--accent-amber); text-decoration:underline;">
              Open in SHODAN.io Intelligence Database &rarr;
            </a>
          </div>
        `;
      }
    } catch (err) { }
  }

  // 3. ENVIRONMENT VARIABLES
  async loadEnvVariables() {
    try {
      this.allEnvVars = await this.api("/api/tools/env");
      this.filterEnvTable();
    } catch (err) { }
  }

  filterEnvTable() {
    const tbody = document.getElementById("env-table-body");
    if (!tbody) return;

    const query = (document.getElementById("env-search-input")?.value || "").toLowerCase().trim();
    const filtered = this.allEnvVars.filter(v => {
      if (!query) return true;
      return v.name.toLowerCase().includes(query) || v.value.toLowerCase().includes(query);
    });

    tbody.innerHTML = "";
    filtered.forEach(v => {
      const tr = document.createElement("tr");
      const shortVal = v.value.length > 80 ? v.value.slice(0, 80) + "..." : v.value;
      tr.innerHTML = `
        <td><strong style="color:#ffffff; font-family:var(--font-mono);">${this.escapeHtml(v.name)}</strong></td>
        <td><span class="pill-risk ${v.scope === 'System' ? 'caution' : 'safe'}">${this.escapeHtml(v.scope)}</span></td>
        <td style="font-family:var(--font-mono); font-size:12px; color:var(--text-secondary); max-width:320px; overflow:hidden; text-overflow:ellipsis; white-space:nowrap;" title="${this.escapeHtml(v.value)}">${this.escapeHtml(shortVal)}</td>
        <td style="text-align:right;">
          <button class="btn-card-util" onclick="supApp.openEditEnvModal('${this.escapeJs(v.name)}', '${this.escapeJs(v.scope)}')" title="Edit Variable (Row & Text Editor)" style="margin-right:6px;">✏️</button>
          <button class="btn-card-util" onclick="supApp.deleteEnvVar('${this.escapeJs(v.name)}', '${this.escapeJs(v.scope)}')" title="Delete Variable" style="color:#ef4444;">🗑️</button>
        </td>
      `;
      tbody.appendChild(tr);
    });
  }

  openEditEnvModal(name, scope) {
    const item = this.allEnvVars.find(v => v.name === name && v.scope === scope);
    if (!item) {
      this.showToast(`Variable '${name}' not found.`, "error");
      return;
    }

    this.currentEditEnv = item;
    this.editEnvIsRaw = false;

    const titleElem = document.getElementById("edit-env-title");
    const nameInput = document.getElementById("edit-env-name");
    const scopeInput = document.getElementById("edit-env-scope");
    const rawText = document.getElementById("edit-env-raw-text");
    const btnToggle = document.getElementById("btn-toggle-env-mode");
    const listView = document.getElementById("edit-env-list-view");
    const rawView = document.getElementById("edit-env-raw-view");

    if (titleElem) titleElem.textContent = `Edit Variable: ${item.name}`;
    if (nameInput) nameInput.value = item.name;
    if (scopeInput) scopeInput.value = item.scope;
    if (rawText) rawText.value = item.value || "";
    if (btnToggle) btnToggle.textContent = "Raw Text Mode";
    if (listView) listView.style.display = "flex";
    if (rawView) rawView.style.display = "none";

    // Split value into rows by semicolon
    const rawVal = item.value || "";
    this.editEnvRows = rawVal.split(";").filter(r => r.length > 0);
    if (this.editEnvRows.length === 0 && rawVal.length > 0) {
      this.editEnvRows = [rawVal];
    } else if (this.editEnvRows.length === 0) {
      this.editEnvRows = [""];
    }

    this.renderEditEnvRows();
    document.getElementById("modal-edit-env")?.classList.add("active");
  }

  renderEditEnvRows() {
    const container = document.getElementById("edit-env-rows-container");
    if (!container) return;

    container.innerHTML = "";
    if (this.editEnvRows.length === 0) {
      container.innerHTML = '<div style="color:var(--text-muted); font-size:12px; padding:12px; text-align:center;">No entries present. Click "+ Add Row" to create one.</div>';
      return;
    }

    this.editEnvRows.forEach((val, idx) => {
      const row = document.createElement("div");
      row.className = "env-row-item";
      row.style.display = "flex";
      row.style.gap = "6px";
      row.style.alignItems = "center";
      row.style.marginBottom = "4px";

      row.innerHTML = `
        <span style="font-size:11px; font-family:var(--font-mono); color:var(--text-muted); width:24px; text-align:right;">${idx + 1}.</span>
        <input type="text" class="search-input-field env-row-input" value="${this.escapeHtml(val)}" data-index="${idx}" oninput="supApp.updateEnvRow(${idx}, this.value)" style="flex:1; height:32px; font-size:12px; font-family:var(--font-mono);">
        <button class="btn-card-util btn-row-action" onclick="supApp.moveEnvRow(${idx}, -1)" title="Move Up" ${idx === 0 ? 'disabled style="opacity:0.3; cursor:not-allowed;"' : ''}>▲</button>
        <button class="btn-card-util btn-row-action" onclick="supApp.moveEnvRow(${idx}, 1)" title="Move Down" ${idx === this.editEnvRows.length - 1 ? 'disabled style="opacity:0.3; cursor:not-allowed;"' : ''}>▼</button>
        <button class="btn-card-util btn-row-action" onclick="supApp.deleteEnvRow(${idx})" title="Delete Row" style="color:#ef4444;">🗑️</button>
      `;
      container.appendChild(row);
    });
  }

  updateEnvRow(index, val) {
    if (index >= 0 && index < this.editEnvRows.length) {
      this.editEnvRows[index] = val;
    }
  }

  addEnvRow(val = "") {
    this.editEnvRows.push(val);
    this.renderEditEnvRows();
    setTimeout(() => {
      const inputs = document.querySelectorAll(".env-row-input");
      if (inputs.length > 0) {
        inputs[inputs.length - 1].focus();
      }
    }, 50);
  }

  moveEnvRow(index, direction) {
    const targetIdx = index + direction;
    if (targetIdx < 0 || targetIdx >= this.editEnvRows.length) return;
    const temp = this.editEnvRows[index];
    this.editEnvRows[index] = this.editEnvRows[targetIdx];
    this.editEnvRows[targetIdx] = temp;
    this.renderEditEnvRows();
  }

  deleteEnvRow(index) {
    if (index >= 0 && index < this.editEnvRows.length) {
      this.editEnvRows.splice(index, 1);
      this.renderEditEnvRows();
    }
  }

  toggleEditEnvMode() {
    this.editEnvIsRaw = !this.editEnvIsRaw;
    const btnToggle = document.getElementById("btn-toggle-env-mode");
    const listView = document.getElementById("edit-env-list-view");
    const rawView = document.getElementById("edit-env-raw-view");
    const rawText = document.getElementById("edit-env-raw-text");

    if (this.editEnvIsRaw) {
      if (btnToggle) btnToggle.textContent = "Row List Mode";
      if (listView) listView.style.display = "none";
      if (rawView) rawView.style.display = "flex";
      if (rawText) rawText.value = this.editEnvRows.filter(r => r.trim().length > 0).join(";");
    } else {
      if (btnToggle) btnToggle.textContent = "Raw Text Mode";
      if (listView) listView.style.display = "flex";
      if (rawView) rawView.style.display = "none";
      if (rawText) {
        this.editEnvRows = rawText.value.split(";").filter(r => r.length > 0);
        this.renderEditEnvRows();
      }
    }
  }

  async saveEditedEnvVar() {
    if (!this.currentEditEnv) return;

    let finalValue = "";
    if (this.editEnvIsRaw) {
      finalValue = document.getElementById("edit-env-raw-text")?.value || "";
    } else {
      const inputs = document.querySelectorAll(".env-row-input");
      const collected = [];
      inputs.forEach(inp => {
        const v = inp.value.trim();
        if (v) collected.push(v);
      });
      finalValue = collected.join(";");
    }

    try {
      const res = await this.api("/api/tools/env/set", "POST", {
        name: this.currentEditEnv.name,
        value: finalValue,
        scope: this.currentEditEnv.scope
      });

      if (res.Success || res.success) {
        this.showToast(`Variable '${this.currentEditEnv.name}' updated successfully!`, "success");
        this.closeAllModals();
        await this.loadEnvVariables();
      } else {
        this.showToast(`Save error: ${res.Message || res.message}`, "error");
      }
    } catch (err) {
      this.showToast(`Error: ${err.message}`, "error");
    }
  }

  openAddEnvModal() {
    document.getElementById("modal-add-env")?.classList.add("active");
  }

  async saveNewEnvVar() {
    const name = document.getElementById("new-env-name")?.value.trim();
    const val = document.getElementById("new-env-val")?.value.trim();
    const scope = document.getElementById("new-env-scope")?.value || "User";

    if (!name) {
      this.showToast("Variable name is required.", "warning");
      return;
    }

    try {
      const res = await this.api("/api/tools/env/set", "POST", { name, value: val, scope });
      if (res.Success || res.success) {
        this.showToast(res.Message || res.message, "success");
        this.closeAllModals();
        await this.loadEnvVariables();
      } else {
        this.showToast(`Error: ${res.Message || res.message}`, "error");
      }
    } catch (err) { }
  }

  async deleteEnvVar(name, scope) {
    if (!confirm(`Are you sure you want to delete environment variable '${name}' (${scope})?`)) {
      return;
    }

    try {
      const res = await this.api("/api/tools/env/set", "POST", { name, value: "", scope });
      this.showToast(`Variable '${name}' removed successfully.`, "success");
      await this.loadEnvVariables();
    } catch (err) { }
  }

  // 4. FILE UNLOCKER
  async inspectFileLocks() {
    const filePath = document.getElementById("unlock-file-input")?.value.trim();
    if (!filePath) {
      this.showToast("Please enter a valid file or folder path.", "warning");
      return;
    }

    try {
      const res = await this.api("/api/tools/unlock/find", "POST", { filePath });
      const resultsBox = document.getElementById("unlock-results-box");
      const tbody = document.getElementById("unlock-tbody");

      if (resultsBox && tbody) {
        tbody.innerHTML = "";
        const procs = res.lockingProcesses || res.LockingProcesses || [];

        if (procs.length === 0) {
          resultsBox.style.display = "block";
          tbody.innerHTML = `<tr><td colspan="4" style="text-align:center; color:var(--safe-green);">No locking handles detected. The file is unlocked!</td></tr>`;
        } else {
          resultsBox.style.display = "block";
          procs.forEach(p => {
            const tr = document.createElement("tr");
            tr.innerHTML = `
              <td><code style="color:#ffffff;">${p.processId || p.ProcessId}</code></td>
              <td><strong style="color:#f59e0b;">${this.escapeHtml(p.processName || p.ProcessName)}</strong></td>
              <td>${this.escapeHtml(p.appName || p.AppName || "N/A")}</td>
              <td>
                <button class="btn-secondary" style="color:#ef4444;" onclick="supApp.killProcessAndUnlock(${p.processId || p.ProcessId})">
                  Terminate & Unlock
                </button>
              </td>
            `;
            tbody.appendChild(tr);
          });
        }
      }
    } catch (err) { }
  }

  async killProcessAndUnlock(pid) {
    try {
      const res = await this.api("/api/tools/unlock/kill", "POST", { processId: pid });
      if (res.Success || res.success) {
        this.showToast(res.Message || res.message, "success");
        await this.inspectFileLocks();
      } else {
        this.showToast(`Failed: ${res.Message || res.message}`, "error");
      }
    } catch (err) { }
  }

  // 5. RUN DIALOG ALIASES (App Paths)
  async loadRunAliases() {
    try {
      this.allRunAliases = await this.api("/api/tools/run-aliases");
      const tbody = document.getElementById("aliases-tbody");
      if (!tbody) return;

      tbody.innerHTML = "";
      this.allRunAliases.forEach(a => {
        const tr = document.createElement("tr");
        tr.innerHTML = `
          <td><strong style="color:var(--accent-amber); font-family:var(--font-mono);">${this.escapeHtml(a.commandName || a.CommandName)}</strong></td>
          <td style="font-family:var(--font-mono); font-size:12px; color:var(--text-secondary);">${this.escapeHtml(a.targetPath || a.TargetPath)}</td>
          <td><span class="pill-risk safe">${a.scope || a.Scope}</span></td>
          <td>
            <button class="btn-card-util" onclick="supApp.removeRunAlias('${a.commandName || a.CommandName}')" title="Remove Alias">🗑️</button>
          </td>
        `;
        tbody.appendChild(tr);
      });
    } catch (err) { }
  }

  openAddAliasModal() {
    document.getElementById("modal-add-alias")?.classList.add("active");
  }

  async saveNewRunAlias() {
    const aliasName = document.getElementById("new-alias-name")?.value.trim();
    const targetPath = document.getElementById("new-alias-target")?.value.trim();

    if (!aliasName || !targetPath) {
      this.showToast("Alias and target path are required.", "warning");
      return;
    }

    try {
      const res = await this.api("/api/tools/run-aliases/add", "POST", { aliasName, targetPath });
      if (res.Success || res.success) {
        this.showToast(res.Message || res.message, "success");
        this.closeAllModals();
        await this.loadRunAliases();
      } else {
        this.showToast(`Error: ${res.Message || res.message}`, "error");
      }
    } catch (err) { }
  }

  async removeRunAlias(aliasName) {
    try {
      const res = await this.api("/api/tools/run-aliases/remove", "POST", { aliasName });
      this.showToast(`Alias ${aliasName} removed.`, "success");
      await this.loadRunAliases();
    } catch (err) { }
  }

  // 6. CONTEXT MENU TWEAKS
  loadContextMenuTweaks() {
    const grid = document.getElementById("contextmenu-tweaks-grid");
    if (!grid) return;

    const ctxTweaks = this.allTweaks.filter(t => t.id.startsWith("ctx_") || t.id === "win_classic_context_menu");
    grid.innerHTML = "";

    ctxTweaks.forEach(tweak => {
      const isEnabled = tweak.currentState === "Enabled";
      const card = document.createElement("div");
      card.className = "tweak-card";
      card.innerHTML = `
        <div class="card-top">
          <div class="card-uuid">${tweak.id}</div>
          <div class="card-title">${this.escapeHtml(tweak.name)}</div>
          <div class="card-desc">${this.escapeHtml(tweak.description)}</div>
        </div>
        <div class="card-bottom">
          <div class="pill-risk safe">🛡️ Safe</div>
          <div class="fluid-switch ${isEnabled ? 'active' : ''}" onclick="supApp.toggleTweak('${tweak.id}', ${!isEnabled})">
            <div class="switch-knob"></div>
          </div>
        </div>
      `;
      grid.appendChild(card);
    });
  }

  // =========================================================================
  // APP STORE / INSTALLER
  // =========================================================================
  async loadInstaller() {
    try {
      this.allCatalogApps = await this.api("/api/installer/catalog");
      this.renderInstallerGrid();
    } catch (err) { }
  }

  filterInstaller(cat, btn) {
    this.activeInstallerSubtab = cat;
    document.querySelectorAll("#tab-installer .subtab-item").forEach(b => b.classList.remove("active"));
    if (btn) btn.classList.add("active");

    const grid = document.getElementById("installer-grid");
    const updatesPanel = document.getElementById("installer-updates-panel");

    if (cat === "Updates") {
      if (grid) grid.style.display = "none";
      if (updatesPanel) updatesPanel.style.display = "block";
      this.scanWingetUpgrades();
    } else {
      if (grid) grid.style.display = "grid";
      if (updatesPanel) updatesPanel.style.display = "none";
      this.renderInstallerGrid();
    }
  }

  async scanWingetUpgrades() {
    const tbody = document.getElementById("installer-updates-tbody");
    const btnAll = document.getElementById("btn-upgrade-all");
    if (!tbody) return;

    tbody.innerHTML = `<tr><td colspan="5" style="text-align:center; padding:24px; color:var(--text-muted);">Querying Winget for outdated applications...</td></tr>`;
    if (btnAll) btnAll.style.display = "none";

    try {
      const upgrades = await this.api("/api/installer/upgrades");
      if (!Array.isArray(upgrades) || upgrades.length === 0) {
        tbody.innerHTML = `<tr><td colspan="5" style="text-align:center; padding:24px; color:var(--safe-green); font-weight:600;">✅ All installed applications are up to date!</td></tr>`;
        return;
      }

      if (btnAll) btnAll.style.display = "inline-flex";

      tbody.innerHTML = upgrades.map(u => `
        <tr>
          <td><strong style="color:#ffffff;">${this.escapeHtml(u.name || u.Name)}</strong></td>
          <td><code style="font-size:11px; color:var(--accent-amber);">${this.escapeHtml(u.id || u.Id)}</code></td>
          <td><span style="font-family:var(--font-mono); color:var(--text-secondary);">${this.escapeHtml(u.installedVersion || u.InstalledVersion)}</span></td>
          <td><span style="font-family:var(--font-mono); color:var(--safe-green); font-weight:700;">${this.escapeHtml(u.availableVersion || u.AvailableVersion)}</span></td>
          <td>
            <button class="btn-primary-amber" style="padding:4px 10px; font-size:11px;" onclick="supApp.runWingetUpgrade('${this.escapeJs(u.id || u.Id)}')">Upgrade</button>
          </td>
        </tr>
      `).join("");
    } catch (err) {
      tbody.innerHTML = `<tr><td colspan="5" style="text-align:center; padding:24px; color:var(--danger-red);">Error checking updates: ${this.escapeHtml(err.message)}</td></tr>`;
    }
  }

  async runWingetUpgrade(packageId) {
    this.showToast(packageId === "all" ? "Starting bulk upgrade for all outdated applications..." : `Upgrading package ${packageId}...`, "info");
    try {
      const res = await this.api("/api/installer/upgrade", "POST", { packageId });
      if (res.Success || res.success) {
        this.showToast(res.Message || res.message, "success");
        await this.scanWingetUpgrades();
      } else {
        this.showToast(`Upgrade error: ${res.Message || res.message}`, "error");
      }
    } catch (err) {
      this.showToast(`Error: ${err.message}`, "error");
    }
  }

  renderInstallerGrid() {
    const grid = document.getElementById("installer-grid");
    if (!grid) return;

    grid.innerHTML = "";
    const filtered = this.allCatalogApps.filter(app => {
      if (this.activeInstallerSubtab === "All") return true;
      return (app.category || "").toLowerCase() === this.activeInstallerSubtab.toLowerCase();
    });

    filtered.forEach(app => {
      const isInst = app.isInstalled || app.IsInstalled;
      const card = document.createElement("div");
      card.className = "tweak-card";
      card.innerHTML = `
        <div class="card-top">
          <div class="card-uuid">winget: ${app.id || app.Id}</div>
          <div class="card-title">${this.escapeHtml(app.name || app.Name)}</div>
          <div class="card-micro-tags">
            <span class="micro-tag">${app.category || app.Category}</span>
            ${isInst ? `<span class="micro-tag" style="color:var(--safe-green);">Installed</span>` : ""}
          </div>
          <div class="card-desc">${this.escapeHtml(app.description || app.Description)}</div>
        </div>
        <div class="card-bottom">
          <div class="pill-risk safe">Winget Verified</div>
          <button class="btn-primary-amber" onclick="supApp.installCatalogApp('${app.id || app.Id}')">
            ${isInst ? "Reinstall / Update" : "Install App"}
          </button>
        </div>
      `;
      grid.appendChild(card);
    });
  }

  async installCatalogApp(pkgId) {
    this.showToast(`Starting silent installation for ${pkgId}...`, "info");
    try {
      const res = await this.api("/api/installer/install", "POST", { packageId: pkgId });
      if (res.Success || res.success) {
        this.showToast(res.Message || res.message, "success");
        await this.loadInstaller();
      } else {
        this.showToast(`Install Failed: ${res.Message || res.message}`, "error");
      }
    } catch (err) { }
  }

  // =========================================================================
  // STARTUP MANAGER
  // =========================================================================
  async loadStartup() {
    try {
      this.allStartup = await this.api("/api/startup");
      const tbody = document.getElementById("startup-table-body");
      if (!tbody) return;

      tbody.innerHTML = "";
      if (this.allStartup.length === 0) {
        tbody.innerHTML = `<tr><td colspan="5" style="text-align:center; padding:20px; color:var(--text-muted);">No startup applications registered or found.</td></tr>`;
        return;
      }

      this.allStartup.forEach(item => {
        const isEnabled = !!(item.isEnabled ?? item.IsEnabled);
        const name = item.name || item.Name || "";
        const command = item.command || item.Command || "";
        const location = item.location || item.Location || "";
        const impact = item.impact || item.Impact || "Normal";

        const statusBadge = isEnabled
          ? `<span class="pill-risk safe" style="min-width:80px; text-align:center; font-size:11px;">Enabled</span>`
          : `<span class="pill-risk" style="min-width:80px; text-align:center; font-size:11px; background:rgba(255,255,255,0.06); color:var(--text-muted); border-color:var(--border-subtle);">Disabled</span>`;

        const tr = document.createElement("tr");
        tr.innerHTML = `
          <td><strong style="color:#ffffff;">${this.escapeHtml(name)}</strong></td>
          <td style="font-family:var(--font-mono); font-size:12px; color:var(--text-secondary); max-width:280px; overflow:hidden; text-overflow:ellipsis; white-space:nowrap;" title="${this.escapeHtml(command)}">${this.escapeHtml(command)}</td>
          <td><span class="pill-risk safe" style="font-size:11px;">${this.escapeHtml(location)}</span></td>
          <td><span class="pill-risk ${impact === 'High' ? 'caution' : 'safe'}" style="font-size:11px;">${this.escapeHtml(impact)}</span></td>
          <td>
            <div class="startup-status-cell">
              ${statusBadge}
              <div class="fluid-switch ${isEnabled ? 'active' : ''}" onclick="supApp.toggleStartup('${this.escapeJs(location)}', '${this.escapeJs(name)}', ${!isEnabled})" title="${isEnabled ? 'Disable startup item' : 'Enable startup item'}">
                <div class="switch-knob"></div>
              </div>
            </div>
          </td>
        `;
        tbody.appendChild(tr);
      });
    } catch (err) { }
  }

  async toggleStartup(location, name, enable) {
    try {
      const res = await this.api("/api/startup/toggle", "POST", { location, name, enable });
      if (res.Success || res.success) {
        this.showToast(`Startup item ${name} updated.`, "success");
        await this.loadStartup();
      } else {
        this.showToast(`Failed: ${res.Message || res.message}`, "error");
      }
    } catch (err) { }
  }

  // =========================================================================
  // DISK CLEANUP
  // =========================================================================
  async loadStorage() {
    try {
      const tbody = document.getElementById("storage-table-body");
      if (!tbody) return;

      tbody.innerHTML = `<tr><td colspan="4" style="text-align:center; padding:24px; color:var(--text-muted);">Analyzing system storage and cache targets...</td></tr>`;

      const analysis = await this.api("/api/storage/analyze");
      if (!analysis) return;

      tbody.innerHTML = "";
      const targets = Array.isArray(analysis) ? analysis : (analysis.targets || analysis.Targets || []);
      this.allStorageTargets = targets;

      if (targets.length === 0) {
        tbody.innerHTML = `<tr><td colspan="4" style="text-align:center; padding:24px; color:var(--text-muted);">No cleanup targets detected.</td></tr>`;
        return;
      }

      targets.forEach(t => {
        const tr = document.createElement("tr");
        const tid = t.id || t.Id || "";
        const tname = t.name || t.Name || "Target";
        const tdesc = t.description || t.Description || "";
        const mb = t.sizeMb != null ? t.sizeMb : (t.SizeMb != null ? t.SizeMb : 0);
        const files = t.fileCount != null ? t.fileCount : (t.FileCount != null ? t.FileCount : 0);
        const sizeFormatted = mb > 0 ? `${mb} MB (${files} files)` : `${files} files`;

        tr.innerHTML = `
          <td><input type="checkbox" class="storage-target-check" value="${tid}" checked></td>
          <td><strong style="color:#ffffff;">${this.escapeHtml(tname)}</strong></td>
          <td style="color:var(--text-secondary);">${this.escapeHtml(tdesc)}</td>
          <td><span style="font-family:var(--font-mono); color:var(--accent-amber); font-weight:700;">${sizeFormatted}</span></td>
        `;
        tbody.appendChild(tr);
      });
    } catch (err) {
      const tbody = document.getElementById("storage-table-body");
      if (tbody) tbody.innerHTML = `<tr><td colspan="4" style="text-align:center; padding:24px; color:var(--danger-red);">Scan error: ${err.message}</td></tr>`;
    }
  }

  toggleAllStorageChecks(master) {
    document.querySelectorAll(".storage-target-check").forEach(c => c.checked = master.checked);
  }

  async runStorageClean() {
    const selected = Array.from(document.querySelectorAll(".storage-target-check:checked")).map(c => c.value);
    if (selected.length === 0) {
      this.showToast("Select at least one cleanup target.", "warning");
      return;
    }

    try {
      const res = await this.api("/api/storage/clean", "POST", { targetIds: selected, dryRun: this.safeTestMode });
      if (res.Success || res.success) {
        this.showToast(`Cleaned: ${res.Message || res.message}`, "success");
        await this.loadStorage();
        await this.fetchMetrics();
      } else {
        this.showToast(`Cleaning Failed: ${res.Message || res.message}`, "error");
      }
    } catch (err) { }
  }

  // =========================================================================
  // SECURITY & REPAIR TERMINAL
  // =========================================================================
  async loadRepair() {
    try {
      const status = await this.api("/api/repair/autologon");
      const badge = document.getElementById("autologon-status-badge");
      if (badge) {
        if (status.Enabled || status.enabled) {
          badge.textContent = `● Enabled (${status.Username || status.username || "Active"})`;
          badge.className = "pill-risk safe";
        } else {
          badge.textContent = "○ Disabled";
          badge.className = "pill-risk";
          badge.style.background = "rgba(255,255,255,0.06)";
          badge.style.color = "var(--text-muted)";
        }
      }
    } catch (err) { }
  }

  openAutoLogonModal() {
    const modal = document.getElementById("modal-autologon");
    if (modal) modal.classList.add("active");
  }

  async saveAutoLogon() {
    const username = document.getElementById("autologon-username")?.value?.trim();
    const domain = document.getElementById("autologon-domain")?.value?.trim() || ".";
    const password = document.getElementById("autologon-password")?.value || "";

    if (!username) {
      this.showToast("Username is required for AutoLogon.", "warning");
      return;
    }

    try {
      const res = await this.api("/api/repair/autologon/set", "POST", { username, domain, password });
      if (res.Success || res.success) {
        this.showToast(res.Message || "AutoLogon configured successfully.", "success");
        this.closeAllModals();
        await this.loadRepair();
      } else {
        this.showToast(res.Message || "Failed to configure AutoLogon.", "error");
      }
    } catch (err) { }
  }

  async disableAutoLogon() {
    try {
      const res = await this.api("/api/repair/autologon/disable", "POST");
      if (res.Success || res.success) {
        this.showToast(res.Message || "AutoLogon disabled.", "success");
        await this.loadRepair();
      } else {
        this.showToast(res.Message || "Failed to disable AutoLogon.", "error");
      }
    } catch (err) { }
  }

  async enableUltimatePerformance() {
    try {
      const res = await this.api("/api/repair/power/ultimate", "POST");
      if (res.Success || res.success) {
        this.showToast(res.Message || "Ultimate Performance Power Plan activated!", "success");
      } else {
        this.showToast(res.Message || "Failed to activate power plan.", "error");
      }
    } catch (err) { }
  }

  async toggleHibernation() {
    try {
      const res = await this.api("/api/repair/power/hibernation", "POST");
      if (res.Success || res.success) {
        this.showToast(res.Message || "Hibernation state toggled.", "success");
      } else {
        this.showToast(res.Message || "Failed to toggle hibernation.", "error");
      }
    } catch (err) { }
  }

  async runRepair(toolId) {
    const outBox = document.getElementById("repair-terminal-output");
    if (outBox) outBox.textContent = `Launching repair operation: ${toolId}...\n`;

    try {
      const res = await this.api("/api/repair/run", "POST", { toolId });
      if (res.Success || res.success) {
        this.showToast(`Tool ${toolId} started.`, "info");
        this.startRepairPolling();
      } else {
        this.showToast(`Failed to start: ${res.Message || res.message}`, "error");
      }
    } catch (err) { }
  }

  startRepairPolling() {
    if (this.repairInterval) clearInterval(this.repairInterval);
    this.repairInterval = setInterval(async () => {
      try {
        const out = await this.api("/api/repair/output");
        const outBox = document.getElementById("repair-terminal-output");
        if (outBox && out.output) {
          outBox.textContent = out.output;
          outBox.scrollTop = outBox.scrollHeight;
        }

        if (!out.isRunning) {
          clearInterval(this.repairInterval);
          this.repairInterval = null;
        }
      } catch (err) {
        clearInterval(this.repairInterval);
      }
    }, 1000);
  }

  // =========================================================================
  // OPTIONAL WINDOWS FEATURES (DISM)
  // =========================================================================
  async loadFeatures() {
    const grid = document.getElementById("features-grid");
    if (grid) grid.innerHTML = `<div style="grid-column:1/-1; text-align:center; padding:30px; color:var(--text-muted);">Querying Windows DISM servicing stack...</div>`;

    try {
      this.allFeatures = await this.api("/api/features");
      this.renderFeaturesGrid();
    } catch (err) {
      if (grid) grid.innerHTML = `<div style="grid-column:1/-1; text-align:center; padding:30px; color:#ef4444;">Failed to query Windows features: ${this.escapeHtml(err.message)}</div>`;
    }
  }

  renderFeaturesGrid() {
    const grid = document.getElementById("features-grid");
    if (!grid) return;

    grid.innerHTML = "";
    if (!this.allFeatures || this.allFeatures.length === 0) {
      grid.innerHTML = `<div style="grid-column:1/-1; text-align:center; padding:30px; color:var(--text-muted);">No features detected.</div>`;
      return;
    }

    this.allFeatures.forEach(feat => {
      const isEnabled = feat.isEnabled || feat.state === "Enabled";
      const badge = isEnabled
        ? `<span class="pill-risk safe" style="font-size:11px; padding:2px 8px;">● Enabled</span>`
        : `<span class="pill-risk" style="background:rgba(255,255,255,0.06); color:var(--text-muted); font-size:11px; padding:2px 8px;">○ Disabled</span>`;

      const actionBtn = isEnabled
        ? `<button class="btn-secondary" style="border-color:rgba(239, 68, 68, 0.3); color:#fca5a5;" onclick="supApp.toggleFeature('${this.escapeJs(feat.featureName)}', false)">Disable Feature</button>`
        : `<button class="btn-primary-amber" onclick="supApp.toggleFeature('${this.escapeJs(feat.featureName)}', true)">Enable Feature</button>`;

      const card = document.createElement("div");
      card.className = "tweak-card";
      card.innerHTML = `
        <div class="card-top">
          <div class="card-uuid">${this.escapeHtml(feat.featureName)}</div>
          <div class="card-title">${this.escapeHtml(feat.displayName)}</div>
          <div class="card-micro-tags">
            <span class="micro-tag">
              <svg viewBox="0 0 24 24"><polygon points="12 2 2 7 12 12 22 7 12 2"></polygon></svg>
              DISM Feature
            </span>
            ${feat.restartRequired ? '<span class="micro-tag" style="color:#f59e0b;">Reboot Required</span>' : ''}
          </div>
          <div class="card-desc">${this.escapeHtml(feat.description)}</div>
        </div>
        <div class="card-bottom">
          <div style="display:flex; align-items:center; gap:8px;">
            ${badge}
          </div>
          <div class="card-actions-row">
            ${actionBtn}
          </div>
        </div>
      `;
      grid.appendChild(card);
    });
  }

  async toggleFeature(featureName, enable) {
    const actionWord = enable ? "enabling" : "disabling";
    this.showToast(`DISM is ${actionWord} '${featureName}', this may take a moment...`, "info");

    try {
      const res = await this.api("/api/features/set", "POST", { featureName, enable });
      if (res.Success || res.success) {
        this.showToast(res.Message || `Feature updated.`, "success");
        await this.loadFeatures();
      } else {
        this.showToast(res.Message || "Feature modification failed.", "error");
      }
    } catch (err) { }
  }

  // =========================================================================
  // SETUP PROFILES & PRESETS
  // =========================================================================
  async loadProfiles() {
    const grid = document.getElementById("profiles-preset-grid");
    if (!grid) return;

    try {
      this.allProfiles = await this.api("/api/profiles/presets");
      grid.innerHTML = "";

      this.allProfiles.forEach(p => {
        const card = document.createElement("div");
        card.className = "stat-card";
        card.style.display = "flex";
        card.style.flexDirection = "column";
        card.style.justifyContent = "space-between";

        const tags = (p.tags || []).map(t => `<span class="micro-tag">${this.escapeHtml(t)}</span>`).join(" ");

        card.innerHTML = `
          <div>
            <div style="display:flex; justify-content:space-between; align-items:center; margin-bottom:8px;">
              <span class="pill-risk safe" style="font-size:11px; padding:2px 8px;">Preset</span>
              <span style="font-size:11px; color:var(--text-muted);">${p.tweakCount || (p.tweakStates ? Object.keys(p.tweakStates).length : 0)} Tweaks</span>
            </div>
            <strong style="color:#ffffff; font-size:14px; display:block; margin-bottom:6px;">${this.escapeHtml(p.name)}</strong>
            <p style="font-size:12px; color:var(--text-secondary); margin-bottom:12px; line-height:1.4;">${this.escapeHtml(p.description)}</p>
            <div style="margin-bottom:16px;">${tags}</div>
          </div>
          <button class="btn-primary-amber" style="width:100%; justify-content:center;" onclick="supApp.applyProfilePreset('${this.escapeJs(p.id)}')">
            Apply Preset
          </button>
        `;
        grid.appendChild(card);
      });
    } catch (err) { }
  }

  async applyProfilePreset(presetId) {
    const preset = (this.allProfiles || []).find(p => p.id === presetId);
    if (!preset) return;

    if (!confirm(`Apply the '${preset.name}' configuration profile?\n\nThis will configure ${preset.tweakCount || Object.keys(preset.tweakStates || {}).length} tweaks and create a rollback ChangeSet.`)) return;

    this.showToast(`Applying profile '${preset.name}'...`, "info");
    try {
      const res = await this.api("/api/profiles/apply", "POST", { profileJson: JSON.stringify(preset) });
      if (res.Success || res.success) {
        this.showToast(res.Message || `Profile '${preset.name}' applied!`, "success");
        await this.loadTweaks();
      } else {
        this.showToast(res.Message || "Failed to apply profile.", "error");
      }
    } catch (err) { }
  }

  async exportProfile() {
    try {
      const data = await this.api("/api/profiles/export", "POST");
      const jsonStr = JSON.stringify(data, null, 2);
      const blob = new Blob([jsonStr], { type: "application/json" });
      const url = URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = `SUPOptimizer_Config_${new Date().toISOString().slice(0, 10)}.json`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      URL.revokeObjectURL(url);
      this.showToast("Configuration profile exported successfully.", "success");
    } catch (err) { }
  }

  async handleProfileUpload(event) {
    const file = event.target.files?.[0];
    if (!file) return;

    try {
      const text = await file.text();
      const parsed = JSON.parse(text);
      if (!confirm(`Import and apply profile '${parsed.name || file.name}' with ${Object.keys(parsed.tweakStates || {}).length} tweaks?`)) return;

      this.showToast("Importing and applying custom configuration profile...", "info");
      const res = await this.api("/api/profiles/apply", "POST", { profileJson: text });
      if (res.Success || res.success) {
        this.showToast(res.Message || "Custom profile imported and applied successfully!", "success");
        await this.loadTweaks();
      } else {
        this.showToast(res.Message || "Failed to apply imported profile.", "error");
      }
    } catch (err) {
      this.showToast(`Invalid profile JSON: ${err.message}`, "error");
    } finally {
      event.target.value = "";
    }
  }

  // =========================================================================
  // AUTOUNATTEND XML GENERATOR
  // =========================================================================
  async generateAutounattendPreview() {
    const req = {
      operatingSystem: document.getElementById("unattend-os")?.value || "Windows 11",
      localUsername: document.getElementById("unattend-user")?.value || "User",
      password: document.getElementById("unattend-pass")?.value || "",
      computerName: document.getElementById("unattend-pcname")?.value || "PC-OPTIMIZED",
      timeZone: document.getElementById("unattend-tz")?.value || "UTC",
      bypassTpmAndSecureBoot: document.getElementById("unattend-tpm")?.checked ?? true,
      bypassMicrosoftAccount: document.getElementById("unattend-local")?.checked ?? true,
      enableAutoLogon: document.getElementById("unattend-autologon")?.checked ?? true,
      enableDarkTheme: document.getElementById("unattend-dark")?.checked ?? true,
      disableTelemetry: document.getElementById("unattend-telemetry")?.checked ?? true,
      preventAutoDriverUpdates: document.getElementById("unattend-drivers")?.checked ?? true,
      enableEndTask: document.getElementById("unattend-endtask")?.checked ?? true
    };

    try {
      const res = await this.api("/api/autounattend/generate", "POST", req);
      const preview = document.getElementById("unattend-xml-preview");
      if (preview && (res.XmlContent || res.xmlContent)) {
        preview.value = res.XmlContent || res.xmlContent;
      }
    } catch (err) { }
  }

  downloadAutounattendXml() {
    const preview = document.getElementById("unattend-xml-preview");
    if (!preview || !preview.value) {
      this.showToast("Please generate the XML first.", "warning");
      return;
    }

    const blob = new Blob([preview.value], { type: "application/xml" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = "autounattend.xml";
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
    this.showToast("Downloaded autounattend.xml. Place this in the root of your Windows install USB.", "success");
  }

  copyAutounattendXml() {
    const preview = document.getElementById("unattend-xml-preview");
    if (!preview || !preview.value) return;

    navigator.clipboard.writeText(preview.value).then(() => {
      this.showToast("autounattend.xml copied to clipboard!", "success");
    }).catch(() => {
      preview.select();
      document.execCommand("copy");
      this.showToast("autounattend.xml copied to clipboard!", "success");
    });
  }

  // =========================================================================
  // HARDWARE & BACKUPS
  // =========================================================================
  async loadHardware() {
    try {
      const hw = await this.api("/api/system/hardware");
      if (!hw) return;

      const cpu = hw.cpu || hw.Cpu || {};
      const mem = hw.memory || hw.Memory || {};
      const gpus = hw.gpus || hw.Gpus || [];
      const mb = hw.motherboard || hw.Motherboard || {};
      const bios = hw.bios || hw.Bios || {};
      const disks = hw.disks || hw.Disks || [];
      const periph = hw.peripherals || hw.Peripherals || {};

      // 1. Top Overview Cards
      const cpuName = document.getElementById("hw-cpu-name");
      const cpuCores = document.getElementById("hw-cpu-cores");
      const cpuClock = document.getElementById("hw-cpu-clock");
      if (cpuName) cpuName.textContent = cpu.name || "Processore Central (CPU)";
      if (cpuCores) cpuCores.textContent = `${cpu.numberOfCores || 1} Core / ${cpu.numberOfLogicalProcessors || 1} Thread`;
      if (cpuClock) {
        const l3 = cpu.l3CacheMb ? ` | Cache L3: ${cpu.l3CacheMb} MB` : "";
        cpuClock.textContent = `Frequenza: ${cpu.maxClockSpeedMhz || cpu.currentClockSpeedMhz || '--'} MHz${l3}`;
      }

      const ramTotal = document.getElementById("hw-ram-total");
      const ramSlots = document.getElementById("hw-ram-slots");
      const ramSpeed = document.getElementById("hw-ram-speed");
      if (ramTotal) {
        const totGb = mem.totalRamGb || 0;
        const typeSum = mem.typeSummary || "DDR";
        ramTotal.textContent = `${totGb.toFixed(1)} GB RAM (${typeSum})`;
      }
      if (ramSlots) ramSlots.textContent = `${mem.slotsUsed || 1} / ${mem.totalSlots || 2} Slot Utilizzati`;
      if (ramSpeed) ramSpeed.textContent = `Velocità: ${mem.speedSummary || '--'}`;

      const primaryGpu = gpus.length > 0 ? gpus[0] : null;
      const gpuName = document.getElementById("hw-gpu-name");
      const gpuRes = document.getElementById("hw-gpu-res");
      const gpuVram = document.getElementById("hw-gpu-vram");
      if (gpuName) gpuName.textContent = primaryGpu ? (primaryGpu.name || "Scheda Video") : "Scheda Video Integrata";
      if (gpuRes) {
        const res = primaryGpu ? `${primaryGpu.currentResolution || 'Display'} @ ${primaryGpu.currentRefreshRate || '60Hz'}` : "Attivo";
        gpuRes.textContent = res;
      }
      if (gpuVram) {
        const vramStr = primaryGpu ? (primaryGpu.memoryFormatted || (primaryGpu.memoryMb ? `${primaryGpu.memoryMb} MB` : 'Dedicata')) : "Memoria Condivisa";
        gpuVram.textContent = `VRAM: ${vramStr}`;
      }

      const mbName = document.getElementById("hw-mb-name");
      const biosVer = document.getElementById("hw-bios-ver");
      if (mbName) mbName.textContent = mb.product && mb.product !== "Unknown" ? `${mb.product} (${mb.manufacturer || ''})` : (mb.manufacturer || "Scheda Madre di Sistema");
      if (biosVer) biosVer.textContent = `BIOS: ${bios.version || 'UEFI'} (${bios.releaseDate || '--'})`;

      // 2. RAM DIMM Modules Table
      const ramTbody = document.getElementById("hw-ram-modules-tbody");
      if (ramTbody) {
        const modules = mem.modules || mem.Modules || [];
        if (modules.length === 0) {
          ramTbody.innerHTML = `
            <tr>
              <td>DIMM Primario</td>
              <td><strong>${(mem.totalRamGb || 0).toFixed(1)} GB</strong></td>
              <td>${mem.speedSummary || '--'}</td>
              <td><span class="pill-risk safe" style="font-size:10px;">${mem.typeSummary || 'DDR'}</span></td>
              <td>DIMM</td>
              <td>OEM Standard</td>
              <td><code>N/A (Mappatura Fisica)</code></td>
              <td><span style="color:var(--text-muted);">Attivo</span></td>
            </tr>
          `;
        } else {
          ramTbody.innerHTML = modules.map((m, idx) => `
            <tr>
              <td><strong style="color:#ffffff;">${this.escapeHtml(m.deviceLocator || m.bankLabel || `Slot ${idx + 1}`)}</strong></td>
              <td><strong style="color:var(--amber-gold);">${this.escapeHtml(m.capacityFormatted || (m.capacityGb ? `${m.capacityGb} GB` : '--'))}</strong></td>
              <td>${m.speedMhz || m.configuredClockSpeedMhz || '--'} MHz</td>
              <td><span class="pill-risk safe" style="font-size:10px;">${this.escapeHtml(m.memoryType || 'DDR4')}</span></td>
              <td>${this.escapeHtml(m.formFactor || 'DIMM')}</td>
              <td>${this.escapeHtml(m.manufacturer || 'Unknown')}</td>
              <td><code style="font-size:11px; color:#ffffff;">${this.escapeHtml(m.partNumber || 'N/A')}</code></td>
              <td><code style="font-size:11px; color:var(--text-muted);">${this.escapeHtml(m.serialNumber || 'N/A')}</code></td>
            </tr>
          `).join("");
        }
      }

      // 3. Physical Storage Disks Table
      const disksTbody = document.getElementById("hw-disks-tbody");
      if (disksTbody) {
        if (disks.length === 0) {
          disksTbody.innerHTML = `<tr><td colspan="5" style="text-align:center; padding:16px; color:var(--text-muted);">No physical disks detected.</td></tr>`;
        } else {
          disksTbody.innerHTML = disks.map(d => `
            <tr>
              <td><strong style="color:#ffffff;">${this.escapeHtml(d.model || 'Physical Disk')}</strong></td>
              <td><span style="font-family:var(--font-mono); font-size:11px;">${this.escapeHtml(d.interfaceType || 'NVMe / SATA')}</span></td>
              <td>${this.escapeHtml(d.mediaType || 'Fixed Hard Disk')}</td>
              <td><strong style="color:var(--amber-gold);">${this.escapeHtml(d.sizeFormatted || (d.sizeGb ? `${d.sizeGb} GB` : '--'))}</strong></td>
              <td><span class="pill-risk safe">${this.escapeHtml(d.status || 'OK')}</span></td>
            </tr>
          `).join("");
        }
      }

      // 4. Video Display Adapters (GPUs) Table
      const gpusTbody = document.getElementById("hw-gpus-tbody");
      if (gpusTbody) {
        if (gpus.length === 0) {
          gpusTbody.innerHTML = `<tr><td colspan="7" style="text-align:center; padding:16px; color:var(--text-muted);">No display adapters detected.</td></tr>`;
        } else {
          gpusTbody.innerHTML = gpus.map(g => `
            <tr>
              <td><strong style="color:#ffffff;">${this.escapeHtml(g.name || 'Video Controller')}</strong></td>
              <td>${this.escapeHtml(g.manufacturer || 'GPU')}</td>
              <td><code style="font-size:11px;">${this.escapeHtml(g.driverVersion || 'N/A')}</code></td>
              <td>${this.escapeHtml(g.driverDate || 'N/A')}</td>
              <td><strong style="color:var(--amber-gold);">${this.escapeHtml(g.memoryFormatted || (g.memoryMb ? `${g.memoryMb} MB` : 'Dedicated'))}</strong></td>
              <td>${this.escapeHtml(g.currentResolution || 'Display')} @ ${this.escapeHtml(g.currentRefreshRate || '60Hz')}</td>
              <td><span class="pill-risk safe">${this.escapeHtml(g.status || 'Active')}</span></td>
            </tr>
          `).join("");
        }
      }

      // 5. Peripherals & Connected Devices Table
      const periphTbody = document.getElementById("hw-peripherals-tbody");
      if (periphTbody) {
        const rows = [];
        const monitors = periph.monitors || periph.Monitors || [];
        monitors.forEach(m => {
          rows.push(`
            <tr>
              <td><span class="pill-risk safe" style="font-size:10px;">🖥️ Display / Monitor</span></td>
              <td><strong style="color:#ffffff;">${this.escapeHtml(m.name || 'Primary Display')}</strong></td>
              <td>${this.escapeHtml(m.resolution || '')} ${m.refreshRate ? `@ ${m.refreshRate}` : ''}</td>
              <td><span class="pill-risk safe">Active</span></td>
            </tr>
          `);
        });

        const audio = periph.audioDevices || periph.AudioDevices || [];
        audio.forEach(a => {
          rows.push(`
            <tr>
              <td><span class="pill-risk safe" style="font-size:10px;">🔊 Audio & Sound</span></td>
              <td><strong style="color:#ffffff;">${this.escapeHtml(a.name || 'Audio Device')}</strong></td>
              <td>${this.escapeHtml(a.manufacturer || 'HD Audio Driver')}</td>
              <td><span class="pill-risk safe">${this.escapeHtml(a.status || 'OK')}</span></td>
            </tr>
          `);
        });

        const inputs = periph.inputDevices || periph.InputDevices || [];
        inputs.forEach(inp => {
          rows.push(`
            <tr>
              <td><span class="pill-risk caution" style="font-size:10px;">⌨️ Input Peripheral</span></td>
              <td><strong style="color:#ffffff;">${this.escapeHtml(inp.name || 'Pointing / Input Device')}</strong></td>
              <td>${this.escapeHtml(inp.deviceType || 'USB / HID Device')}</td>
              <td><span class="pill-risk safe">${this.escapeHtml(inp.status || 'Connected')}</span></td>
            </tr>
          `);
        });

        const nics = periph.networkControllers || periph.NetworkControllers || [];
        nics.forEach(n => {
          rows.push(`
            <tr>
              <td><span class="pill-risk safe" style="font-size:10px;">🌐 Network Adapter</span></td>
              <td><strong style="color:#ffffff;">${this.escapeHtml(n.name || 'Network Interface')}</strong></td>
              <td>${this.escapeHtml(n.manufacturer || '')} ${n.speedFormatted ? `(${n.speedFormatted})` : ''}</td>
              <td><span class="pill-risk safe">${this.escapeHtml(n.status || 'Active')}</span></td>
            </tr>
          `);
        });

        if (rows.length === 0) {
          periphTbody.innerHTML = `<tr><td colspan="4" style="text-align:center; padding:16px; color:var(--text-muted);">No connected peripherals detected.</td></tr>`;
        } else {
          periphTbody.innerHTML = rows.join("");
        }
      }

      await this.loadBatteryInfo();
    } catch (err) { }
  }

  async loadBackups() {
    try {
      const changeSets = await this.api("/api/backups");
      const tbody = document.getElementById("backups-tbody");
      if (!tbody) return;

      tbody.innerHTML = "";
      if (!changeSets || changeSets.length === 0) {
        tbody.innerHTML = `<tr><td colspan="5" style="text-align:center; color:var(--text-muted);">No rollback checkpoints recorded yet.</td></tr>`;
        return;
      }

      changeSets.forEach(cs => {
        const tr = document.createElement("tr");
        tr.innerHTML = `
          <td><code style="color:#38bdf8;">${cs.id || cs.Id}</code></td>
          <td>${cs.timestamp || cs.Timestamp}</td>
          <td><strong style="color:#ffffff;">${this.escapeHtml(cs.description || cs.Description)}</strong></td>
          <td>${(cs.items || cs.Items || []).length} keys recorded</td>
          <td>
            <button class="btn-secondary" onclick="supApp.rollbackChangeSet('${cs.id || cs.Id}')">
              Rollback
            </button>
          </td>
        `;
        tbody.appendChild(tr);
      });
    } catch (err) { }
  }

  async createRestorePoint() {
    try {
      const res = await this.api("/api/backups/create_restore_point", "POST", { description: "Manual SUPOptimizer Snapshot" });
      if (res.Success || res.success) {
        this.showToast(res.Message || res.message, "success");
        await this.loadBackups();
      } else {
        this.showToast(`Error: ${res.Message || res.message}`, "error");
      }
    } catch (err) { }
  }

  async rollbackChangeSet(changeSetId) {
    try {
      const res = await this.api("/api/backups/rollback", "POST", { changeSetId });
      if (res.Success || res.success) {
        this.showToast(res.Message || res.message, "success");
        await this.loadTweaks();
        await this.loadBackups();
      } else {
        this.showToast(`Rollback Failed: ${res.Message || res.message}`, "error");
      }
    } catch (err) { }
  }

  // =========================================================================
  // HOST & SETTINGS
  // =========================================================================
  toggleSafeTestMode(enabled) {
    this.safeTestMode = enabled;
    localStorage.setItem("sup_safe_mode", enabled ? "true" : "false");
    this.showToast(`Safe Test Mode: ${enabled ? "ON (Simulated dry-runs)" : "OFF (Live registry changes)"}`, "info");
  }

  async clearAuditLogs() {
    try {
      await this.api("/api/logs/clear", "POST", {});
      this.showToast("Audit logs flushed.", "success");
    } catch (err) { }
  }

  initWindowControls() {
    const header = document.querySelector(".app-header");
    if (header) {
      header.addEventListener("mousedown", (e) => {
        if (e.target.closest("button, input, select, a, .telemetry-pill, .header-search-bar")) {
          return;
        }
        if (e.button === 0) {
          this.api("/api/host/drag", "POST", {}).catch(() => {});
        }
      });

      header.addEventListener("dblclick", (e) => {
        if (e.target.closest("button, input, select, a, .telemetry-pill, .header-search-bar")) {
          return;
        }
        this.toggleMaximize();
      });
    }

    window.addEventListener("resize", () => {
      this.updateMaximizeButtonIcon();
    });
    this.updateMaximizeButtonIcon();
  }

  updateMaximizeButtonIcon() {
    const btn = document.getElementById("btn-win-max");
    if (!btn) return;
    const isMax = (window.innerWidth >= screen.availWidth - 25 && window.innerHeight >= screen.availHeight - 25);
    btn.innerHTML = isMax ? "&#128471;" : "&#9634;";
    btn.title = isMax ? "Restore Window" : "Maximize";
  }

  async toggleMaximize() {
    try {
      await this.api("/api/host/maximize", "POST", {});
      setTimeout(() => this.updateMaximizeButtonIcon(), 150);
    } catch (err) { }
  }

  async requestElevation() {
    try {
      const res = await this.api("/api/host/elevate", "POST", {});
      if (res.alreadyAdmin) {
        this.showToast("Application is already running with full Administrator privileges.", "info");
        await this.fetchMetrics();
        return;
      }
      if (res.success || res.Success) {
        this.showToast("Requesting elevation via Windows UAC... SUPOptimizer will restart.", "info");
      } else {
        this.showToast("Elevation request cancelled or failed.", "warning");
      }
    } catch (err) {
      this.showToast("Elevation error: " + err.message, "error");
    }
  }

  async openExternalBrowser() {
    try {
      await this.api("/api/host/open-browser", "POST", {});
    } catch (err) { }
  }

  async minimizeToTray() {
    try {
      await this.api("/api/host/minimize", "POST", {});
    } catch (err) { }
  }

  closeWindow() {
    this.closeAllModals();
    const modal = document.getElementById("modal-close-confirm");
    if (modal) {
      modal.classList.add("active");
    } else {
      this.confirmExitApp();
    }
  }

  promptCloseAction() {
    this.closeWindow();
  }

  async confirmMinimizeToTray() {
    this.closeAllModals();
    await this.minimizeToTray();
  }

  async confirmExitApp() {
    this.closeAllModals();
    try {
      await this.api("/api/host/exit", "POST", {});
    } catch (err) { }
  }

  // =========================================================================
  // COMMAND PALETTE (Ctrl+K)
  // =========================================================================
  buildPaletteIndex() {
    this.paletteItems = [
      { title: "Dashboard Overview", category: "Navigation", action: () => this.switchTab("dashboard") },
      { title: "Optimize & Tweaks", category: "Navigation", action: () => this.switchTab("optimize") },
      { title: "Bloatware & Telemetry", category: "Navigation", action: () => this.switchTab("debloat") },
      { title: "Startup Applications", category: "Navigation", action: () => this.switchTab("startup") },
      { title: "Disk Cleaner", category: "Navigation", action: () => this.switchTab("storage") },
      { title: "App Store (Winget)", category: "Navigation", action: () => this.switchTab("installer") },
      { title: "DNS Switcher", category: "Network", action: () => this.switchTab("network") },
      { title: "HOSTS File Editor", category: "System Tools", action: () => { this.switchTab("systemtools"); this.switchSystemTool("hosts"); } },
      { title: "Environment Variables", category: "System Tools", action: () => { this.switchTab("systemtools"); this.switchSystemTool("env"); } },
      { title: "File Unlocker", category: "System Tools", action: () => { this.switchTab("systemtools"); this.switchSystemTool("unlock"); } },
      { title: "Run Dialog Aliases", category: "System Tools", action: () => { this.switchTab("systemtools"); this.switchSystemTool("aliases"); } },
      { title: "SFC /scannow Integrity", category: "Repair", action: () => { this.switchTab("repair"); this.runRepair("sfc_scannow"); } },
      { title: "DISM Restore Health", category: "Repair", action: () => { this.switchTab("repair"); this.runRepair("dism_restorehealth"); } },
      { title: "Fix Registry Issues", category: "Repair", action: () => { this.switchTab("repair"); this.runRepair("fix_registry_issues"); } },
      { title: "Apply All Safe Optimizations", category: "Action", action: () => this.applyAllSafeTweaks() },
      { title: "Purge All Recommended Bloat", category: "Action", action: () => this.removeRecommendedBloat() },
      { title: "Purge Standby RAM Cache", category: "Memory", action: () => this.purgeRamMemory() },
      { title: "Windows License & Activation", category: "System", action: () => { this.switchTab("systemtools"); this.switchSystemTool("license"); } },
      { title: "Winget Software Updates", category: "Installer", action: () => { this.switchTab("installer"); this.filterInstaller("Updates"); } },
      { title: "Generate Battery Diagnostic Report", category: "Power", action: () => this.openBatteryReport() }
    ];
  }

  openCommandPalette() {
    const modal = document.getElementById("modal-palette");
    const input = document.getElementById("palette-search-input");
    if (modal) modal.classList.add("active");
    if (input) {
      input.value = "";
      input.focus();
    }
    this.filterPalette();
  }

  filterPalette() {
    const query = (document.getElementById("palette-search-input")?.value || "").toLowerCase().trim();
    const list = document.getElementById("palette-results-list");
    if (!list) return;

    list.innerHTML = "";
    const filtered = this.paletteItems.filter(item => {
      if (!query) return true;
      return item.title.toLowerCase().includes(query) || item.category.toLowerCase().includes(query);
    });

    filtered.forEach(item => {
      const div = document.createElement("div");
      div.style.padding = "10px 14px";
      div.style.borderRadius = "6px";
      div.style.display = "flex";
      div.style.justifyContent = "space-between";
      div.style.cursor = "pointer";
      div.style.transition = "background 0.15s";
      div.innerHTML = `
        <span style="color:#ffffff; font-weight:600;">${item.title}</span>
        <span class="pill-risk safe" style="font-size:10px;">${item.category}</span>
      `;
      div.onmouseenter = () => div.style.background = "rgba(255,255,255,0.06)";
      div.onmouseleave = () => div.style.background = "transparent";
      div.onclick = () => {
        this.closeAllModals();
        item.action();
      };
      list.appendChild(div);
    });
  }

  // =========================================================================
  // MEMORY PURGE (STANDBY RAM)
  // =========================================================================
  async purgeRamMemory() {
    this.showToast("Purging standby cache & working sets...", "info");
    try {
      const res = await this.api("/api/system/memory/purge", "POST", {});
      if (res.Success || res.success) {
        this.showToast(`✅ ${res.Message || res.message}`, "success");
        await this.fetchMetrics();
      } else {
        this.showToast(`Memory Purge Error: ${res.Message || res.message}`, "error");
      }
    } catch (err) {
      this.showToast(`Error: ${err.message}`, "error");
    }
  }

  // =========================================================================
  // WINDOWS LICENSE & ACTIVATION STATUS
  // =========================================================================
  async loadLicenseInfo() {
    try {
      const info = await this.api("/api/system/license");
      if (!info) return;

      const isAct = !!(info.isActivated ?? info.IsActivated);
      const statusText = info.licenseStatus || info.LicenseStatus || "Unknown";
      const channel = info.channel || info.Channel || "Unknown";
      const key = info.partialKey || info.PartialKey || "N/A";
      const edition = info.edition || info.Edition || "Windows";
      const expiry = info.expirationDate || info.ExpirationDate || "Permanent";

      // Update Dashboard Baseline Table
      const dashLicense = document.getElementById("dash-spec-license");
      const dashBadge = document.getElementById("dash-spec-license-badge");
      if (dashLicense) {
        dashLicense.textContent = `${edition} (${channel}) - Key: *****-${key}`;
      }
      if (dashBadge) {
        dashBadge.textContent = isAct ? "Genuine Active" : statusText;
        dashBadge.className = isAct ? "pill-risk safe" : "pill-risk caution";
      }

      // Update System Tools License Panel
      const sBadge = document.getElementById("lic-status-badge");
      const sDesc = document.getElementById("lic-status-desc");
      const sChan = document.getElementById("lic-channel-badge");
      const sKey = document.getElementById("lic-partial-key");
      const sEd = document.getElementById("lic-edition");
      const sExp = document.getElementById("lic-expiry");

      if (sBadge) {
        sBadge.textContent = isAct ? "Genuine / Licensed" : statusText;
        sBadge.className = isAct ? "pill-risk safe" : "pill-risk caution";
      }
      if (sDesc) sDesc.textContent = info.description || info.Description || (isAct ? "Permanently activated via digital license or retail key." : statusText);
      if (sChan) sChan.textContent = channel;
      if (sKey) sKey.textContent = key !== "N/A" ? `***** - ${key}` : "N/A";
      if (sEd) sEd.textContent = edition;
      if (sExp) sExp.textContent = `Validity: ${expiry}`;
    } catch (err) { }
  }

  async openActivationSettings() {
    try {
      await this.api("/api/system/license/open-settings", "POST", {});
      this.showToast("Opening Windows Activation Settings...", "info");
    } catch (err) { }
  }

  // =========================================================================
  // BATTERY & POWER REPORT
  // =========================================================================
  async loadBatteryInfo() {
    try {
      const bat = await this.api("/api/system/power/battery");
      const container = document.getElementById("hw-battery-container");
      if (!bat || !container) return;

      const hasBat = !!(bat.hasBattery ?? bat.HasBattery);
      const bPercent = document.getElementById("hw-bat-percent");
      const bStatus = document.getElementById("hw-bat-status");
      const bHealth = document.getElementById("hw-bat-health");
      const bCap = document.getElementById("hw-bat-capacity");
      const bCycles = document.getElementById("hw-bat-cycles");
      const bPlan = document.getElementById("hw-bat-plan");

      if (hasBat) {
        container.style.display = "block";
        if (bPercent) bPercent.textContent = `${bat.batteryPercent ?? bat.BatteryPercent ?? 100}%`;
        if (bStatus) bStatus.textContent = bat.batteryStatus || bat.BatteryStatus || "Normal";
        if (bHealth) {
          const hVal = (bat.healthPercent ?? bat.HealthPercent ?? 100);
          bHealth.textContent = `${hVal}%`;
          bHealth.className = hVal >= 80 ? "pill-risk safe" : (hVal >= 60 ? "pill-risk caution" : "pill-risk");
        }
        if (bCap) {
          const des = bat.designCapacityMwh ?? bat.DesignCapacityMwh ?? 0;
          const full = bat.fullChargeCapacityMwh ?? bat.FullChargeCapacityMwh ?? 0;
          bCap.textContent = des > 0 ? `${full} / ${des} mWh` : "Standard Capacity";
        }
        if (bCycles) bCycles.textContent = String(bat.cycleCount ?? bat.CycleCount ?? 0);
        if (bPlan) bPlan.textContent = bat.activePowerPlan || bat.ActivePowerPlan || "Balanced";
      } else {
        // Desktop PC without battery
        if (bPercent) bPercent.textContent = "N/A (Desktop)";
        if (bStatus) bStatus.textContent = "Direct AC Wall Power";
        if (bHealth) bHealth.textContent = "100% Direct";
        if (bCap) bCap.textContent = "Continuous AC Power";
        if (bCycles) bCycles.textContent = "0 (Desktop)";
        if (bPlan) bPlan.textContent = bat.activePowerPlan || bat.ActivePowerPlan || "Balanced";
      }
    } catch (err) { }
  }

  async openBatteryReport() {
    this.showToast("Generating official Windows battery report...", "info");
    try {
      const res = await this.api("/api/system/power/battery/open-report", "POST", {});
      if (res.Success || res.success) {
        this.showToast("Battery report opened in web browser.", "success");
      } else {
        this.showToast(`Could not generate report: ${res.Message || res.message}`, "warning");
      }
    } catch (err) { }
  }

  closeAllModals() {
    document.querySelectorAll(".modal-backdrop").forEach(m => m.classList.remove("active"));
  }

  escapeHtml(str) {
    if (!str) return "";
    return String(str)
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;")
      .replace(/'/g, "&#039;");
  }

  escapeJs(str) {
    if (!str) return "";
    return String(str)
      .replace(/\\/g, "\\\\")
      .replace(/'/g, "\\'")
      .replace(/"/g, "\\\"");
  }
}

// Global App Instance
window.addEventListener("DOMContentLoaded", () => {
  window.supApp = new SUPApp();
});
