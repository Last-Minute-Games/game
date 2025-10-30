# Build script for Castle of Time Updater
# Builds self-contained executables for Windows and/or Linux

param(
    [string]$Configuration = "Release",
    [ValidateSet("Windows", "Linux", "Both")]
    [string]$Platform = "Both"
)

Write-Host "=== Building Castle of Time Updater ===" -ForegroundColor Cyan

$UpdaterDir = $PSScriptRoot
$ProjectFile = Join-Path $UpdaterDir "CastleOfTimeUpdater.csproj"
$OutputDir = Join-Path $UpdaterDir "bin"

# Clean previous builds for the target platform(s)
if ($Platform -eq "Both") {
    if (Test-Path $OutputDir) {
        Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
        Remove-Item $OutputDir -Recurse -Force
    }
} else {
    $platformDir = Join-Path $OutputDir $Platform
    if (Test-Path $platformDir) {
        Write-Host "Cleaning previous $Platform build..." -ForegroundColor Yellow
        Remove-Item $platformDir -Recurse -Force
    }
}

# Build Windows version
if ($Platform -eq "Windows" -or $Platform -eq "Both") {
    Write-Host "`nBuilding Windows updater..." -ForegroundColor Green
    dotnet publish $ProjectFile `
        -c $Configuration `
        -r win-x64 `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:PublishTrimmed=true `
        -p:Version=1.0.0 `
        -o "$OutputDir/Windows"

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Windows build failed!"
        exit 1
    }
    Write-Host "✅ Windows updater built: $OutputDir\Windows\CastleOfTimeUpdater.exe" -ForegroundColor Cyan
}

# Build Linux version
if ($Platform -eq "Linux" -or $Platform -eq "Both") {
    Write-Host "`nBuilding Linux updater..." -ForegroundColor Green
    dotnet publish $ProjectFile `
        -c $Configuration `
        -r linux-x64 `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:PublishTrimmed=true `
        -p:Version=1.0.0 `
        -o "$OutputDir/Linux"

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Linux build failed!"
        exit 1
    }
    Write-Host "✅ Linux updater built: $OutputDir\Linux\CastleOfTimeUpdater" -ForegroundColor Cyan
}

Write-Host "`n✅ Build complete!" -ForegroundColor Green
