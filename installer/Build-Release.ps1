$ErrorActionPreference = "Stop"

$installerDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $installerDir
$projectPath = Join-Path $rootDir "src\FileRenameAssistant\FileRenameAssistant.csproj"
$publishDir = Join-Path $rootDir "artifacts\publish\win-x64"
$installerOutDir = Join-Path $rootDir "artifacts\installer"
$issPath = Join-Path $installerDir "FileRenameAssistant.iss"

function Write-Step([string]$Text) {
    Write-Host ""
    Write-Host "==> $Text" -ForegroundColor Cyan
}

function Find-Iscc {
    $candidates = @(
        "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
        "C:\Program Files\Inno Setup 6\ISCC.exe"
    )

    foreach ($path in $candidates) {
        if (Test-Path $path) {
            return $path
        }
    }

    $command = Get-Command "ISCC.exe" -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    return $null
}

function Assert-File([string]$Path, [string]$Message) {
    if (-not (Test-Path $Path)) {
        throw "$Message Missing file: $Path"
    }
}

Write-Step "Check build tools"

$dotnet = Get-Command "dotnet.exe" -ErrorAction SilentlyContinue
if (-not $dotnet) {
    throw "dotnet.exe was not found. Install .NET 8 SDK and run this script again."
}

$iscc = Find-Iscc
if (-not $iscc) {
    throw "ISCC.exe was not found. Install Inno Setup 6 or add ISCC.exe to PATH."
}

Write-Host "dotnet: $($dotnet.Source)"
Write-Host "ISCC: $iscc"

Write-Step "Clean old artifacts"
Remove-Item (Join-Path $rootDir "artifacts") -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $publishDir -Force | Out-Null
New-Item -ItemType Directory -Path $installerOutDir -Force | Out-Null

Write-Step "Restore NuGet packages"
dotnet restore $projectPath
if ($LASTEXITCODE -ne 0) {
    throw "dotnet restore failed."
}

Write-Step "Publish WPF application (self-contained, includes .NET runtime)"
dotnet publish $projectPath `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=false `
    -p:PublishReadyToRun=true `
    -o $publishDir

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed."
}

Assert-File (Join-Path $publishDir "FileRenameAssistant.exe") "Published application was not found."

Write-Step "Build installer"
Push-Location $installerDir
try {
    & $iscc $issPath
    if ($LASTEXITCODE -ne 0) {
        throw "Inno Setup failed."
    }
}
finally {
    Pop-Location
}

$setupPath = Get-ChildItem $installerOutDir -Filter "*.exe" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $setupPath) {
    throw "Installer exe was not found after build."
}

Write-Step "Done"
Write-Host "Installer generated: $($setupPath.FullName)" -ForegroundColor Green
