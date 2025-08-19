#!/bin/bash
# -----------------------------------------------------------------------------
# Usage: ./scripts/pack.sh [VERSION] [OUTPUT_DIR] [CONFIGURATION]
#
# Create NuGet packages for all cycod tools.
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
OUTPUT_DIR=${2:-./nuget-packages}
CONFIGURATION=${3:-Release}

# Calculate version components
cycod_version_calculate "$VERSION"

# Build and pack the tools
cycod_pack_dotnet "$OUTPUT_DIR" "$CONFIGURATION"

echo "Created NuGet packages with version $VERSION in $OUTPUT_DIR"