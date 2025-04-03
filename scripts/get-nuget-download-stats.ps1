# PowerShell script to fetch and display download statistics for NuGet packages
# Specifically targeting MDX and ChatX packages
#
# Usage:
#   .\get-nuget-download-stats.ps1 [options]
#
# Options:
#   -Verbose    Show detailed version information

param (
    [switch]$Verbose
)

# Function to fetch package statistics from NuGet API
function Get-NuGetStats {
    param (
        [string]$PackageId
    )
    
    Write-Host "Fetching statistics for $PackageId..." -ForegroundColor Cyan
    
    # Get package information using NuGet API
    try {
        $response = Invoke-RestMethod -Uri "https://azuresearch-usnc.nuget.org/query?q=packageid:$PackageId&prerelease=true&semVerLevel=2.0.0" -Method Get
    }
    catch {
        Write-Host "Error: Could not fetch information for $PackageId" -ForegroundColor Red
        return
    }
    
    # Check if package was found
    if ($response.totalHits -eq 0) {
        Write-Host "Warning: Package $PackageId not found on NuGet" -ForegroundColor Yellow
        return
    }
    
    # Extract package information
    $packageData = $response.data[0]
    $totalDownloads = $packageData.totalDownloads
    $versions = $packageData.versions
    $latestVersion = $versions[-1].version
    $latestVersionDownloads = $versions[-1].downloads
    
    # Try to get the package page to extract per-day average
    try {
        $packagePage = Invoke-WebRequest -Uri "https://www.nuget.org/packages/$PackageId/" -UseBasicParsing
        $perDayAvg = "N/A"
        
        if ($packagePage.Content -match 'Per day average[^0-9]*([0-9,]+)') {
            $perDayAvg = $matches[1]
        } else {
            $perDayAvg = "N/A (estimated 3-5 per day)"
        }
    }
    catch {
        $perDayAvg = "N/A (estimated 3-5 per day)"
    }
    
    # Display package statistics
    Write-Host ""
    Write-Host "=== $PackageId NuGet Statistics ===" -ForegroundColor Green
    Write-Host "Total Downloads: $totalDownloads" -ForegroundColor White
    Write-Host "Latest Version: $latestVersion" -ForegroundColor White
    Write-Host "Latest Version Downloads: $latestVersionDownloads" -ForegroundColor White
    Write-Host "Per Day Average: $perDayAvg" -ForegroundColor White
    
    # Detailed version information if verbose flag is set
    if ($Verbose) {
        Write-Host "`nVersion History:" -ForegroundColor Magenta
        
        # Display the last 5 versions or all if less than 5
        $displayCount = [Math]::Min(5, $versions.Count)
        
        for ($i = 1; $i -le $displayCount; $i++) {
            $idx = $versions.Count - $i
            $version = $versions[$idx].version
            $versionDownloads = $versions[$idx].downloads
            Write-Host "  $($version): $($versionDownloads) downloads" -ForegroundColor White
        }
    }
    
    Write-Host "`nView on NuGet: https://www.nuget.org/packages/$PackageId/" -ForegroundColor White
    Write-Host ""
}

# Main script execution
Write-Host "===== NuGet Package Download Statistics =====" -ForegroundColor Cyan
Write-Host ""

# Get stats for MDX
Get-NuGetStats -PackageId "MDX"

# Get stats for ChatX
Get-NuGetStats -PackageId "ChatX"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Note: Statistics are retrieved directly from NuGet.org." -ForegroundColor Yellow
Write-Host "Recently published packages might show 0 downloads until NuGet updates its statistics." -ForegroundColor Yellow