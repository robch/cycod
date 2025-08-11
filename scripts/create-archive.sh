#!/bin/bash

# --- Script to Create and Tag a Release Archive for Homebrew ---
# This script should be run from the root of your mercersoft/cycod repository.
# It creates a gzipped tar archive, calculates its SHA-256 checksum,
# and creates a Git tag for a new release.

# Exit immediately if a command exits with a non-zero status.
set -e

# --- Configuration ---
# Set the desired version number as an argument to the script.
# Example usage: ./create_archive.sh 1.0.1
if [ -z "$1" ]; then
  echo "Usage: $0 <version>"
  echo "Example: $0 1.0.1"
  exit 1
fi

VERSION="v$1"
ARCHIVE_NAME="cycod-archive-${VERSION}.tar.gz"

echo "--- Creating release for version: ${VERSION} ---"

# --- Create the Tarball ---
# We use 'git archive' to create the tarball from the latest commit,
# which ensures we get a clean copy without any uncommitted changes or .git folder.
# We then pipe the output to 'gzip' to compress it.
echo "Creating archive file: ${ARCHIVE_NAME}..."
git archive --format=tar --prefix="cycod-${VERSION}/" HEAD | gzip > "${ARCHIVE_NAME}"

# --- Calculate the SHA-256 Checksum ---
# This checksum is a critical component of the Homebrew formula for security
# and integrity verification.
echo "Calculating SHA-256 checksum..."
SHA256_CHECKSUM=$(shasum -a 256 "${ARCHIVE_NAME}" | awk '{print $1}')
echo "SHA-256: ${SHA256_CHECKSUM}"

# --- Create and Push the Git Tag ---
# An annotated tag is best practice for releases as it contains a message.
echo "Creating and pushing Git tag: ${VERSION}..."
git tag -a "${VERSION}" -m "Release ${VERSION}"
git push origin "${VERSION}"

# --- Clean up the local archive file ---
echo "Cleaning up local archive file..."
rm "${ARCHIVE_NAME}"

# --- Provide Homebrew formula snippet ---
# This is the information you will need to add to your Homebrew formula.
echo ""
echo "-----------------------------------"
echo "✅ Done! Use the following in your Homebrew formula:"
echo "-----------------------------------"
echo "  url \"https://github.com/robch/cycod/archive/refs/tags/${VERSION}.tar.gz\""
echo "  sha256 \"${SHA256_CHECKSUM}\""
echo ""
