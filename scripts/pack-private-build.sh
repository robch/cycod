#!/usr/bin/env bash
set -euo pipefail

# -----------------------------------------------------------------------------
# usage: ./scripts/pack-private-build.sh 1.0.0-alpha-20250527.1
# -----------------------------------------------------------------------------

# Resolve this script's directory and then the repo root (parent of scripts/)
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

VERSION=${1:?You must supply a version (e.g. 1.0.0-alpha-20250527.1)}
OUTPUT_ZIP="${REPO_ROOT}/CycoDevTools-${VERSION}.zip"
PKG_DIR="${REPO_ROOT}/nuget-packages"

# Clean up old artifacts
rm -rf "${PKG_DIR}" "${REPO_ROOT}/install-cycod.sh" "${OUTPUT_ZIP}"
mkdir -p "${PKG_DIR}"

# The three tool projects (folder names under src/)
TOOLS=(cycod cycodt cycodmd)
# The runtimes you want to publish for each
RIDS=(win-x64 linux-x64 osx-x64)

echo "Building & packing tools at version ${VERSION}…"

# Calculate numeric version components for use in AssemblyVersion and FileVersion
# Extract the major.minor.build parts from the version
MAJOR=$(echo $VERSION | sed -E 's/^([0-9]+).*/\1/')
MINOR=$(echo $VERSION | sed -E 's/^[0-9]+\.([0-9]+).*/\1/')
BUILD=$(echo $VERSION | sed -E 's/^[0-9]+\.[0-9]+\.([0-9]+).*/\1/')

# Try to extract a YYYYMMDD date pattern from the version
DATE_PART=$(echo $VERSION | grep -oE '20[0-9]{2}(0[1-9]|1[0-2])(0[1-9]|[12][0-9]|3[01])')

if [ -n "$DATE_PART" ]; then
  echo "Found date in version: $DATE_PART"
  # Convert YYYYMMDD to day of year
  # Extract year, month, day
  YEAR=${DATE_PART:0:4}
  MONTH=${DATE_PART:4:2}
  DAY=${DATE_PART:6:2}
  
  # Use date command to convert to day of year
  DAY_OF_YEAR=$(date -d "$YEAR-$MONTH-$DAY" +%j | sed 's/^0*//')
  echo "Using date from version: $YEAR-$MONTH-$DAY (day of year: $DAY_OF_YEAR)"
else
  echo "No date found in version, using today's date as fallback"
  DAY_OF_YEAR=$(date '+%j' | sed 's/^0*//')
fi

# Extract final decimal part if it exists (match the last .digit sequence)
FINAL_PART=$(echo $VERSION | grep -oE '\.[0-9]+$' | grep -oE '[0-9]+')
if [ -z "$FINAL_PART" ]; then
  FINAL_PART=0
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

# Update version information in all project files
for TOOL in "${TOOLS[@]}"; do
  PROJECT_PATH="${REPO_ROOT}/src/${TOOL}/${TOOL}.csproj"
  
  # Update versions in project files similar to release.yml
  echo "→ Updating version information in ${TOOL}.csproj"
  
  # Apply semantic version (can include pre-release tags)
  sed -i "s/<Version>.*<\/Version>/<Version>${VERSION}<\/Version>/" "$PROJECT_PATH"
  
  # Apply informational version (can include pre-release tags)
  sed -i "s/<InformationalVersion>.*<\/InformationalVersion>/<InformationalVersion>${VERSION}<\/InformationalVersion>/" "$PROJECT_PATH"
  
  # Apply numeric versions for assembly and file versions (must be numeric only)
  sed -i "s/<AssemblyVersion>.*<\/AssemblyVersion>/<AssemblyVersion>${NUMERIC_VERSION}<\/AssemblyVersion>/" "$PROJECT_PATH"
  sed -i "s/<FileVersion>.*<\/FileVersion>/<FileVersion>${NUMERIC_VERSION}<\/FileVersion>/" "$PROJECT_PATH"
done

cd "${REPO_ROOT}"
dotnet restore
dotnet build -c Release

for TOOL in "${TOOLS[@]}"; do
  echo "→ ${TOOL}"
  # 1) publish self-contained bits
  for RID in "${RIDS[@]}"; do
    dotnet publish "${REPO_ROOT}/src/${TOOL}/${TOOL}.csproj" \
      -c Release -r "${RID}" --no-restore
  done

  # 2) pack into a nupkg, using the updated version from the project file
  dotnet pack "${REPO_ROOT}/src/${TOOL}/${TOOL}.csproj" \
    -c Release --no-build \
    -o "${PKG_DIR}"
done

# Generate the install script at the repo root
cat > "${REPO_ROOT}/install-cycod.sh" <<EOF
#!/usr/bin/env bash
set -euo pipefail

VERSION="${VERSION}"
TOOLS=(cycod cycodt cycodmd)

# Resolve this script's folder, then the feed folder
DIR="\$(cd "\$(dirname "\${BASH_SOURCE[0]}")" && pwd)"

for TOOL in "\${TOOLS[@]}"; do
  echo "Installing \${TOOL} (v\${VERSION}) from local feed…"
  dotnet tool install --global \${TOOL} \
    --version "\${VERSION}" \
    --add-source "\${DIR}/nuget-packages"
done

echo "✅ All tools installed."
EOF

chmod +x "${REPO_ROOT}/install-cycod.sh"

# Zip up the feed and installer into the repo root
(
  cd "${REPO_ROOT}"
  if command -v zip >/dev/null 2>&1; then
    # Use zip command if available
    zip -r "$(basename "${OUTPUT_ZIP}")" "nuget-packages" "install-cycod.sh"
  else
    # Use PowerShell on Windows if zip is not available
    powershell -Command "Compress-Archive -Path 'nuget-packages', 'install-cycod.sh' -DestinationPath '$(basename "${OUTPUT_ZIP}")' -Force"
  fi
)

echo
echo "🎁 Packaged everything into ${OUTPUT_ZIP}"