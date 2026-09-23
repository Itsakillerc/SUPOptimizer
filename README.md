# SUPOptimizer

<div align="center">
  <h1><span style="color:#00f2fe;font-weight:900;">SUPO</span><span style="color:#f0f6fc;font-weight:700;">ptimizer</span></h1>
  <p><strong>Suite Professionale Open-Source di Amministrazione, Ottimizzazione, Debloating e Manutenzione per Windows 10 & Windows 11</strong></p>
  <p><em>Eseguibile portatile singolo (Zero-Install), motore Web locale integrato, interfaccia ultra-moderna e tray host nativo.</em></p>

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

## Indice dei Contenuti

1. [Cos'è SUPOptimizer](#cosè-supoptimizer)
2. [Caratteristiche Distintive](#caratteristiche-distintive)
3. [Edizioni Disponibili (Standalone vs Lite)](#edizioni-disponibili-standalone-vs-lite)
4. [Requisiti e Avvio Rapido](#requisiti-e-avvio-rapido)
5. [Guida Dettagliata ai Moduli (19 Sezioni)](#guida-dettagliata-ai-moduli-19-sezioni)
   - [Cluster Core](#1-cluster-core)
   - [Cluster Windows](#2-cluster-windows)
   - [Cluster Performance](#3-cluster-performance)
   - [Cluster Maintenance](#4-cluster-maintenance)
   - [Cluster System](#5-cluster-system)
6. [Sicurezza, Reversibilità e Rollback](#sicurezza-reversibilità-e-rollback)
7. [Scorciatoie da Tastiera](#scorciatoie-da-tastiera)
8. [Compilazione e Build Pipeline](#compilazione-e-build-pipeline)
9. [Test e Suite di Verifica Automatica](#test-e-suite-di-verifica-automatica)
10. [FAQ e Risoluzione Problemi](#faq-e-risoluzione-problemi)
11. [Registro Versioni e Ultime Modifiche](#registro-versioni-e-ultime-modifiche)

---

## Cos'è SUPOptimizer

**SUPOptimizer** è una suite completa e all'avanguardia per l'amministrazione, la pulizia e il tuning avanzato dei sistemi operativi **Microsoft Windows 10 e Windows 11 (64-bit)**. 

Combina la potenza, la sicurezza e la reattività di un backend nativo in **C# / .NET 8** con un'interfaccia grafica moderna, elegante e reattiva ispirata ai design system di *Linear*, *Raycast* e *Apple macOS Sonoma*. 

L'applicazione è progettata per essere **completamente portabile**: non richiede installazione, non installa driver di terze parti a basso livello, non sporca il registro di sistema e può essere avviata direttamente da una chiavetta USB di assistenza tecnica.

---

## Caratteristiche Distintive

- **Interfaccia Grafica Dual-Engine**:
  - Finestra nativa accelerata via **Microsoft Edge WebView2**.
  - Tasto **"Web View"**: visualizzazione immediata della dashboard nel browser predefinito dell'utente (Chrome, Edge, Brave, Firefox) collegandosi all'endpoint locale `http://127.0.0.1:<porta>/`.
  - Funzionamento non bloccante: anche in ambienti privi del runtime WebView2 (ad esempio macchine virtuali minimali o ambienti Windows PE), l'app reindirizza automaticamente al browser web predefinito.
- **Tray Host Integrato**:
  - Riduzione discreta nella System Tray di Windows con menu contestuale per accesso rapido, gestione e chiusura.
- **Oltre 80 Tweak di Registro e Kernel**:
  - Personalizzazione dell'interfaccia utente, impostazioni di privacy, disattivazione telemetria, ottimizzazione di rete e latenza audio/video.
- **Debloater UWP Selettivo e Profilato**:
  - Rimozione di app preinstallate sponsorizzate e bloatware di sistema con preset guidati (*Safe*, *Balanced*, *Aggressive*).
- **Console di Riparazione e Diagnostica Live**:
  - Esecuzione trasparente e guidata di `SFC /scannow`, `DISM /RestoreHealth` e `CHKDSK` con streaming in tempo reale dei log a video.
- **Generatore Autounattend.xml**:
  - Creazione guidata del file di risposta XML per installazioni pulite e non presidiate di Windows 10 e 11, con bypass automatico dei requisiti TPM 2.0 / Secure Boot e configurazione di account locali offline.
- **Command Palette Globale (`Ctrl+K`)**:
  - Ricerca istantanea da tastiera per navigare tra i 19 moduli ed eseguire azioni rapide.

---

## Edizioni Disponibili (Standalone vs Lite)

La pipeline di compilazione automatizzata (`build-release.ps1`) produce due varianti portatili nella cartella `dist/`:

| Caratteristica | Standalone (`SUPOptimizer.exe`) | Lite (`SUPOptimizer-Lite.exe`) |
| :--- | :--- | :--- |
| **Dimensione** | ~68.9 MB | ~2.1 MB |
| **Runtime .NET 8** | **Embedded / Incluso** (Self-contained) | Richiede .NET 8 Desktop Runtime installato nel PC |
| **Installazione** | Nessuna (Zero-Install) | Nessuna (Zero-Install) |
| **Caso d'uso ideale** | PC puliti, formattazioni fresche, chiavette USB di supporto tecnico, VM offline | Download ultra-rapido per macchine con .NET 8 già presente |

---

## Requisiti e Avvio Rapido

### Requisiti di Sistema
- **Sistema Operativo**: Windows 10 (versione 1809 o superiore) oppure Windows 11 (tutte le edizioni, x64).
- **Privilegi**: È consigliato eseguire l'eseguibile con privilegi di amministratore (*Tasto destro > Esegui come amministratore*) per consentire la modifica di chiavi di registro di sistema (`HKLM`), servizi Windows e funzionalità DISM.

### Come Avviare l'Applicazione
1. Scarica `SUPOptimizer.exe` dalla cartella `dist/` o dai release ufficiali.
2. Fai doppio clic sul file per avviarlo.
3. Se desideri visualizzare la dashboard a tutto schermo nel tuo browser (es. Google Chrome o Microsoft Edge), clicca sul pulsante **"Web View"** in alto a destra nella barra dell'applicazione.
4. Per chiudere o ridurre l'applicazione, puoi usare i controlli finestra o fare clic destro sull'icona nella barra delle applicazioni (System Tray).

---

## Guida Dettagliata ai Moduli (19 Sezioni)

I moduli dell'applicazione sono raggruppati in 5 cluster logici nella barra laterale di navigazione:

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

### 1. Cluster Core

- **Dashboard**:
  - Monitoraggio in tempo reale con micro-aggiornamenti della percentuale di utilizzo CPU, consumo RAM (usata/totale), spazio disponibile su disco e tempo di accensione del sistema (Uptime).
  - **⚡ Purge RAM**: liberazione istantanea delle working set e della memoria in standby con calcolo in tempo reale dei MB recuperati.
  - Rilevamento automatico di ambienti virtualizzati (Hyper-V, VMware, VirtualBox, QEMU) e stato conformità licenza Windows.
  - Azioni rapide a 1 clic per manutenzione ordinaria e stato dei privilegi di amministratore.
- **Health Scan**:
  - Scansione diagnostica rapida a 6 punti: integrità disco, servizi critici, stato antivirus, integrità file di sistema, spazio su disco e stato ripristino.
  - Risultati classificati per severità visiva: *OK (Verde)*, *Info (Blu)*, *Attenzione (Giallo)*, *Critico (Rosso)*.
- **Audit Logs**:
  - Storico trasparente di ogni singola modifica apportata al sistema (valore precedente, valore applicato, timestamp e stato di successo).
- **Settings**:
  - Modalità sicura (Safe Mode / Dry Run per testare le operazioni senza scrivere su disco o registro).
  - Gestione token di autenticazione per le API locali.
  - Opzioni di comportamento alla chiusura della finestra (riduzione a icona nella Tray o chiusura completa).

---

### 2. Cluster Windows

- **Windows Tweaks**:
  - **Interfaccia Utente ed Esplora File**: visualizzazione estensioni dei file noti, visualizzazione file e cartelle nascosti, ripristino del menu contestuale classico di Windows 10 su Windows 11, lettere di unità prima del nome del disco, rimozione delle cartelle Home e Raccolte da Esplora File.
  - **Barra delle Applicazioni (Windows 11)**: opzione "Termina Attività" (End Task) col tasto destro sulle app aperte, allineamento a sinistra o al centro, commutazione rapida all'ultima finestra attiva (*LastActiveClick*).
  - **Comportamento di Sistema**: disattivazione messaggi di errore inviati a Microsoft, disattivazione suggerimenti nella schermata di blocco, disattivazione notifiche fastidiose e riavvio automatico forzato dopo gli aggiornamenti.
- **Privacy & Telemetry**:
  - Disattivazione del servizio di telemetria e diagnostica Microsoft (`DiagTrack`).
  - Disabilitazione dell'Advertising ID per la profilazione pubblicitaria.
  - Disattivazione di Cortana, Bing nella ricerca del menu Start e cronologia attività (Timeline).
  - Disabilitazione di Microsoft Copilot AI, Windows Recall (istantanee schermo) e funzionalità AI intrusive in Blocco Note e Paint.
- **Debloater (UWP AppX)**:
  - Catalogo curato di 38 pacchetti preinstallati spesso non necessari (app sponsorizzate, giochi promozionali, utility duplicate).
  - Filtri per preset: **Safe** (app promozionali sicure da rimuovere), **Balanced** e **Aggressive**.
  - Possibilità di disinstallazione profonda per Microsoft OneDrive e Microsoft Edge.
- **Apps Manager**:
  - Elenco completo dei programmi desktop (Win32 / x64) installati nel sistema.
  - Ricerca rapida, ispezione del percorso di installazione e avvio del disinstallatore ufficiale con 1 clic.

---

### 3. Cluster Performance

- **System Optimizer**:
  - Ottimizzazione dello scheduling multimediale (`MMCSS`) e disattivazione del throttling di rete per lo streaming e i download ad alta velocità.
  - Disattivazione del timestamp dell'ultimo accesso NTFS (`NtfsDisableLastAccessUpdate`) per ridurre le scritture inutili su SSD/NVMe.
  - Attivazione del supporto ai percorsi lunghi oltre 260 caratteri (*Win32 Long Paths*).
- **Gaming Mode**:
  - Attivazione della modalità gioco nativa di Windows.
  - Disattivazione di Game DVR e della registrazione video in background per liberare cicli GPU.
  - Disattivazione dell'accelerazione del puntatore del mouse (Enhanced Pointer Precision) per garantire un puntamento 1:1 raw input nei videogiochi.
  - Disattivazione del Multiplane Overlay (MPO) per prevenire micro-stuttering e sfarfallii su GPU NVIDIA e AMD.
- **Network Engine**:
  - Ispezione completa degli adattatori di rete attivi, indirizzi IP locali, Gateway e DNS configurati.
  - Strumenti rapidi a 1 clic: **Flush DNS**, **Reset Winsock**, **Reset Stack TCP/IP**.
  - Preset DNS veloci e sicuri applicabili istantaneamente: *Cloudflare (1.1.1.1)*, *Google (8.8.8.8)*, *Quad9 (9.9.9.9)* e ripristino *DHCP automatico*.
  - Strumento integrato di Ping diagnostico con statistiche di latenza.

---

### 4. Cluster Maintenance

- **Storage Cleaner**:
  - Scansione e pulizia sicura di 12 categorie di file temporanei e inutilizzati:
    - File temporanei utente (`%TEMP%`) e di sistema (`C:\Windows\Temp`).
    - Cache delle anteprime di Esplora Risorse (Thumbnails).
    - Cache e dati temporanei dei browser Chromium (Google Chrome, Microsoft Edge, Brave).
    - File di log vecchi, dump di crash di Windows Error Reporting (WER) e log del setup di Windows.
  - Modalità Anteprima per visualizzare lo spazio recuperabile prima di procedere con l'eliminazione.
- **Repair Center**:
  - Console interattiva guidata con output in streaming in tempo reale per gli strumenti di manutenzione di Windows:
    - **Controllo File di Sistema**: `sfc /scannow` per verificare e ripristinare file di sistema corrotti.
    - **Manutenzione Immagine Windows**: `dism /online /cleanup-image /restorehealth` per riparare il component store di Windows.
    - **Controllo Integrità Disco**: `chkdsk` in modalità lettura o pianificazione per il riavvio successivo.
    - **Reset Servizi Windows Update**: arresto, pulizia cartella `SoftwareDistribution` e riavvio dei servizi di aggiornamento.
- **Backup & Restore**:
  - Creazione immediata di un **Punto di Ripristino del Sistema (VSS - Volume Shadow Copy)** prima di applicare modifiche massive.
  - Gestione dei **ChangeSet**: ogni tweak modificato registra lo stato precedente in formato JSON (`data/snapshots/`), permettendo il rollback mirato a uno stato precedente.

---

### 5. Cluster System

- **Services Manager**:
  - Elenco completo dei servizi Windows con filtro di ricerca per nome o stato.
  - Possibilità di avviare, arrestare, riavviare e modificare la modalità di avvio (Automatico, Manuale, Disabilitato).
- **Startup Manager**:
  - Gestione trasparente dei programmi eseguiti all'avvio del computer.
  - Ispezione delle chiavi di registro `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`, `HKLM\...` e delle cartelle Startup del menu Start, con possibilità di disabilitazione senza cancellazione distruttiva.
- **App Installer**:
  - Catalogo curato di software open-source ed essenziale (7-Zip, Notepad++, Git, VLC, Visual Studio Code, Firefox, Chrome, Brave, Revo Uninstaller).
  - Installazione automatica e silenziosa basata su package manager nativo (senza adware o barre degli strumenti indesiderate).
  - **⚡ Winget Software Updates**: scansione rapida dei programmi installati nel PC con aggiornamenti disponibili e pulsante per l'aggiornamento massivo a 1 clic.
- **Hardware Specs**:
  - Profilazione sub-millisecondo dell'hardware del computer: processore (modello, core, thread), scheda madre, produttore e versione BIOS, RAM fisica installata, schede video (GPU) e volumi di archiviazione.
  - **🔋 Diagnostica Batteria**: rilevamento salute e livello di usura della batteria, cicli di carica, piano energetico attivo e generazione del report HTML ufficiale (`powercfg /batteryreport`).
- **Security Overview**:
  - Monitoraggio dello stato in tempo reale di Microsoft Defender (servizio attivo, protezione in tempo reale attiva).
  - Stato dei profili del Firewall di Windows (Dominio, Privato, Pubblico).
  - Livello di notifica del Controllo dell'Account Utente (UAC).
- **Windows Features**:
  - Interrogazione e attivazione/disattivazione dei componenti opzionali di sistema via DISM:
    - Windows Sandbox
    - Windows Subsystem for Linux (WSL)
    - Hyper-V e Piattaforma Macchina Virtuale
    - Componenti legacy come DirectPlay e Client Telnet.
- **Setup Profiles**:
  - Salvataggio, esportazione e importazione in formato JSON di profili completi di configurazione del PC.
  - Preset inclusi: *Gaming*, *Privacy Focus*, *Minimal*, *Standard Workstation*.
- **Unattended ISO Generator**:
  - Generatore guidato del file `autounattend.xml` da posizionare nella radice di una chiavetta USB di installazione di Windows 10/11.
  - Include opzioni per:
    - Bypass automatico dei requisiti TPM 2.0, Secure Boot e RAM minima (`LabConfig`).
    - Bypass della richiesta obbligatoria di account Microsoft (`BypassNRO`) per creare subito un account locale.
    - Impostazione del fuso orario, lingua, nome utente e auto-logon opzionale.
- **System Tools**:
  - **Editor del File HOSTS**: visualizzazione, modifica sicura e pulsante rapido per bloccare i server di telemetria Microsoft e tracking con 1 clic.
  - **Gestore Porte & Connessioni Socket**: ispettore live delle connessioni TCP/UDP aperte, indirizzi remoti e PID dei processi collegati.
  - **Variabili d'Ambiente**: visualizzazione e modifica rapida delle variabili utente e di sistema.
  - **Alias di Esecuzione (Run Aliases)**: gestione dei comandi rapidi eseguibili dalla finestra `Win + R`.
  - **Centro Ufficiale Licenza & Attivazione**: interrogazione sicura tramite WMI `SoftwareLicensingProduct` per verificare lo stato di attivazione genuina di Windows, canale di licenza (Retail, OEM, Volume KMS), codice Product Key parziale e collegamento diretto alle Impostazioni di Sistema.

---

## Sicurezza, Reversibilità e Rollback

SUPOptimizer è stato sviluppato seguendo rigorosi standard di sicurezza per evitare danni accidentali al sistema operativo:

1. **Nessuna Esecuzione Arbitraria di Shell dal Frontend**:
   - L'interfaccia HTML/JS non ha facoltà di iniettare comandi shell arbitrari. Ogni operazione passa attraverso servizi C# fortemente tipizzati e validati.
2. **Token di Sessione Effimero (`X-SUP-Token`)**:
   - Tutte le richieste REST inviate al server locale `127.0.0.1` sono autenticate da un token crittografico generato casualmente ad ogni avvio dell'applicazione.
3. **Modalità Anteprima (Dry-Run)**:
   - È possibile ispezionare le chiavi di registro esatte, i percorsi e i valori proposti prima di confermare qualsiasi modifica.
4. **Punti di Ripristino e Snapshot JSON**:
   - Prima di modifiche estese, SUPOptimizer può creare un punto di ripristino del sistema (*System Restore Point*) e conserva i file snapshot di rollback nella sottocartella `data/snapshots/`.

---

## Scorciatoie da Tastiera

| Tasto / Scorciatoia | Funzione |
| :--- | :--- |
| `Ctrl + K` | Apre la **Command Palette** per cercare rapidamente moduli, tweak e impostazioni |
| `Esc` | Chiude modali attive, finestre di dialogo o la Command Palette |
| `F5` / `Ctrl + R` | Ricarica la vista corrente e aggiorna la telemetria |
| `Alt + ←` | Torna alla schermata precedente nella cronologia di navigazione |
| `Alt + →` | Avanza alla schermata successiva nella cronologia di navigazione |

---

## Compilazione e Build Pipeline

Il progetto utilizza .NET 8 SDK e uno script PowerShell automatizzato che esegue pulizia, compilazione, ottimizzazione e pubblicazione a singolo file.

Per compilare entrambe le edizioni (*Standalone* e *Lite*):

```powershell
# Dalla cartella principale del repository:
powershell -ExecutionPolicy Bypass -File .\build-release.ps1
```

Gli eseguibili compilati saranno generati nella directory `dist/`:
- `dist\SUPOptimizer.exe` (Standalone, ~68.9 MB)
- `dist\SUPOptimizer-Lite.exe` (Lite, ~2.1 MB)

---

## Test e Suite di Verifica Automatica

È disponibile una suite completa di test end-to-end (`test-verification.ps1`) che valida automaticamente oltre 20 aspetti funzionali del binario compilato (avvio headless, binding porte, token di sicurezza, API REST, moduli DISM, generazione XML, pulizia e shutdown ordinato).

Per eseguire i test:

```powershell
# Esegui la verifica per l'edizione Standalone:
powershell -ExecutionPolicy Bypass -File .\test-verification.ps1 -Edition Standalone

# Esegui la verifica per l'edizione Lite:
powershell -ExecutionPolicy Bypass -File .\test-verification.ps1 -Edition Lite
```

---

## FAQ e Risoluzione Problemi

### L'antivirus segnala il file come sospetto?
Alcuni motori antivirus applicano rilevamenti euristici generici sui file eseguibili appena compilati a singolo file o su software che modificano chiavi di registro di sistema (come le impostazioni di telemetria). Il codice sorgente di SUPOptimizer è 100% trasparente, privo di payload malevoli e compilabile direttamente dal sorgente.

### Come posso ripristinare un tweak che ho applicato?
Nella sezione **Maintenance > Backup & Restore** puoi visualizzare l'elenco dei ChangeSet precedenti e ripristinare i valori originali con un solo clic. In alternativa, puoi utilizzare i Punti di Ripristino del Sistema di Windows creati prima delle modifiche.

### Posso utilizzare l'app su più computer da una chiavetta USB?
Sì. L'edizione **Standalone** (`SUPOptimizer.exe`) contiene l'intero runtime all'interno del singolo file `.exe`: basta copiare il file su una chiavetta USB e avviarlo su qualsiasi computer con Windows 10 o Windows 11.

---

## Registro Versioni e Ultime Modifiche

### Versione Attuale: `1.0.1` (Rilascio Ufficiale)
*Data di rilascio: Settembre 2026*

#### Note di Rilascio & Nuove Funzionalità (v1.0.1)
- **🪪 Centro Ufficiale Licenza & Attivazione Windows**:
  - Integrazione dell'interrogazione nativa WMI/CIM tramite la classe di sistema `SoftwareLicensingProduct` (senza script esterni invasivi o rischi di sicurezza).
  - Rilevamento in tempo reale dello stato genuino (*Licensed / Permanent*, *Grace Period*, *Unlicensed*), del canale di distribuzione (*Retail*, *OEM:DM*, *Volume KMS/MAK*), degli ultimi 5 caratteri del Product Key attivo (`PartialProductKey`) e dell'edizione di Windows.
  - Pulsante a 1 clic per aprire direttamente la schermata ufficiale delle Impostazioni di Windows (`ms-settings:activation`).
- **⚡ Purger Standby Memory / Cache RAM**:
  - Nuovo modulo C# ad alte prestazioni basato sulle API Win32 `EmptyWorkingSet` e `GlobalMemoryStatusEx` per svuotare le working set e la memoria in standby non essenziale.
  - Pulsante rapido integrato nella card telemetria RAM della Dashboard con notifica toast istantanea e conteggio in tempo reale dei megabyte di RAM recuperati (oltre 600 MB liberati nei test).
- **📦 Winget Bulk Package Upgrader**:
  - Integrazione dello scanner di aggiornamenti software basato sul motore nativo `winget upgrade`.
  - Nuova scheda *"⚡ Software Updates"* nella sezione App Store / Installer con visualizzazione tabellare (Versione Attuale vs Nuova Versione Disponibile) e pulsante per l'aggiornamento massivo a 1 clic.
- **🔋 Diagnostica Integrità Batteria & Piani Energetici**:
  - Ispezione completa dell'hardware di alimentazione tramite `Win32_Battery` e `root\wmi` (capacità di fabbrica vs capacità massima attuale, percentuale di usura della batteria, cicli di carica totali e profilo energetico attivo).
  - Generazione ed apertura automatica nel browser del report diagnostico HTML ufficiale di Windows (`powercfg /batteryreport`).
- **📚 Documentazione & User Guide Completa**:
  - Riscrittura integrale del file `README.md` con guida esaustiva e accessibile per tutti i 19 moduli della suite, istruzioni passo-passo per l'utente, guida alla compilazione automatizzata e FAQ di sicurezza.
  - Aggiornamento della versione e del titolo dell'applicazione su interfaccia grafica, finestra WebView2 e menu contestuale della tray icon (`SUPOptimizer v1.0.1`).

---

<div align="center">
  <sub>SUPOptimizer v1.0.1 — Sviluppato con passione per la trasparenza, la privacy e le prestazioni di Windows.</sub>
</div>
