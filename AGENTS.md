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

For detailed coding style guidance, refer to our:
- [C# Coding Style Essential Guide](docs/C%23-Coding-Style-Essential.md) - Concise reference for experienced developers
- [C# Coding Style Expanded Guide](docs/C%23-Coding-Style-Expanded.md) - In-depth explanations for those new to C#

### General Guidelines
- Follow Microsoft C# coding guidelines
- Avoid magic strings and numbers
- Use meaningful variable and method names
- "When in Rome, do as the Romans do" - match the style of the file you're modifying
- Organize code by feature/functionality rather than by type (e.g., group files by feature in directories)

### Variables and Types
- Use `var` consistently for local variable declarations
- Only use explicit types when it improves clarity or the type is not obvious from the right side

### Expressions and Statements
- Prefer concise code (e.g., ternary operators for simple conditionals)
- Prefer LINQ and functional programming patterns where appropriate
- Maintain existing functional patterns (e.g., if code uses SelectMany, continue that pattern)

### Methods and Functions
- Prefer smaller, focused methods over large ones with multiple responsibilities
- Prefer singular methods (e.g., ProcessFile) over batch methods (e.g., ProcessFiles)
- Keep the original structure of methods when adding error handling

### Comments and Documentation
- Use XML documentation comments for public members only
- Follow the commenting style of the file - if a file doesn't use comments, don't add them
- Let code be self-documenting whenever possible

### Error Handling
- Use specific exception types rather than string pattern checking in catch blocks
- Follow existing error handling patterns in the codebase
- For critical errors, log clear messages and prevent further execution
- For non-critical errors, log warnings and allow execution to continue

### Console Output
- Use `ConsoleHelpers` methods instead of direct `Console` calls for all user output
- Follow existing color conventions:
  - Red for errors (`ConsoleHelpers.WriteErrorLine`)
  - Yellow for warnings and guidance (`ConsoleHelpers.WriteLine` with ConsoleColor.Yellow)
  - White/default for standard output

### Code Organization
- Place helper methods near related primary methods
- Keep changes localized to specific files when possible
- When adding new code, follow the existing organization pattern of the file
- Put methods in a logical order that follows the execution flow

### Refactoring and Modifying Code
- Prefer minimal changes that preserve existing structure
- When fixing bugs, focus on addressing the root cause rather than symptoms
- When adding error handling, try to keep the original code flow intact
- Create specific types (e.g., custom exceptions) rather than using string checking or other workarounds
- Keep changes backward compatible when possible

### Asynchronous Programming
- Use async/await consistently throughout the codebase
- Return Task or Task<T> from async methods, not void (except for event handlers)
- Always name async methods with the "Async" suffix
- Never use ConfigureAwait(false) in application code - it complicates debugging and can lead to bugs

### Resource Management
- Use `using` statements/declarations for all disposable resources
- Prefer using declarations (C# 8.0+) for simple resource cleanup
- Use try/finally blocks only for complex cleanup scenarios with multiple steps

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

**Handling Tests for Broken Product Functionality:**
When discovering broken functionality while writing tests:
- Mark tests as optional with the `broken-test` category: `optional: broken-test`
- Document the issue in a `todo-{problem}.md` file with detailed findings
- Include reproduction steps using `--include-optional broken-test` in the TODO
- This keeps broken tests separate from working tests while preserving evidence

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