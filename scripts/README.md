# Scripts

This directory contains various utility scripts for the cycod project.

## setup-debug-path.sh

Sets up the development environment to prioritize debug builds over globally installed dotnet tools.

### Usage

```bash
# Setup both current session and ~/.bashrc
./scripts/setup-debug-path.sh

# Only setup current session (don't modify ~/.bashrc)
./scripts/setup-debug-path.sh --session-only

# Only setup ~/.bashrc (don't modify current session)
./scripts/setup-debug-path.sh --bashrc-only

# Show help
./scripts/setup-debug-path.sh --help
```

## cycodpath.cmd

Windows equivalent of setup-debug-path.sh. Sets up PATH for current session to prioritize debug builds.

### Usage

```cmd
REM Setup PATH for current session
scripts\cycodpath.cmd
```

### What they do

- Add debug binary paths to the front of PATH:
  - `src/cycod/bin/Debug/net9.0`
  - `src/cycodmd/bin/Debug/net9.0`
  - `src/cycodt/bin/Debug/net9.0`
  - `src/cycodgr/bin/Debug/net9.0`
  - `src/mcp/geolocation/bin/Debug/net9.0`
  - `src/mcp/mxlookup/bin/Debug/net9.0`
  - `src/mcp/osm/bin/Debug/net9.0`
  - `src/mcp/weather/bin/Debug/net9.0`
  - `src/mcp/whois/bin/Debug/net9.0`
- When you run any cycod tool, it will use debug versions if built
- Falls back to global dotnet tool versions if debug versions don't exist
- **setup-debug-path.sh** can persist changes to ~/.bashrc for permanent setup
- **cycodpath.cmd** sets up PATH for current Windows session only
- Automatically runs in codespaces via `postCreateCommand` in devcontainer.json

### Integration with .bashrc

See `bashrc-setup-example.sh` for examples of how to integrate this with your ~/.bashrc file.

## Other Scripts

- `build.sh` - Build the solution
- `pack.sh` - Package the solution  
- `publish-self-contained.sh` - Publish self-contained executables
- `get-nuget-download-stats.sh` - Get NuGet download statistics