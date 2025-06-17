#!/bin/bash
# -----------------------------------------------------------------------------
# Usage: ./scripts/build.sh [VERSION] [CONFIGURATION]
#
# Build the cycod projects without creating self-contained executables.
# If no version is specified, a development version will be generated.
# -----------------------------------------------------------------------------

# Exit on any error
set -e

# Resolve script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# Source the functions library
source "${SCRIPT_DIR}/_functions.sh"

# Check if required tools are installed
cycod_check_environment

# Get parameters
VERSION=${1:-$(cycod_version_get_dev)}
CONFIGURATION=${2:-Debug}

# Build the projects
cycod_build_dotnet "$VERSION" "$CONFIGURATION"