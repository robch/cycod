#!/bin/bash
# ===== HERE.SH WRAPPER =====
# Thin wrapper that calls the real implementation in scripts/

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
IMPL_SCRIPT="$SCRIPT_DIR/scripts/here-impl.sh"

if [[ ! -f "$IMPL_SCRIPT" ]]; then
    echo "Error: scripts/here-impl.sh not found"
    echo "Make sure you're running this from the repository root."
    exit 1
fi

# Pass through all arguments to the implementation
exec "$IMPL_SCRIPT" "$@"