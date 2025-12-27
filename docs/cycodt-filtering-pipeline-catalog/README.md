# cycodt CLI - Filtering Pipeline Catalog

This directory contains detailed documentation of how the cycodt CLI implements the 9-layer filtering pipeline for each of its commands.

## Commands

The cycodt CLI has the following commands:

### 1. [list](list/README.md)
Lists tests matching specified criteria.

### 2. [run](run/README.md)
Runs tests and produces test result files.

### 3. [expect check](expect-check/README.md)
Checks text output against expectations (regex patterns and AI instructions).

### 4. [expect format](expect-format/README.md)
Formats text for use as test expectations (escapes regex special characters).

## The 9-Layer Filtering Pipeline

Each command documentation follows this structure:

1. **TARGET SELECTION** - What to search/process (test files, input text)
2. **CONTAINER FILTERING** - Which containers to include/exclude (test files, test cases)
3. **CONTENT FILTERING** - What content within containers to show (test name patterns)
4. **CONTENT REMOVAL** - What content to actively remove (negative filters)
5. **CONTEXT EXPANSION** - How to expand around matches (N/A for most cycodt commands)
6. **DISPLAY CONTROL** - How to present results (formatting, verbosity)
7. **OUTPUT PERSISTENCE** - Where to save results (result files)
8. **AI PROCESSING** - AI-assisted analysis (expect instructions)
9. **ACTIONS ON RESULTS** - What to do with results (list, run, validate, format)

## Source Code Structure

- **Command-line parser**: `src/cycodt/CommandLine/CycoDtCommandLineOptions.cs`
- **Command implementations**: `src/cycodt/CommandLineCommands/`
  - Test commands: `TestCommands/TestBaseCommand.cs`, `TestListCommand.cs`, `TestRunCommand.cs`
  - Expect commands: `ExpectCommands/ExpectBaseCommand.cs`, `ExpectCheckCommand.cs`, `ExpectFormatCommand.cs`

## Reading This Documentation

Each command has:
- A `README.md` that links to the 9 layer files
- 9 layer files (`layer-{1-9}.md`) describing how that layer is implemented
- 9 proof files (`layer-{1-9}-proof.md`) with source code evidence including line numbers

The proof files contain:
- Exact line numbers from source code
- Code snippets showing implementation
- Call stack/data flow explanations
- Cross-references to related code
