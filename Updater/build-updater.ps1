# Build script for Castle of Time Updater
# Builds self-contained executables for Windows and Linux

param(
    [string]$Configuration = "Release"
)

Write-Host "=== Building Castle of Time Updater ===" -ForegroundColor Cyan

$UpdaterDir = $PSScriptRoot
$OutputDir = Join-Path $UpdaterDir "bin"

# Clean previous builds
if (Test-Path $OutputDir) {
    Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
    Remove-Item $OutputDir -Recurse -Force
}

# Build Windows version
Write-Host "`nBuilding Windows updater..." -ForegroundColor Green
dotnet publish $UpdaterDir `
    -c $Configuration `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:PublishTrimmed=true `
    -o "$OutputDir/Windows"

if ($LASTEXITCODE -ne 0) {
    Write-Error "Windows build failed!"
    exit 1
}

# Build Linux version
Write-Host "`nBuilding Linux updater..." -ForegroundColor Green
dotnet publish $UpdaterDir `
    -c $Configuration `
    -r linux-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:PublishTrimmed=true `
    -o "$OutputDir/Linux"

if ($LASTEXITCODE -ne 0) {
    Write-Error "Linux build failed!"
    exit 1
}

Write-Host "`nBuild complete!" -ForegroundColor Green
Write-Host "Windows updater: $OutputDir\Windows\CastleOfTimeUpdater.exe" -ForegroundColor Cyan
Write-Host "Linux updater:   $OutputDir\Linux\CastleOfTimeUpdater" -ForegroundColor Cyan

Write-Host "`nCopy these files to your Unity build output folders:" -ForegroundColor Yellow
Write-Host "  - CastleOfTimeUpdater.exe to Windows build root" -ForegroundColor White
Write-Host "  - CastleOfTimeUpdater to Linux build root" -ForegroundColor White
