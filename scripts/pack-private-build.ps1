# -----------------------------------------------------------------------------
# usage: ./scripts/pack-private-build.ps1 1.0.0-alpha-20250527.1
# -----------------------------------------------------------------------------

[CmdletBinding()]
param (
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$Version
)

# Set strict error handling
$ErrorActionPreference = "Stop"

# Resolve this script's directory and then the repo root (parent of scripts/)
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$RepoRoot = Split-Path -Parent $ScriptDir

$OutputZip = Join-Path -Path $RepoRoot -ChildPath "CycoDevTools-$Version.zip"
$PkgDir = Join-Path -Path $RepoRoot -ChildPath "nuget-packages"

# Clean up old artifacts
if (Test-Path -Path $PkgDir) {
    Remove-Item -Path $PkgDir -Recurse -Force
}
$InstallPs1 = Join-Path -Path $RepoRoot -ChildPath "install-cycod.ps1"
if (Test-Path -Path $InstallPs1) {
    Remove-Item -Path $InstallPs1 -Force
}
$InstallCmd = Join-Path -Path $RepoRoot -ChildPath "install-cycod.cmd"
if (Test-Path -Path $InstallCmd) {
    Remove-Item -Path $InstallCmd -Force
}
if (Test-Path -Path $OutputZip) {
    Remove-Item -Path $OutputZip -Force
}
New-Item -Path $PkgDir -ItemType Directory -Force | Out-Null

# The three tool projects (folder names under src/)
$Tools = @("cycod", "cycodt", "cycodmd")
# The runtimes you want to publish for each
$Rids = @("win-x64", "linux-x64", "osx-x64")

Write-Host "Building & packing tools at version $Version…"

# Calculate numeric version components for use in AssemblyVersion and FileVersion
# Extract the major.minor.build parts from the version
if ($Version -match "^(\d+)\.(\d+)\.(\d+)") {
    $Major = $Matches[1]
    $Minor = $Matches[2]
    $Build = $Matches[3]
} else {
    Write-Error "Version doesn't match expected format: $Version"
    exit 1
}

# Try to extract a YYYYMMDD date pattern from the version
$DatePattern = [regex]::Match($Version, '20\d{2}(0[1-9]|1[0-2])(0[1-9]|[12][0-9]|3[01])')
$DayOfYear = 0

if ($DatePattern.Success) {
    $DatePart = $DatePattern.Value
    Write-Host "Found date in version: $DatePart"

    # Extract year, month, day
    $Year = $DatePart.Substring(0, 4)
    $Month = $DatePart.Substring(4, 2)
    $Day = $DatePart.Substring(6, 2)

    # Convert to day of year
    $DateObj = Get-Date -Year $Year -Month ([int]$Month) -Day ([int]$Day)
    $DayOfYear = $DateObj.DayOfYear
    Write-Host "Using date from version: $Year-$Month-$Day (day of year: $DayOfYear)"
} else {
    Write-Host "No date found in version, using today's date as fallback"
    $DayOfYear = (Get-Date).DayOfYear
}

# Extract final decimal part if it exists (match the last .digit sequence)
$FinalPart = 0
if ($Version -match '\.(\d+)$') {
    $FinalPart = [int]$Matches[1]
}

# Calculate revision: day_of_year * 100 + final_part
$Revision = $DayOfYear * 100 + $FinalPart

# Safety check: ensure revision doesn't exceed maximum allowed value
if ($Revision -gt 65535) {
    Write-Warning "Revision exceeds maximum allowed value (65535). Capping at 65535."
    $Revision = 65535
}

# Combine for a valid .NET version with 4 parts
$NumericVersion = "$Major.$Minor.$Build.$Revision"
Write-Host "Using numeric version: $NumericVersion for AssemblyVersion and FileVersion"

# Update version information in all project files
foreach ($Tool in $Tools) {
    $ProjectPath = Join-Path -Path $RepoRoot -ChildPath "src/$Tool/$Tool.csproj"
    
    Write-Host "-> Updating version information in $Tool.csproj"
    
    [xml]$ProjectXml = Get-Content -Path $ProjectPath
    
    # Get or create PropertyGroup
    $PropertyGroups = $ProjectXml.Project.PropertyGroup
    $VersionPropertyGroup = $null
    
    foreach ($pg in $PropertyGroups) {
        if ($pg.Version -ne $null) {
            $VersionPropertyGroup = $pg
            break
        }
    }
    
    if ($null -eq $VersionPropertyGroup) {
        $VersionPropertyGroup = $ProjectXml.CreateElement("PropertyGroup")
        $ProjectXml.Project.AppendChild($VersionPropertyGroup)
    }
    
    # Update or create Version element
    if ($null -eq $VersionPropertyGroup.Version) {
        $VersionElement = $ProjectXml.CreateElement("Version")
        $VersionElement.InnerText = $Version
        $VersionPropertyGroup.AppendChild($VersionElement)
    } else {
        $VersionPropertyGroup.Version = $Version
    }
    
    # Update or create InformationalVersion element
    if ($null -eq $VersionPropertyGroup.InformationalVersion) {
        $InfoVersionElement = $ProjectXml.CreateElement("InformationalVersion")
        $InfoVersionElement.InnerText = $Version
        $VersionPropertyGroup.AppendChild($InfoVersionElement)
    } else {
        $VersionPropertyGroup.InformationalVersion = $Version
    }
    
    # Update or create AssemblyVersion element
    if ($null -eq $VersionPropertyGroup.AssemblyVersion) {
        $AssemblyVersionElement = $ProjectXml.CreateElement("AssemblyVersion")
        $AssemblyVersionElement.InnerText = $NumericVersion
        $VersionPropertyGroup.AppendChild($AssemblyVersionElement)
    } else {
        $VersionPropertyGroup.AssemblyVersion = $NumericVersion
    }
    
    # Update or create FileVersion element
    if ($null -eq $VersionPropertyGroup.FileVersion) {
        $FileVersionElement = $ProjectXml.CreateElement("FileVersion")
        $FileVersionElement.InnerText = $NumericVersion
        $VersionPropertyGroup.AppendChild($FileVersionElement)
    } else {
        $VersionPropertyGroup.FileVersion = $NumericVersion
    }
    
    # Save the project file
    $ProjectXml.Save($ProjectPath)
}

# Build the .NET projects
Push-Location $RepoRoot
try {
    & dotnet restore
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet restore failed with exit code $LASTEXITCODE"
    }
    
    & dotnet build -c Release
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build failed with exit code $LASTEXITCODE"
    }
    
    foreach ($Tool in $Tools) {
        Write-Host "→ $Tool"
        
        # 1) publish self-contained bits
        foreach ($Rid in $Rids) {
            Write-Host "  Publishing for $Rid..."
            & dotnet publish "$RepoRoot/src/$Tool/$Tool.csproj" -c Release -r $Rid --no-restore
            if ($LASTEXITCODE -ne 0) {
                throw "dotnet publish for $Tool ($Rid) failed with exit code $LASTEXITCODE"
            }
        }
        
        # 2) pack into a nupkg, using the updated version from the project file
        Write-Host "  Creating NuGet package..."
        & dotnet pack "$RepoRoot/src/$Tool/$Tool.csproj" -c Release --no-build -o $PkgDir
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet pack for $Tool failed with exit code $LASTEXITCODE"
        }
    }
}
finally {
    Pop-Location
}

# Generate the PowerShell install script at the repo root
$InstallScriptContent = @"
# PowerShell script to install Cycod tools
# Usage: ./install-cycod.ps1

`$ErrorActionPreference = "Stop"

`$Version = "$Version"
`$Tools = @("cycod", "cycodt", "cycodmd")

# Resolve this script's folder, then the feed folder
# Use more reliable method to get script path
`$ScriptDir = Split-Path -Parent `$MyInvocation.MyCommand.Path
`$NuGetPackagesPath = Join-Path -Path `$ScriptDir -ChildPath "nuget-packages"

# Verify the NuGet packages folder exists
if (-not (Test-Path -Path `$NuGetPackagesPath -PathType Container)) {
    Write-Error "Cannot find NuGet packages folder at: `$NuGetPackagesPath"
    Write-Error "Make sure you're running this script from the unzipped folder that contains the nuget-packages directory."
    exit 1
}

Write-Host "Installing Cycod tools from `$ScriptDir"
Write-Host "NuGet packages located at: `$NuGetPackagesPath"

foreach (`$Tool in `$Tools) {
    Write-Host ""
    Write-Host "Installing `$Tool (v`$Version) from local feed..."
    
    # Use single line command to avoid line continuation issues
    & dotnet tool install --global `$Tool --version "`$Version" --add-source "`$NuGetPackagesPath"
        
    if (`$LASTEXITCODE -ne 0) {
        Write-Error "Failed to install `$Tool. Exit code: `$LASTEXITCODE"
        exit `$LASTEXITCODE
    }
}

Write-Host ""
Write-Host "✅ All tools installed successfully."
"@

Set-Content -Path $InstallPs1 -Value $InstallScriptContent

# Create CMD wrapper for install-cycod.ps1
$CmdWrapperPath = Join-Path -Path $RepoRoot -ChildPath "install-cycod.cmd"
$CmdWrapperContent = @"
@echo off
REM CMD wrapper for the PowerShell install-cycod script
SETLOCAL

echo Installing Cycod tools...

REM First check if PowerShell is available
WHERE powershell >nul 2>&1
if %ERRORLEVEL% neq 0 (
  echo Error: PowerShell is not found in your PATH. Please install PowerShell to continue.
  exit /b 1
)

REM Check if the PS1 script exists
if not exist "%~dp0install-cycod.ps1" (
  echo Error: Cannot find install-cycod.ps1 in the same directory as this CMD file.
  echo This batch file should be run from the unzipped folder containing install-cycod.ps1.
  exit /b 1
)

REM Check if the NuGet packages folder exists
if not exist "%~dp0nuget-packages" (
  echo Error: Cannot find nuget-packages folder in the same directory as this CMD file.
  echo This batch file should be run from the unzipped folder containing the nuget-packages directory.
  exit /b 1
)

echo Running PowerShell installation script...
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0install-cycod.ps1"
if %ERRORLEVEL% neq 0 (
  echo Error: PowerShell installation script failed with exit code %ERRORLEVEL%
  exit /b %ERRORLEVEL%
)

echo.
echo Installation complete! The tools should now be available in your PATH.
ENDLOCAL
"@

Set-Content -Path $CmdWrapperPath -Value $CmdWrapperContent
Write-Host "Created CMD wrapper for installation"

# Zip up the feed and installer into the repo root
try {
    $FilesToZip = @("nuget-packages", "install-cycod.ps1", "install-cycod.cmd")
    Compress-Archive -Path ($FilesToZip | ForEach-Object { Join-Path -Path $RepoRoot -ChildPath $_ }) -DestinationPath $OutputZip -Force
    
    Write-Host ""
    Write-Host "🎁 Packaged everything into $OutputZip"
}
catch {
    Write-Error "Failed to create ZIP file: $_"
    exit 1
}