#!/bin/bash
# -----------------------------------------------------------------------------
# CycoDev Build System Functions
# -----------------------------------------------------------------------------

# Function: cycod_version_get_dev
# Description: Generate a development version with username and timestamp
# Parameters: None
# Outputs:
#   Returns a version string like "1.0.0-DEV-username-20250612.1"
#
cycod_version_get_dev() {
  # Base version
  local BASE_VERSION="1.0.0"
  
  # Get username
  local USERNAME=$(whoami)
  
  # Get date in YYYYMMDD format
  local DATE_TODAY=$(date +%Y%m%d)
  
  # Get a unique hourly number (hour + minute/60 as decimal)
  local HOUR=$(date +%H)
  local MINUTE=$(date +%M)
  local TIME_PART="${HOUR}${MINUTE}"
  
  echo "${BASE_VERSION}-DEV-${USERNAME}-${DATE_TODAY}${TIME_PART}"
}

# Function: cycod_version_calculate
# Description: Calculate numeric version components from a version string
# Parameters:
#   $1: Version string (e.g., "1.0.0-alpha-20250612.1")
# Outputs:
#   VERSION: Full semantic version (e.g., "1.0.0-alpha-20250612.1")
#   MAJOR, MINOR, BUILD: Version components
#   NUMERIC_VERSION: Four-part numeric version suitable for AssemblyVersion/FileVersion
#
cycod_version_calculate() {
  # Set the VERSION variable
  VERSION=$1
  echo "Calculating version components from: $VERSION"
  
  # Extract the major.minor.build parts from the version
  if [[ $VERSION =~ ^([0-9]+)\.([0-9]+)\.([0-9]+) ]]; then
    MAJOR="${BASH_REMATCH[1]}"
    MINOR="${BASH_REMATCH[2]}"
    BUILD="${BASH_REMATCH[3]}"
  else
    echo "Error: Version doesn't match expected format: $VERSION"
    return 1
  fi
  
  # Try to extract a YYYYMMDD date pattern from the version
  DATE_PART=$(echo "$VERSION" | grep -oE '20[0-9]{2}(0[1-9]|1[0-2])(0[1-9]|[12][0-9]|3[01])')
  DAY_OF_YEAR=0
  
  if [ -n "$DATE_PART" ]; then
    echo "Found date in version: $DATE_PART"
    
    # Extract year, month, day
    YEAR=${DATE_PART:0:4}
    MONTH=${DATE_PART:4:2}
    DAY=${DATE_PART:6:2}
    
    # Convert to day of year
    DAY_OF_YEAR=$(date -d "$YEAR-$MONTH-$DAY" +%j | sed 's/^0*//')
    echo "Using date from version: $YEAR-$MONTH-$DAY (day of year: $DAY_OF_YEAR)"
  else
    echo "No date found in version, using today's date as fallback"
    DAY_OF_YEAR=$(date '+%j' | sed 's/^0*//')
  fi
  
  # Extract final decimal part if it exists
  FINAL_PART=0
  if [[ $VERSION =~ \.([0-9]+)$ ]]; then
    FINAL_PART="${BASH_REMATCH[1]}"
  fi
  
  # Calculate revision: day_of_year * 100 + final_part
  REVISION=$((DAY_OF_YEAR * 100 + FINAL_PART))
  
  # Safety check: ensure revision doesn't exceed maximum allowed value
  if [ $REVISION -gt 65535 ]; then
    echo "WARNING: Revision exceeds maximum allowed value (65535). Capping at 65535."
    REVISION=65535
  fi
  
  # Combine for a valid .NET version with 4 parts
  NUMERIC_VERSION="$MAJOR.$MINOR.$BUILD.$REVISION"
  echo "Using numeric version: $NUMERIC_VERSION for AssemblyVersion and FileVersion"
  
  # Export the variables so they're available to the caller
  export VERSION
  export MAJOR
  export MINOR
  export BUILD
  export REVISION
  export NUMERIC_VERSION
}

# Function: cycod_build_dotnet
# Description: Build all cycod projects with the specified version
# Parameters:
#   $1: Version string (optional, defaults to dev version)
#   $2: Configuration (optional, defaults to Debug)
#
cycod_build_dotnet() {
  local VERSION=${1:-$(cycod_version_get_dev)}
  local CONFIGURATION=${2:-Debug}
  
  # Calculate numeric version based on the input version
  cycod_version_calculate "$VERSION"
  
  echo "Building cycod projects with Version=$VERSION, NumericVersion=$NUMERIC_VERSION"
  
  # List of projects to build
  local PROJECTS=("src/common/common.csproj" "src/cycod/cycod.csproj" "src/cycodt/cycodt.csproj" "src/cycodmd/cycodmd.csproj" "src/cycodgr/cycodgr.csproj")
  
  # First restore dependencies
  echo "Restoring dependencies..."
  dotnet restore
  
  # Build each project
  for PROJECT in "${PROJECTS[@]}"; do
    echo "Building $PROJECT..."
    dotnet build "$PROJECT" \
      -c "$CONFIGURATION" \
      --framework net9.0 \
      --no-restore \
      -p:Version="$VERSION" \
      -p:AssemblyVersion="$NUMERIC_VERSION" \
      -p:FileVersion="$NUMERIC_VERSION" \
      -p:InformationalVersion="$VERSION"
      
    if [ $? -ne 0 ]; then
      echo "Error: Failed to build $PROJECT"
      return 1
    fi
  done
  
  echo "✅ Build completed successfully!"
}

# Function: cycod_pack_dotnet
# Description: Create NuGet packages with the calculated version
# Parameters:
#   $1: Output directory (optional, defaults to ./nuget-packages)
#   $2: Configuration (optional, defaults to Release)
#
cycod_pack_dotnet() {
  local OUTPUT_DIR=${1:-./nuget-packages}
  local CONFIGURATION=${2:-Release}
  
  # Ensure VERSION and NUMERIC_VERSION are set
  if [ -z "$VERSION" ] || [ -z "$NUMERIC_VERSION" ]; then
    echo "Error: Version information not set. Call cycod_version_calculate first."
    return 1
  fi
  
  # Create output directory if it doesn't exist
  mkdir -p "$OUTPUT_DIR"
  
  # List of tools to pack
  local TOOLS=("cycod" "cycodt" "cycodmd" "cycodgr")
  
  # List of runtimes to publish for
  local RIDS=("win-x64" "linux-x64" "osx-x64")
  
  echo "Packing projects with Version=$VERSION, NumericVersion=$NUMERIC_VERSION"
  
  for TOOL in "${TOOLS[@]}"; do
    echo "→ Packing $TOOL"
    
    dotnet pack "src/$TOOL/$TOOL.csproj" \
      -c "$CONFIGURATION" \
      -p:Version="$VERSION" \
      -p:AssemblyVersion="$NUMERIC_VERSION" \
      -p:FileVersion="$NUMERIC_VERSION" \
      -p:InformationalVersion="$VERSION" \
      -o "$OUTPUT_DIR"
      
    if [ $? -ne 0 ]; then
      echo "Error: Failed to pack $TOOL"
      return 1
    fi
  done
  
  # Generate checksums for the packages
  pushd "$OUTPUT_DIR" > /dev/null
  for f in *.nupkg; do
    sha256sum "$f" > "${f}.sha256"
  done
  popd > /dev/null
  
  echo "✅ Packages created successfully in $OUTPUT_DIR"
  
  # Create the install script
  local INSTALL_SCRIPT="./install-cycod.sh"
  cat > "$INSTALL_SCRIPT" <<EOF
#!/usr/bin/env bash
set -euo pipefail

VERSION="${VERSION}"
TOOLS=("cycod" "cycodt" "cycodmd" "cycodgr")

# Resolve this script's folder, then the feed folder
DIR="\$(cd "\$(dirname "\${BASH_SOURCE[0]}")" && pwd)"

for TOOL in "\${TOOLS[@]}"; do
  echo "Installing \${TOOL} (v\${VERSION}) from local feed…"
  dotnet tool install --global \${TOOL} \\
    --version "\${VERSION}" \\
    --add-source "\${DIR}/nuget-packages"
done

echo "✅ All tools installed."
EOF

  chmod +x "$INSTALL_SCRIPT"
  echo "Created install script: $INSTALL_SCRIPT"
}

# Function: cycod_publish_self_contained
# Description: Create self-contained single-file executables and package them into ZIP files
# Parameters:
#   $1: Output directory (optional, defaults to ./self-contained)
#   $2: Configuration (optional, defaults to Release)
#
cycod_publish_self_contained() {
  local OUTPUT_DIR=${1:-./self-contained}
  local CONFIGURATION=${2:-Release}
  
  # Ensure VERSION and NUMERIC_VERSION are set
  if [ -z "$VERSION" ] || [ -z "$NUMERIC_VERSION" ]; then
    echo "Error: Version information not set. Call cycod_version_calculate first."
    return 1
  fi
  
  # Clean up old artifacts
  if [ -d "$OUTPUT_DIR" ]; then
    rm -rf "$OUTPUT_DIR"
  fi
  mkdir -p "$OUTPUT_DIR"
  
  # List of tools to publish
  local TOOLS=("cycod" "cycodt" "cycodmd" "cycodgr")
  
  # List of runtimes to publish for
  local RIDS=("win-x64" "linux-x64" "osx-x64")
  
  # First, restore dependencies
  dotnet restore
  
  # Process each platform first
  for RID in "${RIDS[@]}"; do
    # Determine platform-specific output directory name and extension
    local PLATFORM_DIR=""
    local EXT=""
    
    if [ "$RID" == "win-x64" ]; then
      PLATFORM_DIR="windows"
      EXT=".exe"
    elif [ "$RID" == "linux-x64" ]; then
      PLATFORM_DIR="linux"
    else
      PLATFORM_DIR="macos"
    fi
    
    echo "→ Processing platform: $PLATFORM_DIR"
    
    # Create platform directory
    local PLATFORM_OUTPUT_DIR="$OUTPUT_DIR/$PLATFORM_DIR"
    mkdir -p "$PLATFORM_OUTPUT_DIR"
    
    # Process each tool for this platform
    for TOOL in "${TOOLS[@]}"; do
      echo "  Building self-contained $TOOL for $PLATFORM_DIR..."
      
      # Create directory for the tool within the platform
      local PLATFORM_TOOL_DIR="$PLATFORM_OUTPUT_DIR/$TOOL"
      mkdir -p "$PLATFORM_TOOL_DIR"
      
      # Build the self-contained executable
      local PUBLISH_DIR="./src/$TOOL/bin/$CONFIGURATION/net9.0/$RID/publish-sc"
      
      # Run the publish command with self-contained and single-file parameters
      dotnet publish "src/$TOOL/$TOOL.csproj" \
        -c "$CONFIGURATION" \
        --framework net9.0 \
        -r "$RID" \
        --self-contained \
        -p:PublishSingleFile=true \
        -p:IncludeNativeLibrariesForSelfExtract=true \
        -p:Version="$VERSION" \
        -p:AssemblyVersion="$NUMERIC_VERSION" \
        -p:FileVersion="$NUMERIC_VERSION" \
        -p:InformationalVersion="$VERSION" \
        -o "$PUBLISH_DIR"
        
      if [ $? -ne 0 ]; then
        echo "Error: Self-contained publish for $TOOL ($RID) failed"
        return 1
      fi
      
      # Copy all files from the publish directory to ensure we get any potential native libraries or other resources
      echo "  Copying all published files to output directory..."
      cp -R "$PUBLISH_DIR/"* "$PLATFORM_TOOL_DIR/"
      
      # Make the Linux and macOS binaries executable
      if [ "$RID" != "win-x64" ]; then
        # Find executables (without extension) and make them executable
        find "$PLATFORM_TOOL_DIR" -type f -not -name "*.*" -exec chmod +x {} \;
        # Also make .sh files executable if any
        find "$PLATFORM_TOOL_DIR" -name "*.sh" -exec chmod +x {} \;
      fi
      
      # Create individual ZIP for platform+tool
      local TOOL_PLATFORM_ZIP="$TOOL-$RID-$VERSION.zip"
      local TOOL_PLATFORM_ZIP_PATH=$(realpath "$OUTPUT_DIR/$TOOL_PLATFORM_ZIP")
      echo "  Creating ZIP for $TOOL-$RID: $TOOL_PLATFORM_ZIP"
      
      # Navigate to the directory and zip its contents
      pushd "$PLATFORM_TOOL_DIR" > /dev/null
      zip -r "$TOOL_PLATFORM_ZIP_PATH" ./*
      popd > /dev/null
    done
    
    # Create platform-specific aggregate ZIP file with all tools
    local PLATFORM_ZIP="cycodev-tools-$RID-$VERSION.zip"
    local PLATFORM_ZIP_PATH=$(realpath "$OUTPUT_DIR/$PLATFORM_ZIP")
    echo "Creating platform aggregate ZIP: $PLATFORM_ZIP"
    
    # Navigate to the directory and zip its contents
    pushd "$PLATFORM_OUTPUT_DIR" > /dev/null
    zip -r "$PLATFORM_ZIP_PATH" ./*
    popd > /dev/null
  done
  
  # Generate checksums for the zip files
  pushd "$OUTPUT_DIR" > /dev/null
  for f in *.zip; do
    sha256sum "$f" > "${f}.sha256"
  done
  popd > /dev/null
  
  echo "✅ Self-contained executables created successfully!"
  echo "Output location: $OUTPUT_DIR"
}

# Function: cycod_check_environment
# Description: Check if all required tools are installed
# Parameters: None
# Returns: 0 if all tools are found, otherwise 1
#
cycod_check_environment() {
  local REQUIRED_TOOLS=("dotnet" "zip" "sha256sum")
  local MISSING_TOOLS=()
  
  for TOOL in "${REQUIRED_TOOLS[@]}"; do
    if ! command -v "$TOOL" &> /dev/null; then
      MISSING_TOOLS+=("$TOOL")
    fi
  done
  
  if [ ${#MISSING_TOOLS[@]} -eq 0 ]; then
    echo "✅ All required tools are installed."
    return 0
  else
    echo "❌ Missing required tools: ${MISSING_TOOLS[*]}"
    return 1
  fi
}