param(
    [string]$Version = "1.1.1"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$iconPath = Join-Path $repoRoot "windows\assets\HumanType.ico"
$nativeProject = Join-Path $repoRoot "windows\HumanType.Native\HumanType.Native.csproj"
$installerScript = Join-Path $repoRoot "windows\installer\HumanType.iss"
$installerAssetScript = Join-Path $repoRoot "windows\installer\generate-assets.ps1"
$distDir = Join-Path $repoRoot "windows\dist"
$exePath = Join-Path $distDir "HumanType.exe"
$programFilesX86 = [Environment]::GetFolderPath("ProgramFilesX86")
$localPrograms = Join-Path $env:LOCALAPPDATA "Programs"

function Resolve-RequiredTool {
    param(
        [string[]]$Candidates,
        [string]$ToolName
    )

    foreach ($candidate in $Candidates) {
        if ($candidate -and (Test-Path $candidate)) {
            return (Resolve-Path $candidate).Path
        }
    }

    throw "$ToolName was not found. Install it or adjust the candidate paths in windows/build-release.ps1."
}

$innoCompiler = Resolve-RequiredTool -ToolName "Inno Setup (ISCC.exe)" -Candidates @(
    (Join-Path $programFilesX86 "Inno Setup 6\ISCC.exe"),
    "$env:ProgramFiles\Inno Setup 6\ISCC.exe",
    (Join-Path $localPrograms "Inno Setup 6\ISCC.exe")
)

New-Item -ItemType Directory -Force -Path $distDir | Out-Null
Get-ChildItem $distDir -Force -ErrorAction SilentlyContinue | Remove-Item -Force -Recurse

Write-Host "Generating branded installer artwork..."
& powershell -ExecutionPolicy Bypass -File $installerAssetScript
if ($LASTEXITCODE -ne 0) {
    throw "Installer asset generation failed with exit code $LASTEXITCODE."
}

Write-Host "Publishing native HumanType.exe..."
& dotnet publish $nativeProject -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:EnableCompressionInSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:DebugSymbols=false /p:DebugType=None /p:Version=$Version /p:AssemblyVersion=$Version.0 /p:FileVersion=$Version.0 /p:InformationalVersion=$Version -o $distDir
if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

if (-not (Test-Path $exePath)) {
    throw "Expected published executable was not created at $exePath."
}

Copy-Item -LiteralPath $iconPath -Destination (Join-Path $distDir "HumanType.ico") -Force

$publishedPdb = Join-Path $distDir "HumanType.pdb"
if (Test-Path $publishedPdb) {
    Remove-Item -LiteralPath $publishedPdb -Force
}

Write-Host "Building HumanType-Installer.exe with Inno Setup..."
& $innoCompiler "/DAppVersion=$Version" "/DSourceExe=$exePath" "/DOutputDir=$distDir" $installerScript
if ($LASTEXITCODE -ne 0) {
    throw "Inno Setup failed with exit code $LASTEXITCODE."
}

Write-Host "Build complete:"
Write-Host "  $exePath"
Write-Host "  $(Join-Path $distDir 'HumanType-Installer.exe')"
