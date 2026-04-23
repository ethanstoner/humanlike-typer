#ifndef AppVersion
  #define AppVersion "1.1.3"
#endif

#ifndef SourceExe
  #define SourceExe "..\\dist\\HumanType.exe"
#endif

#ifndef OutputDir
  #define OutputDir "..\\dist"
#endif

[Setup]
AppId={{8EE6DF9A-0C5B-44B5-8C4E-54E3F4D3B754}
AppName=HumanType
AppVersion={#AppVersion}
AppVerName=HumanType {#AppVersion}
AppPublisher=Ethan Stoner
AppPublisherURL=https://github.com/ethanstoner/humanlike-typer
AppSupportURL=https://github.com/ethanstoner/humanlike-typer/issues
AppUpdatesURL=https://github.com/ethanstoner/humanlike-typer/releases/latest
VersionInfoCompany=Ethan Stoner
VersionInfoDescription=HumanType Installer
VersionInfoProductName=HumanType
VersionInfoProductVersion={#AppVersion}
VersionInfoVersion={#AppVersion}
DefaultDirName={autopf}\HumanType
DefaultGroupName=HumanType
AllowNoIcons=yes
LicenseFile=..\..\LICENSE
OutputDir={#OutputDir}
OutputBaseFilename=HumanType-Installer
Compression=lzma
SolidCompression=yes
WizardStyle=modern
SetupIconFile=..\assets\HumanType.ico
WizardImageFile=assets\wizard-sidebar.bmp
WizardSmallImageFile=assets\wizard-header.bmp
UninstallDisplayIcon={app}\HumanType.exe
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
DisableProgramGroupPage=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked
Name: "startup"; Description: "Launch HumanType when I sign in"; GroupDescription: "Startup:"; Flags: unchecked

[Files]
Source: "{#SourceExe}"; DestDir: "{app}"; DestName: "HumanType.exe"; Flags: ignoreversion
Source: "..\assets\HumanType.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\README_WINDOWS.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\..\LICENSE"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\HumanType"; Filename: "{app}\HumanType.exe"; IconFilename: "{app}\HumanType.ico"
Name: "{autodesktop}\HumanType"; Filename: "{app}\HumanType.exe"; IconFilename: "{app}\HumanType.ico"; Tasks: desktopicon
Name: "{userstartup}\HumanType"; Filename: "{app}\HumanType.exe"; Tasks: startup

[Run]
Filename: "{app}\HumanType.exe"; Description: "Launch HumanType"; Flags: nowait postinstall skipifsilent
