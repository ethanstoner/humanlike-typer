param()

$ErrorActionPreference = "Stop"

$installerDir = $PSScriptRoot
$repoRoot = Split-Path -Parent (Split-Path -Parent $installerDir)
$sourceLogo = Join-Path $repoRoot "assets\branding\humantype-logo-source.png"
$outputDir = Join-Path $installerDir "assets"
$wizardImage = Join-Path $outputDir "wizard-sidebar.bmp"
$smallImage = Join-Path $outputDir "wizard-header.bmp"

if (-not (Test-Path $sourceLogo)) {
    throw "Source logo not found at $sourceLogo"
}

$magick = (Get-Command magick -ErrorAction Stop).Source

New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

$sidebarArgs = @(
    "-size", "164x314",
    "xc:#10131a",
    "(",
    "-size", "164x314",
    "radial-gradient:#2b2f45-#10131a",
    "-rotate", "90",
    "-sigmoidal-contrast", "4,50%",
    ")",
    "-compose", "screen",
    "-composite",
    "(",
    "-size", "164x314",
    "xc:none",
    "-fill", "rgba(140,155,255,0.10)",
    "-draw", "circle 132,40 164,12",
    ")",
    "-compose", "screen",
    "-composite",
    "(",
    $sourceLogo,
    "-resize", "112x112",
    ")",
    "-gravity", "north",
    "-geometry", "+0+44",
    "-compose", "over",
    "-composite",
    "-font", "Segoe-UI-Bold",
    "-fill", "#F3F5FF",
    "-pointsize", "22",
    "-gravity", "north",
    "-annotate", "+0+182", "HumanType",
    "-font", "Segoe-UI",
    "-fill", "#AEB8DA",
    "-pointsize", "10",
    "-gravity", "north",
    "-annotate", "+0+214", "Natural typing automation",
    "-font", "Segoe-UI",
    "-fill", "#7F89A8",
    "-pointsize", "9",
    "-gravity", "south",
    "-annotate", "+0+18", "Installer",
    "BMP3:$wizardImage"
)

& $magick @sidebarArgs

$headerArgs = @(
    "-size", "55x55",
    "xc:#10131a",
    "(",
    "-size", "55x55",
    "radial-gradient:#272c3f-#10131a",
    "-rotate", "90",
    ")",
    "-compose", "screen",
    "-composite",
    "(",
    $sourceLogo,
    "-resize", "34x34",
    ")",
    "-gravity", "center",
    "-compose", "over",
    "-composite",
    "BMP3:$smallImage"
)

& $magick @headerArgs

Write-Host "Generated installer assets:"
Write-Host "  $wizardImage"
Write-Host "  $smallImage"
