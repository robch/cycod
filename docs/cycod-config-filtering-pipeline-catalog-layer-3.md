# cycod config - Layer 3: Content Filter

**Layer Purpose**: Define what content WITHIN selected containers (config files, config entries) to show, filter, or process.

## Overview

The `config` commands in cycod manage configuration settings across different scopes (global, user, local, file-specific). Layer 3 content filtering is minimal for these commands since they operate on structured key-value data rather than free-form content.

## Command Overview

- **config list**: Lists all configuration entries (optionally filtered by scope)
- **config get**: Retrieves a single configuration value by key
- **config set**: Sets a configuration value
- **config clear**: Clears a configuration value
- **config add**: Adds a value to a list-type configuration
- **config remove**: Removes a value from a list-type configuration

## Content Filter Mechanisms

### 1. Key-Based Content Selection (`config get`)

The `config get` command filters to a single configuration key:

**Positional Argument**:
- First positional arg: Configuration key

**Processing**:
1. Key is validated (must not be empty)
2. Key is normalized if it's a known setting (e.g., `anthropic.api.key` → canonical form)
3. Value is retrieved from the specified scope
4. Single key-value pair is displayed

**Source**: See [Layer 3 Proof](cycod-config-filtering-pipeline-catalog-layer-3-proof.md#config-get-key-filtering)

### 2. Scope-Based Content Filtering (`config list`)

The `config list` command filters configuration entries by scope:

**Options**:
- `--global`, `-g`: Show only global scope
- `--user`, `-u`: Show only user scope
- `--local`, `-l`: Show only local scope
- `--file <path>`: Show only specified file scope
- `--any`, `-a` (default): Show all scopes

**Processing**:
1. Determine which scopes to display based on option
2. For each scope, retrieve all key-value pairs from that scope
3. Display each scope's settings with a location header

**Source**: See [Layer 3 Proof](cycod-config-filtering-pipeline-catalog-layer-3-proof.md#config-list-scope-filtering)

### 3. Key Normalization (Content Transformation)

Configuration keys undergo normalization when they match known settings:

**Normalization Process**:
```csharp
if (KnownSettings.IsKnown(key))
{
    key = KnownSettings.GetCanonicalForm(key);
}
```

**Purpose**: Allows users to use shorthand or alternative names for configuration keys, which are then transformed to their canonical form.

**Example**: Various anthropic-related key names might all normalize to the same canonical key.

**Source**: See [Layer 3 Proof](cycod-config-filtering-pipeline-catalog-layer-3-proof.md#key-normalization)

## No Advanced Content Filtering

Unlike the `chat` command, config commands do NOT have:
- ❌ Template variable substitution (config values are literal)
- ❌ Token-based filtering (config entries are small key-value pairs)
- ❌ Content removal mechanisms
- ❌ Pattern matching or regex filtering
- ❌ Context expansion

This is appropriate because config commands operate on structured metadata (key-value pairs) rather than content.

## Content Filter Relationship to Other Layers

### From Layer 2 (Container Filter)
- Layer 2 determines WHICH config file(s) to access (via scope)
- Layer 3 determines WHICH entries within those files to show (all, or single key)

### To Layer 4 (Content Removal)
- Layer 3 selects content to include
- Layer 4 (not applicable for config commands - no removal mechanisms)

### To Layer 6 (Display Control)
- Layer 3 defines which entries to show
- Layer 6 controls HOW they are formatted and displayed

## Command-Line Options Summary

| Command | Options | Layer 3 Impact |
|---------|---------|----------------|
| `config list` | `--global`, `--user`, `--local`, `--file`, `--any` | Filters which scopes to display |
| `config get` | `<key>` (positional) | Filters to single key |
| `config set` | `<key> <value>` (positional) | Targets single key (write operation) |
| `config clear` | `<key>` (positional) | Targets single key (write operation) |
| `config add` | `<key> <value>` (positional) | Targets single key (write operation) |
| `config remove` | `<key> <value>` (positional) | Targets single key (write operation) |

## Implementation Notes

1. **Scope Resolution**: The scope option (Layer 2) determines which file(s) to access; Layer 3 operates on entries within those files
2. **Key Normalization**: Happens in `config get` but not in `config list` (list shows keys as stored)
3. **No Template Processing**: Config values are stored and displayed literally
4. **Atomic Operations**: Write commands (`set`, `clear`, `add`, `remove`) target single keys atomically

## See Also

- **[Layer 3 Proof](cycod-config-filtering-pipeline-catalog-layer-3-proof.md)**: Detailed source code evidence
- **[Layer 2: Container Filter](cycod-config-filtering-pipeline-catalog-layer-2.md)**: How config files/scopes are selected
- **[Layer 6: Display Control](cycod-config-filtering-pipeline-catalog-layer-6.md)**: How config values are formatted and displayed
