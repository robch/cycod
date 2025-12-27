# cycodgr Filter Pipeline Catalog

## Overview

This document catalogs the **complete filtering pipeline** for the `cycodgr` CLI tool, which provides GitHub search and repository management capabilities.

## Command Structure

cycodgr has a single primary command:

- **search** (default) - Search GitHub repositories and code with advanced filtering

## Documentation Organization

Each command's filtering pipeline is documented across 9 conceptual layers:

1. **TARGET SELECTION** - What to search (repos, code, patterns)
2. **CONTAINER FILTER** - Which repos/files to include/exclude
3. **CONTENT FILTER** - What content within files to show
4. **CONTENT REMOVAL** - What content to actively remove
5. **CONTEXT EXPANSION** - How to expand around matches
6. **DISPLAY CONTROL** - How to present results
7. **OUTPUT PERSISTENCE** - Where to save results
8. **AI PROCESSING** - AI-assisted analysis
9. **ACTIONS ON RESULTS** - What to do with results

## Commands

### search (Default Command)

- [search Command Pipeline Catalog](cycodgr-search-filtering-pipeline-catalog-README.md)
  - [Layer 1: Target Selection](cycodgr-search-layer-1.md) | [Proof](cycodgr-search-layer-1-proof.md)
  - [Layer 2: Container Filter](cycodgr-search-layer-2.md) | [Proof](cycodgr-search-layer-2-proof.md)
  - [Layer 3: Content Filter](cycodgr-search-layer-3.md) | [Proof](cycodgr-search-layer-3-proof.md)
  - [Layer 4: Content Removal](cycodgr-search-layer-4.md) | [Proof](cycodgr-search-layer-4-proof.md)
  - [Layer 5: Context Expansion](cycodgr-search-layer-5.md) | [Proof](cycodgr-search-layer-5-proof.md)
  - [Layer 6: Display Control](cycodgr-search-layer-6.md) | [Proof](cycodgr-search-layer-6-proof.md)
  - [Layer 7: Output Persistence](cycodgr-search-layer-7.md) | [Proof](cycodgr-search-layer-7-proof.md)
  - [Layer 8: AI Processing](cycodgr-search-layer-8.md) | [Proof](cycodgr-search-layer-8-proof.md)
  - [Layer 9: Actions on Results](cycodgr-search-layer-9.md) | [Proof](cycodgr-search-layer-9-proof.md)

## Source Code Structure

Key files:
- `src/cycodgr/CommandLine/CycoGrCommandLineOptions.cs` - Command-line parsing
- `src/cycodgr/CommandLineCommands/SearchCommand.cs` - Search command properties
- `src/cycodgr/CommandLine/CycoGrCommand.cs` - Base command class
- `src/cycodgr/Helpers/GitHubSearchHelpers.cs` - GitHub API interaction
- `src/cycodgr/Program.cs` - Command execution and orchestration

## Key Concepts

### Multi-Level Filtering
cycodgr operates on multiple hierarchy levels:
1. **Organization/Owner Level** - Filter by GitHub user or org
2. **Repository Level** - Filter which repos to search
3. **File Level** - Filter which files within repos to process
4. **Line Level** - Filter which lines within files to display

### Dual Behavior Patterns
Some options have dual behavior based on context:
- `--file-contains`: Can pre-filter repos OR filter files within repos
- `--contains`: Searches both repo metadata AND code

### Pre-filtering Stages
The search command operates in stages:
1. **Stage 1**: Pre-filter repos using `--repo-file-contains`
2. **Stage 2**: Apply dual behavior for `--file-contains`
3. **Stage 3**: Execute main search (repo, code, or unified)

## Usage Patterns

### Repository-Only Search
```bash
cycodgr microsoft/terminal
cycodgr --repo-contains "terminal emulator" --min-stars 1000
```

### Code Search
```bash
cycodgr --file-contains "ConPTY" --language cpp
cycodgr microsoft/terminal --cs-file-contains "IConsoleApplication"
```

### Unified Search
```bash
cycodgr --contains "terminal emulator" --min-stars 500
```

### Multi-Repo Code Search
```bash
cycodgr microsoft/terminal wezterm/wezterm --file-contains "OSC 133"
```

## Design Philosophy

1. **Layered Filtering**: Each layer filters/transforms results from previous layers
2. **Composability**: Options can be combined to create complex queries
3. **Performance**: Pre-filtering reduces API calls and processing time
4. **Flexibility**: Multiple output formats support different workflows
5. **AI Integration**: Optional AI processing for result analysis and transformation
