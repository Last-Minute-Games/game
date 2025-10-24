param(
  [string]$UnityVersion = "6000.2.2f1",
  [string]$ProjectPath = (Resolve-Path "$PSScriptRoot\..").Path,
  [string]$OutDir      = $null    # <-- NEW: explicit output dir from CI (optional)
)

$unity = "C:\Program Files\Unity\Hub\Editor\$UnityVersion\Editor\Unity.exe"
if (-not (Test-Path $unity)) { throw "Unity not found at $unity" }

# If CI passes an explicit folder, use it; otherwise make a timestamped one.
if ([string]::IsNullOrWhiteSpace($OutDir)) {
  $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
  $OutDir = "C:\Builds\Build_$timestamp"
}
New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

# Preflight: if the GUI editor is open on this project, fail fast
$lock = Join-Path $ProjectPath "Temp\UnityLockfile"
if (Get-Process Unity -ErrorAction SilentlyContinue) {
  if (Test-Path $lock) { throw "Project open in another Unity instance. Close it or build from a worktree." }
}

Write-Host "Starting Unity build to $OutDir"

& $unity `
  -batchmode -nographics -quit `
  -projectPath "$ProjectPath" `
  -buildTarget StandaloneWindows64 `
  -logFile "$OutDir\unity-build.log" -stackTraceLogType Full `
  -executeMethod BuildScript.BuildWindows `
  -customBuildPath "$OutDir" `
  -buildVersion $Env:GITHUB_RUN_NUMBER

$exit = $LASTEXITCODE
if ($exit -ne 0) {
  Write-Host "❌ Unity build failed with exit code: $exit"
  Write-Host "Waiting 10 seconds for log file to be written..."
  Start-Sleep -Seconds 10
  
  $logFile = "$OutDir\unity-build.log"
  if (Test-Path $logFile) {
    Write-Host "Tail of log:"
    Get-Content $logFile -Tail 120
  } else {
    Write-Host "Log file not found at: $logFile"
  }
  exit $exit
}

Write-Host "✅ Build completed. Output: $OutDir"
# Also emit the path for CI steps that want to read it
"$OutDir" | Out-File -FilePath "$env:GITHUB_WORKSPACE\_last_build_dir.txt" -Encoding ascii
