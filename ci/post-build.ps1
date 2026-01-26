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

Write-Host "\nRemoving Unity 'Do Not Ship' folders..." -ForegroundColor Cyan

# Common Unity debug/backup folders that should not be shipped
$doNotShipPatterns = @(
    "*_BurstDebugInformation_DoNotShip",
    "*BackUpThisFolder_ButDontShipItWithYourGame*",
    "BackUpThisFolder_ButDontShipItWithYourGame",
    "*DoNotShip*",
    "*donotship*",
    "Castle of Time_BurstDebugInformation_DoNotShip"
)

foreach ($pattern in $doNotShipPatterns) {
    Get-ChildItem -Path $BuildOutputDir -Recurse -Directory -Force -ErrorAction SilentlyContinue | Where-Object { $_.Name -like $pattern } | ForEach-Object {
        try {
            Remove-Item -LiteralPath $_.FullName -Recurse -Force -ErrorAction Stop
            Write-Host "  Removed DoNotShip folder: $($_.FullName)" -ForegroundColor Gray
        } catch {
            Write-Host "  ⚠️  Failed to remove: $($_.FullName) - $($_.Exception.Message)" -ForegroundColor Yellow
        }
    }
}

Write-Host "`n✅ Post-build complete!" -ForegroundColor Green
