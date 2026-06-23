# 随包运行环境文件

Place the following file in this folder:

1. WindowsAppRuntimeInstall-x64.exe
   - Download from Microsoft Windows App SDK official page
   - https://learn.microsoft.com/windows/apps/windows-app-sdk/downloads-archive
   - This will be bundled into the installer and installed silently for the end user

Note: .NET 8 runtime is NO LONGER needed here because the project is now published as self-contained (--self-contained true), which bundles the .NET runtime into the output folder.

Only ONE file is required:
  WindowsAppRuntimeInstall-x64.exe
