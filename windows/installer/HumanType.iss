#ifndef AppVersion
  #define AppVersion "1.1.4"
#endif

#ifndef SourceExe
  #define SourceExe "..\\dist\\HumanType.exe"
#endif

#ifndef OutputDir
  #define OutputDir "..\\dist"
#endif

#define HumanTypeAppId "{{8EE6DF9A-0C5B-44B5-8C4E-54E3F4D3B754}"

[Setup]
AppId={#HumanTypeAppId}
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

[Code]
type
  TInstallMode = (imInstall, imRepair, imReinstall, imUninstall);

var
  ExistingInstallPage: TWizardPage;
  ExistingInstallLabel: TNewStaticText;
  RepairRadioButton: TNewRadioButton;
  ReinstallRadioButton: TNewRadioButton;
  UninstallRadioButton: TNewRadioButton;
  SelectedInstallMode: TInstallMode;
  ExistingUninstallString: string;
  ExistingInstallLocation: string;
  ExistingInstallDetected: Boolean;

function GetUninstallKeyName(): string;
begin
  // Hardcoded to avoid preprocessor double-brace expansion bug in Pascal Script strings.
  // Must match AppId in [Setup] section (without the outer curly-brace Inno escaping).
  Result := 'Software\Microsoft\Windows\CurrentVersion\Uninstall\{8EE6DF9A-0C5B-44B5-8C4E-54E3F4D3B754}_is1';
end;

procedure KillHumanTypeIfRunning();
var
  ResultCode: Integer;
begin
  // Silently kill any running HumanType.exe before install/uninstall.
  // Ignore errors — the process simply may not be running.
  Exec(ExpandConstant('{sys}\taskkill.exe'), '/F /IM HumanType.exe', '',
    SW_HIDE, ewWaitUntilTerminated, ResultCode);
end;

function TryReadExistingInstallData(const RootKey: Integer): Boolean;
begin
  Result :=
    RegQueryStringValue(RootKey, GetUninstallKeyName(), 'UninstallString', ExistingUninstallString) and
    RegQueryStringValue(RootKey, GetUninstallKeyName(), 'InstallLocation', ExistingInstallLocation);
end;

function DetectExistingInstall(): Boolean;
begin
  ExistingUninstallString := '';
  ExistingInstallLocation := '';
  Result := TryReadExistingInstallData(HKCU);
  if not Result then
  begin
    Result := TryReadExistingInstallData(HKLM);
  end;
end;

procedure InitializeWizard();
begin
  SelectedInstallMode := imInstall;
  ExistingInstallDetected := DetectExistingInstall();
  if not ExistingInstallDetected then
  begin
    exit;
  end;

  ExistingInstallPage := CreateCustomPage(
    wpWelcome,
    'HumanType Is Already Installed',
    'Choose what to do with the existing installation.');

  ExistingInstallLabel := TNewStaticText.Create(ExistingInstallPage);
  ExistingInstallLabel.Parent := ExistingInstallPage.Surface;
  ExistingInstallLabel.Left := ScaleX(0);
  ExistingInstallLabel.Top := ScaleY(0);
  ExistingInstallLabel.Width := ExistingInstallPage.SurfaceWidth;
  ExistingInstallLabel.Height := ScaleY(48);
  ExistingInstallLabel.AutoSize := False;
  ExistingInstallLabel.WordWrap := True;
  ExistingInstallLabel.Caption :=
    'HumanType is already installed at:' + #13#10 +
    ExistingInstallLocation + #13#10#13#10 +
    'Repair keeps the current installation and refreshes the program files. Reinstall removes the existing installation first and then installs a fresh copy. Uninstall removes HumanType and exits setup.';

  RepairRadioButton := TNewRadioButton.Create(ExistingInstallPage);
  RepairRadioButton.Parent := ExistingInstallPage.Surface;
  RepairRadioButton.Left := ScaleX(0);
  RepairRadioButton.Top := ExistingInstallLabel.Top + ExistingInstallLabel.Height + ScaleY(12);
  RepairRadioButton.Width := ExistingInstallPage.SurfaceWidth;
  RepairRadioButton.Caption := 'Repair the current installation';
  RepairRadioButton.Checked := True;

  ReinstallRadioButton := TNewRadioButton.Create(ExistingInstallPage);
  ReinstallRadioButton.Parent := ExistingInstallPage.Surface;
  ReinstallRadioButton.Left := ScaleX(0);
  ReinstallRadioButton.Top := RepairRadioButton.Top + RepairRadioButton.Height + ScaleY(10);
  ReinstallRadioButton.Width := ExistingInstallPage.SurfaceWidth;
  ReinstallRadioButton.Caption := 'Reinstall from scratch';

  UninstallRadioButton := TNewRadioButton.Create(ExistingInstallPage);
  UninstallRadioButton.Parent := ExistingInstallPage.Surface;
  UninstallRadioButton.Left := ScaleX(0);
  UninstallRadioButton.Top := ReinstallRadioButton.Top + ReinstallRadioButton.Height + ScaleY(10);
  UninstallRadioButton.Width := ExistingInstallPage.SurfaceWidth;
  UninstallRadioButton.Caption := 'Uninstall HumanType';
end;

function NextButtonClick(CurPageID: Integer): Boolean;
var
  ResultCode: Integer;
  UninstallArgs: string;
begin
  Result := True;

  if ExistingInstallDetected and (CurPageID = ExistingInstallPage.ID) then
  begin
    if UninstallRadioButton.Checked then
    begin
      SelectedInstallMode := imUninstall;
      KillHumanTypeIfRunning();
      UninstallArgs := '/VERYSILENT /SUPPRESSMSGBOXES /NORESTART';
      if not Exec(RemoveQuotes(ExistingUninstallString), UninstallArgs, '', SW_SHOWNORMAL, ewWaitUntilTerminated, ResultCode) then
      begin
        MsgBox('HumanType could not be uninstalled automatically.', mbError, MB_OK);
        Result := False;
        exit;
      end;

      if ResultCode <> 0 then
      begin
        MsgBox('HumanType uninstall did not complete successfully.', mbError, MB_OK);
        Result := False;
        exit;
      end;

      MsgBox('HumanType was uninstalled successfully.', mbInformation, MB_OK);
      WizardForm.Close;
      Result := False;
      exit;
    end;

    if ReinstallRadioButton.Checked then
    begin
      SelectedInstallMode := imReinstall;
    end
    else
    begin
      SelectedInstallMode := imRepair;
    end;
  end;
end;

function ShouldSkipPage(PageID: Integer): Boolean;
begin
  Result := False;
  if ExistingInstallDetected and (SelectedInstallMode = imUninstall) then
  begin
    Result := (PageID <> wpFinished);
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
  UninstallArgs: string;
begin
  if (CurStep = ssInstall) then
  begin
    KillHumanTypeIfRunning();
  end;

  if (CurStep = ssInstall) and ExistingInstallDetected and (SelectedInstallMode = imReinstall) then
  begin
    UninstallArgs := '/VERYSILENT /SUPPRESSMSGBOXES /NORESTART';
    if not Exec(RemoveQuotes(ExistingUninstallString), UninstallArgs, '', SW_SHOWNORMAL, ewWaitUntilTerminated, ResultCode) then
    begin
      RaiseException('HumanType could not remove the existing installation before reinstalling.');
    end;

    if ResultCode <> 0 then
    begin
      RaiseException('HumanType uninstall did not complete successfully before reinstalling.');
    end;
  end;
end;

function UpdateReadyMemo(Space, NewLine, MemoUserInfoInfo, MemoDirInfo, MemoTypeInfo,
  MemoComponentsInfo, MemoGroupInfo, MemoTasksInfo: String): String;
var
  InstallModeText: string;
begin
  case SelectedInstallMode of
    imRepair:
      InstallModeText := 'Repair existing installation';
    imReinstall:
      InstallModeText := 'Reinstall from scratch';
    else
      InstallModeText := 'Install HumanType';
  end;

  Result :=
    'Setup action:' + NewLine +
    Space + InstallModeText + NewLine + NewLine +
    MemoDirInfo + NewLine + NewLine +
    MemoTasksInfo;
end;
