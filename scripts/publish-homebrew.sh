#!/bin/bash

# --- Script to Create and Tag a Release Archive for Homebrew ---
# This script should be run from the root of your mercersoft/cycod repository.
# It creates a Git tag for a new release, downloads the GitHub-generated archive,
# and calculates its SHA-256 checksum.

# Exit immediately if a command exits with a non-zero status.
set -e

# --- Configuration ---
# Set the desired version number as an argument to the script.
# Example usage: ./publish-homebrew.sh 1.0.1
# If no version is provided, the script will suggest the next version

if [ -z "$1" ]; then
  # Get the latest tag
  LATEST_TAG=$(git tag -l --sort=-v:refname | head -n 1)

  if [ -z "$LATEST_TAG" ]; then
    # No tags exist yet, suggest v1.0.0
    SUGGESTED_VERSION="1.0.0"
  else
    # Parse the latest tag (format: v1.0.X) and increment the patch version
    if [[ $LATEST_TAG =~ ^v([0-9]+)\.([0-9]+)\.([0-9]+)$ ]]; then
      MAJOR="${BASH_REMATCH[1]}"
      MINOR="${BASH_REMATCH[2]}"
      PATCH="${BASH_REMATCH[3]}"
      NEXT_PATCH=$((PATCH + 1))
      SUGGESTED_VERSION="${MAJOR}.${MINOR}.${NEXT_PATCH}"
    else
      echo "Error: Could not parse latest tag: ${LATEST_TAG}"
      echo "Please provide a version number manually."
      echo "Usage: $0 <version>"
      echo "Example: $0 1.0.1"
      exit 1
    fi
  fi

  # Prompt the user
  echo "Latest tag: ${LATEST_TAG:-none}"
  echo "Suggested next version: v${SUGGESTED_VERSION}"
  read -p "Use v${SUGGESTED_VERSION}? (y/n): " -n 1 -r
  echo

  if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "Release cancelled."
    exit 0
  fi

  VERSION="v${SUGGESTED_VERSION}"
else
  VERSION="v$1"
fi
ARCHIVE_NAME="cycod-archive-${VERSION}.tar.gz"
GITHUB_URL="https://github.com/robch/cycod/archive/refs/tags/${VERSION}.tar.gz"

echo "--- Creating release for version: ${VERSION} ---"

# --- Create and Push the Git Tag ---
# An annotated tag is best practice for releases as it contains a message.
echo "Creating and pushing Git tag: ${VERSION}..."
git tag -a "${VERSION}" -m "Release ${VERSION}"
git push origin "${VERSION}"

# --- Wait a moment for GitHub to process the tag ---
echo "Waiting for GitHub to process the tag..."
sleep 5

# --- Download the Archive from GitHub ---
# This ensures we get the exact same archive that Homebrew will download
echo "Downloading archive from GitHub: ${GITHUB_URL}..."
curl -L "${GITHUB_URL}" -o "${ARCHIVE_NAME}"

# --- Calculate the SHA-256 Checksum ---
# This checksum is a critical component of the Homebrew formula for security
# and integrity verification.
echo "Calculating SHA-256 checksum..."
SHA256_CHECKSUM=$(shasum -a 256 "${ARCHIVE_NAME}" | awk '{print $1}')
echo "SHA-256: ${SHA256_CHECKSUM}"

# --- Clean up the local archive file ---
echo "Cleaning up local archive file..."
rm "${ARCHIVE_NAME}"

# --- Provide Homebrew formula snippet ---
# This is the information you will need to add to your Homebrew formula.
echo ""
echo "-----------------------------------"
echo "✅ Done! Use the following in your Homebrew formula:"
echo "-----------------------------------"
echo "  url \"${GITHUB_URL}\""
echo "  sha256 \"${SHA256_CHECKSUM}\""
echo ""
