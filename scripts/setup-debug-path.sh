#!/bin/bash

# Cycod Debug Environment Setup Script
# This script sets up the PATH to prioritize debug builds of all cycod tools over global dotnet tools

set -e

# Get the repository root directory (where this script is located relative to scripts/)
REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

# Define the debug binary paths
DEBUG_PATHS=(
    "$REPO_ROOT/src/cycod/bin/Debug/net9.0"
    "$REPO_ROOT/src/cycodmd/bin/Debug/net9.0"
    "$REPO_ROOT/src/cycodt/bin/Debug/net9.0"
    "$REPO_ROOT/src/cycodgr/bin/Debug/net9.0"
    "$REPO_ROOT/src/mcp/geolocation/bin/Debug/net9.0"
    "$REPO_ROOT/src/mcp/mxlookup/bin/Debug/net9.0"
    "$REPO_ROOT/src/mcp/osm/bin/Debug/net9.0"
    "$REPO_ROOT/src/mcp/weather/bin/Debug/net9.0"
    "$REPO_ROOT/src/mcp/whois/bin/Debug/net9.0"
)

# Function to check if PATH already contains our debug paths
path_already_configured() {
    local path_entry="${DEBUG_PATHS[0]}"
    [[ ":$PATH:" == *":$path_entry:"* ]]
}

# Function to add debug paths to current session
configure_current_session() {
    local debug_path_string=""
    for path in "${DEBUG_PATHS[@]}"; do
        if [[ -n "$debug_path_string" ]]; then
            debug_path_string="$debug_path_string:"
        fi
        debug_path_string="$debug_path_string$path"
    done
    
    export PATH="$debug_path_string:$PATH"
    echo "✅ Debug paths added to current session PATH"
}

# Function to add debug paths to bashrc
configure_bashrc() {
    local bashrc_file="$HOME/.bashrc"
    local marker_comment="# Cycod Debug Environment - Auto-configured"
    
    # Check if already configured
    if grep -q "$marker_comment" "$bashrc_file" 2>/dev/null; then
        echo "ℹ️  ~/.bashrc already configured for cycod debug environment"
        return 0
    fi
    
    # Build the PATH export line
    local debug_path_string=""
    for path in "${DEBUG_PATHS[@]}"; do
        if [[ -n "$debug_path_string" ]]; then
            debug_path_string="$debug_path_string:"
        fi
        debug_path_string="$debug_path_string$path"
    done
    
    # Add to bashrc
    {
        echo ""
        echo "$marker_comment"
        echo "export PATH=\"$debug_path_string:\$PATH\""
    } >> "$bashrc_file"
    
    echo "✅ Added cycod debug environment to ~/.bashrc"
}

# Function to test the setup
test_setup() {
    echo ""
    echo "🧪 Testing debug environment setup:"
    echo "=================================="
    
    for tool in cycod cycodmd cycodt cycodgr cycod-mcp-geolocation cycod-mcp-mxlookup cycod-mcp-osm cycod-mcp-weather cycod-mcp-whois; do
        local tool_path=$(which "$tool" 2>/dev/null || echo "not found")
        echo "  $tool: $tool_path"
        
        if [[ "$tool_path" != "not found" && "$tool_path" == *"/Debug/net9.0/"* ]]; then
            echo "    ✅ Using debug version"
        elif [[ "$tool_path" != "not found" ]]; then
            echo "    ⚠️  Using global version (debug not built or not in PATH)"
        else
            echo "    ❌ Not found (will use global if available)"
        fi
    done
    
    echo ""
    echo "To verify versions, run:"
    echo "  cycod --version"
    echo "  cycodmd --version" 
    echo "  cycodt --version"
}

# Main execution
main() {
    echo "🔧 Cycod Debug Environment Setup"
    echo "================================="
    echo "Repository root: $REPO_ROOT"
    echo ""
    
    # Parse command line arguments
    local configure_bashrc_flag=true
    local configure_session_flag=true
    local test_flag=true
    
    while [[ $# -gt 0 ]]; do
        case $1 in
            --no-bashrc)
                configure_bashrc_flag=false
                shift
                ;;
            --no-session)
                configure_session_flag=false
                shift
                ;;
            --no-test)
                test_flag=false
                shift
                ;;
            --session-only)
                configure_bashrc_flag=false
                configure_session_flag=true
                shift
                ;;
            --bashrc-only)
                configure_bashrc_flag=true
                configure_session_flag=false
                shift
                ;;
            -h|--help)
                echo "Usage: $0 [options]"
                echo ""
                echo "Options:"
                echo "  --no-bashrc      Don't modify ~/.bashrc"
                echo "  --no-session     Don't modify current session PATH"
                echo "  --no-test        Don't run setup test"
                echo "  --session-only   Only configure current session (don't modify ~/.bashrc)"
                echo "  --bashrc-only    Only configure ~/.bashrc (don't modify current session)"
                echo "  -h, --help       Show this help"
                exit 0
                ;;
            *)
                echo "Unknown option: $1"
                echo "Use --help for usage information"
                exit 1
                ;;
        esac
    done
    
    # Configure current session if requested
    if [[ "$configure_session_flag" == true ]]; then
        if path_already_configured; then
            echo "ℹ️  Current session PATH already configured"
        else
            configure_current_session
        fi
    fi
    
    # Configure bashrc if requested
    if [[ "$configure_bashrc_flag" == true ]]; then
        configure_bashrc
    fi
    
    # Run test if requested
    if [[ "$test_flag" == true ]]; then
        test_setup
    fi
    
    echo ""
    echo "✅ Setup complete!"
    
    if [[ "$configure_bashrc_flag" == true ]]; then
        echo "💡 New terminals will automatically use debug builds when available"
    fi
    
    if [[ "$configure_session_flag" == false && "$configure_bashrc_flag" == true ]]; then
        echo "💡 Run 'source ~/.bashrc' to apply changes to current session"
    fi
}

# Run main function with all arguments
main "$@"