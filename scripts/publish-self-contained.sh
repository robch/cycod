#!/bin/bash
# -----------------------------------------------------------------------------
# Usage: ./scripts/publish-self-contained.sh [VERSION] [OUTPUT_DIR] [CONFIGURATION]
#
# Build self-contained executables for all cycod tools and package them into zip files.
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
OUTPUT_DIR=${2:-./self-contained}
CONFIGURATION=${3:-Release}

# Calculate version components
cycod_version_calculate "$VERSION"

# Build and pack self-contained executables
cycod_publish_self_contained "$OUTPUT_DIR" "$CONFIGURATION"

echo "Created self-contained executables with version $VERSION in $OUTPUT_DIR"