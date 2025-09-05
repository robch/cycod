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

## Operating System + Shell Commands
The application is designed to run on Windows, macOS, and Linux environments. Be mindful of:
- Path separators (`\` vs `/`)
- Line endings (CRLF vs LF)
- Process execution differences across platforms
- File system permissions

Shell commands are executed through helper methods that handle platform-specific considerations.