# ===== HERE.PS1 WRAPPER =====
# Thin wrapper that calls the real implementation in scripts/

param(
    [Parameter(ValueFromRemainingArguments)]
    [string[]]$Arguments
)

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ImplScript = Join-Path $ScriptDir "scripts\here-impl.ps1"

if (-not (Test-Path $ImplScript)) {
    Write-Error "Error: scripts\here-impl.ps1 not found"
    Write-Error "Make sure you're running this from the repository root."
    exit 1
}

# Pass through all arguments to the implementation
& $ImplScript @Arguments