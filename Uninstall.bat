@echo off
setlocal enabledelayedexpansion

:: ===================================================
:: Model Doctor Revit Add-In Multi-Version Uninstaller
:: Supports: Autodesk Revit 2025, 2026, 2027
:: Created by: Arman Arisman
:: Copyright (c) 2026 Arman Arisman
:: License: MIT License (https://opensource.org/licenses/MIT)
:: ===================================================

title Revit Model Doctor Uninstaller

echo ===================================================
echo   Model Doctor Revit Add-In Uninstaller
echo   Supports: Revit 2025, 2026, 2027
echo ===================================================
echo.
echo Select target Revit version to uninstall Model Doctor:
echo   [1] Revit 2025
echo   [2] Revit 2026
echo   [3] Revit 2027
echo   [4] Uninstall from ALL versions (2025, 2026, 2027)
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
    echo Invalid choice. Uninstallation cancelled.
    pause
    exit /b 1
)

echo.
for %%V in (%VERSIONS%) do (
    set "TARGET_DIR=%APPDATA%\Autodesk\Revit\Addins\%%V"
    
    if exist "!TARGET_DIR!\model-doctor.addin" (
        del /F /Q "!TARGET_DIR!\model-doctor.addin"
        echo Removed model-doctor.addin for Revit %%V
    )

    if exist "!TARGET_DIR!\ModelDoctor" (
        rmdir /S /Q "!TARGET_DIR!\ModelDoctor"
        echo Removed ModelDoctor folder for Revit %%V
    )
)

echo.
echo ===================================================
echo [SUCCESS] Model Doctor Add-In uninstalled successfully!
echo Target Version(s): %VERSIONS%
echo ===================================================
pause
