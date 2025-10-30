# Post-build script to add updater and version file to Unity builds
# Call this after Unity completes the build

param(
    [Parameter(Mandatory=$true)]
    [string]$BuildOutputDir,
    
    [Parameter(Mandatory=$true)]
    [ValidateSet("Windows", "Linux")]
    [string]$Platform,
    
    [Parameter(Mandatory=$true)]
    [string]$Version
)

Write-Host "=== Post-Build: Adding Updater ===" -ForegroundColor Cyan
Write-Host "Platform: $Platform" -ForegroundColor Yellow
Write-Host "Version:  $Version" -ForegroundColor Yellow
Write-Host "Output:   $BuildOutputDir" -ForegroundColor Yellow

$ProjectRoot = Split-Path $PSScriptRoot -Parent

# 1. Write version file
$versionFile = Join-Path $BuildOutputDir "version.txt"
Set-Content -Path $versionFile -Value $Version -NoNewline
Write-Host "✅ Created version.txt" -ForegroundColor Green

# 2. Copy updater executable
$updaterDir = Join-Path $ProjectRoot "Updater\bin\$Platform"
$updaterExe = if ($Platform -eq "Windows") { "CastleOfTimeUpdater.exe" } else { "CastleOfTimeUpdater" }
$updaterSrc = Join-Path $updaterDir $updaterExe
$updaterDst = Join-Path $BuildOutputDir $updaterExe

if (Test-Path $updaterSrc) {
    Copy-Item $updaterSrc -Destination $updaterDst -Force
    Write-Host "✅ Copied updater: $updaterExe" -ForegroundColor Green
    
    # Set executable permissions on Linux
    if ($Platform -eq "Linux") {
        # This assumes you're building on a system with chmod available
        # On Windows, the permissions will be set when the user runs it on Linux
        try {
            & chmod +x $updaterDst 2>$null
        } catch {
            Write-Host "⚠️  Could not set +x permission (will be set on Linux)" -ForegroundColor Yellow
        }
    }
} else {
    Write-Host "❌ Updater not found at: $updaterSrc" -ForegroundColor Red
    Write-Host "   Run 'Updater\build-updater.ps1' first!" -ForegroundColor Yellow
    exit 1
}

# 3. Clean up cross-platform files
Write-Host "`nCleaning cross-platform files..." -ForegroundColor Cyan

if ($Platform -eq "Windows") {
    # Remove Linux-specific files from Windows build
    $linuxUpdater = Join-Path $BuildOutputDir "CastleOfTimeUpdater"
    if (Test-Path $linuxUpdater) {
        Remove-Item $linuxUpdater -Force
        Write-Host "  Removed Linux updater from Windows build" -ForegroundColor Gray
    }
    
    # Remove .so files (Linux shared libraries)
    Get-ChildItem -Path $BuildOutputDir -Recurse -Filter "*.so*" | ForEach-Object {
        Remove-Item $_.FullName -Force
        Write-Host "  Removed: $($_.Name)" -ForegroundColor Gray
    }
    
    # Remove Linux executables
    Get-ChildItem -Path $BuildOutputDir -Recurse -Filter "*.x86_64" | ForEach-Object {
        Remove-Item $_.FullName -Force
        Write-Host "  Removed: $($_.Name)" -ForegroundColor Gray
    }
}
elseif ($Platform -eq "Linux") {
    # Remove Windows-specific files from Linux build
    $windowsUpdater = Join-Path $BuildOutputDir "CastleOfTimeUpdater.exe"
    if (Test-Path $windowsUpdater) {
        Remove-Item $windowsUpdater -Force
        Write-Host "  Removed Windows updater from Linux build" -ForegroundColor Gray
    }
    
    # Note: Unity should not include .dll/.exe in Linux builds by default
    # But we'll clean them just in case
    Get-ChildItem -Path $BuildOutputDir -Recurse -Filter "*.exe" | Where-Object { $_.Name -ne "CastleOfTime.exe" } | ForEach-Object {
        Remove-Item $_.FullName -Force
        Write-Host "  Removed: $($_.Name)" -ForegroundColor Gray
    }
}
    exit 1
}

Write-Host "`n✅ Post-build complete!" -ForegroundColor Green
