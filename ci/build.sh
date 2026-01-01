#!/bin/bash
# macOS build script for Unity projects (runs on Linux, cross-compiles to macOS)

set -e

# Default parameters
UNITY_VERSION="${1:-6000.2.2f1}"
PROJECT_PATH="${2:-$(cd "$(dirname "$0")/.." && pwd)}"
OUT_DIR="${3:-}"
BUILD_TARGET="${4:-macOS}"
VERSION="${5:-}"
ARCHITECTURE="${6:-x64}"

# Detect OS and set Unity path accordingly
if [[ "$OSTYPE" == "darwin"* ]]; then
    # macOS
    UNITY="/Applications/Unity/Hub/Editor/${UNITY_VERSION}/Unity.app/Contents/MacOS/Unity"
else
    # Linux (cross-compile to macOS)
    UNITY="$HOME/Unity/Hub/Editor/${UNITY_VERSION}/Editor/Unity"
    # Alternative paths to check
    if [ ! -f "$UNITY" ]; then
        UNITY="/opt/unity/Editor/${UNITY_VERSION}/Editor/Unity"
    fi
    if [ ! -f "$UNITY" ]; then
        UNITY="/usr/share/unity/Editor/${UNITY_VERSION}/Editor/Unity"
    fi
fi

if [ ! -f "$UNITY" ]; then
    echo "❌ Unity not found. Checked paths:"
    if [[ "$OSTYPE" == "darwin"* ]]; then
        echo "   /Applications/Unity/Hub/Editor/${UNITY_VERSION}/Unity.app/Contents/MacOS/Unity"
    else
        echo "   $HOME/Unity/Hub/Editor/${UNITY_VERSION}/Editor/Unity"
        echo "   /opt/unity/Editor/${UNITY_VERSION}/Editor/Unity"
        echo "   /usr/share/unity/Editor/${UNITY_VERSION}/Editor/Unity"
    fi
    exit 1
fi

echo "Using Unity at: $UNITY"
echo "Host OS: $OSTYPE"

# Determine build method and target based on platform
# Convert to lowercase for comparison (compatible with bash 3.2 on macOS)
BUILD_TARGET_LOWER=$(echo "$BUILD_TARGET" | tr '[:upper:]' '[:lower:]')
case "$BUILD_TARGET_LOWER" in
    "macos"|"osx")
        BUILD_METHOD="BuildScript.BuildMacOS"
        UNITY_BUILD_TARGET="StandaloneOSX"
        ;;
    *)
        echo "❌ Invalid BuildTarget: $BUILD_TARGET. Must be 'macOS' or 'OSX'"
        exit 1
        ;;
esac

# If CI passes an explicit folder, use it; otherwise use run number or fallback
if [ -z "$OUT_DIR" ]; then
    RUN_NUMBER="${GITHUB_RUN_NUMBER:-local}"
    OUT_DIR="$HOME/Builds/CastleOfTime-${RUN_NUMBER}-${BUILD_TARGET}-${ARCHITECTURE}"
fi

mkdir -p "$OUT_DIR"

# Preflight: if the GUI editor is open on this project, fail fast
LOCK_FILE="$PROJECT_PATH/Temp/UnityLockfile"
if pgrep -x "Unity" > /dev/null; then
    if [ -f "$LOCK_FILE" ]; then
        echo "❌ Project open in another Unity instance. Close it or build from a worktree."
        exit 1
    fi
fi

echo "Starting Unity $BUILD_TARGET build ($ARCHITECTURE) to $OUT_DIR"

# Run Unity build with architecture parameter
"$UNITY" \
    -batchmode \
    -nographics \
    -quit \
    -projectPath "$PROJECT_PATH" \
    -buildTarget "$UNITY_BUILD_TARGET" \
    -logFile "$OUT_DIR/unity-build-$BUILD_TARGET-$ARCHITECTURE.log" \
    -stackTraceLogType Full \
    -executeMethod "$BUILD_METHOD" \
    -customBuildPath "$OUT_DIR" \
    -buildArchitecture "$ARCHITECTURE" \
    -buildVersion "${GITHUB_RUN_NUMBER:-}"

EXIT_CODE=$?

if [ $EXIT_CODE -ne 0 ]; then
    if [ -n "$ARCHITECTURE" ] && [ "$ARCHITECTURE" != "" ]; then
        echo "❌ Unity $BUILD_TARGET build ($ARCHITECTURE) failed with exit code: $EXIT_CODE"
    else
        echo "❌ Unity $BUILD_TARGET build (Universal) failed with exit code: $EXIT_CODE"
    fi
    echo "Waiting 10 seconds for log file to be written..."
    sleep 10
    if [ -f "$LOG_FILE" ]; then
        echo "Tail of log:"
        tail -n 120 "$LOG_FILE"
    else
        echo "Log file not found at: $LOG_FILE"
    fi
    exit $EXIT_CODE
fi

if [ -n "$ARCHITECTURE" ] && [ "$ARCHITECTURE" != "" ]; then
    echo "✅ $BUILD_TARGET build ($ARCHITECTURE) completed. Output: $OUT_DIR"
else
    echo "✅ $BUILD_TARGET build (Universal) completed. Output: $OUT_DIR"
fi

# Remove Unity backup folder that shouldn't be shipped
echo ""
echo "Removing Unity backup folder..."
find "$OUT_DIR" -type d -name "*BackUpThisFolder_ButDontShipItWithYourGame*" -exec rm -rf {} + 2>/dev/null || true
if [ $? -eq 0 ]; then
    echo "✅ Cleaned up backup folder"
fi

# Also emit the path for CI steps that want to read it
if [ -n "$GITHUB_WORKSPACE" ]; then
    echo "$OUT_DIR" > "$GITHUB_WORKSPACE/_last_build_dir.txt"
fi

# Run post-build script to add updater and version file
if [ -n "$VERSION" ]; then
    echo ""
    echo "Running post-build integration..."
    POST_BUILD_SCRIPT="$(dirname "$0")/post-build.sh"
    if [ -f "$POST_BUILD_SCRIPT" ]; then
        # Pass architecture only if specified, otherwise pass "universal"
        ARCH_PARAM="${ARCHITECTURE:-universal}"
        bash "$POST_BUILD_SCRIPT" "$OUT_DIR" "$BUILD_TARGET" "$VERSION" "$ARCH_PARAM"
        if [ $? -ne 0 ]; then
            echo "⚠️  Post-build script failed, but Unity build succeeded"
        fi
    else
        echo "⚠️  Post-build script not found at: $POST_BUILD_SCRIPT"
    fi
else
    echo "⚠️  No version specified, skipping post-build (updater won't be included)"
fi

