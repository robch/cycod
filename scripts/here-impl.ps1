# ===== HERE-IMPL.PS1 =====
# Environment activation script for cycod development

param(
    [switch]$Shell,
    [switch]$S,
    [switch]$Stay,
    [switch]$Help,
    [switch]$H,
    [Parameter(ValueFromRemainingArguments)]
    [string[]]$RemainingArgs
)

# Get the repository root directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Split-Path -Parent $ScriptDir

# Parse arguments
$Mode = "light"
$StayFlag = $Stay
$ShowHelp = $Help -or $H

if ($Shell -or $S -or ($RemainingArgs -contains "shell")) {
    $Mode = "heavy"
}

function Show-Usage {
    Write-Host ""
    Write-Host "Usage: here.ps1 [options]"
    Write-Host ""
    Write-Host "Options:"
    Write-Host "  (no options)    Light mode: Set up environment in current shell"
    Write-Host "  -Shell, -S      Heavy mode: Launch new shell with full environment"
    Write-Host "  -Stay           (with -Shell) Stay in current directory"
    Write-Host "  -Help, -H       Show this help"
    Write-Host ""
    Write-Host "Examples:"
    Write-Host "  .\here.ps1                   Light: Quick environment setup"
    Write-Host "  .\here.ps1 -Shell            Heavy: New shell at repo root"
    Write-Host "  .\here.ps1 -Shell -Stay      Heavy: New shell at current location"
}

if ($ShowHelp) {
    Show-Usage
    exit
}

Write-Host ""
Write-Host "Cycod Environment Activation" -ForegroundColor Cyan
Write-Host "============================" -ForegroundColor Cyan

if ($Mode -eq "light") {
    Write-Host "Light mode: Setting up environment in current shell..." -ForegroundColor Yellow
    Write-Host ""
    
    # Call the existing PATH setup script
    $CycodPathScript = Join-Path $RepoRoot "scripts\cycodpath.cmd"
    if (Test-Path $CycodPathScript) {
        cmd /c """$CycodPathScript"""
    }
    
    # Set additional environment variables
    $env:CYCOD_DEV_MODE = "1"
    $env:CYCOD_REPO_ROOT = $RepoRoot
    
    Write-Host ""
    Write-Host "Environment activated!" -ForegroundColor Green
    Write-Host "Tip: Use './here.ps1 -Shell' to launch new environment shell" -ForegroundColor Yellow
    
} elseif ($Mode -eq "heavy") {
    Write-Host "Heavy mode: Launching new environment shell..." -ForegroundColor Yellow
    Write-Host ""
    
    # Set environment variables
    $env:CYCOD_DEV_MODE = "1"
    $env:CYCOD_REPO_ROOT = $RepoRoot
    
    # Determine target directory
    if (-not $StayFlag) {
        $TargetDir = $RepoRoot
        Write-Host "Changing to repository root..." -ForegroundColor Yellow
    } else {
        $TargetDir = Get-Location
        Write-Host "Staying in current directory..." -ForegroundColor Yellow
    }
    
    Write-Host "Starting new PowerShell with cycod environment..." -ForegroundColor Yellow
    Write-Host "Type 'exit' to return to previous environment." -ForegroundColor Yellow
    Write-Host ""
    
    # Launch new PowerShell with custom prompt and setup
    $setupCommands = @"
`$env:CYCOD_DEV_MODE='1'
`$env:CYCOD_REPO_ROOT='$RepoRoot'
function prompt { "(here:cycod) PS " + (Get-Location) + "> " }
Set-Location '$TargetDir'
cmd /c '$CycodPathScript' | Out-Null
Write-Host 'Environment shell ready!' -ForegroundColor Green
"@
    
    powershell -NoExit -Command $setupCommands
}