# New File Inventory

This document identifies all new files that need to be created as part of the refactoring project. These are files that don't exist in the original codebase and need to be created from scratch, rather than files that are being moved or renamed.

## Solution Files

| File | Description |
|------|-------------|
| `CycoDevSolution.sln` | New solution file that contains all projects |

## Project Files

| File | Description |
|------|-------------|
| `CycoDev.Common/CycoDev.Common.csproj` | Project file for the shared common library |
| `CycoDev/CycoDev.csproj` | Project file for the main application (cycod) |
| `CycoDevTest/CycoDevTest.csproj` | Project file for the test application (cycodt) |
| `CycoDev.Tests/CycoDev.Tests.csproj` | Project file for unit tests |

## CycoDev.Common Library

### Command Line Infrastructure

| File | Description |
|------|-------------|
| `CycoDev.Common/CommandLine/CommandLineOptionsBase.cs` | Abstract base class for command line options parsing |
| `CycoDev.Common/CommandLine/CommandRegistry.cs` | Registry for command registration and discovery |
| `CycoDev.Common/CommandLine/CommandExecutionHelpers.cs` | Helper methods for executing commands with proper throttling |

### Extension Points

| File | Description |
|------|-------------|
| `CycoDev.Common/CommandLine/ICommandRegistration.cs` | Interface for command registration |
| `CycoDev.Common/Helpers/IHelpSystem.cs` | Interface for help system |
| `CycoDev.Common/Helpers/HelpRegistry.cs` | Registry for registering help topics |

## CycoDev Application

### Core Files

| File | Description |
|------|-------------|
| `CycoDev/Program.cs` | Main program entry point for CycoDev |
| `CycoDev/CommandLineOptions.cs` | CycoDev-specific command line parser (derived from CommandLineOptionsBase) |

### Command Files

| File | Description |
|------|-------------|
| `CycoDev/CommandLineCommands/HelpCommand.cs` | Specialized help command for CycoDev that filters out test commands |

## CycoDevTest Application

### Core Files

| File | Description |
|------|-------------|
| `CycoDevTest/Program.cs` | Main program entry point for CycoDevTest |
| `CycoDevTest/CommandLineOptions.cs` | CycoDevTest-specific command line parser (derived from CommandLineOptionsBase) |

### Command Files

| File | Description |
|------|-------------|
| `CycoDevTest/CommandLineCommands/TestBaseCommand.cs` | Modified base class for test commands with simplified naming |
| `CycoDevTest/CommandLineCommands/HelpCommand.cs` | Specialized help command for CycoDevTest that shows only test commands |

## Test Project

| File | Description |
|------|-------------|
| `CycoDev.Tests/GlobalUsings.cs` | Global using statements for tests |

## Directory Structure

The following directories need to be created. While they don't represent individual files, they're important to establish the correct project structure:

```
CycoDevSolution/
├── CycoDev.Common/
│   ├── Configuration/
│   ├── CommandLine/
│   │   └── Commands/
│   ├── Helpers/
│   └── Templates/
├── CycoDev/
│   ├── CommandLineCommands/
│   │   ├── AliasCommands/
│   │   ├── ConfigCommands/
│   │   ├── McpCommands/
│   │   └── PromptCommands/
│   ├── FunctionCalling/
│   ├── FunctionCallingTools/
│   ├── ShellHelpers/
│   ├── SlashCommands/
│   ├── McpHelpers/
│   └── assets/help/
├── CycoDevTest/
│   ├── TestFramework/
│   ├── CommandLineCommands/
│   └── assets/help/
└── CycoDev.Tests/
    ├── CommonTests/
    ├── CycoDevTests/
    └── TestHelpers/
```

## Help Content Files

In addition to moving and adapting existing help files, the following new help files need to be created specifically for CycoDevTest:

| File | Description |
|------|-------------|
| `CycoDevTest/assets/help/usage.txt` | Simplified usage overview specific to cycodt |
| `CycoDevTest/assets/help/help.txt` | Main help file for cycodt |

## Implementation Notes

1. When implementing the abstract base class `CommandLineOptionsBase`, ensure it contains all shared functionality that both derived classes will need.

2. For the command registry, implement proper command name resolution with support for multi-word commands.

3. The specialized help commands should implement filtering based on the application type.

4. Directory structures should be created before attempting to move files into them.

5. Project files should include all necessary package references as specified in project-structure.md.