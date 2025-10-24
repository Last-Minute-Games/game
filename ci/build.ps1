param(
  [string]$UnityVersion = "6000.2.2f1",
  [string]$ProjectPath = (Resolve-Path "$PSScriptRoot\..").Path
)

$unity = "C:\Program Files\Unity\Hub\Editor\$UnityVersion\Editor\Unity.exe"
if (-not (Test-Path $unity)) { throw "Unity not found at $unity" }

# === Create timestamped output path ===
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$buildRoot = "C:\Builds"
$buildDir = Join-Path $buildRoot "Build_$timestamp"

# Ensure directories exist
New-Item -ItemType Directory -Force -Path $buildDir | Out-Null

# === Run Unity build ===
Write-Host "Starting Unity build to $buildDir"
& $unity `
  -batchmode -nographics -quit `
  -projectPath "$ProjectPath" `
  -logFile "$buildDir\unity-build.log" `
  -executeMethod BuildScript.BuildWindows `
  -customBuildPath "$buildDir"

$code = $LASTEXITCODE
if ($code -ne 0) {
  Write-Host "❌ Unity build failed. See log below:"
  Get-Content "$buildDir\unity-build.log" -Tail 50
  exit $code
}

Write-Host "`n✅ Build completed successfully!"
Write-Host "📦 Build output path: $buildDir"
