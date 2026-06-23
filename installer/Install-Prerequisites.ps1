param(
    [string]$RedistributableDir = ""
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($RedistributableDir)) {
    $RedistributableDir = Join-Path $PSScriptRoot "redistributables"
}

$desktopRuntimeInstaller = Join-Path $RedistributableDir "windowsdesktop-runtime-8.0-win-x64.exe"
$windowsAppRuntimeInstaller = Join-Path $RedistributableDir "WindowsAppRuntimeInstall-x64.exe"

function Test-DotNetDesktopRuntime8 {
    try {
        $runtimes = & dotnet --list-runtimes 2>$null
        return ($runtimes | Where-Object { $_ -match "^Microsoft\.WindowsDesktop\.App 8\." }).Count -gt 0
    }
    catch {
        return $false
    }
}

function Test-WindowsAppRuntime {
    try {
        $packages = Get-AppxPackage -Name "Microsoft.WindowsAppRuntime.*" -ErrorAction SilentlyContinue
        return $null -ne $packages
    }
    catch {
        return $false
    }
}

if (-not (Test-DotNetDesktopRuntime8)) {
    if (-not (Test-Path $desktopRuntimeInstaller)) {
        throw ".NET 8 Desktop Runtime installer was not found: $desktopRuntimeInstaller"
    }

    Start-Process -FilePath $desktopRuntimeInstaller -ArgumentList "/install", "/quiet", "/norestart" -Wait
}

if (-not (Test-WindowsAppRuntime)) {
    if (-not (Test-Path $windowsAppRuntimeInstaller)) {
        throw "Windows App Runtime installer was not found: $windowsAppRuntimeInstaller"
    }

    Start-Process -FilePath $windowsAppRuntimeInstaller -ArgumentList "--quiet" -Wait
}
