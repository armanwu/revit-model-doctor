@echo off
setlocal enabledelayedexpansion

:: ===================================================
:: Model Doctor Revit Add-In Multi-Version Installer
:: Created by: Arman Arisman
:: Copyright (c) 2026 Arman Arisman
:: License: MIT License (https://opensource.org/licenses/MIT)
:: ===================================================

echo ===================================================
echo   Model Doctor Revit Add-In Installer
echo   Created by: Arman Arisman
echo   License: MIT License (Open Source Software)
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

if "%CHOICE%"=="1" set "VERSIONS=2025"
if "%CHOICE%"=="2" set "VERSIONS=2026"
if "%CHOICE%"=="3" set "VERSIONS=2027"
if "%CHOICE%"=="4" set "VERSIONS=2025 2026 2027"
if "%CHOICE%"=="5" exit /b 0
if "%VERSIONS%"=="" (
    echo Invalid choice. Installation cancelled.
    pause
    exit /b 1
)

echo.
echo Building C# solution (.NET modern runtime)...
dotnet build "%~dp0src\ModelDoctor.csproj" -c Release

if %ERRORLEVEL% neq 0 (
    echo.
    echo [ERROR] Build failed. Please check build errors above.
    pause
    exit /b %ERRORLEVEL%
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
    copy /Y "%~dp0src\bin\Release\net10.0-windows\ModelDoctor.dll" "!ADDIN_SUBDIR!\" >nul
    copy /Y "%~dp0src\Manifest\model-doctor.addin" "!TARGET_DIR!\" >nul
    echo [OK] Installed for Revit %%V
)

echo.
echo ===================================================
echo [SUCCESS] Model Doctor Add-In installed successfully!
echo Target Version(s): %VERSIONS%
echo License: MIT License (Free & Open Source)
echo ===================================================
pause
