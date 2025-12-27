# cycod CLI Filtering Pipeline Catalog

## Overview

This catalog documents the **filtering pipeline** for all commands in the `cycod` CLI tool. Each command's options and behaviors are analyzed across **9 conceptual layers** that represent the flow from target selection through to actions on results.

## The 9 Conceptual Layers

```
1. TARGET SELECTION    → What to search/process (input, history, configuration)
2. CONTAINER FILTER    → Which containers to include/exclude
3. CONTENT FILTER      → What content within containers to show
4. CONTENT REMOVAL     → What content to actively remove from display
5. CONTEXT EXPANSION   → How to expand around matches
6. DISPLAY CONTROL     → How to present results (formatting, verbosity)
7. OUTPUT PERSISTENCE  → Where to save results (files, formats)
8. AI PROCESSING       → AI-assisted analysis and transformation
9. ACTIONS ON RESULTS  → What to do with results (execute, modify)
```

## Commands

### Primary Commands

- **[chat](cycod-chat-filtering-pipeline-catalog-README.md)** (default) - Main AI interaction command with the most complex pipeline

### Configuration Management Commands

- **[config-list](cycod-config-list-filtering-pipeline-catalog-README.md)** - List configuration items
- **[config-get](cycod-config-get-filtering-pipeline-catalog-README.md)** - Get configuration value
- **[config-set](cycod-config-set-filtering-pipeline-catalog-README.md)** - Set configuration value
- **[config-clear](cycod-config-clear-filtering-pipeline-catalog-README.md)** - Clear configuration value
- **[config-add](cycod-config-add-filtering-pipeline-catalog-README.md)** - Add to configuration list
- **[config-remove](cycod-config-remove-filtering-pipeline-catalog-README.md)** - Remove from configuration list

### Alias Management Commands

- **[alias-list](cycod-alias-list-filtering-pipeline-catalog-README.md)** - List aliases
- **[alias-get](cycod-alias-get-filtering-pipeline-catalog-README.md)** - Get alias content
- **[alias-add](cycod-alias-add-filtering-pipeline-catalog-README.md)** - Add/update alias
- **[alias-delete](cycod-alias-delete-filtering-pipeline-catalog-README.md)** - Delete alias

### Prompt Management Commands

- **[prompt-list](cycod-prompt-list-filtering-pipeline-catalog-README.md)** - List prompts
- **[prompt-get](cycod-prompt-get-filtering-pipeline-catalog-README.md)** - Get prompt content
- **[prompt-create](cycod-prompt-create-filtering-pipeline-catalog-README.md)** - Create prompt
- **[prompt-delete](cycod-prompt-delete-filtering-pipeline-catalog-README.md)** - Delete prompt

### MCP Server Management Commands

- **[mcp-list](cycod-mcp-list-filtering-pipeline-catalog-README.md)** - List MCP servers
- **[mcp-get](cycod-mcp-get-filtering-pipeline-catalog-README.md)** - Get MCP server configuration
- **[mcp-add](cycod-mcp-add-filtering-pipeline-catalog-README.md)** - Add MCP server
- **[mcp-remove](cycod-mcp-remove-filtering-pipeline-catalog-README.md)** - Remove MCP server

### GitHub Integration Commands

- **[github-login](cycod-github-login-filtering-pipeline-catalog-README.md)** - Login to GitHub
- **[github-models](cycod-github-models-filtering-pipeline-catalog-README.md)** - List available GitHub models

## Structure

Each command has its own README linking to:
- **9 layer files**: `cycod-{command}-layer-{n}.md` describing each layer
- **9 proof files**: `cycod-{command}-layer-{n}-proof.md` with source code evidence

## Legend

- ✅ **Implemented** - Feature is fully implemented
- ⚠️ **Partial** - Feature is partially implemented or has limitations
- ❌ **Not Applicable** - Layer doesn't apply to this command
- 🔍 **Implicit** - Feature exists but not exposed as explicit option
