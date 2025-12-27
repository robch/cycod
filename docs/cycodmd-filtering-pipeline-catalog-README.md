# cycodmd CLI Filtering Pipeline Catalog

This directory contains detailed documentation of the filtering pipeline for the **cycodmd** CLI tool, organized by command and layer.

## Overview

The cycodmd CLI implements a 9-layer filtering pipeline that processes inputs through progressive refinement stages:

```
1. TARGET SELECTION    → What to search (files, web pages, scripts)
2. CONTAINER FILTER    → Which containers to include/exclude
3. CONTENT FILTER      → What content within containers to show
4. CONTENT REMOVAL     → What content to actively remove from display
5. CONTEXT EXPANSION   → How to expand around matches (before/after lines)
6. DISPLAY CONTROL     → How to present results (formatting, numbering, highlighting)
7. OUTPUT PERSISTENCE  → Where to save results (files, formats)
8. AI PROCESSING       → AI-assisted analysis of results
9. ACTIONS ON RESULTS  → What to do with results (replace, execute)
```

## Commands

The cycodmd CLI supports four distinct commands, each with its own filtering pipeline implementation:

### 1. File Search (Default Command)

**Command**: `cycodmd [globs...]` (FindFilesCommand)

The default command for searching and processing local files using glob patterns.

- [Layer 1: Target Selection](cycodmd-files-layer-1.md) | [Proof](cycodmd-files-layer-1-proof.md)
- [Layer 2: Container Filter](cycodmd-files-layer-2.md) | [Proof](cycodmd-files-layer-2-proof.md)
- [Layer 3: Content Filter](cycodmd-files-layer-3.md) | [Proof](cycodmd-files-layer-3-proof.md)
- [Layer 4: Content Removal](cycodmd-files-layer-4.md) | [Proof](cycodmd-files-layer-4-proof.md)
- [Layer 5: Context Expansion](cycodmd-files-layer-5.md) | [Proof](cycodmd-files-layer-5-proof.md)
- [Layer 6: Display Control](cycodmd-files-layer-6.md) | [Proof](cycodmd-files-layer-6-proof.md)
- [Layer 7: Output Persistence](cycodmd-files-layer-7.md) | [Proof](cycodmd-files-layer-7-proof.md)
- [Layer 8: AI Processing](cycodmd-files-layer-8.md) | [Proof](cycodmd-files-layer-8-proof.md)
- [Layer 9: Actions on Results](cycodmd-files-layer-9.md) | [Proof](cycodmd-files-layer-9-proof.md)

### 2. Web Search

**Command**: `cycodmd web search <terms...>`

Search the web for content matching specified terms.

- [Layer 1: Target Selection](cycodmd-websearch-layer-1.md) | [Proof](cycodmd-websearch-layer-1-proof.md)
- [Layer 2: Container Filter](cycodmd-websearch-layer-2.md) | [Proof](cycodmd-websearch-layer-2-proof.md)
- [Layer 3: Content Filter](cycodmd-websearch-layer-3.md) | [Proof](cycodmd-websearch-layer-3-proof.md)
- [Layer 4: Content Removal](cycodmd-websearch-layer-4.md) | [Proof](cycodmd-websearch-layer-4-proof.md)
- [Layer 5: Context Expansion](cycodmd-websearch-layer-5.md) | [Proof](cycodmd-websearch-layer-5-proof.md)
- [Layer 6: Display Control](cycodmd-websearch-layer-6.md) | [Proof](cycodmd-websearch-layer-6-proof.md)
- [Layer 7: Output Persistence](cycodmd-websearch-layer-7.md) | [Proof](cycodmd-websearch-layer-7-proof.md)
- [Layer 8: AI Processing](cycodmd-websearch-layer-8.md) | [Proof](cycodmd-websearch-layer-8-proof.md)
- [Layer 9: Actions on Results](cycodmd-websearch-layer-9.md) | [Proof](cycodmd-websearch-layer-9-proof.md)

### 3. Web Get

**Command**: `cycodmd web get <urls...>`

Retrieve and process specific web pages by URL.

- [Layer 1: Target Selection](cycodmd-webget-layer-1.md) | [Proof](cycodmd-webget-layer-1-proof.md)
- [Layer 2: Container Filter](cycodmd-webget-layer-2.md) | [Proof](cycodmd-webget-layer-2-proof.md)
- [Layer 3: Content Filter](cycodmd-webget-layer-3.md) | [Proof](cycodmd-webget-layer-3-proof.md)
- [Layer 4: Content Removal](cycodmd-webget-layer-4.md) | [Proof](cycodmd-webget-layer-4-proof.md)
- [Layer 5: Context Expansion](cycodmd-webget-layer-5.md) | [Proof](cycodmd-webget-layer-5-proof.md)
- [Layer 6: Display Control](cycodmd-webget-layer-6.md) | [Proof](cycodmd-webget-layer-6-proof.md)
- [Layer 7: Output Persistence](cycodmd-webget-layer-7.md) | [Proof](cycodmd-webget-layer-7-proof.md)
- [Layer 8: AI Processing](cycodmd-webget-layer-8.md) | [Proof](cycodmd-webget-layer-8-proof.md)
- [Layer 9: Actions on Results](cycodmd-webget-layer-9.md) | [Proof](cycodmd-webget-layer-9-proof.md)

### 4. Run Script

**Command**: `cycodmd run [script...]`

Execute shell scripts with specified shell types.

- [Layer 1: Target Selection](cycodmd-run-layer-1.md) | [Proof](cycodmd-run-layer-1-proof.md)
- [Layer 2: Container Filter](cycodmd-run-layer-2.md) | [Proof](cycodmd-run-layer-2-proof.md)
- [Layer 3: Content Filter](cycodmd-run-layer-3.md) | [Proof](cycodmd-run-layer-3-proof.md)
- [Layer 4: Content Removal](cycodmd-run-layer-4.md) | [Proof](cycodmd-run-layer-4-proof.md)
- [Layer 5: Context Expansion](cycodmd-run-layer-5.md) | [Proof](cycodmd-run-layer-5-proof.md)
- [Layer 6: Display Control](cycodmd-run-layer-6.md) | [Proof](cycodmd-run-layer-6-proof.md)
- [Layer 7: Output Persistence](cycodmd-run-layer-7.md) | [Proof](cycodmd-run-layer-7-proof.md)
- [Layer 8: AI Processing](cycodmd-run-layer-8.md) | [Proof](cycodmd-run-layer-8-proof.md)
- [Layer 9: Actions on Results](cycodmd-run-layer-9.md) | [Proof](cycodmd-run-layer-9-proof.md)

## Document Structure

Each layer document contains:
- **Purpose**: What this layer does in the filtering pipeline
- **Options**: All command-line options that affect this layer
- **Data Flow**: How data flows through this layer
- **Integration**: How this layer connects to adjacent layers

Each proof document contains:
- **Source Code References**: Line numbers and file paths
- **Parser Implementation**: How options are parsed
- **Command Properties**: Which properties store the configuration
- **Execution Path**: How the layer is executed during command processing

## Navigation

- [← Back to Main CLI Filtering Patterns Catalog](CLI-Filtering-Patterns-Catalog.md)
- [cycodmd Source Code](../src/cycodmd/)
