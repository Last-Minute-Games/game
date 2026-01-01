#!/bin/bash
# =============================================================================
# macOS Build Script for Unity Projects
# =============================================================================
# This script runs on a Linux X64 runner and uses Unity to cross-compile
# a macOS universal binary (Intel 64-bit + Apple Silicon).
#
# Usage: ./build.sh <UnityVersion> <ProjectPath> <OutputDir> <BuildTarget> <Version>
#
# Requirements:
#   - Unity Editor for Linux with "Mac Build Support (Mono)" module installed
#   - .NET SDK 6.0+ (for building the updater)
# =============================================================================

set -e

# =============================================================================
# Parameters
# =============================================================================
UNITY_VERSION="${1:-6000.2.2f1}"
PROJECT_PATH="${2:-$(cd "$(dirname "$0")/.." && pwd)}"
OUT_DIR="${3:-}"
BUILD_TARGET="${4:-macOS}"
VERSION="${5:-}"

# =============================================================================
# Find Unity Installation
# =============================================================================
UNITY=""
UNITY_PATHS=(
    "$HOME/Unity/Hub/Editor/${UNITY_VERSION}/Editor/Unity"
    "/opt/unity/Editor/${UNITY_VERSION}/Editor/Unity"
    "/usr/share/unity/Editor/${UNITY_VERSION}/Editor/Unity"
)

for path in "${UNITY_PATHS[@]}"; do
    if [ -f "$path" ]; then
        UNITY="$path"
        break
    fi
done

if [ -z "$UNITY" ] || [ ! -f "$UNITY" ]; then
    echo "❌ Unity ${UNITY_VERSION} not found. Checked paths:"
    for path in "${UNITY_PATHS[@]}"; do
        echo "   - $path"
    done
    exit 1
fi

UNITY_DIR="$(dirname "$UNITY")"

# =============================================================================
# Validate Build Target
# =============================================================================
BUILD_TARGET_LOWER=$(echo "$BUILD_TARGET" | tr '[:upper:]' '[:lower:]')
case "$BUILD_TARGET_LOWER" in
    "macos"|"osx")
        BUILD_METHOD="BuildScript.BuildMacOS"
        ;;
    *)
        echo "❌ This script only supports macOS builds."
        echo "   For Windows/Linux builds, use build.ps1 on a Windows runner."
        exit 1
        ;;
esac

# =============================================================================
# Setup Output Directory
# =============================================================================
if [ -z "$OUT_DIR" ]; then
    RUN_NUMBER="${GITHUB_RUN_NUMBER:-local}"
    OUT_DIR="$HOME/Builds/CastleOfTime-${RUN_NUMBER}-macOS"
fi

mkdir -p "$OUT_DIR"

# =============================================================================
# Define Log File
# =============================================================================
LOG_FILE="$OUT_DIR/unity-build-macOS.log"

# =============================================================================
# Check for Lock File
# =============================================================================
LOCK_FILE="$PROJECT_PATH/Temp/UnityLockfile"
if [ -f "$LOCK_FILE" ]; then
    echo "⚠️  Removing stale Unity lock file..."
    rm -f "$LOCK_FILE"
fi

# =============================================================================
# Print Build Info
# =============================================================================
echo "============================================="
echo "Unity macOS Build"
echo "============================================="
echo "Unity:         $UNITY"
echo "Unity Version: $UNITY_VERSION"
echo "Project:       $PROJECT_PATH"
echo "Output:        $OUT_DIR"
echo "Version:       ${VERSION:-<not specified>}"
echo "Build Method:  $BUILD_METHOD"
echo "Log File:      $LOG_FILE"
echo "============================================="

# =============================================================================
# Verify macOS Build Support is Installed
# =============================================================================
MAC_SUPPORT="$UNITY_DIR/Data/PlaybackEngines/MacStandaloneSupport"
if [ ! -d "$MAC_SUPPORT" ]; then
    echo "❌ macOS Build Support is not installed!"
    echo "   Install it via Unity Hub: Add Modules > Mac Build Support (Mono)"
    exit 1
fi

# Check for Mono variations (required for cross-compilation from Linux)
MONO_PLAYER="$MAC_SUPPORT/Variations/macos_x64arm64_player_nondevelopment_mono"
if [ ! -d "$MONO_PLAYER" ]; then
    echo "❌ macOS Mono player not found!"
    echo "   Make sure 'Mac Build Support (Mono)' is installed, not just IL2CPP."
    exit 1
fi

echo "✅ macOS Build Support (Mono) verified"

# =============================================================================
# Run Unity Build
# =============================================================================
echo ""
echo "Starting Unity build..."
echo ""

# Disable exit on error to capture Unity's exit code
set +e

# Build the command arguments
UNITY_ARGS=(
    -batchmode
    -nographics
    -quit
    -projectPath "$PROJECT_PATH"
    -logFile "$LOG_FILE"
    -executeMethod "$BUILD_METHOD"
    -customBuildPath "$OUT_DIR"
)

# Only add buildVersion if it's not empty
if [ -n "$VERSION" ]; then
    UNITY_ARGS+=(-buildVersion "$VERSION")
fi

echo "Running: $UNITY ${UNITY_ARGS[*]}"
echo ""

"$UNITY" "${UNITY_ARGS[@]}"

EXIT_CODE=$?

# Re-enable exit on error
set -e

# =============================================================================
# Handle Build Result
# =============================================================================
if [ $EXIT_CODE -ne 0 ]; then
    echo ""
    echo "❌ Unity macOS build failed with exit code: $EXIT_CODE"
    
    # Check for segfault
    if [ $EXIT_CODE -eq 139 ]; then
        echo ""
        echo "⚠️  Unity crashed with SIGSEGV (Segmentation fault)"
        echo ""
        echo "Common causes:"
        echo "  1. macOS Build Support module not properly installed"
        echo "  2. Missing system libraries"
        echo "  3. Unity bug with specific project configuration"
        echo ""
        echo "Try:"
        echo "  - Reinstall Mac Build Support (Mono) via Unity Hub"
        echo "  - Check: ldd $UNITY | grep 'not found'"
    fi
    
    echo ""
    echo "Waiting for log file to be written..."
    sleep 3
    
    if [ -f "$LOG_FILE" ]; then
        echo ""
        echo "=== Last 100 lines of Unity log ==="
        tail -n 100 "$LOG_FILE"
    else
        echo "Log file not found at: $LOG_FILE"
    fi
    
    exit $EXIT_CODE
fi

echo ""
echo "✅ macOS build completed successfully!"
echo "   Output: $OUT_DIR"

# =============================================================================
# Cleanup
# =============================================================================
echo ""
echo "Cleaning up build output..."

# Remove Unity backup folders
find "$OUT_DIR" -type d -name "*BackUpThisFolder_ButDontShipItWithYourGame*" -exec rm -rf {} + 2>/dev/null || true
echo "✅ Removed Unity backup folders"

# =============================================================================
# Run Post-Build Script
# =============================================================================
if [ -n "$VERSION" ]; then
    echo ""
    echo "Running post-build integration..."
    POST_BUILD_SCRIPT="$(dirname "$0")/post-build.sh"
    if [ -f "$POST_BUILD_SCRIPT" ]; then
        chmod +x "$POST_BUILD_SCRIPT"
        bash "$POST_BUILD_SCRIPT" "$OUT_DIR" "macOS" "$VERSION" "universal"
        if [ $? -ne 0 ]; then
            echo "⚠️  Post-build script failed, but Unity build succeeded"
        fi
    else
        echo "⚠️  Post-build script not found at: $POST_BUILD_SCRIPT"
    fi
fi

# =============================================================================
# Done
# =============================================================================
echo ""
echo "============================================="
echo "Build Complete"
echo "============================================="
echo "Output: $OUT_DIR"
if [ -d "$OUT_DIR" ]; then
    echo "Size:   $(du -sh "$OUT_DIR" | cut -f1)"
    echo "Files:  $(find "$OUT_DIR" -type f | wc -l)"
fi
echo "============================================="

