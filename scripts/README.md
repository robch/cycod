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

### What it does

- Adds debug binary paths to the front of PATH:
  - `src/cycod/bin/Debug/net9.0`
  - `src/cycodmd/bin/Debug/net9.0`
  - `src/cycodt/bin/Debug/net9.0`
- When you run `cycod`, `cycodmd`, or `cycodt`, it will use debug versions if built
- Falls back to global dotnet tool versions if debug versions don't exist
- Automatically runs in codespaces via `postCreateCommand` in devcontainer.json

### Integration with .bashrc

See `bashrc-setup-example.sh` for examples of how to integrate this with your ~/.bashrc file.

## Other Scripts

- `build.sh` - Build the solution
- `pack.sh` - Package the solution  
- `publish-self-contained.sh` - Publish self-contained executables
- `get-nuget-download-stats.sh` - Get NuGet download statistics