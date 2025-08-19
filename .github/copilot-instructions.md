# CycoD - AI-powered CLI Development Environment

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the information here.

## Working Effectively

### Prerequisites and Installation
- Install .NET 9.0 SDK - the project requires .NET 9.0 (not .NET 8.0):
  ```bash
  curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version 9.0.101
  export PATH="$HOME/.dotnet:$PATH"
  dotnet --version  # Should show 9.0.101 or later
  ```

### Build Process
- Make scripts executable first:
  ```bash
  chmod +x ./scripts/*.sh
  ```
- Build the projects using the build script:
  ```bash
  export PATH="$HOME/.dotnet:$PATH"
  ./scripts/build.sh
  ```
  **NEVER CANCEL**: Initial build takes approximately 33 seconds. Set timeout to 90+ seconds.
- For Release builds:
  ```bash
  dotnet build --configuration Release
  ```
  **NEVER CANCEL**: Release build takes approximately 4-5 seconds. Set timeout to 30+ seconds.

### Testing
- Run unit tests:
  ```bash
  dotnet test --configuration Debug --verbosity normal
  ```
  **NEVER CANCEL**: Unit tests take approximately 8-9 seconds (~256 tests, 1 may fail intermittently). Set timeout to 60+ seconds.
- Run integration tests (cycodt framework):
  ```bash
  export PATH="$HOME/.dotnet:$PATH"
  $HOME/.dotnet/dotnet src/cycodt/bin/Debug/net9.0/cycodt.dll run
  ```
  **NEVER CANCEL**: Integration tests take approximately 8-10 seconds. Set timeout to 60+ seconds.

### Running the Applications
All applications must be run using the .NET 9.0 runtime:

#### CycoD (Main AI CLI)
- Help: `$HOME/.dotnet/dotnet src/cycod/bin/Release/net9.0/cycod.dll --help`
- Version: `$HOME/.dotnet/dotnet src/cycod/bin/Release/net9.0/cycod.dll version`
- Config test: `$HOME/.dotnet/dotnet src/cycod/bin/Release/net9.0/cycod.dll config get NonexistentKey --local`

#### CycoDT (Test Framework)
- Help: `$HOME/.dotnet/dotnet src/cycodt/bin/Release/net9.0/cycodt.dll --help`
- List tests: `$HOME/.dotnet/dotnet src/cycodt/bin/Release/net9.0/cycodt.dll list`

#### CycoDMD (Markdown Generator)
- Help: `$HOME/.dotnet/dotnet src/cycodmd/bin/Release/net9.0/cycodmd.dll --help`

## Validation

### Essential Validation Steps
Always validate changes by running these commands:

1. **Build Validation**:
   ```bash
   export PATH="$HOME/.dotnet:$PATH"
   dotnet build --configuration Release
   ```

2. **Unit Test Validation**:
   ```bash
   dotnet test --configuration Release --verbosity normal
   ```

3. **Application Functionality Tests**:
   ```bash
   # Test cycod help
   $HOME/.dotnet/dotnet src/cycod/bin/Release/net9.0/cycod.dll --help
   
   # Test cycod version
   $HOME/.dotnet/dotnet src/cycod/bin/Release/net9.0/cycod.dll version
   
   # Test config system
   $HOME/.dotnet/dotnet src/cycod/bin/Release/net9.0/cycod.dll config get NonexistentKey --local
   
   # Test cycodt help
   $HOME/.dotnet/dotnet src/cycodt/bin/Release/net9.0/cycodt.dll --help
   
   # Test cycodmd help
   $HOME/.dotnet/dotnet src/cycodmd/bin/Release/net9.0/cycodmd.dll --help
   
   # Test cycodt list functionality
   $HOME/.dotnet/dotnet src/cycodt/bin/Release/net9.0/cycodt.dll list
   ```

### Manual Validation Scenarios
After making code changes, always test these complete user scenarios:

1. **Configuration Management**:
   - Test setting and getting configuration values:
     ```bash
     $HOME/.dotnet/dotnet src/cycod/bin/Release/net9.0/cycod.dll config set Test.Value "hello world" --local
     $HOME/.dotnet/dotnet src/cycod/bin/Release/net9.0/cycod.dll config get Test.Value --local
     $HOME/.dotnet/dotnet src/cycod/bin/Release/net9.0/cycod.dll config clear Test.Value --local
     ```
   - Verify config scopes (local, user, global) work correctly
   - Test environment variable integration
   - Note: Unknown configuration keys will show a warning but still be set

2. **Help System**:
   - Verify all help commands work for cycod, cycodt, and cycodmd
   - Test that --help flag displays properly formatted output

3. **Version Information**:
   - Ensure version command works and displays correct format

## Common Tasks

### Project Structure
```
src/
├── common/           # Shared libraries
├── cycod/           # Main AI CLI application  
├── cycodmd/         # Markdown generator CLI
├── cycodt/          # Test framework CLI
└── mcp/             # Model Context Protocol plugins
tests/
├── cycod/           # Unit tests (C#)
├── cycod-yaml/      # Integration tests (YAML)
└── cycodt-yaml/     # Test framework tests (YAML)
```

### Key Solution File
- Main solution: `cycod.sln` - contains all projects

### Build Targets
- **Debug**: Default development build
- **Release**: Optimized production build with cross-platform support (Windows, Linux, macOS)

### Common Development Patterns

#### Adding Features to CycoD
1. Add code to `src/cycod/` 
2. Add unit tests to `tests/cycod/`
3. Build and test: `dotnet build && dotnet test`
4. Test manually with validation scenarios above

#### Working with Configuration
- Configuration files: `.cycod/config` (YAML or INI format)
- Scopes: global (`~/.local/share/.cycod`), user (`~/.cycod`), local (`./.cycod`)
- Environment variables: Prefixed with provider names (e.g., `OPENAI_API_KEY`, `GITHUB_TOKEN`)

#### Working with Tests
- Unit tests: Use MSTest framework in `tests/cycod/`
- Integration tests: YAML-based tests in `tests/cycodt-yaml/`
- Test runner: Use cycodt for YAML-based tests

### Build Artifacts
After successful builds, binaries are located at:
- `src/cycod/bin/{Configuration}/net9.0/cycod.dll`
- `src/cycodt/bin/{Configuration}/net9.0/cycodt.dll`  
- `src/cycodmd/bin/{Configuration}/net9.0/cycodmd.dll`

### CI/CD Integration
- GitHub Actions workflows: `.github/workflows/ci.yml` and `.github/workflows/release.yml`
- CI runs on .NET 9.0, uses script-based builds, runs both unit and integration tests
- Artifacts include cross-platform builds and test results

## Troubleshooting

### Common Issues

#### .NET Version Mismatch
**Error**: "You must install or update .NET to run this application"
**Solution**: Ensure .NET 9.0 SDK is installed and in PATH:
```bash
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version 9.0.101
export PATH="$HOME/.dotnet:$PATH"
```

#### Build Script Permission Issues
**Error**: Permission denied running build scripts
**Solution**: Make scripts executable:
```bash
chmod +x ./scripts/*.sh
```

#### Integration Test Failures
**Issue**: cycodt tests fail because they can't find .NET 9.0 runtime
**Solution**: Use explicit dotnet runtime path:
```bash
$HOME/.dotnet/dotnet src/cycodt/bin/Debug/net9.0/cycodt.dll run
```

### Performance Expectations
- **First build**: ~33 seconds (includes package restore)
- **Subsequent builds**: ~4-5 seconds  
- **Unit tests**: ~8-9 seconds (~256 tests, 1 may fail intermittently)
- **Integration tests**: ~8-10 seconds
- **Total development cycle**: ~45-50 seconds for full build + test

### Key Files to Monitor
- `cycod.sln` - Main solution file
- `src/*/**.csproj` - Project files with dependencies
- `.github/workflows/ci.yml` - CI build configuration  
- `scripts/build.sh` - Build automation
- `BuildCommon.targets` - Shared build settings

Always run validation commands after making changes to ensure the applications remain functional.