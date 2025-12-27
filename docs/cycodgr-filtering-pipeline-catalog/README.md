# cycodgr Filtering Pipeline Catalog

## Overview

This directory contains comprehensive documentation of the **cycodgr** CLI's filtering pipeline, organized by the 9 conceptual layers that define how users search, filter, and process GitHub repositories and code.

## Commands in cycodgr

cycodgr has **one primary command**:

1. **[search](search-README.md)** - Search GitHub repositories and code (default command)

## The 9-Layer Filtering Pipeline

Each command's filtering pipeline is documented across 9 layers:

1. **TARGET SELECTION** - What to search (repos, code)
2. **CONTAINER FILTERING** - Which containers to include/exclude (repo-level, file-level)
3. **CONTENT FILTERING** - What content within containers to show (line-level)
4. **CONTENT REMOVAL** - What content to actively remove from display
5. **CONTEXT EXPANSION** - How to expand around matches (before/after lines)
6. **DISPLAY CONTROL** - How to present results (formatting, ordering)
7. **OUTPUT PERSISTENCE** - Where to save results (files, formats)
8. **AI PROCESSING** - AI-assisted analysis of results
9. **ACTIONS ON RESULTS** - What to do with results (clone, etc.)

## Documentation Structure

For each command, you'll find:

- **Command README** (`{command}-README.md`) - Overview and layer summary
- **Layer Documentation** (`{command}-layer-{N}.md`) - Detailed layer description
- **Proof Documentation** (`{command}-layer-{N}-proof.md`) - Source code evidence

## Navigation

- [Search Command Documentation](search-README.md)
  - [Layer 1: Target Selection](search-layer-1.md) | [Proof](search-layer-1-proof.md)
  - [Layer 2: Container Filtering](search-layer-2.md) | [Proof](search-layer-2-proof.md)
  - [Layer 3: Content Filtering](search-layer-3.md) | [Proof](search-layer-3-proof.md)
  - [Layer 4: Content Removal](search-layer-4.md) | [Proof](search-layer-4-proof.md)
  - [Layer 5: Context Expansion](search-layer-5.md) | [Proof](search-layer-5-proof.md)
  - [Layer 6: Display Control](search-layer-6.md) | [Proof](search-layer-6-proof.md)
  - [Layer 7: Output Persistence](search-layer-7.md) | [Proof](search-layer-7-proof.md)
  - [Layer 8: AI Processing](search-layer-8.md) | [Proof](search-layer-8-proof.md)
  - [Layer 9: Actions on Results](search-layer-9.md) | [Proof](search-layer-9-proof.md)

## Source Code References

Key source files:
- **Command Line Parser**: `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs`
- **Search Command**: `src/cycodgr/CommandLineCommands/SearchCommand.cs`
- **Base Command**: `src/cycodgr/CommandLine/CycoGrCommand.cs`
- **Program Logic**: `src/cycodgr/Program.cs`
- **Search Helpers**: `src/cycodgr/Helpers/GitHubSearchHelpers.cs`

## Related Documentation

- [Parent CLI Filtering Patterns Catalog](../CLI-Filtering-Patterns-Catalog.md)
- [cycodgr Help](../../src/cycodgr/assets/help/usage.txt)
