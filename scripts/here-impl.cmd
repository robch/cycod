@echo off

REM ===== HERE-IMPL.CMD =====
REM Environment activation script for cycod development

set "REPO_ROOT=%~dp0\.."
set "MODE=light"
set "STAY_FLAG="

REM Parse arguments
if "%~1"=="--shell" set "MODE=heavy"
if "%~1"=="-s" set "MODE=heavy"
if "%~1"=="shell" set "MODE=heavy"
if "%~2"=="--stay" set "STAY_FLAG=yes"
if "%~1"=="--help" goto :show_usage
if "%~1"=="-h" goto :show_usage

echo.
echo Cycod Environment Activation
echo ============================

if "%MODE%"=="light" goto :light_mode
if "%MODE%"=="heavy" goto :heavy_mode

:light_mode
echo Light mode: Setting up environment in current shell...
echo.

call "%REPO_ROOT%\scripts\cycodpath.cmd"

set "CYCOD_DEV_MODE=1"
set "CYCOD_REPO_ROOT=%REPO_ROOT%"

echo.
echo Environment activated in current shell!
echo Tip: Use 'here.cmd --shell' to launch new environment shell
goto :end

:heavy_mode
echo Heavy mode: Launching new environment shell...
echo.

set "CYCOD_DEV_MODE=1"
set "CYCOD_REPO_ROOT=%REPO_ROOT%"

if not defined STAY_FLAG (
    echo Changing to repository root...
    cd /d "%REPO_ROOT%"
)

echo Starting new shell with cycod environment...
echo Type 'exit' to return to previous environment.
echo.

cmd /k "call "%REPO_ROOT%\scripts\cycodpath.cmd" >nul & prompt (here:cycod) $P$G & echo Environment shell ready!"

goto :end

:show_usage
echo.
echo Usage: here.cmd [options]
echo.
echo Options:
echo   (no options)    Light mode: Set up environment in current shell
echo   --shell, -s     Heavy mode: Launch new shell with full environment
echo   --stay          (with --shell) Stay in current directory
echo   --help, -h      Show this help
goto :end

:end