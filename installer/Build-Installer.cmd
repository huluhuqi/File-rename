@echo off
setlocal EnableExtensions
cd /d "%~dp0"

echo Building File Organizer Master installer...
echo.

where powershell.exe >nul 2>nul
if errorlevel 1 (
    echo ERROR: powershell.exe was not found.
    pause
    exit /b 1
)

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Build-Release.ps1"

if errorlevel 1 (
    echo.
    echo ERROR: Installer build failed. Please read the message above.
    pause
    exit /b 1
)

echo.
echo DONE: Installer build completed.
pause
