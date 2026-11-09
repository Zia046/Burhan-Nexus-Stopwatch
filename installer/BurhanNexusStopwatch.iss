; Inno Setup script for Burhan Nexus Stopwatch
; Build:  ISCC.exe /DMyAppVersion=1.0.0 /DPublishDir=..\publish\win-x64 installer\BurhanNexusStopwatch.iss

#ifndef MyAppVersion
  #define MyAppVersion "1.0.0"
#endif
#ifndef PublishDir
  #define PublishDir "..\publish\win-x64"
#endif

#define MyAppName      "Burhan Nexus Stopwatch"
#define MyAppPublisher "Burhan Nexus"
#define MyAppExeName   "BurhanNexusStopwatch.exe"
#define MyAppContact   "ziaullah.046@gmail.com"
#define MyAppPhone     "+923475093446"

[Setup]
; Fixed AppId: lets future versions upgrade this installation in place.
AppId={{B7E3A1C4-5D2F-4E8A-9C61-3F0B8D2A7E15}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppContact={#MyAppContact}
AppSupportPhone={#MyAppPhone}
AppCopyright=Copyright (C) 2026 {#MyAppPublisher}. All rights reserved.
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription={#MyAppName} Setup
VersionInfoProductName={#MyAppName}

DefaultDirName={autopf}\{#MyAppPublisher}\{#MyAppName}
DefaultGroupName={#MyAppPublisher}
DisableProgramGroupPage=yes
UninstallDisplayName={#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}

; Install for all users (asks for admin) or just the current user.
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog

ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0

OutputDir=..\installer-output
OutputBaseFilename=BurhanNexusStopwatch-Setup-{#MyAppVersion}
SetupIconFile=..\src\BurhanNexusStopwatch\Assets\AppIcon.ico
WizardStyle=modern
WizardSmallImageFile=WizardSmall-55.bmp,WizardSmall-110.bmp
Compression=lzma2/max
SolidCompression=yes
CloseApplications=yes
RestartApplications=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "{#PublishDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#PublishDir}\*"; Excludes: "{#MyAppExeName},README.md"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs skipifsourcedoesntexist

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Comment: "Classic digital stopwatch for your desktop"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Comment: "Classic digital stopwatch for your desktop"; Tasks: desktopicon

[Registry]
; Remove the "Start with Windows" entry (if the user enabled it) when uninstalling.
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: none; ValueName: "BurhanNexusStopwatch"; Flags: uninsdeletevalue dontcreatekey

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent
