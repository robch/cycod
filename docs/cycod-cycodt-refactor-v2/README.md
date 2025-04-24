# cycod and cycodt Refactoring Plan

This document outlines the plan for refactoring the current "chatx" application into two separate executables:

1. **cycod** - Main application with all features EXCEPT test commands
2. **cycodt** - Application focused solely on test functionality

## Overview of Changes

The refactoring will involve:

1. Creating a shared library for common components
2. Splitting the application into two executables with distinct functionality
3. Modifying command-line parsing to accommodate both applications
4. Ensuring help systems are properly adjusted for each executable
5. Updating all hardcoded "chatx" references throughout the codebase

## Detailed Documentation

The refactoring plan is broken down into several sections:

- [Project Structure](project-structure.md) - How the solution and projects will be organized
- [Common Components](common-components.md) - What code will be shared between applications
- [Command Line Handling](command-line-handling.md) - Changes to command processing
- [Implementation Steps](implementation-steps.md) - Step-by-step implementation plan
- File-by-file inventories:
  - [New Files](files-new-inventory.md) - New files to be created
  - [Modified/Refactored Files](files-modified-or-refactored-inventory.md) - Files that will be modified or refactored
  - [Moved Files](files-moved-inventory.md) - Files that will be moved to new locations

## Summary of Key Changes

### Project Structure
- Create a new solution with a Layer-First approach:
  - **src/common** - Shared library for common components
  - **src/cycod** - Main application executable with all features except testing
  - **src/cycodt** - Test application executable containing the TestFramework and test commands
  - **tests/** - All test code organized by component

### Command Line Changes
- **cycod** will have no test commands - they will be completely removed
- **cycodt** will exclusively contain all test functionality with simplified commands:
  - Current: `chatx test list` → New: `cycodt list`
  - Current: `chatx test run` → New: `cycodt run`
- Class names should remain as `TestListCommand` and `TestRunCommand` for clarity, even though command names will be simplified
- Comprehensive refactoring of CommandLineOptions.cs to support both applications with a base class containing shared functionality and derived classes implementing application-specific behaviors

### Help System Changes
- Each application will have its own focused help system with independent help files in their respective `assets/help/` folders
- **cycod help** will show all commands except test-related ones
- **cycodt help** will only show test-related commands
- No help content will be shared between applications

### Configuration System
- Both applications will share the configuration system in a `.cycod` directory (not `.cycodev`)
- Configuration files will be accessible to both applications
- Environment variables with "CHATX_" prefix will be updated to use "CYCODEV_" prefix
- cycodt can only read configuration settings, not modify them
- If application-specific settings are needed in the future, they will use dot notation prefixes (`cycod.<setting>` and `cycodt.<setting>`)

### Test Resources Organization
- Test resources will be organized in a top-level `tests/` folder with:
  - `common/` - Most tests will migrate here
  - `cycod/` - A small number of app-specific tests
  - `cycodt/` - Currently no tests specific to this component

### Cross-Application References
- cycodt will launch cycod as a child process (similar to how it currently launches chatx)
- cycod has no need to launch cycodt
- Fixed string approach should be used for CLI references (not parameterized)

### Versioning Strategy
- Both applications will share the same version number
- They will be versioned together and built by the same CI/CD system
- No migration plan is needed for users transitioning from chatx

### String Reference Updates
- All hardcoded references to "chatx" in code files will be identified and updated
- This includes user-facing messages, error messages, and functional code

### Error Handling
- Error handling will remain the same as the current implementation
- Exceptions will typically be caught at top level in Program.cs

## Timeline and Approach

The implementation will follow a phased approach:

1. Create the src/common library and move shared code
2. Create the src/cycod (main) application
3. Create the src/cycodt (test) application
4. Set up the tests/ project structure
5. Perform global search for "chatx" references and update them
6. Test and validate both applications function correctly