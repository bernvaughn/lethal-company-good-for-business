#!/bin/bash
# Asset Bundle Build Script for GoodForBusiness
# Runs Unity in batch mode to build asset bundles
# Requires Unity 2022.3.9f1 installed

set -e

UNITY_VERSION="2022.3.9f1"
PROJECT_PATH="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BUILD_METHOD="GoodForBusiness.Editor.AssetBundleBuilder.BuildAssetBundlesRelease"

# Detect Unity installation path
if [[ "$OSTYPE" == "linux-gnu"* ]]; then
    # Linux (typical CI paths)
    UNITY_PATHS=(
        "/opt/unity/${UNITY_VERSION}/Editor/Unity"
        "/usr/local/unity/${UNITY_VERSION}/Editor/Unity"
        "$HOME/unity/${UNITY_VERSION}/Editor/Unity"
    )
elif [[ "$OSTYPE" == "darwin"* ]]; then
    # macOS
    UNITY_PATHS=(
        "/Applications/Unity/Hub/Editor/${UNITY_VERSION}/Unity.app/Contents/MacOS/Unity"
        "/Applications/Unity/${UNITY_VERSION}/Unity.app/Contents/MacOS/Unity"
    )
elif [[ "$OSTYPE" == "msys" || "$OSTYPE" == "cygwin" || "$OSTYPE" == "win32" ]]; then
    # Windows (Git Bash)
    UNITY_PATHS=(
        "/c/Program Files/Unity/Hub/Editor/${UNITY_VERSION}/Editor/Unity.exe"
        "/c/Program Files/Unity/${UNITY_VERSION}/Editor/Unity.exe"
    )
fi

UNITY_EXECUTABLE=""
for path in "${UNITY_PATHS[@]}"; do
    if [[ -f "$path" ]]; then
        UNITY_EXECUTABLE="$path"
        break
    fi
done

if [[ -z "$UNITY_EXECUTABLE" ]]; then
    echo "ERROR: Unity ${UNITY_VERSION} not found in standard locations."
    echo "Please install Unity ${UNITY_VERSION} or set UNITY_EXECUTABLE environment variable."
    exit 1
fi

echo "Using Unity: $UNITY_EXECUTABLE"
echo "Project: $PROJECT_PATH"
echo "Build Method: $BUILD_METHOD"

# Run Unity in batch mode
"$UNITY_EXECUTABLE" \
    -batchmode \
    -nographics \
    -projectPath "$PROJECT_PATH" \
    -executeMethod "$BUILD_METHOD" \
    -quit \
    -logFile - \
    -version

# Check exit code
EXIT_CODE=$?
if [[ $EXIT_CODE -eq 0 ]]; then
    echo "Asset bundle build completed successfully!"
    echo "Output: $PROJECT_PATH/Build/AssetBundles/StandaloneWindows64/gfbassets"
else
    echo "Asset bundle build failed with exit code $EXIT_CODE"
    exit $EXIT_CODE
fi