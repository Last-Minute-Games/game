param(
  [string]$UnityVersion = "6000.2.2f1",
  [string]$ProjectPath = (Resolve-Path "$PSScriptRoot\..").Path,
  [string]$OutDir      = $null,    # <-- NEW: explicit output dir from CI (optional)
  [string]$BuildTarget = "Windows", # <-- NEW: Windows or Linux
  [string]$Version     = $null     # <-- NEW: Version string for version.txt
)

$unity = "C:\Program Files\Unity\Hub\Editor\$UnityVersion\Editor\Unity.exe"
if (-not (Test-Path $unity)) { throw "Unity not found at $unity" }

# Determine build method and target based on platform
$buildMethod = ""
$unityBuildTarget = ""
switch ($BuildTarget.ToLower()) {
  "windows" {
    $buildMethod = "BuildScript.BuildWindows"
    $unityBuildTarget = "StandaloneWindows64"
  }
  "linux" {
    $buildMethod = "BuildScript.BuildLinux"
    $unityBuildTarget = "StandaloneLinux64"
  }
  default {
    throw "Invalid BuildTarget: $BuildTarget. Must be 'Windows' or 'Linux'"
  }
}

# If CI passes an explicit folder, use it; otherwise use run number or fallback
if ([string]::IsNullOrWhiteSpace($OutDir)) {
  $runNumber = if ($Env:GITHUB_RUN_NUMBER) { $Env:GITHUB_RUN_NUMBER } else { "local" }
  $OutDir = "C:\Builds\CastleOfTime-$runNumber-$BuildTarget"
}
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

# Preflight: if the GUI editor is open on this project, fail fast
$lock = Join-Path $ProjectPath "Temp\UnityLockfile"
if (Get-Process Unity -ErrorAction SilentlyContinue) {
  if (Test-Path $lock) { throw "Project open in another Unity instance. Close it or build from a worktree." }
}

Write-Host "Starting Unity $BuildTarget build to $OutDir"

$process = Start-Process -FilePath $unity -ArgumentList @(
  "-batchmode",
  "-nographics",
  "-quit",
  "-projectPath", "`"$ProjectPath`"",
  "-buildTarget", $unityBuildTarget,
  "-logFile", "`"$OutDir\unity-build-$BuildTarget.log`"",
  "-stackTraceLogType", "Full",
  "-executeMethod", $buildMethod,
  "-customBuildPath", "`"$OutDir`"",
  "-buildVersion", "$Env:GITHUB_RUN_NUMBER"
) -PassThru -Wait

$exit = $process.ExitCode
if ($exit -ne 0) {
  Write-Host "❌ Unity $BuildTarget build failed with exit code: $exit"
  Write-Host "Waiting 10 seconds for log file to be written..."
  Start-Sleep -Seconds 10
  
  $logFile = "$OutDir\unity-build-$BuildTarget.log"
  if (Test-Path $logFile) {
    Write-Host "Tail of log:"
    Get-Content $logFile -Tail 120
  } else {
    Write-Host "Log file not found at: $logFile"
  }
  exit $exit
}

Write-Host "✅ $BuildTarget build completed. Output: $OutDir"
# Also emit the path for CI steps that want to read it
"$OutDir" | Out-File -FilePath "$env:GITHUB_WORKSPACE\_last_build_dir.txt" -Encoding ascii

# Run post-build script to add updater and version file
if (-not [string]::IsNullOrWhiteSpace($Version)) {
  Write-Host "`nRunning post-build integration..."
  $postBuildScript = Join-Path $PSScriptRoot "post-build.ps1"
  if (Test-Path $postBuildScript) {
    & $postBuildScript -BuildOutputDir $OutDir -Platform $BuildTarget -Version $Version
    if ($LASTEXITCODE -ne 0) {
      Write-Host "⚠️  Post-build script failed, but Unity build succeeded" -ForegroundColor Yellow
    }
  } else {
    Write-Host "⚠️  Post-build script not found at: $postBuildScript" -ForegroundColor Yellow
  }
} else {
  Write-Host "⚠️  No version specified, skipping post-build (updater won't be included)" -ForegroundColor Yellow
}
