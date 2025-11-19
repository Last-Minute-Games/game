#!/bin/bash
# Build script for Castle of Time Updater (macOS)
# Builds self-contained executables for macOS

set -e

CONFIGURATION="${1:-Release}"
PLATFORM="${2:-macOS}"
ARCHITECTURE="${3:-x64}"

if [ "$PLATFORM" != "macOS" ] && [ "$PLATFORM" != "OSX" ] && [ "$PLATFORM" != "Both" ]; then
    echo "❌ Invalid Platform: $PLATFORM. Must be 'macOS', 'OSX', or 'Both'"
    exit 1
fi

# Determine runtime identifier based on architecture
if [ "$ARCHITECTURE" = "ARM64" ] || [ "$ARCHITECTURE" = "arm64" ]; then
    RUNTIME_ID="osx-arm64"
    ARCH_LABEL="ARM64"
elif [ "$ARCHITECTURE" = "x64" ] || [ "$ARCHITECTURE" = "x86_64" ]; then
    RUNTIME_ID="osx-x64"
    ARCH_LABEL="x64"
else
    echo "❌ Invalid Architecture: $ARCHITECTURE. Must be 'x64' or 'ARM64'"
    exit 1
fi

echo "=== Building Castle of Time Updater ==="

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_FILE="$SCRIPT_DIR/CastleOfTimeUpdater.csproj"
OUTPUT_DIR="$SCRIPT_DIR/bin"

# Clean previous builds for the target platform(s)
if [ "$PLATFORM" = "Both" ]; then
    if [ -d "$OUTPUT_DIR" ]; then
        echo "Cleaning previous builds..."
        rm -rf "$OUTPUT_DIR"
    fi
else
    PLATFORM_DIR="$OUTPUT_DIR/$PLATFORM"
    if [ -d "$PLATFORM_DIR" ]; then
        echo "Cleaning previous $PLATFORM build..."
        rm -rf "$PLATFORM_DIR"
    fi
fi

# Build macOS version
if [ "$PLATFORM" = "macOS" ] || [ "$PLATFORM" = "OSX" ] || [ "$PLATFORM" = "Both" ]; then
    echo ""
    echo "Building macOS updater for $ARCH_LABEL..."
    
    # Determine output directory based on architecture
    if [ "$PLATFORM" = "Both" ]; then
        OUTPUT_SUBDIR="$OUTPUT_DIR/macOS-$ARCH_LABEL"
    else
        OUTPUT_SUBDIR="$OUTPUT_DIR/macOS-$ARCH_LABEL"
    fi
    
    dotnet publish "$PROJECT_FILE" \
        -c "$CONFIGURATION" \
        -r "$RUNTIME_ID" \
        --self-contained true \
        -p:PublishSingleFile=true \
        -p:PublishTrimmed=false \
        -p:Version=1.0.0 \
        -o "$OUTPUT_SUBDIR"

    if [ $? -ne 0 ]; then
        echo "❌ macOS $ARCH_LABEL build failed!"
        exit 1
    fi
    
    # Make executable
    chmod +x "$OUTPUT_SUBDIR/CastleOfTimeUpdater"
    echo "✅ macOS $ARCH_LABEL updater built: $OUTPUT_SUBDIR/CastleOfTimeUpdater"
    
    # If building both, also build the other architecture
    if [ "$PLATFORM" = "Both" ]; then
        if [ "$ARCH_LABEL" = "x64" ]; then
            OTHER_RUNTIME="osx-arm64"
            OTHER_ARCH="ARM64"
        else
            OTHER_RUNTIME="osx-x64"
            OTHER_ARCH="x64"
        fi
        
        OTHER_OUTPUT="$OUTPUT_DIR/macOS-$OTHER_ARCH"
        echo ""
        echo "Building macOS updater for $OTHER_ARCH..."
        
        dotnet publish "$PROJECT_FILE" \
            -c "$CONFIGURATION" \
            -r "$OTHER_RUNTIME" \
            --self-contained true \
            -p:PublishSingleFile=true \
            -p:PublishTrimmed=false \
            -p:Version=1.0.0 \
            -o "$OTHER_OUTPUT"

        if [ $? -ne 0 ]; then
            echo "❌ macOS $OTHER_ARCH build failed!"
            exit 1
        fi
        
        chmod +x "$OTHER_OUTPUT/CastleOfTimeUpdater"
        echo "✅ macOS $OTHER_ARCH updater built: $OTHER_OUTPUT/CastleOfTimeUpdater"
    fi
fi

echo ""
echo "✅ Build complete!"

