# Runner Verification Script
# Run this on your self-hosted runner to verify all requirements are met

Write-Host "=== Castle of Time Runner Verification ===" -ForegroundColor Cyan
Write-Host ""

$allGood = $true

# 1. Check Unity
Write-Host "Checking Unity..." -ForegroundColor Yellow
$unityPath = "C:\Program Files\Unity\Hub\Editor\6000.2.2f1\Editor\Unity.exe"
if (Test-Path $unityPath) {
    Write-Host "  ✅ Unity 6000.2.2f1 found" -ForegroundColor Green
} else {
    Write-Host "  ❌ Unity 6000.2.2f1 NOT found at expected path" -ForegroundColor Red
    Write-Host "     Expected: $unityPath" -ForegroundColor Gray
    $allGood = $false
}

# Check Unity modules
$unityModulesPath = "C:\Program Files\Unity\Hub\Editor\6000.2.2f1\Editor\Data\PlaybackEngines"
if (Test-Path $unityModulesPath) {
    $hasWindows = Test-Path "$unityModulesPath\WindowsStandaloneSupport"
    $hasLinux = Test-Path "$unityModulesPath\LinuxStandaloneSupport"
    
    if ($hasWindows) {
        Write-Host "  ✅ Windows Build Support installed" -ForegroundColor Green
    } else {
        Write-Host "  ❌ Windows Build Support NOT installed" -ForegroundColor Red
        $allGood = $false
    }
    
    if ($hasLinux) {
        Write-Host "  ✅ Linux Build Support installed" -ForegroundColor Green
    } else {
        Write-Host "  ❌ Linux Build Support NOT installed" -ForegroundColor Red
        Write-Host "     Install via Unity Hub → Installs → Add Modules" -ForegroundColor Gray
        $allGood = $false
    }
}

# 2. Check .NET SDK
Write-Host "`nChecking .NET SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = & dotnet --version 2>$null
    if ($dotnetVersion) {
        $major = [int]($dotnetVersion -split '\.')[0]
        if ($major -ge 6) {
            Write-Host "  ✅ .NET SDK $dotnetVersion found" -ForegroundColor Green
        } else {
            Write-Host "  ⚠️  .NET SDK $dotnetVersion found (version 6.0+ recommended)" -ForegroundColor Yellow
            $allGood = $false
        }
    }
} catch {
    Write-Host "  ❌ .NET SDK NOT found" -ForegroundColor Red
    Write-Host "     Download from: https://dotnet.microsoft.com/download" -ForegroundColor Gray
    $allGood = $false
}

# 3. Check Git
Write-Host "`nChecking Git..." -ForegroundColor Yellow
try {
    $gitVersion = & git --version 2>$null
    if ($gitVersion) {
        Write-Host "  ✅ $gitVersion" -ForegroundColor Green
    }
} catch {
    Write-Host "  ❌ Git NOT found" -ForegroundColor Red
    Write-Host "     Download from: https://git-scm.com/download/win" -ForegroundColor Gray
    $allGood = $false
}

# 4. Check PowerShell
Write-Host "`nChecking PowerShell..." -ForegroundColor Yellow
$psVersion = $PSVersionTable.PSVersion
if ($psVersion.Major -ge 5) {
    Write-Host "  ✅ PowerShell $($psVersion.Major).$($psVersion.Minor) found" -ForegroundColor Green
} else {
    Write-Host "  ⚠️  PowerShell $($psVersion.Major).$($psVersion.Minor) (5.1+ recommended)" -ForegroundColor Yellow
}

# 5. Check Disk Space
Write-Host "`nChecking Disk Space..." -ForegroundColor Yellow
$drive = Get-PSDrive C
$freeGB = [math]::Round($drive.Free / 1GB, 2)
$usedGB = [math]::Round($drive.Used / 1GB, 2)
$totalGB = [math]::Round(($drive.Free + $drive.Used) / 1GB, 2)

Write-Host "  Drive C:" -ForegroundColor White
Write-Host "    Total: $totalGB GB" -ForegroundColor Gray
Write-Host "    Used:  $usedGB GB" -ForegroundColor Gray
Write-Host "    Free:  $freeGB GB" -ForegroundColor Gray

if ($freeGB -ge 50) {
    Write-Host "  ✅ Sufficient disk space" -ForegroundColor Green
} elseif ($freeGB -ge 30) {
    Write-Host "  ⚠️  Low disk space (50+ GB recommended)" -ForegroundColor Yellow
} else {
    Write-Host "  ❌ Insufficient disk space (50+ GB recommended)" -ForegroundColor Red
    $allGood = $false
}

# 6. Check Build Directory
Write-Host "`nChecking Build Directory..." -ForegroundColor Yellow
$buildsDir = "C:\Builds"
if (Test-Path $buildsDir) {
    Write-Host "  ✅ C:\Builds exists" -ForegroundColor Green
} else {
    Write-Host "  ⚠️  C:\Builds does not exist (will be created automatically)" -ForegroundColor Yellow
    try {
        New-Item -ItemType Directory -Force -Path $buildsDir | Out-Null
        Write-Host "  ✅ Created C:\Builds" -ForegroundColor Green
    } catch {
        Write-Host "  ❌ Failed to create C:\Builds" -ForegroundColor Red
        $allGood = $false
    }
}

# 7. Check GitHub Runner
Write-Host "`nChecking GitHub Actions Runner..." -ForegroundColor Yellow
$runnerServices = Get-Service -Name "actions.runner.*" -ErrorAction SilentlyContinue
if ($runnerServices) {
    foreach ($service in $runnerServices) {
        if ($service.Status -eq "Running") {
            Write-Host "  ✅ Runner service '$($service.Name)' is Running" -ForegroundColor Green
        } else {
            Write-Host "  ⚠️  Runner service '$($service.Name)' is $($service.Status)" -ForegroundColor Yellow
        }
    }
} else {
    Write-Host "  ⚠️  No GitHub Actions runner service found" -ForegroundColor Yellow
    Write-Host "     This is OK if running runner manually" -ForegroundColor Gray
}

# 8. Check Network Connectivity
Write-Host "`nChecking Network Connectivity..." -ForegroundColor Yellow
try {
    $null = Invoke-WebRequest -Uri "https://github.com" -UseBasicParsing -TimeoutSec 5
    Write-Host "  ✅ GitHub.com reachable" -ForegroundColor Green
} catch {
    Write-Host "  ❌ Cannot reach GitHub.com" -ForegroundColor Red
    $allGood = $false
}

try {
    $null = Invoke-WebRequest -Uri "https://itch.io" -UseBasicParsing -TimeoutSec 5
    Write-Host "  ✅ itch.io reachable" -ForegroundColor Green
} catch {
    Write-Host "  ⚠️  Cannot reach itch.io (check firewall)" -ForegroundColor Yellow
}

# 9. Check Updater Build Capability
Write-Host "`nChecking Updater Build..." -ForegroundColor Yellow
if (Test-Path ".\Updater\CastleOfTimeUpdater.csproj") {
    try {
        Push-Location ".\Updater"
        $null = & dotnet build --configuration Release 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✅ Updater builds successfully" -ForegroundColor Green
        } else {
            Write-Host "  ⚠️  Updater build had issues (check manually)" -ForegroundColor Yellow
        }
        Pop-Location
    } catch {
        Write-Host "  ⚠️  Could not test updater build" -ForegroundColor Yellow
        Pop-Location
    }
} else {
    Write-Host "  ⚠️  Updater project not found (run from repo root)" -ForegroundColor Yellow
}

# Summary
Write-Host "`n========================================" -ForegroundColor Cyan
if ($allGood) {
    Write-Host "✅ ALL CHECKS PASSED!" -ForegroundColor Green
    Write-Host "Your runner is ready to build Castle of Time!" -ForegroundColor Green
} else {
    Write-Host "⚠️  SOME CHECKS FAILED" -ForegroundColor Yellow
    Write-Host "Fix the issues above before running builds." -ForegroundColor Yellow
    Write-Host "See RUNNER_REQUIREMENTS.md for detailed installation instructions." -ForegroundColor Gray
}
Write-Host "========================================`n" -ForegroundColor Cyan

# Return exit code
if ($allGood) { exit 0 } else { exit 1 }
