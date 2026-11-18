@echo off
REM Asset Bundle Build Script for GoodForBusiness (Windows)
REM Runs Unity in batch mode to build asset bundles
REM Requires Unity 2022.3.9f1 installed

set UNITY_VERSION=2022.3.9f1
set PROJECT_PATH=%~dp0
set BUILD_METHOD=GoodForBusiness.Editor.AssetBundleBuilder.BuildAssetBundlesRelease

REM Detect Unity installation path
set UNITY_EXECUTABLE=
if exist "C:\Program Files\Unity\Hub\Editor\%UNITY_VERSION%\Editor\Unity.exe" (
    set UNITY_EXECUTABLE=C:\Program Files\Unity\Hub\Editor\%UNITY_VERSION%\Editor\Unity.exe
) else if exist "C:\Program Files\Unity\%UNITY_VERSION%\Editor\Unity.exe" (
    set UNITY_EXECUTABLE=C:\Program Files\Unity\%UNITY_VERSION%\Editor\Unity.exe
) else if exist "%USERPROFILE%\Unity\Hub\Editor\%UNITY_VERSION%\Editor\Unity.exe" (
    set UNITY_EXECUTABLE=%USERPROFILE%\Unity\Hub\Editor\%UNITY_VERSION%\Editor\Unity.exe
)

if "%UNITY_EXECUTABLE%"=="" (
    echo ERROR: Unity %UNITY_VERSION% not found in standard locations.
    echo Please install Unity %UNITY_VERSION% or set UNITY_EXECUTABLE environment variable.
    exit /b 1
)

echo Using Unity: %UNITY_EXECUTABLE%
echo Project: %PROJECT_PATH%
echo Build Method: %BUILD_METHOD%

REM Run Unity in batch mode
"%UNITY_EXECUTABLE%" ^
    -batchmode ^
    -nographics ^
    -projectPath "%PROJECT_PATH%" ^
    -executeMethod %BUILD_METHOD% ^
    -quit ^
    -logFile - ^
    -version

REM Check exit code
if %ERRORLEVEL% neq 0 (
    echo Asset bundle build failed with exit code %ERRORLEVEL%
    exit /b %ERRORLEVEL%
)

echo Asset bundle build completed successfully!
echo Output: %PROJECT_PATH%Build\AssetBundles\StandaloneWindows64\gfbassets