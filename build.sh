#!/bin/bash
# Build script for GoodForBusiness mod
# Usage: ./build.sh [Debug|Release] [--package]

set -e

CONFIGURATION="${1:-Release}"
PACKAGE=false

if [[ "$2" == "--package" ]] || [[ "$1" == "--package" ]]; then
    PACKAGE=true
fi

echo "Building GoodForBusiness in $CONFIGURATION configuration..."

# Build the project
dotnet build GoodForBusiness.csproj --configuration "$CONFIGURATION"

if [[ "$PACKAGE" == true ]]; then
    echo "Building Thunderstore package..."
    # Install tcli if not present
    if ! command -v tcli &> /dev/null; then
        echo "Installing tcli..."
        dotnet tool install --global tcli
        export PATH="$PATH:$HOME/.dotnet/tools"
    fi
    
    # Build the package
    tcli build
    
    echo "Package created successfully!"
    ls -la *.zip
fi

echo "Build complete!"