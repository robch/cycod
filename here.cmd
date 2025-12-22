@echo off
REM ===== HERE.CMD WRAPPER =====
REM Thin wrapper that calls the real implementation in scripts/

if not exist "scripts\here-impl.cmd" (
    echo Error: scripts\here-impl.cmd not found
    echo Make sure you're running this from the repository root.
    exit /b 1
)

call scripts\here-impl.cmd %*