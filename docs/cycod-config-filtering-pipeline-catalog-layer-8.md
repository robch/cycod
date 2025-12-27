# cycod `config` Commands - Layer 8: AI Processing

## Overview

**Layer 8** focuses on AI-assisted analysis and processing of content. The `config` commands (`list`, `get`, `set`, `clear`, `add`, `remove`) are **administrative commands** that manage configuration settings and **do not involve AI processing**.

## Commands

```bash
cycod config list [--global|--user|--local|--any]
cycod config get <key> [--global|--user|--local|--any]
cycod config set <key> <value> [--global|--user|--local]
cycod config clear <key> [--global|--user|--local]
cycod config add <key> <value> [--global|--user|--local]
cycod config remove <key> <value> [--global|--user|--local]
```

## Purpose

Config commands perform CRUD operations on configuration files stored in JSON format. They are purely administrative and do not interact with AI models or process content through AI.

## Layer 8 Applicability

**Layer 8 (AI Processing) is NOT APPLICABLE** to config commands because:

1. **No AI model interaction** - Commands read/write JSON files directly
2. **No content analysis** - Simple key-value storage, no interpretation needed
3. **No tool calling** - No functions or external tools involved
4. **No conversation context** - Single-operation commands with no history
5. **No template processing** - Configuration values stored/retrieved verbatim
6. **No multi-modal input** - Text-only key-value pairs
7. **Deterministic output** - No generative or probabilistic processing

## What Config Commands DO

### Layer 1: Target Selection
- Select configuration scope (`--global`, `--user`, `--local`, `--any`)
- Select configuration key(s) to operate on

### Layer 6: Display Control
- Format configuration output to console (list/get operations)
- Display success/error messages

### Layer 7: Output Persistence
- Read from JSON configuration files (`.cycod/config.json` in various locations)
- Write to JSON configuration files
- See [Layer 7 documentation](cycod-config-filtering-pipeline-catalog-layer-7.md)

### Layer 9: Actions on Results
- Modify configuration files (set/clear/add/remove operations)
- Create configuration files if they don't exist

## Relationship to AI Processing

While config commands themselves don't use AI, they **enable** AI processing in other commands by:

1. **Setting AI provider** - `config set app.preferred_provider anthropic`
2. **Configuring API keys** - `config set anthropic.api_key sk-ant-...`
3. **Setting token limits** - `config set app.max_output_tokens 4096`
4. **Defining variables** - `config set Var.PROJECT_NAME myproject`

These settings are then **consumed** by the `chat` command during Layer 8 processing.

## Source Code Evidence

### Command-Line Parsing

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

```csharp
// Lines 225-258: Scope option parsing (applies to all config commands)
private bool TryParseConfigCommandOptions(ConfigBaseCommand? command, string[] args, ref int i, string arg)
{
    if (command == null)
    {
        return false;
    }

    bool parsed = true;

    if (arg == "--file")
    {
        var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
        var configPath = ValidateOkFileName(max1Arg.FirstOrDefault());
        ConfigStore.Instance.LoadConfigFile(configPath);
        command.Scope = ConfigFileScope.FileName;
        command.ConfigFileName = configPath;
        i += max1Arg.Count();
    }
    else if (arg == "--global" || arg == "-g")
    {
        command.Scope = ConfigFileScope.Global;
        command.ConfigFileName = null;
    }
    else if (arg == "--user" || arg == "-u")
    {
        command.Scope = ConfigFileScope.User;
        command.ConfigFileName = null;
    }
    else if (arg == "--local" || arg == "-l")
    {
        command.Scope = ConfigFileScope.Local;
        command.ConfigFileName = null;
    }
    else if (arg == "--any" || arg == "-a")
    {
        command.Scope = ConfigFileScope.Any;
        command.ConfigFileName = null;
    }
    else
    {
        parsed = false;
    }

    return parsed;
}
```

**Evidence**: Config commands only parse scope options. No AI-related options.

### Command Execution

Config command execution involves:
1. Reading JSON configuration file
2. Performing requested operation (get/set/clear/add/remove)
3. Writing JSON configuration file (for write operations)
4. Displaying result to console

No AI model calls, no function calling, no prompt processing.

## Related Documentation

- [Layer 7: Output Persistence](cycod-config-filtering-pipeline-catalog-layer-7.md) - Configuration file storage
- [Main Catalog](cycod-filter-pipeline-catalog-README.md)
- [Chat Layer 8](cycod-chat-filtering-pipeline-catalog-layer-8.md) - How config values are consumed for AI processing

## Proof Document

See [Layer 8 Proof](cycod-config-filtering-pipeline-catalog-layer-8-proof.md) for confirmation that no AI processing code exists in config commands.
