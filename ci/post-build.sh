#!/bin/bash
# Post-build script to add updater and version file to Unity builds (macOS)
# Call this after Unity completes the build

set -e

if [ $# -lt 3 ]; then
    echo "Usage: $0 <BuildOutputDir> <Platform> <Version> [Architecture]"
    exit 1
fi

BUILD_OUTPUT_DIR="$1"
PLATFORM="$2"
VERSION="$3"
ARCHITECTURE="${4:-x64}"

if [ "$PLATFORM" != "macOS" ] && [ "$PLATFORM" != "OSX" ]; then
    echo "❌ Invalid Platform: $PLATFORM. Must be 'macOS' or 'OSX'"
    exit 1
fi

# Normalize architecture label
if [ "$ARCHITECTURE" = "ARM64" ] || [ "$ARCHITECTURE" = "arm64" ]; then
    ARCH_LABEL="ARM64"
elif [ "$ARCHITECTURE" = "x64" ] || [ "$ARCHITECTURE" = "x86_64" ]; then
    ARCH_LABEL="x64"
else
    echo "⚠️  Unknown architecture '$ARCHITECTURE', defaulting to x64"
    ARCH_LABEL="x64"
fi

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "=== Post-Build: Adding Updater ==="
echo "Platform:     $PLATFORM"
echo "Architecture: $ARCH_LABEL"
echo "Version:      $VERSION"
echo "Output:       $BUILD_OUTPUT_DIR"

# 1. Write version file
VERSION_FILE="$BUILD_OUTPUT_DIR/version.txt"
echo -n "$VERSION" > "$VERSION_FILE"
echo "✅ Created version.txt"

# 2. Copy updater executable (architecture-specific)
UPDATER_DIR="$PROJECT_ROOT/Updater/bin/macOS-$ARCH_LABEL"
UPDATER_EXE="CastleOfTimeUpdater"
UPDATER_SRC="$UPDATER_DIR/$UPDATER_EXE"
UPDATER_DST="$BUILD_OUTPUT_DIR/$UPDATER_EXE"

if [ -f "$UPDATER_SRC" ]; then
    cp "$UPDATER_SRC" "$UPDATER_DST"
    chmod +x "$UPDATER_DST"
    echo "✅ Copied updater ($ARCH_LABEL): $UPDATER_EXE"
else
    echo "❌ Updater not found at: $UPDATER_SRC"
    echo "   Run 'Updater/build-updater.sh Release macOS $ARCH_LABEL' first!"
    exit 1
fi

# 3. Clean up cross-platform files
echo ""
echo "Cleaning cross-platform files..."

# Remove Windows-specific files from macOS build
find "$BUILD_OUTPUT_DIR" -name "*.exe" -type f ! -name "CastleOfTime.exe" -exec rm -f {} \; -print | while read file; do
    echo "  Removed: $(basename "$file")"
done

find "$BUILD_OUTPUT_DIR" -name "*.dll" -type f -exec rm -f {} \; -print | while read file; do
    echo "  Removed: $(basename "$file")"
done

# Remove Linux-specific files from macOS build
find "$BUILD_OUTPUT_DIR" -name "*.so*" -type f -exec rm -f {} \; -print | while read file; do
    echo "  Removed: $(basename "$file")"
done

find "$BUILD_OUTPUT_DIR" -name "*.x86_64" -type f -exec rm -f {} \; -print | while read file; do
    echo "  Removed: $(basename "$file")"
done

# 4. Remove Unity 'Do Not Ship' folders
echo ""
echo "Removing Unity 'Do Not Ship' folders..."

DO_NOT_SHIP_PATTERNS=(
    "*_BurstDebugInformation_DoNotShip"
    "BackUpThisFolder_ButDontShipItWithYourGame"
    "*DoNotShip*"
    "*donotship*"
    "Castle of Time_BurstDebugInformation_DoNotShip"
)

IFS=$'\n'
for pattern in "${DO_NOT_SHIP_PATTERNS[@]}"; do
    find "$BUILD_OUTPUT_DIR" -type d -name "$pattern" -exec rm -rf {} + 2>/dev/null | while read dir; do
        echo "  Removed DoNotShip folder: $dir"
    done || true
done
unset IFS

echo ""
echo "✅ Post-build complete!"

