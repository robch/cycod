#!/bin/bash

# ===== HERE-IMPL.SH =====
# Environment activation script for cycod development
# Light mode: Sets up PATH and environment in current shell
# Heavy mode: Launches new shell with full environment setup

set -e

# Get the repository root directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

# Default values
MODE="light"
STAY_FLAG=""
SHOW_HELP=""

# ===== PARSE ARGUMENTS =====
while [[ $# -gt 0 ]]; do
    case $1 in
        --shell|-s|shell)
            MODE="heavy"
            shift
            ;;
        --stay)
            STAY_FLAG="yes"
            shift
            ;;
        --help|-h)
            SHOW_HELP="yes"
            shift
            ;;
        *)
            echo "Unknown argument: $1"
            show_usage
            exit 1
            ;;
    esac
done

show_usage() {
    cat << EOF

Usage: here.sh [options]

Options:
  (no options)    Light mode: Set up environment in current shell
  --shell, -s     Heavy mode: Launch new shell with full environment
  --stay          (with --shell) Stay in current directory  
  --help, -h      Show this help

Examples:
  ./here.sh                    Light: Quick environment setup
  ./here.sh --shell            Heavy: New shell at repo root
  ./here.sh --shell --stay     Heavy: New shell at current location

Note: For light mode to affect your current shell, source this script:
  source ./here.sh             Light mode in current shell
EOF
}

if [[ -n "$SHOW_HELP" ]]; then
    show_usage
    exit 0
fi

echo ""
echo "🔧 Cycod Environment Activation"
echo "=============================="

if [[ "$MODE" == "light" ]]; then
    echo "Light mode: Setting up environment..."
    echo ""
    
    # Call the existing PATH setup script
    source "$REPO_ROOT/scripts/setup-debug-path.sh" --session-only --no-test
    
    # Set additional environment variables
    export CYCOD_DEV_MODE=1
    export CYCOD_REPO_ROOT="$REPO_ROOT"
    
    echo ""
    echo "✅ Environment activated!"
    echo "💡 Tip: Use './here.sh --shell' to launch new environment shell at repo root"
    echo "💡 Note: To affect current shell, use: source ./here.sh"
    
elif [[ "$MODE" == "heavy" ]]; then
    echo "Heavy mode: Launching new environment shell..."
    echo ""
    
    # Prepare environment for new shell
    export CYCOD_DEV_MODE=1
    export CYCOD_REPO_ROOT="$REPO_ROOT"
    
    # Set up PATH
    source "$REPO_ROOT/scripts/setup-debug-path.sh" --session-only --no-test >/dev/null 2>&1
    
    # Prepare custom prompt
    CUSTOM_PS1="(here:cycod) \u@\h:\w\$ "
    
    # Determine target directory
    if [[ -z "$STAY_FLAG" ]]; then
        TARGET_DIR="$REPO_ROOT"
        echo "Changing to repository root..."
    else
        TARGET_DIR="$(pwd)"
        echo "Staying in current directory..."
    fi
    
    echo "Starting new shell with cycod environment..."
    echo "Type 'exit' to return to previous environment."
    echo ""
    
    # Launch new bash with custom environment
    cd "$TARGET_DIR"
    PS1="$CUSTOM_PS1" bash --rcfile <(cat ~/.bashrc 2>/dev/null || echo ""; echo "echo '✅ Cycod environment shell ready!'; echo")
fi