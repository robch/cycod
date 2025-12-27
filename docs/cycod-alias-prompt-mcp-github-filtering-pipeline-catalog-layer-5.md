# cycod alias/prompt/mcp/github Commands - Layer 5: Context Expansion

## Overview

Layer 5 (Context Expansion) defines how commands provide or expand context around the primary content being processed. For resource management commands (alias, prompt, mcp, github), Layer 5 is **not applicable** in the traditional sense.

## Command Groups Covered

- **alias**: `alias list`, `alias get`, `alias add`, `alias delete`
- **prompt**: `prompt list`, `prompt get`, `prompt create`, `prompt delete`
- **mcp**: `mcp list`, `mcp get`, `mcp add`, `mcp remove`
- **github**: `github login`, `github models`

## Why Layer 5 Does Not Apply

These commands are **resource management operations** (CRUD + authentication/listing), not search or content analysis operations.

### What "Context Expansion" Would Mean

In file search tools (cycodmd, cycodgr), context expansion means showing N lines before/after a match. In conversation tools (cycodj), it means showing N messages before/after a search result.

For these resource commands:
- **alias get <name>**: Retrieves alias content - no surrounding context to expand
- **prompt get <name>**: Retrieves prompt text - no surrounding context to expand
- **mcp get <name>**: Retrieves MCP config - no surrounding context to expand
- **github models**: Lists available models - shows everything already
- **list commands**: Show all resources in scope - no additional expansion possible

## Potential Context Expansion Mechanisms (Not Implemented)

If context expansion were to be implemented, it could theoretically mean:

### For Alias Commands

**Showing Related Aliases**:
For `alias get deploy`, also show:
- `deploy-dev`, `deploy-prod` (related aliases)
- Aliases that reference this alias

**Not implemented**: Each operation is on a single alias only.

### For Prompt Commands

**Showing Prompt Usage**:
For `prompt get code-review`, show:
- Which conversations used this prompt
- Related prompts (similar names, shared content)

**Not implemented**: Prompts are isolated resources.

### For MCP Commands

**Showing MCP Dependencies**:
For `mcp get filesystem`, show:
- Which other MCPs depend on it
- Which chat sessions use it
- Environment variables it requires

**Not implemented**: MCPs are configured independently.

### For GitHub Commands

**Showing Model Details**:
For `github models`, expand each model with:
- Capabilities, context window, pricing
- Example use cases

**Not implemented**: Only lists model names/IDs.

## What These Commands DO Have

While Layer 5 doesn't apply, these commands do have:
- **Layer 1 (Target Selection)**: Scope filtering (`--global`, `--user`, `--local`, `--any`)
- **Layer 2 (Container Filter)**: Scope-based filtering for `list` commands
- **Layer 6 (Display Control)**: Console output formatting
- **Layer 9 (Actions on Results)**: CRUD operations

## Command-Specific Notes

### Alias Commands

- **alias list**: Lists all aliases in scope - already shows everything
- **alias get**: Retrieves single alias content - no context to expand
- **alias add**: Creates/updates alias - no surrounding context
- **alias delete**: Deletes alias - no context involved

### Prompt Commands

- **prompt list**: Lists all prompts in scope - already shows everything
- **prompt get**: Retrieves single prompt - no context to expand
- **prompt create**: Creates/updates prompt - no surrounding context
- **prompt delete**: Deletes prompt - no context involved

### MCP Commands

- **mcp list**: Lists all MCP servers in scope - already shows everything
- **mcp get**: Retrieves single MCP config - no context to expand
- **mcp add**: Creates/updates MCP server config - explicit args, no context
- **mcp remove**: Deletes MCP config - no context involved

### GitHub Commands

- **github login**: Authentication flow - no content, no context
- **github models**: Lists available models - shows all models already

## Related Layers

- **Layer 1 (Target Selection)**: Selects which scope to operate on (except github)
- **Layer 2 (Container Filter)**: Filters which scopes to search
- **Layer 5 (Context Expansion)**: ← **YOU ARE HERE** - Not applicable
- **Layer 6 (Display Control)**: Controls how resources are displayed
- **Layer 9 (Actions on Results)**: Performs CRUD/auth operations

## Source Code Reference

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

- Lines 258-289: Alias command option parsing (scope selection only)
- Lines 291-322: Prompt command option parsing (scope selection only)
- Lines 324-395: MCP command option parsing (scope selection + add options)
- Lines 681-725: GitHub command option parsing (scope selection for login)

**No context expansion options** in any of these sections.

## Summary

Layer 5 (Context Expansion) **does not apply** to cycod's resource management commands (alias, prompt, mcp, github). These are simple CRUD and listing operations that don't involve searching, filtering, or analyzing content that would benefit from context expansion.

Any future context expansion features would be new capabilities, not currently present in the codebase.
