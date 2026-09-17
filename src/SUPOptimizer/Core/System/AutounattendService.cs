using System;
using System.Text;
using SUPOptimizer.Core.Logging;

namespace SUPOptimizer.Core.System
{
    public class AutounattendOptions
    {
        // General / Region
        public string ComputerName { get; set; } = "SUP-PC";
        public string Username { get; set; } = "Admin";
        public string Password { get; set; } = "";
        public bool AutoLogon { get; set; } = true;
        public string Language { get; set; } = "en-US";
        public string KeyboardLocale { get; set; } = "0409:00000409";
        public string TimeZone { get; set; } = "W. Europe Standard Time";

        // Windows 11 Bypasses
        public bool BypassTpm { get; set; } = true;
        public bool BypassSecureBoot { get; set; } = true;
        public bool BypassRamAndCpu { get; set; } = true;
        public bool BypassStorage { get; set; } = true;
        public bool BypassMicrosoftAccount { get; set; } = true;

        // OOBE Privacy & Setup
        public bool DisableTelemetry { get; set; } = true;
        public bool EnableClassicContextMenu { get; set; } = true;
        public bool ShowFileExtensions { get; set; } = true;
        public bool EnableDarkMode { get; set; } = true;
        public bool DisableCopilot { get; set; } = true;
        public bool DisableStickyKeys { get; set; } = true;
    }

    public static class AutounattendService
    {
        public static string GenerateXml(AutounattendOptions options)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine("<unattend xmlns=\"urn:schemas-microsoft-com:unattend\">");

            // 1. windowsPE phase (Bypass checks & setup UI)
            sb.AppendLine("  <settings pass=\"windowsPE\">");
            sb.AppendLine("    <component name=\"Microsoft-Windows-International-Core-WinPE\" processorArchitecture=\"amd64\" publicKeyToken=\"31bf3856ad364e35\" language=\"neutral\" versionScope=\"nonSxS\" xmlns:wcm=\"http://schemas.microsoft.com/WMIConfig/2002/State\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
            sb.AppendLine($"      <SetupUILanguage><UILanguage>{options.Language}</UILanguage></SetupUILanguage>");
            sb.AppendLine($"      <InputLocale>{options.KeyboardLocale}</InputLocale>");
            sb.AppendLine($"      <SystemLocale>{options.Language}</SystemLocale>");
            sb.AppendLine($"      <UILanguage>{options.Language}</UILanguage>");
            sb.AppendLine($"      <UserLocale>{options.Language}</UserLocale>");
            sb.AppendLine("    </component>");

            sb.AppendLine("    <component name=\"Microsoft-Windows-Setup\" processorArchitecture=\"amd64\" publicKeyToken=\"31bf3856ad364e35\" language=\"neutral\" versionScope=\"nonSxS\" xmlns:wcm=\"http://schemas.microsoft.com/WMIConfig/2002/State\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
            sb.AppendLine("      <UserData>");
            sb.AppendLine("        <AcceptEula>true</AcceptEula>");
            sb.AppendLine("      </UserData>");

            // Inject LabConfig bypasses for Windows 11 hardware checks
            if (options.BypassTpm || options.BypassSecureBoot || options.BypassRamAndCpu || options.BypassStorage)
            {
                sb.AppendLine("      <RunSynchronous>");
                int order = 1;
                if (options.BypassTpm)
                {
                    sb.AppendLine($"        <RunSynchronousCommand wcm:action=\"add\"><Order>{order++}</Order><Path>reg add HKLM\\SYSTEM\\Setup\\LabConfig /v BypassTPMCheck /t REG_DWORD /d 1 /f</Path></RunSynchronousCommand>");
                }
                if (options.BypassSecureBoot)
                {
                    sb.AppendLine($"        <RunSynchronousCommand wcm:action=\"add\"><Order>{order++}</Order><Path>reg add HKLM\\SYSTEM\\Setup\\LabConfig /v BypassSecureBootCheck /t REG_DWORD /d 1 /f</Path></RunSynchronousCommand>");
                }
                if (options.BypassRamAndCpu)
                {
                    sb.AppendLine($"        <RunSynchronousCommand wcm:action=\"add\"><Order>{order++}</Order><Path>reg add HKLM\\SYSTEM\\Setup\\LabConfig /v BypassRAMCheck /t REG_DWORD /d 1 /f</Path></RunSynchronousCommand>");
                    sb.AppendLine($"        <RunSynchronousCommand wcm:action=\"add\"><Order>{order++}</Order><Path>reg add HKLM\\SYSTEM\\Setup\\LabConfig /v BypassCPUCheck /t REG_DWORD /d 1 /f</Path></RunSynchronousCommand>");
                }
                if (options.BypassStorage)
                {
                    sb.AppendLine($"        <RunSynchronousCommand wcm:action=\"add\"><Order>{order++}</Order><Path>reg add HKLM\\SYSTEM\\Setup\\LabConfig /v BypassStorageCheck /t REG_DWORD /d 1 /f</Path></RunSynchronousCommand>");
                }
                sb.AppendLine("      </RunSynchronous>");
            }

            sb.AppendLine("    </component>");
            sb.AppendLine("  </settings>");

            // 2. specialize phase (ComputerName, TimeZone)
            sb.AppendLine("  <settings pass=\"specialize\">");
            sb.AppendLine("    <component name=\"Microsoft-Windows-Shell-Setup\" processorArchitecture=\"amd64\" publicKeyToken=\"31bf3856ad364e35\" language=\"neutral\" versionScope=\"nonSxS\" xmlns:wcm=\"http://schemas.microsoft.com/WMIConfig/2002/State\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
            sb.AppendLine($"      <ComputerName>{options.ComputerName}</ComputerName>");
            sb.AppendLine($"      <TimeZone>{options.TimeZone}</TimeZone>");
            sb.AppendLine("    </component>");
            sb.AppendLine("  </settings>");

            // 3. oobeSystem phase (User account creation, auto-logon, and first boot tweaks)
            sb.AppendLine("  <settings pass=\"oobeSystem\">");
            sb.AppendLine("    <component name=\"Microsoft-Windows-International-Core\" processorArchitecture=\"amd64\" publicKeyToken=\"31bf3856ad364e35\" language=\"neutral\" versionScope=\"nonSxS\" xmlns:wcm=\"http://schemas.microsoft.com/WMIConfig/2002/State\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
            sb.AppendLine($"      <InputLocale>{options.KeyboardLocale}</InputLocale>");
            sb.AppendLine($"      <SystemLocale>{options.Language}</SystemLocale>");
            sb.AppendLine($"      <UILanguage>{options.Language}</UILanguage>");
            sb.AppendLine($"      <UserLocale>{options.Language}</UserLocale>");
            sb.AppendLine("    </component>");

            sb.AppendLine("    <component name=\"Microsoft-Windows-Shell-Setup\" processorArchitecture=\"amd64\" publicKeyToken=\"31bf3856ad364e35\" language=\"neutral\" versionScope=\"nonSxS\" xmlns:wcm=\"http://schemas.microsoft.com/WMIConfig/2002/State\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
            sb.AppendLine("      <OOBE>");
            sb.AppendLine("        <HideEULAPage>true</HideEULAPage>");
            sb.AppendLine("        <HideLocalAccountScreen>true</HideLocalAccountScreen>");
            sb.AppendLine("        <HideOnlineAccountScreens>true</HideOnlineAccountScreens>");
            sb.AppendLine("        <HideWirelessSetupInOOBE>true</HideWirelessSetupInOOBE>");
            sb.AppendLine("        <NetworkLocation>Work</NetworkLocation>");
            sb.AppendLine("        <ProtectYourPC>3</ProtectYourPC>");
            sb.AppendLine("      </OOBE>");

            // Local user creation
            sb.AppendLine("      <UserAccounts>");
            sb.AppendLine("        <LocalAccounts>");
            sb.AppendLine("          <LocalAccount wcm:action=\"add\">");
            sb.AppendLine($"            <Name>{options.Username}</Name>");
            sb.AppendLine($"            <DisplayName>{options.Username}</DisplayName>");
            sb.AppendLine("            <Group>Administrators</Group>");
            if (!string.IsNullOrEmpty(options.Password))
            {
                sb.AppendLine("            <Password>");
                sb.AppendLine($"              <Value>{options.Password}</Value>");
                sb.AppendLine("              <PlainText>true</PlainText>");
                sb.AppendLine("            </Password>");
            }
            else
            {
                sb.AppendLine("            <Password><Value></Value><PlainText>true</PlainText></Password>");
            }
            sb.AppendLine("          </LocalAccount>");
            sb.AppendLine("        </LocalAccounts>");
            sb.AppendLine("      </UserAccounts>");

            if (options.AutoLogon)
            {
                sb.AppendLine("      <AutoLogon>");
                sb.AppendLine($"        <Username>{options.Username}</Username>");
                sb.AppendLine("        <Enabled>true</Enabled>");
                sb.AppendLine("        <LogonCount>99999</LogonCount>");
                if (!string.IsNullOrEmpty(options.Password))
                {
                    sb.AppendLine("        <Password>");
                    sb.AppendLine($"          <Value>{options.Password}</Value>");
                    sb.AppendLine("          <PlainText>true</PlainText>");
                    sb.AppendLine("        </Password>");
                }
                sb.AppendLine("      </AutoLogon>");
            }

            // FirstLogonCommands to configure optimization tweaks
            sb.AppendLine("      <FirstLogonCommands>");
            int flIndex = 1;

            if (options.DisableTelemetry)
            {
                sb.AppendLine($"        <SynchronousCommand wcm:action=\"add\"><Order>{flIndex++}</Order><CommandLine>reg add \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows\\DataCollection\" /v AllowTelemetry /t REG_DWORD /d 0 /f</CommandLine></SynchronousCommand>");
                sb.AppendLine($"        <SynchronousCommand wcm:action=\"add\"><Order>{flIndex++}</Order><CommandLine>sc config DiagTrack start=disabled</CommandLine></SynchronousCommand>");
            }
            if (options.EnableClassicContextMenu)
            {
                sb.AppendLine($"        <SynchronousCommand wcm:action=\"add\"><Order>{flIndex++}</Order><CommandLine>reg add \"HKCU\\Software\\Classes\\CLSID\\{{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}}\\InprocServer32\" /ve /d \"\" /f</CommandLine></SynchronousCommand>");
            }
            if (options.ShowFileExtensions)
            {
                sb.AppendLine($"        <SynchronousCommand wcm:action=\"add\"><Order>{flIndex++}</Order><CommandLine>reg add \"HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Advanced\" /v HideFileExt /t REG_DWORD /d 0 /f</CommandLine></SynchronousCommand>");
            }
            if (options.EnableDarkMode)
            {
                sb.AppendLine($"        <SynchronousCommand wcm:action=\"add\"><Order>{flIndex++}</Order><CommandLine>reg add \"HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize\" /v AppsUseLightTheme /t REG_DWORD /d 0 /f</CommandLine></SynchronousCommand>");
                sb.AppendLine($"        <SynchronousCommand wcm:action=\"add\"><Order>{flIndex++}</Order><CommandLine>reg add \"HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize\" /v SystemUsesLightTheme /t REG_DWORD /d 0 /f</CommandLine></SynchronousCommand>");
            }
            if (options.DisableCopilot)
            {
                sb.AppendLine($"        <SynchronousCommand wcm:action=\"add\"><Order>{flIndex++}</Order><CommandLine>reg add \"HKCU\\Software\\Policies\\Microsoft\\Windows\\WindowsCopilot\" /v TurnOffWindowsCopilot /t REG_DWORD /d 1 /f</CommandLine></SynchronousCommand>");
            }
            if (options.DisableStickyKeys)
            {
                sb.AppendLine($"        <SynchronousCommand wcm:action=\"add\"><Order>{flIndex++}</Order><CommandLine>reg add \"HKCU\\Control Panel\\Accessibility\\StickyKeys\" /v Flags /t REG_SZ /d 506 /f</CommandLine></SynchronousCommand>");
            }

            sb.AppendLine("      </FirstLogonCommands>");
            sb.AppendLine("    </component>");
            sb.AppendLine("  </settings>");

            sb.AppendLine("</unattend>");

            AuditLogger.Log("Autounattend", "Generated XML Answer File", $"ComputerName: {options.ComputerName}, User: {options.Username}");
            return sb.ToString();
        }
    }
}
