# SUPOptimizer v1.0.2 — Elenco Completo e Dettagliato delle Feature

Documento ufficiale contenente la catalogazione esaustiva, passo dopo passo e modulo per modulo, di **TUTTE** le funzionalità, strumenti, tweak, servizi e componenti architetturali integrati nella suite **SUPOptimizer v1.0.2**.

---

## Indice Generale

1. [Architettura Generale & Host Portatile](#1-architettura-generale--host-portatile)
2. [Header, Barra Superiore & Telemetria Live](#2-header-barra-superiore--telemetria-live)
3. [Dashboard Overview & Centro Prestazioni](#3-dashboard-overview--centro-prestazioni)
4. [Scansione Salute di Sistema (Health Scan)](#4-scansione-salute-di-sistema-health-scan)
5. [Ottimizzazioni & Tweak di Sistema (Catalogo degli 80 Tweak)](#5-ottimizzazioni--tweak-di-sistema-catalogo-degli-80-tweak)
   - [5.1 Privacy & Hardening Telemetria (19 Tweak)](#51-privacy--hardening-telemetria-19-tweak)
   - [5.2 Performance, CPU & Gaming (23 Tweak)](#52-performance-cpu--gaming-23-tweak)
   - [5.3 Windows Shell & Esplora File (30 Tweak)](#53-windows-shell--esplora-file-30-tweak)
   - [5.4 Sistema & Avanzate (8 Tweak)](#54-sistema--avanzate-8-tweak)
6. [Rimozione Bloatware & App Preinstallate (UWP Debloater)](#6-rimozione-bloatware--app-preinstallate-uwp-debloater)
7. [Gestione Applicazioni all'Avvio (Startup Manager)](#7-gestione-applicazioni-allavvio-startup-manager)
8. [Pulizia Disco & File Temporanei (Disk Cleanup)](#8-pulizia-disco--file-temporanei-disk-cleanup)
9. [App Store & Aggiornamenti Software (Winget Package Manager)](#9-app-store--aggiornamenti-software-winget-package-manager)
10. [Rete, Connessioni & DNS Switcher (Network Engine)](#10-rete-connessioni--dns-switcher-network-engine)
11. [Strumenti di Sistema Nativi (System Tools)](#11-strumenti-di-sistema-nativi-system-tools)
    - [11.1 Editor File HOSTS con Blocchi Integrati](#111-editor-file-hosts-con-blocchi-integrati)
    - [11.2 Gestore Variabili d'Ambiente (Utente & Sistema)](#112-gestore-variabili-dambiente-utente--sistema)
    - [11.3 File Unlocker (Windows Restart Manager API)](#113-file-unlocker-windows-restart-manager-api)
    - [11.4 Gestore Alias Esegui (Win+R App Paths)](#114-gestore-alias-esegui-winr-app-paths)
    - [11.5 Centro Licenza & Attivazione Windows Ufficiale](#115-centro-licenza--attivazione-windows-ufficiale)
12. [Sicurezza, Manutenzione & Riparazioni (Repair Center)](#12-sicurezza-manutenzione--riparazioni-repair-center)
13. [Specifiche Hardware & Diagnostica Periferiche (Hardware Specs)](#13-specifiche-hardware--diagnostica-periferiche-hardware-specs)
    - [13.1 Componenti Principali (CPU, RAM, GPU, Scheda Madre, BIOS)](#131-componenti-principali-cpu-ram-gpu-scheda-madre-bios)
    - [13.2 Dischi di Archiviazione & Volumi](#132-dischi-di-archiviazione--volumi)
    - [13.3 Periferiche & Dispositivi Collegati](#133-periferiche--dispositivi-collegati)
    - [13.4 Diagnostica Batteria & Piani di Alimentazione](#134-diagnostica-batteria--piani-di-alimentazione)
14. [Funzionalità Windows Opzionali & Virtualizzazione (DISM)](#14-funzionalità-windows-opzionali--virtualizzazione-dism)
15. [Profili di Configurazione & Preset (Setup Profiles)](#15-profili-di-configurazione--preset-setup-profiles)
16. [Generatore File di Risposta Installazione ISO (Autounattend.xml)](#16-generatore-file-di-risposta-installazione-iso-autounattendxml)
17. [Punti di Ripristino, ChangeSet & Audit Logs](#17-punti-di-ripristino-changeset--audit-logs)
18. [Impostazioni & Preferenze Piattaforma](#18-impostazioni--preferenze-piattaforma)
19. [Command Palette Globale (Ctrl+K) & Interazione da Tastiera](#19-command-palette-globale-ctrlk--interazione-da-tastiera)
20. [Modalità Silent, Interfaccia CLI & Parametri di Avvio](#20-modalità-silent-interfaccia-cli--parametri-di-avvio)

---

## 1. Architettura Generale & Host Portatile

- **Eseguibile Singolo Portatile (Zero-Install)**: l'intera applicazione viene fornita come singolo binario eseguibile `.exe` pronto all'uso, senza wizard di installazione, senza dipendenze esterne obbligatorie e senza inquinamento del registro di sistema.
- **Due Varianti di Compilazione**:
  - `SUPOptimizer.exe` (Standalone, ~68.9 MB): include al proprio interno l'intero runtime .NET 8 (Self-contained), avviabile su qualsiasi PC Windows 10/11 pulito o chiavetta USB.
  - `SUPOptimizer-Lite.exe` (Lite, ~2.1 MB): binario ultra-compatto che sfrutta il .NET 8 Desktop Runtime già presente sul computer.
- **Host Grafico a Doppio Motore (Dual-Engine Interface)**:
  - *Motore Principale*: Finestra nativa C# WinForms con WebView2 accelerato via hardware, tema scuro immersivo DWM (`DwmSetWindowAttribute`) e supporto nativo ad Aero Snap e ancoraggio ai bordi dello schermo.
  - *Fallback Automatico*: In ambienti privi del runtime WebView2 (come ambienti di ripristino WinPE o macchine virtuali minime), l'applicazione si avvia automaticamente aprendo l'interfaccia nel browser web predefinito di sistema.
- **Pulsante Web View**: possibilità in qualunque momento di aprire la dashboard nel proprio browser preferito (Google Chrome, Microsoft Edge, Brave, Mozilla Firefox) collegandosi all'endpoint locale `http://127.0.0.1:<port>/`.
- **Server Web REST Locale Asincrono (`LocalApiServer.cs`)**:
  - Web server HTTP interno basato su `HttpListener` multithread.
  - Selezione automatica di una porta TCP libera o specificata tramite parametro.
  - Servizio di file statici incorporati (Embedded WebAssets: HTML, CSS, JS, SVG) con routing SPA.
  - **Token di Sessione Effimero (`X-SUP-Token`)**: generato in modo crittografico casuale ad ogni avvio dell'applicazione. Tutte le richieste REST inviate a `127.0.0.1` richiedono la presenza e la corrispondenza del token nell'header HTTP per prevenire accessi non autorizzati da siti web esterni.
- **Garanzia di Istanza Singola (Global Mutex)**:
  - Utilizzo di un Mutex di sistema a livello globale (`Global\SUPOptimizer_SingleInstance_Mutex`) per prevenire conflitti o esecuzioni multiple concorrenti.
- **Risoluzione Intelligente della Cartella Dati**:
  - Identificazione automatica dell'ambiente: se l'eseguibile risiede su un supporto scrivibile (come una chiavetta USB di un tecnico IT), salva snapshot, backup e log nella cartella locale `data/`.
  - Fallback automatico su `%LOCALAPPDATA%\SUPOptimizer` qualora la cartella dell'eseguibile sia di sola lettura (es. `Program Files`).
- **Integrazione Windows System Tray (`TrayManager.cs`)**:
  - Minimizzazione discreta nell'area di notifica di Windows (System Tray).
  - Icona dinamica e menu contestuale con comandi rapidi:
    - *Open SUPOptimizer*: ripristina e porta in primo piano la finestra principale.
    - *Open in Web Browser (Chrome / Edge)*: apre l'URL locale nel browser predefinito.
    - *Dashboard*: navigazione diretta al pannello principale.
    - *Quick Optimize (Safe Profile)*: esecuzione immediata del profilo di ottimizzazione sicuro con notifica a fumetto (balloon toast) del numero di tweak applicati.
    - *System Health Scan*: scorciatoia diretta alla scansione diagnostica.
    - *Settings & Backups*: accesso alle preferenze e ai punti di ripristino.
    - *Restart Application*: riavvio pulito del processo.
    - *Exit SUPOptimizer*: chiusura ordinata di finestra, server API e thread in background.

---

## 2. Header, Barra Superiore & Telemetria Live

- **Frecce di Navigazione della Cronologia (History Stack)**: pulsanti freccia (Indietro / Avanti) che consentono di navigare tra le varie viste visitate precedentemente, con storico mantenuto in memoria.
- **Titolo Dinamico della Sezione (Breadcrumb)**: aggiornamento in tempo reale del nome e del cluster della vista attualmente attiva.
- **Badge Telemetrico CPU**: percentuale di utilizzo istantanea della CPU aggiornata ogni 2,5 secondi con animazione a impulso verde.
- **Badge Telemetrico RAM**: indicatore in tempo reale della memoria RAM occupata rispetto alla RAM totale installata nel computer (es. `4.2 / 15.9 GB`).
- **Badge di Sicurezza & Elevazione UAC**:
  - Rileva in tempo reale se il processo viene eseguito con privilegi di Amministratore (`ELEVATED (ADMIN)`) o Utente Standard (`STANDARD USER`).
  - Cliccando sul badge quando non elevato, viene lanciata una richiesta interattiva UAC per riavviare istantaneamente l'app con privilegi amministrativi completi (`/api/host/elevate`).
- **Pulsante "Web View"**: apre la sessione del server locale nel browser web di sistema predefinito.
- **Pulsante Command Palette (`Ctrl+K`)**: campo di ricerca visibile nella barra superiore per accedere rapidamente a tutti gli 80 tweak e ai comandi della suite.
- **Controlli Finestra Personalizzati (Window Controls)**:
  - *Riduci a icona*: invia la finestra al System Tray.
  - *Ingrandisci / Ripristina*: commuta la finestra nativa tra schermo intero e dimensione standard.
  - *Chiudi*: apre un dialog personalizzato che consente all'utente di scegliere tra la minimizzazione nel System Tray per continuare a ottimizzare in background o la chiusura totale dell'applic---

## 3. Dashboard Overview & Centro Prestazioni

- **Banner di Comando Futuristico con Effetto Aurora & Azioni Rapide**:
  - Grafica immersiva con gradiente ad aura radiale e badge di stato "Native System Engineering Engine".
  - Fila di azioni rapide a un clic:
    - ⚡ **1-Click Safe Boost**: esecuzione istantanea delle 6 ottimizzazioni non invasive con log in tempo reale.
    - ⚡ **Purge Standby RAM**: svuotamento immediato del working set e della cache di standby con report dei MB liberati.
    - 🛡️ **Full Health Scan**: passaggio immediato alla diagnostica approfondita a 7 checkpoint.
    - 🧹 **Clean Disk**: scorciatoia diretta al modulo di pulizia file temporanei.
- **Quattro Indicatori Tachimetrici Radiali SVG (Circular Radial Gauges)**:
  1. *Processor Load*: cerchio SVG con animazione continua di stroke glow, percentuale istantanea, badge di carico (*Normal*, *Moderate*, *Heavy Load*), modello esatto del processore e mini-track graduata.
  2. *Memory Allocation*: indicatore tachimetrico circolare con memoria usata in %, pulsante rapido `⚡ Purge`, dettaglio esatto dei MB usati / liberi / totali e mini-barra colorata.
  3. *System Storage (C:)*: indicatore tachimetrico circolare con percentuale di spazio occupato, badge di salute dell'unità, gigabyte liberi e mini-track.
  4. *System Health Score*: indicatore tachimetrico circolare con punteggio sintetico da 0 a 100, badge di stato dinamico (*Optimal*, *Notice*, *Attention*), uptime del computer e interazione click per aprire la vista diagnostica completa.
- **Griglia Volumi di Archiviazione Multi-Disco (Multi-Disk Storage Volumes)**:
  - Rileva e mostra schede dettagliate per tutte le partizioni e unità montate (C:, D:, E:, unità USB, dischi esterni).
  - Per ciascuna unità mostra: lettera del disco, etichetta di volume, file system (NTFS, FAT32, exFAT), barra di riempimento, gigabyte occupati, liberi e totali, con evidenziazione speciale dell'unità di sistema.
- **Tabella Baseline Specifiche Hardware & Licenza**:
  - *Operating System*: versione esatta di Windows (Windows 10 / Windows 11), numero di build e architettura del sistema (x64).
  - *Windows License & Activation*: verifica genuinità licenza con badge interattivo.
  - *Central Processing Unit*: nome commerciale e architettura della CPU.
  - *Dedicated Graphics*: modello scheda video GPU e livello supporto DirectX.
  - *Motherboard & BIOS*: produttore scheda madre e modalità BIOS (UEFI / Secure Boot).

---

## 4. Scansione Salute di Sistema (Health Scan)

- **Pannello Diagnostico Completo & Dedicato (`#tab-health`)**:
  - Vista completa accessibile dalla barra laterale (categoria *Overview*) e dal System Tray.
  - Grande indicatore radiale circolare da 120px con punteggio ponderato 0-100, animazione fluida del tratto e colorazione dinamica (verde per >=90, ambra per >=75, rosso per <75).
  - Badge di stato complessivo (*System Fully Optimized*, *Attention Needed*, *Critical Remediation*).
  - Contatori sintetici a chip per severità: *Critical*, *Warnings*, *Attention*, *Passed*.
  - Griglia interattiva dei riscontri con filtro rapido (*All* vs *Issues Only*).
  - Pulsante **⚡ Optimize Now** diretto su ogni anomalia per applicare all'istante il tweak risolutivo senza doverlo cercare manualmente nel catalogo.
  - Pulsante **Optimize All Recommended** per correggere in sequenza automatizzata tutte le anomalie riscontrate.
- **Diagnostica ad Alta Precisione a 7 Checkpoint (`HealthScanService.cs`)**:
  1. *Stato Telemetria Diagnostica*: verifica se la trasmissione di diagnostica e telemetria a Microsoft è attiva o ridotta al minimo.
  2. *ID Pubblicitario*: controlla se le app possono profilare l'utente tramite identificatore di advertising.
  3. *Network Throttling Index*: controlla se Windows sta limitando il throughput di rete non multimediale.
  4. *Game DVR in Background*: verifica se l'encoder della GPU registra continuamente sessioni di gioco.
  5. *Spazio Libero sull'Unità di Sistema C:*: segnala avvisi se lo spazio libero scende sotto il 20% e avvisi critici se scende sotto il 10%.
  6. *Accumulo Cartella Temporanea (%TEMP%)*: controlla se i file temporanei accumulati superano i 500 MB.
  7. *Controllo Controllo Account Utente (UAC)*: controlla se l'isolamento dei privilegi UAC è attivo e sicuro.erlo istantaneamente.

---

## 5. Ottimizzazioni & Tweak di Sistema (Catalogo degli 80 Tweak)

L'applicazione include esattamente **80 tweak** nativi di registro e kernel (`TweakRegistry.cs`), suddivisi in 4 macro-gruppi. Ogni tweak dispone di stato attuale ON/OFF, nome, descrizione, dettagli tecnici esatti del registro, livello di rischio (*Safe*, *Low*, *Caution*), indicatore di riavvio richiesto e pulsante di ispezione tecnica (`< / >`).

### 5.1 Privacy & Hardening Telemetria (19 Tweak)

1. **Disable Windows Diagnostic Data & Telemetry (`privacy_telemetry`)**: imposta `AllowTelemetry` a 0 in `HKLM\SOFTWARE\Policies\Microsoft\Windows\DataCollection`.
2. **Disable Advertising ID for Tailored Ads (`privacy_advertising_id`)**: disattiva l'ID pubblicitario impostando `Enabled` a 0 in `HKCU\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo`.
3. **Disable Timeline Activity History (`privacy_activity_history`)**: blocca la registrazione delle attività su Timeline impostando `EnableActivityFeed` a 0 in `HKLM\SOFTWARE\Policies\Microsoft\Windows\System`.
4. **Disable Windows Start & Settings Suggestions (`privacy_suggestions`)**: blocca app raccomandate e sponsorizzate impostando `SystemPaneSuggestionsEnabled` a 0 in `HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager`.
5. **Disable Bing Web Search in Start Menu (`privacy_bing_search`)**: limita la ricerca di Start ai file e app locali impostando `DisableSearchBoxSuggestions` a 1 in `HKCU\Software\Policies\Microsoft\Windows\Explorer`.
6. **Disable Windows Feedback Prompts (`privacy_feedback`)**: azzera i sondaggi e popup di feedback impostando `NumberOfSIUFInPeriod` a 0 in `HKCU\Software\Microsoft\Siuf\Rules`.
7. **Disable Windows Device Location Tracking (`privacy_location`)**: blocca la localizzazione geografica hardware impostando `DisableLocation` a 1 in `HKLM\SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors`.
8. **Disable Windows Copilot AI Integration (`privacy_copilot`)**: disattiva l'integrazione di Copilot AI in Windows 11 impostando `TurnOffWindowsCopilot` a 1 in `HKCU\Software\Policies\Microsoft\Windows\WindowsCopilot`.
9. **Disable Cortana Voice Assistant & Background Indexing (`privacy_cortana`)**: disattiva l'ascolto vocale di Cortana impostando `AllowCortana` a 0 in `HKLM\SOFTWARE\Policies\Microsoft\Windows\Windows Search`.
10. **Disable Windows App Launch Tracking (`privacy_app_launch_tracking`)**: impedisce a Windows di tracciare la frequenza di apertura delle app impostando `Start_TrackProgs` a 0 in `HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced`.
11. **Disable Windows 'Find My Device' Background Tracking (`privacy_find_my_device`)**: blocca la trasmissione delle coordinate al cloud impostando `AllowFindMyDevice` a 0 in `HKLM\SOFTWARE\Policies\Microsoft\FindMyDevice`.
12. **Disable Windows App Location Access Sensor (`privacy_app_location`)**: blocca l'accesso al sensore di posizione per tutte le app impostando `Value` a `Deny` in `HKCU\Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\location`.
13. **Disable Windows Recall AI Snapshot & Data Analysis (`privacy_windows_recall`)**: blocca la cattura periodica dello schermo e l'OCR di Recall impostando `DisableAIDataAnalysis` a 1 in `HKLM` e `HKCU\Software\Policies\Microsoft\Windows\WindowsAI`.
14. **Disable Windows AI 'Click to Do' Screen Context Actions (`privacy_click_to_do`)**: disattiva l'analisi generativa dei pixel dello schermo impostando `DisableClickToDo` a 1 in `HKCU\Software\Policies\Microsoft\Windows\WindowsAI`.
15. **Disable AI Features in Paint & Notepad (`privacy_notepad_paint_ai`)**: disattiva Cocreator in Paint e le funzioni di riscrittura AI nel Blocco Note.
16. **Disable Microsoft Edge Ads, Shopping Tips & First-Run Promos (`privacy_edge_ads_recommendations`)**: blocca promozioni, splash screen e assistente shopping in Edge configurando le policy di `HKLM\SOFTWARE\Policies\Microsoft\Edge`.
17. **Prevent Auto-Installing Device Companion & OEM Apps (`privacy_consumer_features`)**: impedisce a Windows di scaricare software partner e app OEM indesiderate tramite `DisableWindowsConsumerFeatures` e blocco metadati di rete.
18. **Disable Phone Link Mobile Devices in Start Menu (`privacy_start_phone_link`)**: nasconde il riquadro laterale dei dispositivi mobili nel menu Start di Windows 11 impostando `ShowPhoneLinkInStart` a 0.
19. **Hide Recommended Section & Account Promotions in Start Menu (`privacy_start_recommendations`)**: rimuove file recenti e promozioni dell'account Microsoft dal menu Start di Windows 11 impostando `Start_IrisRecommendations`, `ShowRecent` e `Start_AccountNotifications` a 0.

### 5.2 Performance, CPU & Gaming (23 Tweak)

20. **Disable Network Throttling Index (`opt_network_throttling`)**: disattiva la limitazione dei pacchetti di rete impostando `NetworkThrottlingIndex` a `0xFFFFFFFF` (-1) in `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile`.
21. **Prioritize Active Process System Responsiveness (`opt_system_responsiveness`)**: assegna il 100% della priorità di scheduling alle applicazioni in primo piano impostando `SystemResponsiveness` a 0 (eliminando la riserva del 20% in background).
22. **Disable Game DVR Background Recording (`opt_game_dvr`)**: disattiva la registrazione video continua del gameplay liberando VRAM e cicli di encoding GPU impostando `GameDVR_Enabled` a 0 in `HKCU\System\GameConfigStore`.
23. **Enable Windows Auto Game Mode (`opt_game_mode`)**: assicura la prioritizzazione delle risorse di sistema per i giochi impostando `AutoGameModeEnabled` a 1 in `HKCU\Software\Microsoft\GameBar`.
24. **Disable Background App Execution (`opt_background_apps`)**: impedisce alle app UWP di rimanere in esecuzione consumando memoria impostando `GlobalUserDisabled` a 1 in `HKCU\Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications`.
25. **Reduce Desktop Menu Show Delay (100ms) (`opt_menu_delay`)**: riduce il ritardo di rendering dei menu a comparsa da 400ms a 100ms impostando `MenuShowDelay` a `100` in `HKCU\Control Panel\Desktop`.
26. **Disable NTFS Last Access Timestamps (`opt_ntfs_last_access`)**: elimina le scritture ridondanti su disco ad ogni lettura di file impostando `NtfsDisableLastAccessUpdate` a 1 in `HKLM\SYSTEM\CurrentControlSet\Control\FileSystem`, prolungando la vita degli SSD.
27. **Enable Win32 Long Paths (>260 Characters) (`opt_long_paths`)**: rimuove il limite MAX_PATH di 260 caratteri per cartelle e percorsi profondi impostando `LongPathsEnabled` a 1 in `HKLM\SYSTEM\CurrentControlSet\Control\FileSystem`.
28. **Disable Windows Update P2P Delivery Optimization (`opt_delivery_opt_p2p`)**: impedisce a Windows di usare l'upload della connessione internet per distribuire aggiornamenti impostando `DODownloadMode` a 0 in `HKLM\SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization`.
29. **Disable Mouse Pointer Precision (Raw 1:1 Input) (`opt_mouse_accel`)**: rimuove l'accelerazione artificiale del cursore impostando `MouseSpeed` a `0` in `HKCU\Control Panel\Mouse` per una mira lineare e precisa nei giochi.
30. **Disable Window Minimize/Maximize Animation Delay (`opt_visual_fx`)**: disattiva le animazioni lente di minimizzazione/ingrandimento finestre impostando `MinAnimate` a `0` in `HKCU\Control Panel\Desktop\WindowMetrics`.
31. **Disable Storage Sense Automatic Disk Cleanup (`perf_disable_storage_sense`)**: impedisce al Sensore Memoria di eliminare file temporanei e download senza consenso dell'utente.
32. **Disable Automatic BitLocker Device Encryption (`perf_disable_bitlocker_auto`)**: impedisce a Windows 11 di crittografare automaticamente le nuove unità con BitLocker impostando `PreventDeviceEncryption` a 1 in `HKLM\SYSTEM\CurrentControlSet\Control\BitLocker`.
33. **Disable Network in Modern Standby (`perf_modern_standby_net`)**: disconnette le schede di rete durante lo sleep S0 Modern Standby, eliminando il surriscaldamento nei portatili e il drenaggio della batteria.
34. **Disable Multiplane Overlay (MPO) (`perf_mpo_disable`)**: disattiva la composizione MPO del DWM impostando `OverlayTestMode` a 5 in `HKLM\SOFTWARE\Microsoft\Windows\Dwm`, risolvendo sfarfallii e micro-stuttering su GPU NVIDIA e AMD.
35. **Turn on Num Lock Automatically on Startup (`perf_numlock_startup`)**: attiva automaticamente il tastierino numerico all'avvio impostando `InitialKeyboardIndicators` a `2`.
36. **Enable Detailed Verbose BSoD Crash Parameters (`perf_bsod_verbose`)**: mostra indirizzi esatti di memoria, driver coinvolto e codici esadecimali nella schermata blu impostando `DisplayParameters` a 1 in `HKLM\SYSTEM\CurrentControlSet\Control\CrashControl`.
37. **Enable Verbose Status Messages During Logon & Boot (`perf_logon_verbose`)**: mostra i passaggi diagnostici dettagliati di avvio e arresto impostando `verbosestatus` a 1 in `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System`.
38. **Prefer IPv4 over IPv6 (`perf_prefer_ipv4`)**: configura lo stack TCP/IP per preferire le connessioni IPv4 impostando `DisabledComponents` a `0x20`, eliminando timeout e lag di risoluzione DNS.
39. **Disable Teredo IPv6 Tunneling (`perf_disable_teredo`)**: disattiva l'interfaccia di tunneling Teredo tramite `netsh interface teredo set state disabled`.
40. **Brave Browser Debloat (`perf_brave_debloat`)**: disattiva tramite policy di gruppo Brave Leo AI, Crypto Wallet, Brave Rewards, VPN e IPFS in `HKLM\SOFTWARE\Policies\BraveSoftware\Brave`.
41. **Disable ms-gamingoverlay / Game Bar Popups (`perf_disable_gamebar_popups`)**: elimina i fastidiosi popup di errore "Avrai bisogno di una nuova app per aprire questo collegamento ms-gamingoverlay" impostando `AppCaptureEnabled` a 0.
42. **Prevent AI Service (WSAIFabricSvc) from Starting Automatically (`perf_disable_wsaifabric`)**: imposta il servizio Windows AI Fabric su avvio Manuale (3), evitando consumi inutili di NPU in background.

### 5.3 Windows Shell & Esplora File (30 Tweak)

43. **Show File Extensions in Explorer (`win_show_extensions`)**: rende visibili le estensioni dei file (.exe, .zip, .docx) impostando `HideFileExt` a 0 in `HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced`.
44. **Show Hidden Files and Folders (`win_show_hidden`)**: rende visibili cartelle e file nascosti impostando `Hidden` a 1.
45. **Open File Explorer to 'This PC' Instead of Quick Access (`win_open_this_pc`)**: apre Esplora File direttamente sulle unità di memoria impostando `LaunchTo` a 1.
46. **Windows 11 Classic Full Context Menu (`win_classic_context_menu`)**: ripristina il menu tasto destro classico e immediato di Windows 10 su Windows 11, eliminando la voce lenta "Mostra altre opzioni".
47. **Disable Windows Error Reporting / WerFault (`win_error_reporting`)**: disattiva la raccolta dei dump e invio report di crash impostando `Disabled` a 1 in `HKLM\SOFTWARE\Microsoft\Windows\Windows Error Reporting`.
48. **Disable Windows Fast Startup / Hybrid Boot (`win_fast_startup`)**: garantisce arresti completi del sistema impostando `HiberbootEnabled` a 0 in `HKLM\SYSTEM\CurrentControlSet\Control\Session Manager\Power`.
49. **Disable Lock Screen Spotlight Ads & Tips (`win_lockscreen_tips`)**: rimuove annunci e curiosità dalla schermata di blocco impostando `RotatingInfo_Enabled` a 0.
50. **Disable Windows 11 Widgets Taskbar Icon (`win_disable_widgets`)**: rimuove l'icona Notizie e Meteo dalla barra delle applicazioni impostando `TaskbarDa` a 0.
51. **Enable 'End Task' in Taskbar Right-Click Menu (`win_end_task_right_click`)**: aggiunge l'opzione "Termina operazione" direttamente col tasto destro sull'icona delle finestre nella barra delle applicazioni di Windows 11.
52. **Enable Taskbar 'Last Active Click' Switching (`win_last_active_click`)**: commuta istantaneamente all'ultima finestra attiva di un'app aperta cliccando sull'icona della barra anziché mostrare le miniature.
53. **Align Taskbar Icons to Left (`win_taskbar_align_left`)**: sposta il pulsante Start e le icone della barra di Windows 11 a sinistra, ripristinando il layout storico.
54. **Hide Search Bar from Taskbar (`win_hide_taskbar_search`)**: nasconde la casella di ricerca dalla barra delle applicazioni per liberare spazio (`SearchboxTaskbarMode` a 0).
55. **Hide Task View Button from Taskbar (`win_hide_task_view`)**: rimuove il pulsante Visualizzazione Attività dalla barra delle applicazioni.
56. **Hide Home & Gallery from File Explorer Navigation Pane (`win_hide_home_gallery`)**: rimuove le voci "Home" e "Raccolta" dal riquadro di navigazione laterale di Esplora File di Windows 11.
57. **Hide Duplicate Removable Drives in Explorer Navigation Pane (`win_hide_duplicate_drives`)**: rimuove le icone duplicate delle chiavette USB che compaiono al di fuori di "Questo PC".
58. **Add Common Folders back to 'This PC' (`win_restore_this_pc_folders`)**: ripristina i collegamenti alle cartelle personali Desktop, Documenti, Download, Musica, Immagini e Video sotto "Questo PC" in Esplora File.
59. **Show Drive Letters Before Drive Names in Explorer (`win_drive_letters_first`)**: visualizza le lettere delle unità prima del nome (es. `(C:) Disco Locale`) per una lettura immediata.
60. **Disable File Explorer Automatic Folder Template Sniffing (`win_disable_folder_discovery`)**: impedisce a Esplora File di rallentare scansionando intere directory per indovinare se contengono musica o documenti (`FolderType` a `NotSpecified`).
61. **Disable Window Snapping & Snap Assist (`win_disable_window_snapping`)**: disattiva il ridimensionamento automatico delle finestre quando vengono trascinate verso i bordi dello schermo.
62. **Exclude Edge/Browser Tabs from Alt+Tab Switcher (`win_alt_tab_windows_only`)**: imposta `MultiTaskingAltTabFilter` a 3 per far commutare Alt+Tab solo tra le finestre delle app, escludendo le singole schede del browser.
63. **Enable Windows System & Apps Dark Theme (`win_dark_mode`)**: attiva la modalità scura ad alto contrasto per la shell di sistema e le applicazioni.
64. **Disable Transparency & Window Animations (`win_disable_visual_effects`)**: disattiva trasparenze acriliche e animazioni dell'interfaccia per la massima reattività GPU.
65. **Hide 'Learn about this picture' Desktop Spotlight Icon (`win_hide_spotlight_icon`)**: rimuove l'icona inamovibile di Windows Spotlight dal desktop.
66. **Disable Windows Lock Screen (`win_disable_lockscreen`)**: elimina la schermata di blocco swipe-up impostando `NoLockScreen` a 1, passando direttamente alla richiesta di password/PIN all'avvio.
67. **Disable Acrylic Blur on Sign-in Screen (`win_disable_acrylic_logon`)**: mostra lo sfondo di accesso nitido senza l'effetto di sfocatura acrilica pesante per la scheda video.
68. **Always Show Scrollbars (`win_always_show_scrollbars`)**: impedisce la scomparsa automatica delle barre di scorrimento nelle impostazioni e nelle app UWP.
69. **Disable 5x Shift Sticky Keys Keyboard Shortcut (`win_sticky_keys_shortcut`)**: disattiva il popup fastidioso dei Tasti Permanenti quando si preme il tasto Shift 5 volte di fila durante la digitazione o il gioco.
70. **Hide Settings 'Home' Page and Microsoft 365 Ads (`win_hide_settings_home`)**: nasconde la pagina promozionale "Home" dall'app Impostazioni di Windows 11, aprendo direttamente le impostazioni di Sistema.
71. **Prevent Automatic Restarts After Updates While Signed In (`win_prevent_update_reboot`)**: impedisce a Windows Update di riavviare forzatamente il PC mentre l'utente è connesso (`NoAutoRebootWithLoggedOnUsers` a 1).
72. **Prevent Windows from Getting Experimental Updates Immediately (`win_prevent_fast_updates`)**: disattiva l'opzione per ricevere aggiornamenti sperimentali appena disponibili, garantendo solo aggiornamenti stabili e testati.

### 5.4 Sistema & Avanzate (8 Tweak)

73. **Disable Microsoft Office Telemetry & Logging (`priv_disable_office_telemetry`)**: disattiva la telemetria di Office (2016, 2019, 2021, M365) e il caricamento dei registri di utilizzo in background.
74. **Stop Automatic Windows Updates (Notify Only) (`win_stop_auto_updates`)**: configura Windows Update in modalità di sola notifica prima del download (`NoAutoUpdate` a 1 e `AUOptions` a 2), bloccando download massivi automatici.
75. **Disable Microsoft Edge Copilot & Sidebar AI (`priv_disable_edge_copilot`)**: disattiva Copilot e la barra laterale Bing all'interno del browser Microsoft Edge.
76. **Enable Hardware Clock UTC Time (Dual-Boot Fix) (`win_enable_utc_time`)**: imposta il clock hardware CMOS su Universal Time (UTC) impostando `RealTimeIsUniversal` a 1, risolvendo il disallineamento dell'ora quando si usa il dual-boot con Linux o macOS.
77. **Disable OneDrive Cloud File Syncing (`win_disable_onedrive_sync`)**: blocca la sincronizzazione file in background di OneDrive impostando `DisableFileSyncNGSC` a 1 in `HKLM`.
78. **Disable HPET (Lower Input Latency in Games) (`perf_disable_hpet`)**: configura il boot loader di Windows per usare i timer hardware TSC invarianti a bassissimo overhead invece di HPET (`bcdedit /set useplatformclock false` e `disabledynamictick yes`), riducendo il micro-stuttering nei giochi competitivi.
79. **Add 'Take Ownership' to Context Menu (`ctx_take_ownership`)**: aggiunge l'opzione "Diventa Proprietario" nel menu del tasto destro per file e cartelle, concedendo istantaneamente il controllo completo agli amministratori tramite `takeown` e `icacls`.
80. **Add 'Open with Notepad' to Context Menu (`ctx_open_with_notepad`)**: aggiunge "Apri con Blocco Note" al menu tasto destro di qualsiasi estensione di file (`HKCR\*\shell\OpenWithNotepad`).

---

## 6. Rimozione Bloatware & App Preinstallate (UWP Debloater)

- **Rilevamento Intelligente dei Pacchetti**: ispezione ad alta velocità tramite registro `AppModel\Repository\Packages` combinata con fallback PowerShell per rilevare solo i pacchetti realmente presenti sul computer.
- **Catalogo di 38 Applicazioni Suddivise per Livello di Rischio**:
  - **Tier Safe (Consumabili & Sponsor OEM)**:
    1. Microsoft News (`Microsoft.BingNews`)
    2. Microsoft Weather (`Microsoft.BingWeather`)
    3. Microsoft Money & Finance (`Microsoft.BingFinance`)
    4. Microsoft Sports (`Microsoft.BingSports`)
    5. Get Help / Richiesta Supporto (`Microsoft.GetHelp`)
    6. Suggerimenti e Guida introduttiva (`Microsoft.Getstarted`)
    7. Solitaire Collection (`Microsoft.MicrosoftSolitaireCollection`)
    8. Windows People Hub / Contatti (`Microsoft.People`)
    9. Skype UWP (`Microsoft.SkypeApp`)
    10. Microsoft To Do (`Microsoft.Todos`)
    11. Editor Video Clipchamp (`Clipchamp.Clipchamp`)
    12. Film e TV / Zune Video (`Microsoft.ZuneVideo`)
    13. Groove Music / Media Player (`Microsoft.ZuneMusic`)
    14. Spotify Music sponsorizzato OEM (`SpotifyAB.SpotifyMusic`)
    15. Disney+ sponsorizzato OEM (`Disney.37853FC22B2CE`)
    16. TikTok sponsorizzato OEM (`BytedancePte.Ltd.TikTok`)
    17. Portale Realtà Mista (`Microsoft.MixedReality.Portal`)
    18. Visualizzatore 3D (`Microsoft.Microsoft3DViewer`)
    19. Paint 3D (`Microsoft.MSPaint`)
    20. Portale Microsoft 365 Office Hub (`Microsoft.MicrosoftOfficeHub`)
    21. Power Automate Desktop (`Microsoft.PowerAutomateDesktop`)
    22. Nuovo Outlook per Windows basato su webview (`Microsoft.OutlookForWindows`)
    23. Posta e Calendario legacy (`Microsoft.WindowsCommunicationsApps`)
    24. OneNote per Windows 10 UWP (`Microsoft.Office.OneNote`)
    25. Memo / Sticky Notes (`Microsoft.MicrosoftStickyNotes`)
    26. Microsoft Teams Personale (`MicrosoftTeams`)
    27. Microsoft Pay / Portafoglio (`Microsoft.Wallet`)
  - **Tier Balanced (Utility Secondarie & Sincronizzazione)**:
    28. Xbox Desktop Companion / Store (`Microsoft.GamingApp`)
    29. Xbox Game Bar Overlay (`Microsoft.XboxGamingOverlay`)
    30. Xbox Identity Provider (`Microsoft.XboxIdentityProvider`)
    31. Finestra Sintesi Vocale Giochi Xbox (`Microsoft.XboxSpeechToTextOverlay`)
    32. Collegamento al Telefono / Phone Link (`Microsoft.YourPhone`)
    33. Hub di Feedback (`Microsoft.WindowsFeedbackHub`)
    34. Assistenza Rapida (`Microsoft.QuickAssist`)
  - **Tier Aggressive (App di Sistema & Assistenti)**:
    35. Assistente Vocale Cortana (`Microsoft.549981C3F5F10`)
    36. Mappe Windows (`Microsoft.WindowsMaps`)
    37. Registratore Vocale (`Microsoft.WindowsSoundRecorder`)
    38. Orologio e Sveglie (`Microsoft.WindowsAlarms`)
- **Preset di Selezione Rapida**:
  - *Safe*: seleziona tutte le 27 app promozionali e sponsorizzate senza alcun impatto sulle funzioni del PC.
  - *Minimal*: include le app Safe più i componenti Xbox non essenziali e Phone Link.
  - *Aggressive*: seleziona l'intero catalogo per un sistema ultra-snello.
- **Pulsanti "Select All" e "Clear" con Contatore Dinamico**: permette di selezionare singolarmente con checkbox i pacchetti e raggrupparli per la rimozione batch.
- **Disinstallazione Sicura a Doppio Livello**:
  - Esegue la rimozione per l'utente corrente tramite `Remove-AppxPackage`.
  - Rimuove contemporaneamente il pacchetto provisioned per tutti i nuovi utenti con `Remove-AppxProvisionedPackage -Online` se eseguito come amministratore.
- **Disinstallatore Profondo OneDrive**:
  - Termina forzatamente i processi `OneDrive.exe` in esecuzione.
  - Individua ed esegue l'uninstaller nativo silenzioso in `SysWOW64`, `System32` o `%LOCALAPPDATA%` con parametro `/uninstall`.
- **Disinstallatore Profondo Microsoft Edge**:
  - Rileva l'eseguibile di configurazione di sistema `setup.exe` nella cartella di installazione di Microsoft Edge.
  - Invia i comandi di disinstallazione forzata a livello di sistema `--uninstall --system-level --force-uninstall`.

---

## 7. Gestione Applicazioni all'Avvio (Startup Manager)

- **Scansione Completa delle Posizioni di Avvio Automatico**:
  - `HKCU\Software\Microsoft\Windows\CurrentVersion\Run` (Chiave avvio utente corrente)
  - `HKLM\Software\Microsoft\Windows\CurrentVersion\Run` (Chiave avvio a livello di sistema)
  - Cartella di Avvio del menu Start (`%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup`)
- **Integrazione Non Distruttiva con Windows Task Manager**:
  - Rispetta e modifica le chiavi binarie native `StartupApproved\Run` e `StartupApproved\StartupFolder`.
  - Permette di abilitare o disabilitare un programma all'avvio senza cancellarne il percorso o la configurazione, esattamente come avviene in Gestione Attività di Windows.
- **Visualizzazione Tabellare Dettagliata**:
  - Nome dell'applicazione.
  - Percorso completo del file binario o comando eseguito.
  - Posizione di origine (Hive di registro o cartella Startup).
  - Impatto stimato sulle prestazioni (Alto, Medio, Normale, Basso).
  - Interruttore a levetta interattivo per abilitare/disabilitare all'istante l'avvio automatico.
  - Pulsante per eliminare definitivamente la voce di avvio.

---

## 8. Pulizia Disco & File Temporanei (Disk Cleanup)

- **Scansione Approfondita su 12 Categorie di File Inutili (`StorageService.cs`)**:
  1. *Windows Recycle Bin (Cestino)*: interrogazione su tutte le unità connesse tramite Win32 `SHQueryRecycleBin` e svuotamento nativo con `SHEmptyRecycleBin`.
  2. *User Temporary Files*: file temporanei e cache delle applicazioni nella cartella `%TEMP%` dell'utente.
  3. *Windows System Temp*: file temporanei generati dai servizi di sistema e dagli installer in `C:\Windows\Temp`.
  4. *Windows Update Download Cache*: file di installazione e pacchetti cumulativi scaricati in `SoftwareDistribution\Download`.
  5. *DirectX / GPU Shader Cache*: cache compilata degli shader grafici per giochi e applicazioni (DirectX D3DSCache, NVIDIA DXCache, AMD DxCache).
  6. *File Explorer Thumbnail Cache*: database delle anteprime delle immagini e delle cartelle (`thumbcache_*.db`).
  7. *Delivery Optimization Cache*: file temporanei della rete P2P in `SoftwareDistribution\DeliveryOptimization`.
  8. *System & Application Crash Dumps*: log e file di dump della memoria (`.dmp`) generati da crash in `%LOCALAPPDATA%\CrashDumps` e `Windows\Minidump`.
  9. *System & Setup Log Files*: log storici di installazione e manutenzione in `Windows\Logs` e `Windows\Panther`.
  10. *Google Chrome Web Cache*: file multimediali, immagini e script temporanei salvati dal browser Chrome.
  11. *Microsoft Edge Web Cache*: file temporanei internet salvati dal browser Edge.
  12. *Windows Error Reporting (WER)*: report diagnostici e dump in coda per la trasmissione a Microsoft.
- **Calcolo Dinamico Spazio Recuperabile**:
  - Calcolo istantaneo dei Megabyte e Gigabyte recuperabili prima di eseguire la pulizia.
  - Conteggio del numero di file analizzati per ogni categoria.
  - Checkbox per includere/escludere ogni singolo target e pulsante master "Seleziona Tutti".
- **Cancellazione Protetta da Errori**:
  - Gestione avanzata delle eccezioni: ignora i file attualmente bloccati o in uso dai processi di sistema senza interrompere la pulizia degli altri file.
  - Supporto per la modalità di simulazione sicura (Dry-Run Preview).

---

## 9. App Store & Aggiornamenti Software (Winget Package Manager)

- **Catalogo di 21 Applicazioni Freeware & Open Source Essenziali**:
  - *Browser*: Google Chrome, Mozilla Firefox, Brave Browser.
  - *Compressione*: 7-Zip, PeaZip.
  - *Utility*: Microsoft PowerToys, Everything Search (Voidtools), ShareX, Notepad++, Sumatra PDF, WinSCP.
  - *Sviluppo*: Visual Studio Code, Git for Windows, Python 3.12, Node.js (LTS), Windows Terminal.
  - *Comunicazione*: Discord, Telegram Desktop, Zoom Workplace.
  - *Media*: VLC Media Player, OBS Studio, Audacity.
  - *Gaming*: Steam, Epic Games Launcher, GOG Galaxy.
- **Rilevamento Automatico Stato di Installazione**:
  - Analisi delle chiavi di disinstallazione di Windows (`Uninstall`) a 32 e 64 bit per identificare se l'applicazione è già installata nel sistema.
- **Installazione Silenziosa Automatizzata**:
  - Invocazione in background di Windows Package Manager (`winget.exe install --id ... -e --silent --accept-source-agreements --accept-package-agreements`).
  - Installazioni pulite, prive di pubblicità, barre degli strumenti o software indesiderato di terze parti.
- **Banner con Barra di Avanzamento e Timer in Tempo Reale**:
  - Mostra nome del pacchetto, spinner animato, barra di caricamento e cronometro durante il download e l'installazione silenziosa.
- **⚡ Software Updates (Aggiornamenti Software Winget)**:
  - Scansione in background delle applicazioni desktop obsolete tramite `winget upgrade`.
  - Tabella comparativa che mostra: nome applicazione, ID pacchetto, versione attualmente installata e nuova versione disponibile.
  - Pulsante per aggiornare il singolo programma selezionato.
  - Pulsante "Upgrade All Packages" per lanciare l'aggiornamento massivo di tutte le app obsolete con un solo clic.

---

## 10. Rete, Connessioni & DNS Switcher (Network Engine)

- **Ispezione Completa Schede di Rete (`NetworkService.cs`)**:
  - Rileva tutte le interfacce fisiche e virtuali (Ethernet, Wi-Fi).
  - Mostra nome adattatore, descrizione hardware, velocità di collegamento (Gbps/Mbps), indirizzo fisico MAC, indirizzo IPv4, Subnet Mask, Gateway predefinito, server DNS primario e secondario, stato DHCP.
- **Azioni Rapide di Riparazione Rete a 1 Clic**:
  - *Flush DNS*: svuota la cache del resolver DNS locale (`ipconfig /flushdns`).
  - *Reset Winsock*: ripristina il catalogo dei socket Winsock (`netsh winsock reset`).
  - *Reset TCP/IP*: reinstalla e resetta lo stack dei protocolli TCP/IP (`netsh int ip reset`).
  - *Rinnova Lease DHCP*: rilascia e rinnova l'indirizzo IP assegnato dal router (`ipconfig /release` && `/renew`).
- **Ottimizzazione Avanzata Stack di Rete Kernel**:
  - Abilita TCP Window Auto-Tuning su `Normal` per massimizzare la banda.
  - Abilita Receive Side Scaling (RSS) su tutti i core della CPU.
  - Abilita Receive Segment Coalescing (RSC) per ridurre l'overhead della CPU durante i trasferimenti dati.
  - Disattiva le euristiche TCP ed ECN restrittive.
  - Rimuove il Network Throttling Index (throughput di pacchetti illimitato).
  - Imposta System Responsiveness a 0% (massima priorità per i giochi e lo streaming).
- **Disattivazione Risparmio Energetico Scheda di Rete (NIC Power Saving)**:
  - Disattiva Energy Efficient Ethernet (EEE), Green Ethernet e Auto Power Save sui driver delle schede di rete per eliminare jitter e micro-disconnessioni nei giochi online.
- **Quick DNS Switcher con 6 Preset Integrati**:
  1. *Cloudflare DNS* (1.1.1.1 / 1.0.0.1): massima velocità di risoluzione e rispetto della privacy.
  2. *Google Public DNS* (8.8.8.8 / 8.8.4.4): robustezza e Anycast routing globale.
  3. *Quad9 Secure DNS* (9.9.9.9 / 149.112.112.112): protezione attiva con blocco automatico di domini malware e phishing.
  4. *AdGuard DNS* (94.140.14.14 / 94.140.15.15): blocco globale di annunci pubblicitari e tracker a livello di sistema operativo.
  5. *Cisco OpenDNS* (208.67.222.222 / 208.67.220.220): affidabilità enterprise e filtro web.
  6. *Ripristino Automatico DHCP*: ripristina la configurazione DNS dinamica assegnata dal router.
- **Monitor Connessioni di Rete Attive (NetLimiter-Style Inspector)**:
  - Interrogazione diretta della tabella TCP estesa di Windows (`GetExtendedTcpTable`).
  - Mostra in tempo reale: PID, nome dell'applicazione responsabile, protocollo (TCP/UDP), endpoint locale (IP e porta), endpoint remoto (IP e porta), servizio riconosciuto (HTTP, HTTPS, DNS, SSH, RDP, Steam, Dev Server, ecc.), stato della connessione (ESTABLISHED, LISTEN, TIME_WAIT, ecc.) e direzione del traffico (Inbound, Outbound, Local Loopback).
  - Quattro metriche sintetiche in tempo reale: Totale socket attivi, Connessioni stabilite, Porte aperte in ascolto, Processi attivi con connessioni di rete.
  - Filtro testuale in tempo reale, filtro per stato di connessione, filtro per direzione e funzione di auto-aggiornamento automatico ogni 3 secondi.
- **Strumento Ping & Latenza**:
  - Esegue ping ICMP diagnostici verso qualsiasi indirizzo IP o hostname (es. `1.1.1.1`, `google.com`) misurando il tempo di risposta Roundtrip in millisecondi.
- **Ispettore Intelligence Shodan.io**:
  - Risoluzione inversa DNS, scansione rapida delle porte standard aperte (80, 443, 22, 53, 3389, 8080) e generazione automatica del collegamento per visualizzare il profilo completo del target su Shodan.io.

---

## 11. Strumenti di Sistema Nativi (System Tools)

### 11.1 Editor File HOSTS con Blocchi Integrati
- Visualizzazione del percorso ufficiale del file HOSTS (`C:\Windows\System32\drivers\etc\hosts`) e rimozione automatica dell'attributo di sola lettura.
- Editor di testo integrato per visualizzare e modificare direttamente le regole di reindirizzamento IP.
- **Backup Automatico ad Ogni Salvataggio**: salva una copia di sicurezza timestampata in `data/backups/hosts/hosts_YYYYMMDD_HHMMSS.bak` prima di scrivere qualsiasi modifica.
- **Blocco Telemetria Microsoft con 1 Clic**: inietta automaticamente le regole di blocco per i domini di diagnostica Microsoft (`v10.events.data.microsoft.com`, `telemetry.microsoft.com`, `watson.telemetry.microsoft.com`, `settings-win.data.microsoft.com`, ecc.).
- **Blocco Tracciamento Adobe con 1 Clic**: inietta automaticamente le regole di blocco per oltre 20 domini di tracking e verifica licenza Adobe (`genuine.adobe.com`, `prod.adobegenuine.com`, `lmlicenses.wip4.adobe.com`, ecc.).

### 11.2 Gestore Variabili d'Ambiente (Utente & Sistema)
- Elenco completo e ordinato di tutte le variabili d'ambiente di Windows suddivise per ambito:
  - *User*: variabili personali dell'utente corrente.
  - *System*: variabili globali della macchina (richiede privilegi amministrativi).
- Filtro di ricerca istantaneo per nome e valore (es. `PATH`, `TEMP`, `JAVA_HOME`).
- Aggiunta di nuove variabili d'ambiente specificando nome, valore e scope.
- **Editor Avanzato a Doppia Modalità per la Modifica**:
  - *Modalità Riga per Riga (Row-by-Row List)*: ideale per il `PATH` e variabili con percorsi multipli separati da punto e virgola; permette di visualizzare i singoli percorsi, aggiungerne di nuovi e gestirli ordinatamente.
  - *Modalità Testo Grezzo (Raw Text Mode)*: textarea tradizionale per modificare direttamente l'intera stringa grezza.

### 11.3 File Unlocker (Windows Restart Manager API)
- Motore basato sulle API native del Windows Restart Manager (`rstrtmgr.dll`: `RmStartSession`, `RmRegisterResources`, `RmGetList`).
- Permette di inserire il percorso di qualsiasi file o cartella bloccata per identificare con precisione chirurgica quali processi di sistema o applicazioni vi mantengono aperto un handle.
- Tabella dei risultati con visualizzazione di: Process ID (PID), Nome del processo eseguibile, Nome dell'applicazione e percorso completo.
- **Pulsante di Terminazione Processo**: consente di terminare all'istante il processo responsabile del blocco per poter eliminare, rinominare o spostare il file desiderato.

### 11.4 Gestore Alias Esegui (Win+R App Paths)
- Gestione trasparente delle chiavi di registro `App Paths` (`HKCU` e `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths`).
- Permette di creare comandi brevi personalizzati digitabili direttamente nel dialog **Win + R** (es. digitando `term` si può aprire il terminale personalizzato, `hosts` per aprire il file hosts, `vsc` per Visual Studio Code).
- Possibilità di aggiungere nuovi alias specificando nome del comando e percorso del file binario di destinazione, ed eliminare gli alias esistenti.

### 11.5 Centro Licenza & Attivazione Windows Ufficiale
- Interrogazione nativa WMI/CIM tramite la classe ufficiale `SoftwareLicensingProduct` (senza eseguire script batch o scaricare tool di terze parti a rischio sicurezza).
- Rilevamento in tempo reale dello stato effettivo di attivazione genuina:
  - *Licensed (Permanently Activated)*
  - *Grace Period* (con calcolo esatto dei giorni rimanenti alla scadenza)
  - *Non-Genuine Grace*
  - *Notification / Expired*
  - *Unlicensed*
- Rilevamento del canale di distribuzione della licenza: *Retail*, *OEM:DM*, *Volume (KMS/MAK)*, *Digital License*.
- Visualizzazione del codice Product Key parziale (ultimi 5 caratteri della chiave).
- Visualizzazione dell'edizione commerciale di Windows.
- Pulsante di scorciatoia diretta per aprire la schermata ufficiale di attivazione nelle Impostazioni di Windows (`ms-settings:activation`).

---

## 12. Sicurezza, Manutenzione & Riparazioni (Repair Center)

- **Panoramica Sicurezza in Tempo Reale**:
  - Stato di esecuzione del servizio Microsoft Defender Antivirus (`WinDefend`).
  - Stato della protezione in tempo reale (Real-Time Protection) verificato direttamente dalle policy di registro.
  - Fornitore Antivirus attivo rilevato tramite `root\SecurityCenter2`.
  - Stato del Firewall di Windows.
  - Stato del Controllo Account Utente (UAC) e livello di consenso configurato (`ConsentPromptBehaviorAdmin`).
- **Console Diagnostica con Output Terminale in Tempo Reale**:
  - Finestra terminale in stile console con visualizzazione in streaming riga per riga di tutto l'output (stdout/stderr) generato dai comandi di riparazione Windows.
  - **9 Strumenti di Riparazione Avanzati**:
    1. *SFC /scannow (System File Checker)*: scansiona tutti i file protetti del sistema operativo e sostituisce quelli danneggiati o mancanti.
    2. *DISM Restore Health*: ripara l'archivio dei componenti di Windows (`dism /online /cleanup-image /restorehealth`) scaricando i file integri dai server Windows Update.
    3. *DISM Check Health*: controllo diagnostico rapido dell'immagine di Windows (`dism /online /cleanup-image /checkhealth`).
    4. *System Corruption Scan*: ciclo di scansione sequenziale a doppio stadio: esegue prima `SFC /scannow` e successivamente `DISM /RestoreHealth`.
    5. *Reset Completo Windows Update*:
       - Arresta i servizi `wuauserv`, `cryptSvc`, `bits`, `msiserver`.
       - Rinomina e purga la cache di download `SoftwareDistribution` e `Catroot2`.
       - Ri-registra nel sistema tutte le DLL essenziali di Windows Update (`wuapi.dll`, `wuaueng.dll`, `wucltui.dll`, `wups.dll`, `wups2.dll`, `wuwebv.dll`, `atl.dll`).
       - Resetta i socket Winsock e riavvia ordinatamente tutti i servizi.
    6. *Reset Completo Rete e Firewall*: ripristina Winsock, protocollo TCP/IP, azzera le regole del Firewall di Windows ai valori predefiniti, svuota la cache DNS e rinnova il lease DHCP.
    7. *Riparazione Problemi Comuni del Registro*:
       - Diagnostica e rimuove blocchi di policy orfani o dannosi (es. `DisableTaskMgr`, `DisableRegistryTools`, `NoFolderOptions`, `NoRun`).
       - Verifica e ripristina i percorsi mancanti delle User Shell Folders (es. cartella Desktop).
       - Pulisce le voci obsolete e corrotte della cache `RunMRU`.
       - Ri-registra i file binari COM/OLE di base (`ole32.dll`, `oleaut32.dll`, `actxprxy.dll`).
       - Ricostruisce il database corrotto della cache delle icone di Esplora File (`IconCache.db`).
    8. *CHKDSK in Sola Lettura*: analizza la tabella di allocazione dei file NTFS dell'unità C: alla ricerca di discrepanze del file system senza richiedere il riavvio immediato.
    9. *Reset Rapido Rete*: svuota i DNS e ripristina Winsock.
- **Configuratore Windows AutoLogon**:
  - Visualizza se l'accesso automatico senza password è abilitato, con relativo nome utente e dominio.
  - Modale guidato per configurare credenziali di accesso automatico per account locali (`.`) o di dominio.
  - Pulsante per disabilitare istantaneamente AutoLogon e cancellare la password salvata nel registro `Winlogon`.
- **Sblocco Piano Energetico Prestazioni Eccellenti (Ultimate Performance)**:
  - Duplica e attiva tramite `powercfg` il piano energetico segreto ad altissime prestazioni GUID `e9a42b02-d5df-448d-aa00-03f14749eb61`.
- **Gestione Ibernazione (`hiberfil.sys`)**:
  - Attiva o disattiva l'ibernazione di sistema tramite `powercfg -h on` / `off`.
  - La disattivazione elimina all'istante il file gigante `hiberfil.sys`, liberando molti gigabyte su dischi SSD.

---

## 13. Specifiche Hardware & Diagnostica Periferiche (Hardware Specs)

Il modulo hardware esegue un'ispezione ad altissima velocità sfruttando WMI, Win32 e API di sistema (`HardwareService.cs`):

### 13.1 Componenti Principali (CPU, RAM, GPU, Scheda Madre, BIOS)
- **Central Processing Unit (CPU)**:
  - Modello e nome commerciale (es. Intel Core i7 / AMD Ryzen).
  - Produttore e architettura del processore (x64).
  - Numero di core fisici e numero di processori logici (Thread).
  - Frequenza di clock massima e frequenza attuale (MHz).
  - Dimensione della memoria cache di Livello 2 (L2) e Livello 3 (L3) in MB.
  - Tipologia di socket del processore.
- **Memoria RAM & Moduli DIMM Fisici**:
  - Capacità totale, memoria occupata, memoria libera e percentuale di carico.
  - Conteggio e riassunto degli slot utilizzati rispetto agli slot totali della scheda madre (es. `2 / 4 Slots Used`).
  - Frequenza di clock e tipologia (DDR4 / DDR5).
  - Dimensione pool di memoria paginata e non paginata.
  - **Tabella di Ciascun Modulo DIMM Fisico Installato**:
    - Slot / Identificatore (es. `DIMM 0`, `BANK 1`).
    - Capacità per singolo banco (GB).
    - Frequenza di clock configurata (MHz).
    - Tipologia di memoria e form factor (DIMM / SO-DIMM).
    - Produttore del modulo (Crucial, Kingston, Corsair, Samsung, ecc.).
    - Codice parte (Part Number) e Numero di serie univoco.
- **Schede Video (GPU)**:
  - Nome della scheda video (NVIDIA GeForce, AMD Radeon, Intel Arc/UHD).
  - Produttore e versione esatta del driver grafico installato.
  - Data di rilascio del driver.
  - Memoria video dedicata (VRAM) in MB e GB.
  - Risoluzione attiva e frequenza di aggiornamento attuale dello schermo (Hz).
- **Scheda Madre & BIOS**:
  - Produttore e modello esatto della scheda madre.
  - Numero di versione e numero di serie.
  - Fornitore del BIOS, versione installata, data di rilascio e revisione SMBIOS.

### 13.2 Dischi di Archiviazione & Volumi
- Rilevamento dei dischi fisici con modello, tipo di interfaccia (NVMe, SATA, USB), tipo di supporto (SSD a stato solido o Hard Disk meccanico), capacità totale formattata e stato di integrità S.M.A.R.T. (OK).
- Elenco delle partizioni e volumi montati con spazio totale, occupato e libero.

### 13.3 Periferiche & Dispositivi Collegati
- **Dispositivi Audio**: schede audio, altoparlanti, cuffie e periferiche di cattura microfono.
- **Monitor e Schermi**: schermi connessi con risoluzioni native.
- **Dispositivi di Input**: tastiere, mouse e controller HID.
- **Controller di Rete**: schede di rete integrate e controller LAN/WLAN.

### 13.4 Diagnostica Batteria & Piani di Alimentazione
- Rilevamento automatico della presenza di batterie (per notebook e tablet) o modalità desktop (Alimentazione CA diretta).
- Percentuale di carica rimanente e stato di alimentazione (In carica, Disconnesso, Carica completa).
- **Capacità di Fabbrica vs Capacità Massima Attuale**:
  - Design Capacity (mWh) rispetto a Full Charge Capacity (mWh).
  - Calcolo della percentuale di usura e salute della batteria (*Battery Health %*).
- **Conteggio Cicli di Ricarica (Cycle Count)**:
  - Lettura del contatore di cicli di vita della batteria tramite query `root\wmi` (`BatteryCycleCount`).
- **Piano di Alimentazione Attivo**:
  - Rilevamento dello schema energetico attivo (Bilanciato, Prestazioni elevate, Risparmio energia, Prestazioni eccellenti).
- **Pulsante "Open Report (HTML)"**:
  - Genera in background il report diagnostico ufficiale di Windows (`powercfg /batteryreport`) e lo apre automaticamente nel browser web con grafici storici di scarica, stime di durata e statistiche complete.

---

## 14. Funzionalità Windows Opzionali & Virtualizzazione (DISM)

- **Gestione Funzionalità Opzionali tramite DISM (`FeaturesService.cs`)**:
  - Interrogazione dello stato effettivo delle funzionalità di sistema Windows.
  - Attivazione e disattivazione a 1 clic con parametro `/norestart` per evitare riavvii indesiderati durante il lavoro.
- **Componenti Gestiti nel Catalogo**:
  1. *Windows Sandbox* (`Containers-DisposableClientVM`): ambiente desktop isolato monouso per testare file e programmi sospetti in sicurezza.
  2. *Windows Subsystem for Linux (WSL)* (`Microsoft-Windows-Subsystem-Linux`): ambiente nativo per eseguire binari e distribuzioni Linux su Windows senza VM pesanti.
  3. *Piattaforma Macchina Virtuale* (`VirtualMachinePlatform`): piattaforma di virtualizzazione richiesta per WSL 2 e sottosistemi di isolamento.
  4. *Motore di Virtualizzazione Hyper-V* (`Microsoft-Hyper-V-All`): hypervisor enterprise nativo per la gestione di macchine virtuali Windows e Linux.
  5. *DirectPlay* (`DirectPlay`): componente di rete legacy DirectX 9 indispensabile per il corretto funzionamento di videogiochi PC classici (1998-2010).
  6. *Client Telnet* (`TelnetClient`): utility a riga di comando per testare connettività su porte TCP remote.
  7. *Client TFTP* (`TFTP`): protocollo leggero di trasferimento file utile per flashare firmware di router e dispositivi di rete.
  8. *Windows Media Player Legacy* (`WindowsMediaPlayer`): motore di riproduzione multimediale classico e codec DirectShow storici.

---

## 15. Profili di Configurazione & Preset (Setup Profiles)

- **Esportazione Configurazione Corrente**:
  - Salva lo stato completo di tutti gli 80 tweak del sistema in un file `.json` portatile, personalizzabile con nome e descrizione.
- **Importazione Profilo Personalizzato**:
  - Carica e applica profili JSON creati da altri utenti o esportati in precedenza, con conteggio trasparente dei tweak applicati, ripristinati ed eventuali falliti.
- **5 Profili Curati Integrati (Built-In Presets)**:
  1. **Recommended Setup (Consigliato)**: bilanciamento ottimale tra privacy, reattività della shell, disattivazione telemetria, rimozione throttling di rete e compattazione memoria, mantenendo intatto l'aspetto visivo originale di Windows.
  2. **Competitive Gaming Mode**: configurato per ottenere la minor latenza di input possibile (raw mouse 1:1, disattivazione Game DVR, MPO off, HPET off, prioritizzazione della GPU e CPU al gioco in primo piano).
  3. **Ultra Low-Latency & Raw Performance**: impostazione aggressiva per il massimo throughput hardware, zero jitter, rimozione di tutti i servizi di telemetria e timer invarianti.
  4. **Maximum Privacy & Debloat**: isolamento completo della privacy (telemetria off, Copilot off, Recall off, sensori di posizione bloccati, tracking app disattivato, promozioni Edge disattivate).
  5. **Clean Minimal Workstation**: ambiente di lavoro privo di distrazioni con barra delle applicazioni allineata a sinistra, Esplora File ripulito da cartelle cloud Home e Raccolta, tema scuro e scorciatoie pulite.

---

## 16. Generatore File di Risposta Installazione ISO (Autounattend.xml)

Permette di creare file di risposta `autounattend.xml` completamente automatizzati da salvare nella radice di una chiavetta USB di installazione di Windows 10 o Windows 11 (`AutounattendService.cs`):

- **Bypass Controlli Hardware Restrittivi di Windows 11 (`LabConfig`)**:
  - Bypass controllo TPM 2.0 (`BypassTPMCheck`)
  - Bypass controllo Secure Boot (`BypassSecureBootCheck`)
  - Bypass controllo memoria RAM minima (`BypassRAMCheck`)
  - Bypass controllo CPU supportata (`BypassCPUCheck`)
  - Bypass controllo spazio disco (`BypassStorageCheck`)
- **Bypass Obbligo Account Microsoft Online (`BypassNRO`)**:
  - Permette di completare l'esperienza OOBE di Windows 11 senza connettersi a internet e senza effettuare l'accesso con un account Microsoft, creando direttamente un account amministratore locale.
- **Personalizzazione Parametri di Sistema**:
  - Nome utente dell'account locale.
  - Password opzionale (o accesso senza password).
  - Abilitazione accesso automatico (Auto-Logon) al primo avvio.
  - Nome host del computer (ComputerName).
  - Selezione del fuso orario (UTC, Europa/Roma, Eastern, Pacific).
  - Lingua e layout di tastiera (Italiano, Inglese US, ecc.).
- **Pre-Applicazione Tweak al Primo Accesso (FirstLogonCommands)**:
  - Abilitazione tema scuro di sistema.
  - Ripristino menu contestuale classico di Windows 11.
  - Visualizzazione estensioni dei file conosciuti.
  - Disattivazione telemetria e diagnostica.
  - Blocco download automatico dei driver tramite Windows Update.
  - Abilitazione opzione "Termina attività" nella barra delle applicazioni.
- **Anteprima, Copia e Download del File XML**:
  - Editor con anteprima del codice XML formattato in tempo reale.
  - Pulsante per copiare l'intero file XML negli appunti.
  - Pulsante per scaricare direttamente il file con codifica UTF-8 senza BOM (`autounattend.xml`), pronto per essere incollato sulla chiavetta USB.

---

## 17. Punti di Ripristino, ChangeSet & Audit Logs

- **Creazione Punti di Ripristino del Sistema (VSS - Volume Shadow Copy)**:
  - Invocazione diretta della classe WMI `SystemRestore.CreateRestorePoint` con tipologia `MODIFY_SETTINGS`.
  - Fallback automatico su cmdlet PowerShell `Checkpoint-Computer` per garantire la creazione del punto di ripristino in qualsiasi ambiente.
- **Gestione ChangeSet & Rollback Snapshots**:
  - Ogni operazione di modifica registra automaticamente il valore precedente delle chiavi di registro coinvolte in file snapshot JSON salvati in `data/snapshots/`.
  - Tabella con visualizzazione di: ID ChangeSet, Data e ora esatta, Operazione scatenante, Numero di chiavi modificate.
  - **Pulsante di Rollback a 1 Clic**: legge lo snapshot e ripristina istantaneamente i valori precedenti del registro di sistema.
- **Registro di Audit & Operazioni (`AuditLogger.cs`)**:
  - Storico trasparente di ogni azione eseguita all'interno della suite.
  - Per ogni voce registra: Data e ora precisa al millisecondo, Categoria, Azione, Dettagli tecnici, Valore precedente, Nuovo valore applicato, Esito (OK / FAIL), Eventuale messaggio d'errore e stato privilegi amministratore.
  - Filtri per categoria (All, Tweaks, Debloat, Network, Repair, Installer, Profiles, Host).
  - Ricerca testuale rapida in tempo reale.
  - Salvataggio persistente su file di testo (`data/logs/audit.log`).
  - Pulsante per svuotare i log della sessione.

---

## 18. Impostazioni & Preferenze Piattaforma

- **Safe Test Mode (Dry-Run Preview)**:
  - Interruttore globale che attiva la modalità di simulazione sicura.
  - Quando abilitato, tutte le operazioni su registro, servizi e pacchetti vengono simulate e registrate a video senza applicare modifiche fisiche a chiavi di registro o file system, consentendo di visualizzare in anteprima l'effetto dei comandi.
- **Informazioni Server API Locale**:
  - Visualizzazione dell'URL di rilegatura locale con porta TCP attiva e token di sessione.
  - Pulsante rapido per aprire l'URL in un browser esterno.
- **Gestione Chiusura & Comportamento Finestra**:
  - Modale interattivo alla chiusura della finestra con scelta rapida:
    - *Riduci a icona nel System Tray*: mantiene l'applicazione attiva per notifiche e ottimizzazioni in background.
    - *Chiudi applicazione completamente*: termina tutti i processi, il server locale e rilascia i mutex.
- **Svuotamento Log**:
  - Pulsante per eliminare la cache di telemetria e svuotare i log di audit salvati su disco.

---

## 19. Command Palette Globale (Ctrl+K) & Interazione da Tastiera

- **Attivazione Istantanea (`Ctrl + K` / `Cmd + K`)**:
  - Apre una finestra modale in sovrimpressione ispirata a Linear e Raycast.
- **Indicizzazione Completa di Tutta l'Applicazione**:
  - Ricerca tra tutti i 19 moduli e schede dell'app.
  - Ricerca su tutti gli 80 tweak di sistema per nome, categoria o chiave.
  - Ricerca rapida per strumenti di riparazione (SFC, DISM, Chkdsk, Windows Update Reset).
  - Comandi diretti (Svuota DNS, Purge RAM, Genera report batteria, Apri file hosts).
- **Scorciatoie da Tastiera Supportate**:
  - `Ctrl + K`: apre la Command Palette.
  - `Esc`: chiude qualsiasi modale attiva, dialog o la Command Palette.
  - `F5` / `Ctrl + R`: aggiorna la vista attiva e i dati telemetrici.
  - `Alt + ←`: torna indietro nella cronologia delle viste.
  - `Alt + →`: avanza nella cronologia delle viste.

---

## 20. Modalità Silent, Interfaccia CLI & Parametri di Avvio

L'applicazione supporta parametri a riga di comando per l'automazione da parte di sistemisti e script batch:

- `--silent` / `-s`: avvia SUPOptimizer in modalità silenziosa headless (senza aprire l'interfaccia grafica), applica il profilo configurato e termina l'esecuzione.
- `--preset=<nome>` / `--apply-preset=<nome>`: specifica il profilo di automazione da applicare in modalità silent (es. `--preset=safe_optimize`, `--preset=gaming_mode`, `--preset=privacy_hardening`, `--preset=fresh_install`, `--preset=developer_workstation`).
- `--template=<percorso.json>`: applica un elenco personalizzato di ID di tweak specificati in un file JSON esterno.
- `--port=<numero>` / `--port <numero>`: forza l'avvio del server web locale su una porta TCP specifica invece di selezionarla casualmente.
- `--minimized` / `-m`: avvia l'applicazione direttamente ridotta a icona nel System Tray.

---

<div align="center">
  <sub>SUPOptimizer v1.0.2 — Documento Generato per la Trasparenza, Sicurezza e Ottimizzazione di Windows 10 & 11</sub>
</div>
