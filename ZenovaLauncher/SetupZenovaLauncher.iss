; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "Zenova Launcher"
#define MyAppVersion "1.0"
#define MyAppPublisher "MinecraftZenova"
#define MyAppURL "https://www.github.com/MinecraftZenova"
#define MyAppExeName "ZenovaLauncher.exe"
#define MyAppDataDir "Zenova"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{6A9E32A1-AF57-4B44-B093-08353E6D1C9A}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
ChangesAssociations=yes
DefaultDirName={autopf}\{#MyAppName}      
DisableWelcomePage=no
DisableProgramGroupPage=yes       
PrivilegesRequiredOverridesAllowed=dialog
OutputBaseFilename=ZenovaLauncher
SetupIconFile=Assets\zenova_icon.ico 
UninstallDisplayIcon={app}\{#MyAppExeName} 
WizardSmallImageFile=Assets\zenova_icon.bmp   
WizardStyle=modern
Compression=lzma
SolidCompression=yes   
ChangesEnvironment=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "bin\Release\ZenovaLauncher.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\WUTokenHelper.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\ControlzEx.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\MahApps.Metro.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\Microsoft.Expression.Interactions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\Microsoft.Xaml.Behaviors.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\ModernWpf.Controls.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\ModernWpf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\ModernWpf.MahApps.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\System.ValueTuple.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\System.Windows.Interactivity.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "bin\Release\Windows.Internal.Security.Authentication.Web.winmd"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Dirs]
Name: "{code:GetDataDir}\versions"
Name: "{code:GetDataDir}\mods"

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Registry]
Root: HKA; Subkey: "{code:GetSubKey}"; \
    ValueType: string; ValueName: "ZENOVA_DATA"; ValueData: "{code:GetDataDir}"; \
    Flags: uninsdeletevalue
Root: HKCR; Subkey: ".zmp"; ValueData: "{#MyAppName}"; \
    Flags: uninsdeletevalue; ValueType: string;  ValueName: ""
Root: HKCR; Subkey: "{#MyAppName}"; ValueData: "{#MyAppName}"; \
    Flags: uninsdeletekey;   ValueType: string;  ValueName: ""
Root: HKCR; Subkey: "{#MyAppName}\DefaultIcon"; \
    ValueData: "{app}\{#MyAppExeName},0"; ValueType: string;  ValueName: ""
Root: HKCR; Subkey: "{#MyAppName}\shell\open\command"; \
    ValueData: """{app}\{#MyAppExeName}"" ""%1""";  ValueType: string;  ValueName: ""

[Code]
var       
  DataDirPage: TInputDirWizardPage;

function GetDataDir(Value: string): string;
begin
  Result := DataDirPage.Values[0];
end;

function GetSubKey(Value: string): string;
begin
  if IsAdminInstallMode then
    Result := 'SYSTEM\CurrentControlSet\Control\Session Manager\Environment'
  else
    Result := 'Environment';
end;

procedure InitializeWizard;
var
  AfterID: Integer;
begin
  AfterID := wpSelectDir;

  DataDirPage := CreateInputDirPage(AfterID, 'Select App Data Location', 'Where should {#MyAppName} store Application Data?', 
  'Profile and Version files will be stored in the following folder.'#13#10#13#10 +
  'To continue, click Next. If you would like to select a different folder, click Browse.',
  False, 'New Folder');
  DataDirPage.Add('');
  DataDirPage.Values[0] := ExpandConstant('{%ZENOVA_DATA|{userappdata}\{#MyAppDataDir}}');;
  AfterID := DataDirPage.ID;
end;
