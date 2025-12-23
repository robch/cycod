# Adding a New CLI Tool to the Project

## Overview

This document describes the exact steps needed to add a new CLI tool to the cycod project ecosystem, based on how `cycodgr` was added.

## Reference Commit

The primary commit that added `cycodgr`: **14169d53** - "feat: Implement cycodgr - GitHub Repository and Code Search CLI (Phases A-E Complete)"

## Required Changes

### 1. Create Project Structure

```
src/<toolname>/
├── <toolname>.csproj          # Project file
├── Program.cs                 # Entry point
├── <ToolName>ProgramInfo.cs   # Program info class
├── README.md                  # Tool-specific documentation
├── CommandLine/               # Command-line parsing
│   ├── <ToolName>Command.cs
│   └── <ToolName>CommandLineOptions.cs
├── CommandLineCommands/       # Command implementations
│   └── <CommandName>Command.cs
├── Helpers/                   # Helper classes
│   └── <Feature>Helpers.cs
├── Models/                    # Data models (if needed)
│   ├── ModelA.cs
│   └── ModelB.cs
└── assets/                    # Embedded resources
    ├── help/                  # Help text files
    │   ├── usage.txt
    │   ├── examples.txt
    │   └── ...
    └── prompts/               # AI prompts (if needed)
        ├── system.md
        └── user.md
```

### 2. Create Project File (`<toolname>.csproj`)

**Key elements:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <!-- Import shared version settings -->
  <Import Project="../../BuildCommon.targets" />

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <EnableDefaultCompileItems>true</EnableDefaultCompileItems>
    <AssemblyName>toolname</AssemblyName>
  
    <OutputType>Exe</OutputType>
    
    <!-- Cross-platform support -->
    <RuntimeIdentifiers>win-x64;linux-x64;osx-x64</RuntimeIdentifiers>
    
    <!-- NuGet Package Properties -->
    <PackageId>ToolName</PackageId>  <!-- PascalCase for NuGet -->
    <Authors>Rob Chambers</Authors>
    <Description>Tool description here</Description>
    <PackageTags>cli;relevant;tags;here</PackageTags>
    <PackageProjectUrl>https://github.com/robch/cycod</PackageProjectUrl>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageReadmeFile>README.md</PackageReadmeFile>
    
    <!-- .NET Tool Configuration -->
    <PackAsTool>true</PackAsTool>
    <ToolCommandName>toolname</ToolCommandName>
    <GeneratePackageOnBuild>false</GeneratePackageOnBuild>
  </PropertyGroup>

  <ItemGroup>
    <None Include="README.md" Pack="true" PackagePath="\" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\common\common.csproj" />
  </ItemGroup>

  <ItemGroup>
    <EmbeddedResource Include="assets\help\**">
      <Link>help\%(RecursiveDir)%(Filename)%(Extension)</Link>
    </EmbeddedResource>
    <EmbeddedResource Include="assets\prompts\**">
      <Link>prompts\%(RecursiveDir)%(Filename)%(Extension)</Link>
    </EmbeddedResource>
  </ItemGroup>

</Project>
```

### 3. Create Program.cs

**Template structure:**

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolNamespace.CommandLine;

class Program
{
    static async Task<int> Main(string[] args)
    {
        ToolNameProgramInfo _programInfo = new();

        LoggingInitializer.InitializeMemoryLogger();
        LoggingInitializer.LogStartupDetails(args);
        Logger.Info($"Starting {ProgramInfo.Name}, version {VersionInfo.GetVersion()}");

        if (!ToolNameCommandLineOptions.Parse(args, out var commandLineOptions, out var ex))
        {
            DisplayBanner();
            if (ex != null)
            {
                Logger.Error($"Command line error: {ex.Message}");
                DisplayException(ex);
                HelpHelpers.DisplayUsage(ex.GetHelpTopic());
                return 2;
            }
            else
            {
                Logger.Warning("Displaying help due to command line parsing issue");
                HelpHelpers.DisplayUsage(commandLineOptions!.HelpTopic);
                return 1;
            }
        }

        var debug = ConsoleHelpers.IsDebug() || commandLineOptions!.Debug;
        var verbose = ConsoleHelpers.IsVerbose() || commandLineOptions!.Verbose;
        var quiet = ConsoleHelpers.IsQuiet() || commandLineOptions!.Quiet;
        ConsoleHelpers.Configure(debug, verbose, quiet);
        
        LoggingInitializer.InitializeLogging(commandLineOptions?.LogFile, debug);

        var helpCommand = commandLineOptions!.Commands.OfType<HelpCommand>().FirstOrDefault();
        if (helpCommand != null)
        {
            DisplayBanner();
            HelpHelpers.DisplayHelpTopic(commandLineOptions.HelpTopic, commandLineOptions.ExpandHelpTopics);
            return 0;
        }

        var versionCommand = commandLineOptions!.Commands.OfType<VersionCommand>().FirstOrDefault();
        if (versionCommand != null)
        {
            DisplayBanner();
            var version = await versionCommand.ExecuteAsync(false);
            ConsoleHelpers.WriteLine(version.ToString()!);
            return 0;
        }

        foreach (var command in commandLineOptions.Commands)
        {
            // Handle your specific commands here
            if (command is YourCommandNamespace.YourCommand yourCommand)
            {
                await HandleYourCommandAsync(yourCommand);
            }
        }

        return 0;
    }

    private static async Task HandleYourCommandAsync(YourCommand command)
    {
        try
        {
            // Command implementation
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteErrorLine($"Error: {ex.Message}");
            Logger.Error($"Command failed: {ex.Message}");
            Logger.Error(ex.StackTrace ?? string.Empty);
        }
    }

    private static void DisplayBanner()
    {
        ConsoleHelpers.WriteWithHighlight($"{ProgramInfo.Name} {VersionInfo.GetVersion()}");
    }

    private static void DisplayException(Exception ex)
    {
        ConsoleHelpers.WriteErrorLine(ex.Message);
        if (ConsoleHelpers.IsDebug())
        {
            ConsoleHelpers.WriteErrorLine(ex.StackTrace ?? string.Empty);
        }
    }
}
```

### 4. Create ProgramInfo Class

```csharp
public class ToolNameProgramInfo : ProgramInfo
{
    public ToolNameProgramInfo() : base(
        () => "toolname",
        () => "Tool Description Here",
        () => ".cycod",  // Config directory
        () => typeof(ToolNameProgramInfo).Assembly)
    {
    }
}
```

### 5. Add to Solution File (`cycod.sln`)

Add these lines:

```xml
<!-- Near the top, with other Project entries -->
Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "toolname", "src\toolname\toolname.csproj", "{GUID-HERE}"
EndProject

<!-- In the GlobalSection(ProjectConfigurationPlatforms) section, add for each configuration: -->
{GUID-HERE}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
{GUID-HERE}.Debug|Any CPU.Build.0 = Debug|Any CPU
{GUID-HERE}.Release|Any CPU.ActiveCfg = Release|Any CPU
{GUID-HERE}.Release|Any CPU.Build.0 = Release|Any CPU
<!-- Plus x64 and x86 variations if needed -->

<!-- Optionally, add to solution folder -->
{GUID-HERE} = {SOLUTION-FOLDER-GUID}
```

**To generate GUID:** Use Visual Studio or `uuidgen` / online GUID generator

### 6. Update CI/CD Workflows

#### `.github/workflows/ci.yml`:

```yaml
# Add to the PATH export
export PATH=$PATH:$(pwd)/src/toolname/bin/Release/net9.0

# Add which check
which toolname

# Add artifact upload
- name: Upload toolname build artifacts
  uses: actions/upload-artifact@v4
  with:
    name: toolname-build
    path: |
      src/toolname/bin/Release/net9.0/
      src/toolname/bin/Release/net9.0/win-x64/publish/
      src/toolname/bin/Release/net9.0/linux-x64/publish/
      src/toolname/bin/Release/net9.0/osx-x64/publish/
```

#### `.github/workflows/release.yml`:

```yaml
# Update PATH export (same as ci.yml)
export PATH=$PATH:$(pwd)/src/toolname/bin/Release/net9.0

# Add which check
which toolname

# Update NuGet packages artifact name
name: cycod-cycodt-cycodmd-toolname-nuget-packages

# Add to release body
- toolname-win-x64-${{ env.VERSION }}.zip
- toolname-linux-x64-${{ env.VERSION }}.zip
- toolname-osx-x64-${{ env.VERSION }}.zip
```

### 7. Update Build Scripts

#### `scripts/_functions.sh`:

Update these arrays to include `"toolname"`:

```bash
# In cycod_build_dotnet()
local PROJECTS=("src/common/common.csproj" "src/cycod/cycod.csproj" "src/cycodt/cycodt.csproj" "src/cycodmd/cycodmd.csproj" "src/toolname/toolname.csproj")

# In cycod_pack_dotnet()
local TOOLS=("cycod" "cycodt" "cycodmd" "toolname")

# In the install-local.sh script generation
TOOLS=("cycod" "cycodt" "cycodmd" "toolname")

# In cycod_publish_self_contained()
local TOOLS=("cycod" "cycodt" "cycodmd" "toolname")
```

### 8. Update `.gitignore` (if needed)

Usually not needed as build artifacts are already ignored, but check if your tool creates any special files.

## Common Code Patterns

### CommandLine Parsing

Follow patterns from `common/CommandLine/` and reference implementations in cycod/cycodmd/cycodgr.

### Console Output

Use `ConsoleHelpers` methods:
- `ConsoleHelpers.WriteLine()` - with color support
- `ConsoleHelpers.WriteErrorLine()` - for errors
- `ConsoleHelpers.WriteDebugLine()` - for debug output
- `ConsoleHelpers.WriteWarning()` - for warnings

### Logging

Use `Logger` class:
- `Logger.Info()` - informational messages
- `Logger.Warning()` - warnings
- `Logger.Error()` - errors
- `Logger.Debug()` - debug messages

### File Operations

Use `FileHelpers` from common:
- `FileHelpers.ReadAllText()`
- `FileHelpers.WriteAllText()`
- `FileHelpers.FileExists()`
- `FileHelpers.GetFileNameFromTemplate()`

### Configuration

Use `ConfigStore` for settings:
```csharp
var setting = ConfigStore.Instance.GetFromAnyScope("setting.key").AsString("default");
```

## Testing Checklist

After adding your new CLI:

- [ ] Solution builds successfully: `dotnet build`
- [ ] Tool runs: `dotnet run --project src/toolname/toolname.csproj -- --help`
- [ ] Tool is in PATH during CI (check `which toolname` output)
- [ ] NuGet package can be created: `dotnet pack src/toolname/toolname.csproj`
- [ ] Self-contained builds work for all platforms
- [ ] CI workflow passes
- [ ] Release workflow includes the new tool
- [ ] Help documentation is accessible
- [ ] Existing tests still pass

## Naming Conventions

- **Project folder:** lowercase `toolname`
- **Executable:** lowercase `toolname`
- **NuGet PackageId:** PascalCase `ToolName` or creative name like `CycoDgr`
- **Namespace:** PascalCase `ToolName` or `ToolNamespace`
- **ProgramInfo class:** `ToolNameProgramInfo`

## Documentation

Create a `README.md` in the tool folder that includes:
- Purpose and goals
- Installation instructions
- Usage examples
- Command reference
- Common use cases

## Common Pitfalls

1. **Forgetting to add to solution file** - Tool won't build in CI
2. **Not updating PATH in CI workflows** - Integration tests will fail
3. **Missing from build scripts** - Won't be included in releases
4. **Incorrect PackageId casing** - NuGet package may conflict
5. **Not setting `PackAsTool=true`** - Won't install as global tool
6. **Missing ProjectReference to common** - Won't have access to shared helpers

## Reference Tools to Study

- **cycod** - Main tool, most complex, has chat/AI features
- **cycodt** - Test framework, good example of file processing
- **cycodmd** - Markdown tool, simpler, good for file operations
- **cycodgr** - GitHub search, external API integration, parallel processing

## Summary

To add a new CLI tool:

1. Create project structure in `src/toolname/`
2. Create `.csproj` with proper NuGet/tool configuration
3. Create `Program.cs` and `ProgramInfo.cs`
4. Add to `cycod.sln`
5. Update CI/CD workflows (`.github/workflows/*.yml`)
6. Update build scripts (`scripts/_functions.sh`)
7. Test locally and in CI
8. Document in README.md

Follow the patterns established in existing tools for consistency!
