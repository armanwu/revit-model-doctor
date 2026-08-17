@echo off
setlocal enabledelayedexpansion

:: ===================================================
:: Model Doctor Revit Add-In Installer
:: Created by: Arman Arisman
:: Copyright (c) 2026 Arman Arisman. All rights reserved.
:: ===================================================

set "REVIT_VERSION=2027"
set "TARGET_DIR=%APPDATA%\Autodesk\Revit\Addins\%REVIT_VERSION%"
set "ADDIN_SUBDIR=%TARGET_DIR%\ModelDoctor"

echo ===================================================
echo   Installing Model Doctor Revit Add-In (%REVIT_VERSION%)
echo   Created by: Arman Arisman
echo   Copyright (c) 2026 Arman Arisman
echo ===================================================

if not exist "%TARGET_DIR%" (
    echo Creating Revit Add-Ins directory for %REVIT_VERSION%...
    mkdir "%TARGET_DIR%"
)

if not exist "%ADDIN_SUBDIR%" (
    mkdir "%ADDIN_SUBDIR%"
)

echo Building C# solution (%REVIT_VERSION% / .NET 10)...
dotnet build "%~dp0src\ModelDoctor\ModelDoctor.csproj" -c Release

if %ERRORLEVEL% neq 0 (
    echo.
    echo [ERROR] Build failed. Please check build errors above.
    pause
    exit /b %ERRORLEVEL%
)

echo Copying compiled files to Revit Add-Ins folder...
copy /Y "%~dp0src\ModelDoctor\bin\Release\net10.0-windows\ModelDoctor.dll" "%ADDIN_SUBDIR%\"
copy /Y "%~dp0src\ModelDoctor\Manifest\model-doctor.addin" "%TARGET_DIR%\"

echo ===================================================
echo [SUCCESS] Model Doctor Add-In installed successfully!
echo Manifest: %TARGET_DIR%\model-doctor.addin
echo Assembly: %ADDIN_SUBDIR%\ModelDoctor.dll
echo ===================================================
pause
