# cycodmd Filter Pipeline Catalog

## Overview

This directory contains detailed documentation of the **9-layer filtering pipeline** for all commands in the `cycodmd` CLI tool. Each command implements these layers differently based on its purpose.

## The 9-Layer Pipeline

1. **TARGET SELECTION** - What to search (files, web pages, etc.)
2. **CONTAINER FILTERING** - Which containers to include/exclude
3. **CONTENT FILTERING** - What content within containers to show
4. **CONTENT REMOVAL** - What content to actively remove from display
5. **CONTEXT EXPANSION** - How to expand around matches
6. **DISPLAY CONTROL** - How to present results
7. **OUTPUT PERSISTENCE** - Where to save results
8. **AI PROCESSING** - AI-assisted analysis of results
9. **ACTIONS ON RESULTS** - What to do with results

## cycodmd Commands

The `cycodmd` CLI has the following commands:

### 1. [FindFilesCommand](cycodmd-findfiles-catalog-README.md) (Default)

**Purpose**: Search and process local files using glob patterns

**Usage**: `cycodmd [patterns] [options]`

**Layers Implemented**: All 9 layers

[View detailed layer-by-layer documentation](cycodmd-findfiles-catalog-README.md)

---

### 2. [WebSearchCommand](cycodmd-websearch-catalog-README.md)

**Purpose**: Search the web using various search engines

**Usage**: `cycodmd web search [terms] [options]`

**Layers Implemented**: Layers 1-4, 6-8 (partial context expansion)

[View detailed layer-by-layer documentation](cycodmd-websearch-catalog-README.md)

---

### 3. [WebGetCommand](cycodmd-webget-catalog-README.md)

**Purpose**: Fetch and process specific web pages

**Usage**: `cycodmd web get [urls] [options]`

**Layers Implemented**: Layers 1-4, 6-8 (partial context expansion)

[View detailed layer-by-layer documentation](cycodmd-webget-catalog-README.md)

---

### 4. [RunCommand](cycodmd-run-catalog-README.md)

**Purpose**: Execute scripts and process their output

**Usage**: `cycodmd run [script] [options]`

**Layers Implemented**: Layers 1, 3-4, 6-8 (specialized for script execution)

[View detailed layer-by-layer documentation](cycodmd-run-catalog-README.md)

---

## Documentation Structure

Each command has:

- **Command README**: Overview and quick reference
- **9 Layer Files**: Detailed description of each layer (`cycodmd-{command}-layer-{N}.md`)
- **9 Proof Files**: Source code evidence with line numbers (`cycodmd-{command}-layer-{N}-proof.md`)

## Source Code References

- **Command Line Parser**: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`
- **Commands**: `src/cycodmd/CommandLineCommands/`
- **Helpers**: `src/common/Helpers/FileHelpers.cs`, `src/common/Helpers/TimeSpecHelpers.cs`

## Navigation

- [Back to Main CLI Filtering Patterns Catalog](CLI-Filtering-Patterns-Catalog.md)
- [FindFilesCommand Documentation](cycodmd-findfiles-catalog-README.md)
- [WebSearchCommand Documentation](cycodmd-websearch-catalog-README.md)
- [WebGetCommand Documentation](cycodmd-webget-catalog-README.md)
- [RunCommand Documentation](cycodmd-run-catalog-README.md)
