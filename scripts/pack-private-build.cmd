@echo off
REM CMD wrapper for the PowerShell pack-private-build script
REM Usage: pack-private-build.cmd 1.0.0-alpha-20250527.1

if "%~1"=="" (
  echo Error: Version parameter is required.
  echo Usage: %~nx0 VERSION
  echo Example: %~nx0 1.0.0-alpha-20250527.1
  exit /b 1
)

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0pack-private-build.ps1" %*
if %ERRORLEVEL% neq 0 (
  echo Error: PowerShell script failed with exit code %ERRORLEVEL%
  exit /b %ERRORLEVEL%
)