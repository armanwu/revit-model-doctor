@echo off
setlocal enabledelayedexpansion

:: ===================================================
:: Model Doctor Revit Add-In Multi-Version Installer
:: Supports: Autodesk Revit 2025, 2026, 2027
:: Created by: Arman Arisman
:: Copyright (c) 2026 Arman Arisman
:: License: MIT License (https://opensource.org/licenses/MIT)
:: ===================================================

title Revit Model Doctor Installer

echo ===================================================
echo   Model Doctor Revit Add-In Installer
echo   Supports: Revit 2025, 2026, 2027
echo   Zero-Dependency 1-Click Plug and Play
echo ===================================================
echo.
echo Select target Revit version to install Model Doctor:
echo   [1] Revit 2025
echo   [2] Revit 2026
echo   [3] Revit 2027
echo   [4] Install for ALL versions (2025, 2026, 2027)
echo   [5] Cancel
echo.
set /p CHOICE="Enter option [1-5]: "
if defined CHOICE set "CHOICE=%CHOICE: =%"

if "%CHOICE%"=="1" set "VERSIONS=2025"
if "%CHOICE%"=="2" set "VERSIONS=2026"
if "%CHOICE%"=="3" set "VERSIONS=2027"
if "%CHOICE%"=="4" set "VERSIONS=2025 2026 2027"
if "%CHOICE%"=="5" exit /b 0
if "%VERSIONS%"=="" (
    echo [ERROR] Invalid choice. Installation cancelled.
    pause
    exit /b 1
)

echo.
echo Searching for Model Doctor add-in binary...

set "SOURCE_DLL="
set "SOURCE_ADDIN="

:: 1. Check release directory (Zero-dependency distribution package)
if exist "%~dp0release\ModelDoctor\ModelDoctor.dll" (
    set "SOURCE_DLL=%~dp0release\ModelDoctor\ModelDoctor.dll"
    set "SOURCE_ADDIN=%~dp0release\model-doctor.addin"
    echo [OK] Found pre-compiled release package: Zero-Dependency Mode.
)

:: 2. Check build output directory if release not found
if "!SOURCE_DLL!"=="" (
    if exist "%~dp0src\bin\Release\net8.0-windows\ModelDoctor.dll" (
        set "SOURCE_DLL=%~dp0src\bin\Release\net8.0-windows\ModelDoctor.dll"
        set "SOURCE_ADDIN=%~dp0src\Manifest\model-doctor.addin"
        echo [OK] Found built binary in bin\Release\net8.0-windows.
    )
)

:: 3. Developer fallback: If no binary exists, attempt build with dotnet SDK
if "!SOURCE_DLL!"=="" (
    echo Binary not found. Checking for .NET SDK Developer Mode...
    where dotnet >nul 2>nul
    if !ERRORLEVEL! equ 0 (
        echo Building C# solution with .NET 8 runtime...
        dotnet build "%~dp0src\ModelDoctor.csproj" -c Release
        if !ERRORLEVEL! equ 0 (
            if exist "%~dp0src\bin\Release\net8.0-windows\ModelDoctor.dll" (
                set "SOURCE_DLL=%~dp0src\bin\Release\net8.0-windows\ModelDoctor.dll"
                set "SOURCE_ADDIN=%~dp0src\Manifest\model-doctor.addin"
            )
        ) else (
            echo.
            echo [ERROR] Build failed. Please check build errors above.
            pause
            exit /b 1
        )
    ) else (
        echo.
        echo [ERROR] Pre-compiled ModelDoctor.dll was not found and .NET SDK is not installed.
        echo Please ensure you download the complete release package containing the release folder.
        pause
        exit /b 1
    )
)

if "!SOURCE_DLL!"=="" (
    echo.
    echo [ERROR] Unable to locate ModelDoctor.dll. Installation aborted.
    pause
    exit /b 1
)

echo.
for %%V in (%VERSIONS%) do (
    set "TARGET_DIR=%APPDATA%\Autodesk\Revit\Addins\%%V"
    set "ADDIN_SUBDIR=!TARGET_DIR!\ModelDoctor"

    if not exist "!TARGET_DIR!" (
        echo Creating Revit Add-Ins directory for %%V...
        mkdir "!TARGET_DIR!"
    )

    if not exist "!ADDIN_SUBDIR!" (
        mkdir "!ADDIN_SUBDIR!"
    )

    echo Installing Model Doctor to Revit %%V...
    copy /Y "!SOURCE_DLL!" "!ADDIN_SUBDIR!\ModelDoctor.dll" >nul
    copy /Y "!SOURCE_ADDIN!" "!TARGET_DIR!\model-doctor.addin" >nul
    echo   - Manifest: !TARGET_DIR!\model-doctor.addin
    echo   - Binary  : !ADDIN_SUBDIR!\ModelDoctor.dll
    echo [OK] Successfully installed for Revit %%V!
    echo.
)

echo ===================================================
echo [SUCCESS] Model Doctor Add-In installed successfully!
echo Target Version(s) : %VERSIONS%
echo Compatibility     : Autodesk Revit 2025, 2026, 2027
echo.
echo HOW TO RUN:
echo 1. Open Autodesk Revit.
echo 2. Go to the "Add-Ins" tab on the ribbon.
echo 3. Click "Run Health Check" in the Model Doctor panel.
echo ===================================================
pause
