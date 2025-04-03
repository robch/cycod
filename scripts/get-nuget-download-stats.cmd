@echo off
REM Simple wrapper to run the PowerShell script
powershell -ExecutionPolicy Bypass -File "%~dp0get-nuget-download-stats.ps1" %*