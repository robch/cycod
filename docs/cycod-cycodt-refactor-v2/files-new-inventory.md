# New File Inventory

This document identifies all new files that need to be created as part of the refactoring project. These are files that don't exist in the original codebase and need to be created from scratch, rather than files that are being moved or renamed.

## Solution Files

| File | Description |
|------|-------------|
| `CycoDevSolution.sln` | New solution file that contains all projects |

## Project Files

| File | Description |
|------|-------------|
| `src/common/common.csproj` | Project file for the shared common library |
| `src/cycod/cycod.csproj` | Project file for the main application (cycod) |
| `src/cycodt/cycodt.csproj` | Project file for the test application (cycodt) |
| `tests/tests.csproj` | Project file for unit tests |

## common Library

### Command Line Infrastructure

| File | Description |
|------|-------------|
| `src/common/CommandLine/CommandLineOptionsBase.cs` | Abstract base class for command line options parsing |
| `src/common/CommandLine/CommandRegistry.cs` | Registry for command registration and discovery |
| `src/common/CommandLine/CommandExecutionHelpers.cs` | Helper methods for executing commands with proper throttling |

### Extension Points

| File | Description |
|------|-------------|
| `src/common/CommandLine/ICommandRegistration.cs` | Interface for command registration |
| `src/common/Helpers/IHelpSystem.cs` | Interface for help system |
| `src/common/Helpers/HelpRegistry.cs` | Registry for registering help topics |

## cycod Application

### Core Files

| File | Description |
|------|-------------|
| `src/cycod/Program.cs` | Main program entry point for cycod |
| `src/cycod/CommandLineOptions.cs` | cycod-specific command line parser (derived from CommandLineOptionsBase) |

### Command Files

| File | Description |
|------|-------------|
| `src/cycod/CommandLineCommands/HelpCommand.cs` | Specialized help command for cycod that filters out test commands |

## cycodt Application

### Core Files

| File | Description |
|------|-------------|
| `src/cycodt/Program.cs` | Main program entry point for cycodt |
| `src/cycodt/CommandLineOptions.cs` | cycodt-specific command line parser (derived from CommandLineOptionsBase) |

### Command Files

| File | Description |
|------|-------------|
| `src/cycodt/CommandLineCommands/TestBaseCommand.cs` | Modified base class for test commands with simplified naming |
| `src/cycodt/CommandLineCommands/HelpCommand.cs` | Specialized help command for cycodt that shows only test commands |

## Test Project

| File | Description |
|------|-------------|
| `tests/GlobalUsings.cs` | Global using statements for tests |

## Directory Structure

The following directories need to be created. While they don't represent individual files, they're important to establish the correct project structure:

```
/
├── src/
│   ├── common/
│   │   ├── Configuration/
│   │   ├── CommandLine/
│   │   │   └── Commands/
│   │   ├── Helpers/
│   │   └── Templates/
│   ├── cycod/
│   │   ├── CommandLineCommands/
│   │   │   ├── AliasCommands/
│   │   │   ├── ConfigCommands/
│   │   │   ├── McpCommands/
│   │   │   └── PromptCommands/
│   │   ├── FunctionCalling/
│   │   ├── FunctionCallingTools/
│   │   ├── ShellHelpers/
│   │   ├── SlashCommands/
│   │   ├── McpHelpers/
│   │   └── assets/help/
│   └── cycodt/
│       ├── TestFramework/
│       ├── CommandLineCommands/
│       └── assets/help/
└── tests/
    ├── common/
    ├── cycod/
    ├── cycodt/
    └── TestHelpers/
```

## Help Content Files

In addition to moving and adapting existing help files, the following new help files need to be created specifically for cycodt:

| File | Description |
|------|-------------|
| `src/cycodt/assets/help/usage.txt` | Simplified usage overview specific to cycodt |
| `src/cycodt/assets/help/help.txt` | Main help file for cycodt |

## Implementation Notes

1. When implementing the abstract base class `CommandLineOptionsBase`, ensure it contains all shared functionality that both derived classes will need.

2. For the command registry, implement proper command name resolution with support for multi-word commands.

3. The specialized help commands should implement filtering based on the application type.

4. Directory structures should be created before attempting to move files into them.

5. Project files should include all necessary package references as specified in project-structure.md.