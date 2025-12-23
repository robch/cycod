# Research Summary: How to Add a New CLI Tool

## What Was Investigated

Analyzed commit **14169d53** which added `cycodgr` to understand the complete process for adding a new CLI tool to the cycod project.

## Key Commit

```
14169d53 - feat: Implement cycodgr - GitHub Repository and Code Search CLI (Phases A-E Complete)
```

## Files Changed When Adding cycodgr

### Core Project Files (in src/cycodgr/)
- `cycodgr.csproj` - Project configuration with NuGet/tool settings
- `Program.cs` - Entry point with command handling
- `CycoGrProgramInfo.cs` - Program information class
- `CommandLine/` - Command-line parsing infrastructure
- `CommandLineCommands/` - Individual command implementations
- `Helpers/` - Helper classes for GitHub API, etc.
- `Models/` - Data models (RepoInfo, CodeMatch, etc.)
- `assets/help/` - Embedded help text files
- `assets/prompts/` - AI prompt templates

### Solution and Build Infrastructure
- `cycod.sln` - Added project reference with all platform configurations
- `.github/workflows/ci.yml` - Added to PATH, which checks, artifact uploads
- `.github/workflows/release.yml` - Added to PATH, NuGet package lists, release artifacts
- `scripts/_functions.sh` - Added to TOOLS arrays in build/pack/publish functions
- `.gitignore` - (usually no changes needed)

### Shared/Common Files (modified for cycodgr)
- `src/common/AiInstructionProcessor.cs` - Enhanced for file/repo instructions
- `src/common/FoundTextFile.cs` - Enhanced for lazy content loading
- `src/common/Helpers/LineHelpers.cs` - Enhanced for context line handling
- `src/common/ThrottledProcessor.cs` - Added for parallel processing

## Critical Infrastructure Elements

### 1. Project File (.csproj) Must Have:
```xml
<Import Project="../../BuildCommon.targets" />  <!-- Version management -->
<TargetFramework>net9.0</TargetFramework>
<OutputType>Exe</OutputType>
<AssemblyName>toolname</AssemblyName>

<!-- NuGet Package Config -->
<PackageId>ToolName</PackageId>     <!-- PascalCase! -->
<PackAsTool>true</PackAsTool>       <!-- Critical for .NET tool -->
<ToolCommandName>toolname</ToolCommandName>

<!-- Cross-platform support -->
<RuntimeIdentifiers>win-x64;linux-x64;osx-x64</RuntimeIdentifiers>

<!-- Reference to common -->
<ProjectReference Include="..\common\common.csproj" />

<!-- Embed help files -->
<EmbeddedResource Include="assets\help\**" />
```

### 2. Solution File Must Include:
- Project entry with GUID
- Build configurations for all platforms (Any CPU, x64, x86) × (Debug, Release)
- Solution folder membership (optional but organized)

### 3. CI/CD Must Be Updated:
- **ci.yml**: Add to PATH, add `which` check, add artifact upload
- **release.yml**: Add to PATH, add `which` check, update artifact names, add to release body

### 4. Build Scripts Must Include:
- Add to PROJECTS array in `cycod_build_dotnet()`
- Add to TOOLS array in `cycod_pack_dotnet()`
- Add to TOOLS array in `cycod_publish_self_contained()`
- Add to TOOLS array in generated `install-local.sh` script

### 5. ProgramInfo Pattern:
```csharp
public class ToolNameProgramInfo : ProgramInfo
{
    public ToolNameProgramInfo() : base(
        () => "toolname",              // Command name
        () => "Description",           // Display description
        () => ".cycod",                // Config directory
        () => typeof(ToolNameProgramInfo).Assembly)
    {
    }
}
```

### 6. Program.cs Pattern:
- Initialize ProgramInfo
- Initialize logging
- Parse command-line options
- Handle help/version commands
- Execute main commands
- Consistent error handling with Logger and ConsoleHelpers

## Code Patterns to Follow

### Console Output
Use `ConsoleHelpers` instead of direct Console calls:
```csharp
ConsoleHelpers.WriteLine("Message", ConsoleColor.Cyan, overrideQuiet: true);
ConsoleHelpers.WriteErrorLine("Error message");
ConsoleHelpers.WriteDebugLine("Debug info");
ConsoleHelpers.WriteWarning("Warning");
```

### Logging
Use `Logger` class:
```csharp
Logger.Info("Informational message");
Logger.Warning("Warning message");
Logger.Error("Error message");
Logger.Debug("Debug details");
```

### File Operations
Use `FileHelpers` from common:
```csharp
FileHelpers.ReadAllText(path);
FileHelpers.WriteAllText(path, content);
FileHelpers.FileExists(path);
FileHelpers.GetFileNameFromTemplate("file-{time}.txt", template);
```

### Configuration
Use `ConfigStore`:
```csharp
var value = ConfigStore.Instance.GetFromAnyScope("key").AsString("default");
```

## Naming Conventions

| Element | Convention | Example |
|---------|-----------|---------|
| Project folder | lowercase | `src/cycodj/` |
| Executable | lowercase | `cycodj` |
| PackageId | PascalCase or Creative | `CycoDj` or `CycoGr` |
| Namespace | PascalCase | `CycoDj` or `CycoDj.CommandLine` |
| ProgramInfo class | ToolNameProgramInfo | `CycoDjProgramInfo` |

## Common Mistakes to Avoid

1. ❌ Forgetting to add to solution file → Won't build in CI
2. ❌ Not updating PATH in workflows → Integration tests fail
3. ❌ Missing from build scripts → Won't be in releases
4. ❌ Incorrect PackageId casing → NuGet conflicts
5. ❌ Not setting `PackAsTool=true` → Won't install as global tool
6. ❌ Missing `ProjectReference` to common → No shared helpers
7. ❌ Not importing `BuildCommon.targets` → Version management breaks

## What Makes cycodgr Complex

cycodgr is more complex than cycodmd/cycodt because it:
- Calls external GitHub CLI (`gh`) for API access
- Processes results in parallel using `ThrottledProcessor`
- Fetches file content from raw.githubusercontent.com
- Applies AI instructions to results (file, repo, final levels)
- Supports multiple output formats (json, csv, markdown, urls)
- Has sophisticated filtering (exclude patterns, file patterns, repo patterns)
- Implements context-aware line display with LineHelpers

For cycodj, we can start much simpler - just read local files and display data.

## References for cycodj

**Simple patterns to follow:**
- Project structure: cycodgr (newest), cycodmd (simpler)
- File reading: cycodmd uses FileHelpers extensively
- Command parsing: All tools use similar patterns
- Console output: All tools use ConsoleHelpers

**What cycodj should be more like:**
- Simpler than cycodgr (no external API calls, no parallel processing initially)
- Similar to cycodt (processes files, displays results)
- Similar to cycodmd (file operations, pattern matching)

## Documentation Created

1. **adding-new-cli-tool.md** - Complete guide with templates and checklists
2. Updated **quick-start.md** - Added Phase 0 for infrastructure setup
3. Updated **architecture.md** - References infrastructure requirements
4. Updated **chat-journal-plan.md** - Added Phase 0 to roadmap
5. Updated **SUMMARY.md** - Emphasizes infrastructure as first step

## Next Steps for Implementation

1. ✅ Research complete (this document)
2. Follow [adding-new-cli-tool.md](adding-new-cli-tool.md) step-by-step
3. Start with Phase 0 infrastructure (solution, CI/CD, build scripts)
4. Then follow [quick-start.md](quick-start.md) for feature implementation
5. Reference [architecture.md](architecture.md) for design decisions

## Key Takeaway

Adding a new CLI tool requires changes to **multiple** infrastructure files, not just creating the project folder. The [adding-new-cli-tool.md](adding-new-cli-tool.md) guide provides a complete checklist to ensure nothing is missed.
