# Project Overview
This is a cycod CLI application written in C#.

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