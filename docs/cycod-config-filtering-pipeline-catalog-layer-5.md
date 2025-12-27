# cycod config Commands - Layer 5: Context Expansion

## Overview

Layer 5 (Context Expansion) defines how commands provide or expand context around the primary content being processed. For configuration management commands, Layer 5 is **not applicable** in the traditional sense.

## Why Layer 5 Does Not Apply

Configuration commands (`config list`, `config get`, `config set`, `config clear`, `config add`, `config remove`) are **CRUD (Create, Read, Update, Delete) operations** on configuration data, not search or content analysis operations.

### What "Context Expansion" Would Mean

In file search tools (cycodmd, cycodgr), context expansion means showing N lines before/after a match. In conversation tools (cycodj), it means showing N messages before/after a search result.

For config commands:
- **config get <key>**: Retrieves a single value - no surrounding context to expand
- **config set <key> <value>**: Sets a single value - no surrounding context
- **config list**: Lists all keys/values - shows everything already, no expansion possible
- **config clear <key>**: Clears a single value - no context involved
- **config add <key> <value>**: Adds to a list - no surrounding context
- **config remove <key> <value>**: Removes from a list - no context beyond the item

## Potential Context Expansion Mechanisms (Not Implemented)

If context expansion were to be implemented for config commands, it could theoretically mean:

### 1. Showing Related Configuration Keys

For `config get service.api.endpoint`, also show:
- `service.api.key`
- `service.api.timeout`
- Other `service.*` keys

**Not implemented**: Each get/set/clear operates on a single key only.

### 2. Showing Configuration Inheritance

For a local config value, also show:
- Where it overrides a user or global setting
- The effective value vs. the scoped value

**Not implemented**: Config operations show only the requested scope.

### 3. Showing Configuration Usage

For a configuration key, show:
- Which commands/features use this key
- Example values or constraints

**Not implemented**: No metadata about key usage exists.

### 4. Showing Configuration History

For a configuration key, show:
- Previous values
- When it was changed
- Who changed it

**Not implemented**: No version history tracking exists.

## What Config Commands DO Have

While Layer 5 doesn't apply, config commands do have:
- **Layer 1 (Target Selection)**: Scope filtering (`--global`, `--user`, `--local`, `--any`)
- **Layer 2 (Container Filter)**: Scope-based filtering for `config list`
- **Layer 6 (Display Control)**: Console output formatting
- **Layer 9 (Actions on Results)**: Get, set, clear, add, remove operations

## Related Layers

- **Layer 1 (Target Selection)**: Selects which config scope to operate on
- **Layer 2 (Container Filter)**: Filters which scopes to search
- **Layer 5 (Context Expansion)**: ← **YOU ARE HERE** - Not applicable
- **Layer 6 (Display Control)**: Controls how config values are displayed
- **Layer 9 (Actions on Results)**: Performs CRUD operations

## Source Code Reference

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`
- Lines 212-256: Config command option parsing (scope selection only)
- Lines 99-147: Positional argument parsing for key/value

**Command Implementations**: `src/cycod/Commands/Config*.cs`
- No context expansion logic present
- Simple get/set/list/clear operations

## Summary

Layer 5 (Context Expansion) **does not apply** to cycod's configuration management commands. These are simple CRUD operations that don't involve searching, filtering, or analyzing content that would benefit from context expansion.

Any future context expansion features for config commands would be new capabilities, not currently present in the codebase.
