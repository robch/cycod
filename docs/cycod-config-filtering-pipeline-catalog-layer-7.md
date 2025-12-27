# cycod config - Layer 7: Output Persistence

## Overview

Layer 7 controls **where and how results are saved** to persistent storage. For the config commands, this involves:

1. Writing configuration values to JSON files
2. Managing multiple configuration scopes (global, user, local, filename)
3. Displaying results to console
4. Maintaining configuration file structure

## Implementation Summary

The config commands implement Layer 7 through:

- **File-based storage**: JSON configuration files at different scopes
- **Scope selection**: `--global`, `--user`, `--local`, `--any`, `--file`
- **Atomic updates**: Read-modify-write cycles
- **Display feedback**: Console output showing what was saved
- **Merge semantics**: List values accumulate across scopes

## Command-Line Options

### Scope Selection (Common to All Config Commands)

| Option | Short | Scope | Description |
|--------|-------|-------|-------------|
| `--global` | `-g` | Global | System-wide settings (`/etc/cycod/config.json` or similar) |
| `--user` | `-u` | User | User-specific settings (`~/.config/cycod/config.json`) |
| `--local` | `-l` | Local | Project-specific settings (`./.cycod.json`) |
| `--any` | `-a` | Any | Read from all scopes (for `config list`, `config get`) |
| `--file <path>` | - | FileName | Specific config file path |

**Default scope:** `--local` (for write operations)

### Command-Specific Behavior

#### config list

**Output:**
- **Console display**: All configuration entries from selected scope(s)
- **No file writes**: Read-only operation

**Scope behavior:**
- `--any` (default): Displays all scopes (global, user, local, filename, command-line)
- `--global/--user/--local`: Displays single scope
- `--file <path>`: Displays specified file only

**Example:**
```bash
cycod config list --any
# Output:
# ~/.config/cycod/config.json (user):
# app.preferred-provider: anthropic
# app.max-output-tokens: 4096
#
# ./.cycod.json (local):
# app.preferred-provider: openai
# Var.projectName: MyCoolProject
```

#### config get <key>

**Output:**
- **Console display**: Value of specified key from selected scope
- **No file writes**: Read-only operation

**Scope behavior:**
- `--any` (default): Returns value from highest-priority scope
- `--global/--user/--local`: Returns value from specific scope
- `--file <path>`: Returns value from specified file

**Example:**
```bash
cycod config get app.preferred-provider --user
# Output:
# anthropic
```

#### config set <key> <value>

**Output:**
- **File write**: Updates configuration file at selected scope
- **Console display**: Confirmation of value set

**Scope behavior:**
- `--local` (default): Writes to `./.cycod.json`
- `--global/--user`: Writes to respective scope file
- `--file <path>`: Writes to specified file

**Example:**
```bash
cycod config set app.preferred-provider anthropic --user
# Output:
# app.preferred-provider: anthropic
# File: ~/.config/cycod/config.json
```

#### config clear <key>

**Output:**
- **File write**: Removes key from configuration file at selected scope
- **Console display**: Confirmation of removal

**Scope behavior:** Same as `config set`

**Example:**
```bash
cycod config clear app.preferred-provider --local
# Output:
# Cleared: app.preferred-provider
# File: ./.cycod.json
```

#### config add <key> <value>

**Output:**
- **File write**: Appends value to list at selected scope
- **Console display**: Updated list values

**Purpose:** For multi-value settings (lists)

**Example:**
```bash
cycod config add app.mcp-servers my-server --user
# Output:
# app.mcp-servers:
# - existing-server
# - my-server
# File: ~/.config/cycod/config.json
```

#### config remove <key> <value>

**Output:**
- **File write**: Removes value from list at selected scope
- **Console display**: Updated list values

**Purpose:** For multi-value settings (lists)

**Example:**
```bash
cycod config remove app.mcp-servers old-server --user
# Output:
# app.mcp-servers:
# - my-server
# File: ~/.config/cycod/config.json
```

## File Locations

### Global Scope

**Linux/macOS:**
```
/etc/cycod/config.json
```

**Windows:**
```
C:\ProgramData\cycod\config.json
```

### User Scope

**Linux/macOS:**
```
~/.config/cycod/config.json
```

**Windows:**
```
%APPDATA%\cycod\config.json
```

### Local Scope

**All platforms:**
```
./.cycod.json
```
(Current working directory)

### FileName Scope

**User-specified path:**
```
/any/path/to/custom-config.json
```

## Configuration File Format

### JSON Structure

Configuration files use standard JSON format:

```json
{
  "app.preferred-provider": "anthropic",
  "app.max-output-tokens": 4096,
  "app.mcp-servers": [
    "server1",
    "server2"
  ],
  "Var.projectName": "MyCoolProject"
}
```

### Key Naming Convention

Keys use dot notation:
- `app.*` - Application settings
- `Var.*` - Template variables
- Custom keys allowed

### Value Types

- **String**: `"anthropic"`
- **Number**: `4096`, `3.14`
- **Boolean**: `true`, `false`
- **List**: `["item1", "item2"]`
- **Object**: Not commonly used, but supported

## Data Flow

### config set Flow

```
1. Command-line parsing:
   CycoDevCommandLineOptions.TryParseOtherCommandArg()
   └─ Lines 99-112: Parse positional args for ConfigSetCommand
      ├─ First arg → Key
      └─ Second arg → Value

2. Scope parsing:
   CycoDevCommandLineOptions.TryParseConfigCommandOptions()
   └─ Lines 218-243: Parse --global/--user/--local/--any/--file
      └─ Set ConfigSetCommand.Scope property

3. Command execution:
   ConfigSetCommand.ExecuteAsync()
   └─ Line 23: Call ExecuteSet(Key, Value, Scope, ConfigFileName)
      └─ Lines 26-75: ExecuteSet implementation
         ├─ Line 40: Key normalization (check if known setting)
         ├─ Lines 44-62: Handle list values
         │  ├─ Lines 60-61: Write to file (ConfigStore.Set)
         │  └─ Line 63: Display result
         └─ Lines 65-75: Handle single values
            ├─ Lines 68-69: Write to file (ConfigStore.Set)
            └─ Line 75: Display result

4. File writing:
   ConfigStore.Set()
   └─ Loads existing config file
      ├─ Merges new value
      ├─ Serializes to JSON
      └─ Writes atomically to disk
```

### config list Flow

```
1. Scope parsing:
   CycoDevCommandLineOptions.TryParseConfigCommandOptions()
   └─ Lines 218-243: Parse scope option
      └─ Default: ConfigFileScope.Any (line 15 in ConfigListCommand)

2. Command execution:
   ConfigListCommand.ExecuteAsync()
   └─ Lines 20-26: Determine execution path
      ├─ If FileName scope → ExecuteList(fileName)
      └─ Otherwise → ExecuteList(scope)

3. Display by scope:
   ConfigListCommand.ExecuteList(ConfigFileScope)
   └─ Lines 34-76: Iterate through scopes
      ├─ Lines 38-42: Display global scope (if any/global)
      ├─ Lines 44-48: Display user scope (if any/user)
      ├─ Lines 50-54: Display local scope (if any/local)
      ├─ Lines 56-65: Display all filename scopes (if any/filename)
      └─ Lines 67-76: Display command-line overrides

4. Console output:
   ConfigDisplayHelpers.DisplayConfigSettings()
   └─ Formats and writes to console
      └─ No file writes (read-only)
```

## ConfigStore Integration

### ConfigStore Methods (Layer 7 Relevant)

#### Set(key, value, scope, saveToFile)

**Purpose:** Write a single value to config file

**Parameters:**
- `key`: Configuration key (e.g., `"app.preferred-provider"`)
- `value`: Value to set
- `scope`: Target scope (Global, User, Local)
- `saveToFile`: If true, immediately persists to disk

**File operations:**
- Loads existing config file
- Updates in-memory representation
- Serializes to JSON
- Writes to file atomically

#### Set(key, value, fileName)

**Purpose:** Write a single value to specific config file

**Parameters:**
- `key`: Configuration key
- `value`: Value to set
- `fileName`: Explicit file path

**File operations:** Same as above, but targets specific file

#### AddToList(key, value, scope, saveToFile)

**Purpose:** Append value to a list setting

**File operations:**
- Loads existing config file
- Parses key's value as list
- Appends new value
- Serializes and writes

#### RemoveFromList(key, value, scope, saveToFile)

**Purpose:** Remove value from a list setting

**File operations:**
- Loads existing config file
- Parses key's value as list
- Removes matching value
- Serializes and writes

## Atomic Updates

### File Locking Strategy

Configuration files use a read-modify-write cycle:

1. **Read**: Load entire config file
2. **Modify**: Update in-memory representation
3. **Write**: Write entire file atomically

### Atomicity Guarantees

- **Single process**: Atomic within a single cycod invocation
- **Multiple processes**: No file locking, last writer wins
- **Concurrent safety**: Not guaranteed across processes

### Transactionality

- **No transactions**: Each command is a separate operation
- **No rollback**: Failed writes may leave partial state
- **Manual recovery**: User must fix corrupted files manually

## Edge Cases and Behavior

### 1. Config File Doesn't Exist

**Input:** `cycod config set mykey myvalue --user`

**Behavior:**
- Creates directory structure if needed
- Creates new config file
- Writes initial value
- **No error**

### 2. Invalid JSON in Config File

**Input:** `cycod config list --local`
(where `.cycod.json` contains invalid JSON)

**Behavior:**
- **Error**: JSON parsing exception
- Command fails
- File not modified

### 3. Key Already Exists (config set)

**Input:**
```bash
cycod config set app.preferred-provider anthropic --user
cycod config set app.preferred-provider openai --user
```

**Behavior:**
- First command: Sets value to "anthropic"
- Second command: Overwrites with "openai"
- **No warning about overwrite**

### 4. Key Doesn't Exist (config get)

**Input:** `cycod config get nonexistent.key --user`

**Behavior:**
- Returns empty/null
- **No error**
- Console may show nothing or "(not set)"

### 5. Adding Duplicate to List (config add)

**Input:**
```bash
cycod config add app.mcp-servers my-server --user
cycod config add app.mcp-servers my-server --user
```

**Behavior:**
- Second command adds duplicate
- List contains: `["my-server", "my-server"]`
- **No deduplication**

### 6. Scope Priority in --any Mode

**Input:** `cycod config get app.preferred-provider --any`

**Behavior:**
- Searches scopes in priority order:
  1. Command line (highest)
  2. Local
  3. User
  4. Global (lowest)
- Returns first match found
- **Shadowing**: Local overrides User overrides Global

## Performance Considerations

### File I/O

- **config list --any**: Reads up to 4 files (global, user, local, + filename scopes)
- **config set**: Read + Write (1 file)
- **config get**: Read only (1 file or multiple for --any)

### JSON Parsing

- Entire config file parsed on each operation
- In-memory representation maintained by ConfigStore
- Re-serialization on every write

### Caching

ConfigStore maintains in-memory cache:
- Parsed config values cached
- Cache invalidated on writes
- Reduces redundant file I/O

## Console Output Format

### config list Output

```
~/.config/cycod/config.json (user):
app.preferred-provider: anthropic
app.max-output-tokens: 4096
app.mcp-servers:
- server1
- server2

./.cycod.json (local):
Var.projectName: MyCoolProject
```

**Format:**
- File location header
- Key-value pairs
- Lists shown with YAML-style `-` prefix
- Blank lines between scopes

### config set/get Output

```
app.preferred-provider: anthropic
```

**Format:**
- Key: Value
- Single line for scalar values
- Multiple lines for lists

## Summary

Layer 7 in the config commands provides:

✅ **Multi-scope persistence**: Global, User, Local, FileName
✅ **Flexible targeting**: Scope flags control where data is saved
✅ **Console feedback**: All operations provide confirmation
✅ **JSON format**: Standard, interoperable format
✅ **Scope priority**: Consistent override semantics
✅ **List management**: Append and remove from list settings

The implementation spans:
- Command-line parsing (CycoDevCommandLineOptions.cs, lines 99-112, 218-243)
- Command execution (ConfigSetCommand.cs, ConfigListCommand.cs, etc.)
- ConfigStore (src/common/Config/ConfigStore.cs)
- Helper utilities (ConfigDisplayHelpers, ConfigFileHelpers)

## Limitations

❌ **No file locking**: Concurrent writes may corrupt files
❌ **No transactions**: Multi-step operations not atomic
❌ **No validation**: Invalid values accepted (except JSON syntax)
❌ **No backup**: Overwrites are permanent
❌ **No merge conflicts**: Last writer wins silently

## See Also

- [Layer 7 Proof (Source Evidence)](cycod-config-filtering-pipeline-catalog-layer-7-proof.md)
- [Config Command Overview](cycod-config-filtering-pipeline-catalog-README.md)
- [All Layers](../cycod-filter-pipeline-catalog-README.md)
