<# 
.SYNOPSIS
    Build script for GoodForBusiness mod

.DESCRIPTION
    Builds the mod and optionally creates a Thunderstore package.

.PARAMETER Configuration
    Build configuration (Debug or Release). Default: Release

.PARAMETER Package
    Whether to create a Thunderstore package. Default: $false

.EXAMPLE
    .\build.ps1 -Configuration Release -Package

.EXAMPLE
    .\build.ps1 Debug
#>

param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    
    [switch]$Package
)

Write-Host "Building GoodForBusiness in $Configuration configuration..." -ForegroundColor Green

# Build the project
dotnet build GoodForBusiness.csproj --configuration $Configuration

if ($Package) {
    Write-Host "Building Thunderstore package..." -ForegroundColor Green
    
    # Install tcli if not present
    if (-not (Get-Command tcli -ErrorAction SilentlyContinue)) {
        Write-Host "Installing tcli..." -ForegroundColor Yellow
        dotnet tool install --global tcli
        $env:PATH += ";$env:USERPROFILE\.dotnet\tools"
    }
    
    # Build the package
    tcli build
    
    Write-Host "Package created successfully!" -ForegroundColor Green
    Get-ChildItem *.zip | ForEach-Object { Write-Host "  $($_.FullName)" }
}

Write-Host "Build complete!" -ForegroundColor Green