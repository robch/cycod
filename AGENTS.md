# Project Overview
This is a cycod CLI application written in C#. It consists of multiple components:

- **cycod**: The main command-line interface for AI chat interactions
- **cycodmd**: Markdown processing tool for documentation
- **cycodt**: Testing framework for the application

## Development Environment Tips
- Use modern C# conventions
- Follow existing patterns in the codebase
- Ensure proper error handling
- Look for existing helper methods before implementing your own functionality
- Leverage the FileHelpers class for file operations

## Code Style
- Follow Microsoft C# coding guidelines
- Use XML documentation comments for public members
- Prefer explicit types over var when the type is not obvious
- Avoid magic strings and numbers
- Use meaningful variable and method names
- Prefer concise code (e.g., ternary operators for simple conditionals)

## PR Instructions
- Run tests before submitting PRs
- Keep changes focused and small when possible
- Follow semantic versioning

## Security Considerations
- Never commit secrets
- Always validate user input
- Don't expose sensitive information in error messages

## Code Organization
- Reuse existing utility classes whenever possible
- Check FileHelpers.cs for file-related operations before creating custom implementations
- When extending functionality, consider adding to existing helper classes rather than creating duplicates

## Key Helper Classes
- **FileHelpers**: Core utility for file operations including reading, writing, finding files, path manipulations
- **AgentsFileHelpers**: Specifically for handling AGENTS.md and similar agent instruction files
- **ChatHistoryFileHelpers**: Manages chat history files and their locations
- **ScopeFileHelpers**: Handles files in different configuration scopes
- **PromptFileHelpers**: Manages prompt templates and files
- **McpFileHelpers**: Handles MCP (Multi-Command Protocol) server configurations

## Configuration System
The application uses a multi-layered configuration system with three scopes:
- **Local**: Project-specific settings (highest priority)
- **User**: User-specific settings across projects
- **Global**: System-wide settings (lowest priority)

Files and configurations are searched across these scopes in order of priority.

## Command Structure
Commands follow a consistent pattern:
- Command classes inherit from appropriate base classes
- Each command handles its own parameter validation
- Commands should use existing helper methods when available
- Commands should respect the configuration scope system

## Testing with cycodt
The project uses a YAML-based test framework called cycodt. Key operations:

- **List tests**: `dotnet run --project src/cycodt/cycodt.csproj list --file <test_file.yaml>`
- **Run specific test**: `dotnet run --project src/cycodt/cycodt.csproj run --file <test_file.yaml> --test "<test_name>"`
- **Run all tests in file**: `dotnet run --project src/cycodt/cycodt.csproj run --file <test_file.yaml>`

Tests are defined in YAML files with:
- Test name and command/script to run
- Expected outputs (regex patterns)
- Environment variables, inputs, and other settings

### Testing Best Practices

**File Creation and Content Verification:**
- Use `cycodmd` with patterns to verify both file creation AND content in one step
- Much cleaner than bash scripting with `ls`/`cat` combinations
- Example: `dotnet run --project ../../src/cycodmd/cycodmd.csproj -- log-*.log`

**Side Effect Detection:**
- Use `not-expect-regex` to catch unwanted files or outputs (great for detecting "turd files")
- Example: `not-expect-regex: "## exception-log-.*\.log"` to ensure no exception logs appear

**Test Structure:**
- Clean up only at the end - allows debugging failed tests by inspecting leftover files
- Use minimal comments in test files - step names should be self-documenting
- Avoid redundant bash comments when the command is obvious

**Debugging Failed Tests:**
- Make tests fail deliberately to see full output using impossible expect-regex patterns
- Use `git status` after test runs to detect unintended side effects
- Check for "turd files" that tests should clean up but don't

When creating YAML tests:
- Use `|` for multi-line scripts/commands to preserve line breaks
- Each line in `expect-regex` matches as a substring of output lines
- Include cleanup steps for resources created during tests

For detailed documentation on creating test files and all available options, refer to:
`src/cycodt/TestFramework/README.md`

Example test files can be found in the `tests/cycodt-yaml/` directory.

## Operating System + Shell Commands
The application is designed to run on Windows, macOS, and Linux environments. Be mindful of:
- Path separators (`\` vs `/`)
- Line endings (CRLF vs LF)
- Process execution differences across platforms
- File system permissions

Shell commands are executed through helper methods that handle platform-specific considerations.