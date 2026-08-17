@echo off
setlocal

:: ===================================================
:: Model Doctor Revit Add-In Uninstaller
:: Created by: Arman Arisman
:: Copyright (c) 2026 Arman Arisman. All rights reserved.
:: ===================================================

set "REVIT_VERSION=2027"
set "TARGET_DIR=%APPDATA%\Autodesk\Revit\Addins\%REVIT_VERSION%"

echo ===================================================
echo   Uninstalling Model Doctor Revit Add-In (%REVIT_VERSION%)
echo   Created by: Arman Arisman
echo   Copyright (c) 2026 Arman Arisman
echo ===================================================

if exist "%TARGET_DIR%\model-doctor.addin" (
    del /F /Q "%TARGET_DIR%\model-doctor.addin"
    echo Removed model-doctor.addin
)

if exist "%TARGET_DIR%\ModelDoctor" (
    rmdir /S /Q "%TARGET_DIR%\ModelDoctor"
    echo Removed ModelDoctor folder
)

echo ===================================================
echo [SUCCESS] Model Doctor Add-In uninstalled successfully!
echo ===================================================
pause
