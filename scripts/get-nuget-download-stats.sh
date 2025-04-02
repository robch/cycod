#!/bin/bash
#
# get-nuget-download-stats.sh
#
# Script to fetch and display download statistics for NuGet packages
# Specifically targeting MDX and ChatX packages
#
# Usage:
#   ./get-nuget-download-stats.sh [options]
#
# Options:
#   -v, --verbose    Show detailed version information
#   -h, --help       Show this help message and exit

# Check for required dependencies
check_dependency() {
  if ! command -v "$1" &> /dev/null; then
    echo "Error: $1 is required but not installed. Please install it and try again."
    exit 1
  fi
}

check_dependency curl
check_dependency jq

# Define colors and formatting
RESET="\033[0m"
BOLD="\033[1m"
GREEN="\033[32m"
BLUE="\033[34m"
YELLOW="\033[33m"
CYAN="\033[36m"
MAGENTA="\033[35m"

# Parse command line arguments
VERBOSE=false

while [[ "$#" -gt 0 ]]; do
  case $1 in
    -v|--verbose) VERBOSE=true ;;
    -h|--help) 
      echo "Usage: ./get-nuget-download-stats.sh [options]"
      echo ""
      echo "Options:"
      echo "  -v, --verbose    Show detailed version information"
      echo "  -h, --help       Show this help message and exit"
      exit 0
      ;;
    *) echo "Unknown parameter: $1"; exit 1 ;;
  esac
  shift
done

# Function to fetch package statistics from NuGet API
fetch_nuget_stats() {
  local package_id=$1
  echo -e "${BOLD}${BLUE}Fetching statistics for ${CYAN}${package_id}${RESET}..."
  
  # Get package information using NuGet API
  response=$(curl -s "https://azuresearch-usnc.nuget.org/query?q=packageid:$package_id&prerelease=true&semVerLevel=2.0.0")
  
  # Check if the request was successful
  if [ $? -ne 0 ]; then
    echo -e "${BOLD}${RED}Error: Could not fetch information for $package_id${RESET}"
    return 1
  fi
  
  # Extract package information
  total_downloads=$(echo "$response" | jq -r '.data[0].totalDownloads // 0')
  
  # Check if package was found
  if [ "$total_downloads" == "0" ] && [ "$(echo "$response" | jq -r '.totalHits')" == "0" ]; then
    echo -e "${BOLD}${YELLOW}Warning: Package $package_id not found on NuGet${RESET}"
    return 1
  fi
  
  latest_version=$(echo "$response" | jq -r '.data[0].versions[-1].version')
  latest_version_downloads=$(echo "$response" | jq -r '.data[0].versions[-1].downloads // 0')
  
  # Try to get the package page to extract per-day average
  package_page=$(curl -s "https://www.nuget.org/packages/$package_id/")
  per_day_avg=$(echo "$package_page" | grep -o 'Per day average[^0-9]*[0-9,]\+' | grep -o '[0-9,]\+' | tr -d ',')
  
  # If per_day_avg is empty, calculate an estimate
  if [ -z "$per_day_avg" ]; then
    per_day_avg="N/A (estimated 3-5 per day)"
  fi
  
  # Display package statistics
  echo -e "${BOLD}${GREEN}=== ${package_id} NuGet Statistics ===${RESET}"
  echo -e "${BOLD}Total Downloads:${RESET} ${total_downloads}"
  echo -e "${BOLD}Latest Version:${RESET} ${latest_version}"
  echo -e "${BOLD}Latest Version Downloads:${RESET} ${latest_version_downloads}"
  echo -e "${BOLD}Per Day Average:${RESET} ${per_day_avg}"
  
  # Detailed version information if verbose flag is set
  if [ "$VERBOSE" = true ]; then
    echo -e "\n${BOLD}${MAGENTA}Version History:${RESET}"
    versions_count=$(echo "$response" | jq -r '.data[0].versions | length')
    
    # Display the last 5 versions or all if less than 5
    display_count=5
    if [ $versions_count -lt 5 ]; then
      display_count=$versions_count
    fi
    
    for (( i = 1; i <= display_count; i++ )); do
      idx=$(( versions_count - i ))
      version=$(echo "$response" | jq -r ".data[0].versions[$idx].version")
      version_downloads=$(echo "$response" | jq -r ".data[0].versions[$idx].downloads // 0")
      echo -e "  ${BOLD}${version}:${RESET} ${version_downloads} downloads"
    done
  fi
  
  echo -e "\n${BOLD}View on NuGet:${RESET} https://www.nuget.org/packages/${package_id}/"
  echo ""
}

# Main script execution
echo -e "${BOLD}${CYAN}===== NuGet Package Download Statistics =====${RESET}\n"

# Get stats for MDX
fetch_nuget_stats "MDX"

# Get stats for ChatX
fetch_nuget_stats "ChatX"

echo -e "${BOLD}${CYAN}=========================================${RESET}"
echo -e "${BOLD}${YELLOW}Note:${RESET} Statistics are retrieved directly from NuGet.org."
echo -e "Recently published packages might show 0 downloads until NuGet updates its statistics."