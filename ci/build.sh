#!/bin/bash
# macOS build script for Unity projects

set -e

# Default parameters
UNITY_VERSION="${1:-6000.2.2f1}"
PROJECT_PATH="${2:-$(cd "$(dirname "$0")/.." && pwd)}"
OUT_DIR="${3:-}"
BUILD_TARGET="${4:-macOS}"
VERSION="${5:-}"
ARCHITECTURE="${6:-x64}"

# Unity installation path (macOS)
UNITY="/Applications/Unity/Hub/Editor/${UNITY_VERSION}/Unity.app/Contents/MacOS/Unity"

if [ ! -f "$UNITY" ]; then
    echo "❌ Unity not found at $UNITY"
    exit 1
fi

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
    echo "❌ Unity $BUILD_TARGET build ($ARCHITECTURE) failed with exit code: $EXIT_CODE"
    echo "Waiting 10 seconds for log file to be written..."
    sleep 10
    
    LOG_FILE="$OUT_DIR/unity-build-$BUILD_TARGET-$ARCHITECTURE.log"
    if [ -f "$LOG_FILE" ]; then
        echo "Tail of log:"
        tail -n 120 "$LOG_FILE"
    else
        echo "Log file not found at: $LOG_FILE"
    fi
    exit $EXIT_CODE
fi

echo "✅ $BUILD_TARGET build ($ARCHITECTURE) completed. Output: $OUT_DIR"
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
        bash "$POST_BUILD_SCRIPT" "$OUT_DIR" "$BUILD_TARGET" "$VERSION" "$ARCHITECTURE"
        if [ $? -ne 0 ]; then
            echo "⚠️  Post-build script failed, but Unity build succeeded"
        fi
    else
        echo "⚠️  Post-build script not found at: $POST_BUILD_SCRIPT"
    fi
else
    echo "⚠️  No version specified, skipping post-build (updater won't be included)"
fi

