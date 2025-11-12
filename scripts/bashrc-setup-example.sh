# Cycod Debug Environment Setup for .bashrc
# 
# Add this to your ~/.bashrc to automatically configure the debug environment:
#
# # Source the cycod debug environment (adjust path as needed)
# if [ -f "/workspaces/cycod/scripts/setup-debug-path.sh" ]; then
#     source <("/workspaces/cycod/scripts/setup-debug-path.sh" --session-only --no-test 2>/dev/null) 2>/dev/null || true
# fi
#
# Or for a more robust approach that works from any directory:
# CYCOD_REPO_ROOT="${CYCOD_REPO_ROOT:-$HOME/cycod}"  # Set this to your repo location
# if [ -f "$CYCOD_REPO_ROOT/scripts/setup-debug-path.sh" ]; then
#     source <("$CYCOD_REPO_ROOT/scripts/setup-debug-path.sh" --session-only --no-test 2>/dev/null) 2>/dev/null || true
# fi

# Alternative: Direct PATH setup (faster, but less flexible)
# export PATH="/workspaces/cycod/src/cycod/bin/Debug/net9.0:/workspaces/cycod/src/cycodmd/bin/Debug/net9.0:/workspaces/cycod/src/cycodt/bin/Debug/net9.0:$PATH"