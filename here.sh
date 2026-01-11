#!/bin/bash

# Determine if this script is being sourced
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    # Script is being executed directly - use exec
    exec "$(dirname "$0")/scripts/here-impl.sh" "$@"
else
    # Script is being sourced - source the implementation
    source "$(dirname "${BASH_SOURCE[0]}")/scripts/here-impl.sh" "$@"
fi