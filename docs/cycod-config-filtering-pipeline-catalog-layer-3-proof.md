# cycod config - Layer 3: Content Filter - PROOF

This document provides detailed source code evidence for all Layer 3 (Content Filter) mechanisms in the `cycod config` commands.

## Table of Contents

1. [Config Get: Key Filtering](#config-get-key-filtering)
2. [Config List: Scope Filtering](#config-list-scope-filtering)
3. [Key Normalization](#key-normalization)
4. [Write Commands: Key Targeting](#write-commands-key-targeting)

---

## Config Get: Key Filtering

### Parsing: Key Positional Argument

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

```csharp
// Lines 99-103: Parsing config get key
if (command is ConfigGetCommand configGetCommand && string.IsNullOrEmpty(configGetCommand.Key))
{
    configGetCommand.Key = arg;
    parsedOption = true;
}
```

**Evidence**: The first positional argument for `config get` is assigned to `configGetCommand.Key`.

### Application: Key-Based Value Retrieval

**File**: `src/cycod/CommandLineCommands/ConfigCommands/ConfigGetCommand.cs`

```csharp
// Lines 20-23: ExecuteAsync entry point
public override async Task<object> ExecuteAsync(bool interactive)
{
    return await Task.Run(() => ExecuteGet(Key, Scope ?? ConfigFileScope.Any, ConfigFileName));
}

// Lines 25-46: Key-based retrieval logic
private int ExecuteGet(string? key, ConfigFileScope scope, string? configFileName)
{
    ConsoleHelpers.WriteDebugLine($"ExecuteGet; key: {key}; scope: {scope}; configFileName: {configFileName}");
    if (string.IsNullOrWhiteSpace(key))
    {
        throw new CommandLineException("Error: No key specified.");
    }

    // Normalize the key if it's a known setting
    if (KnownSettings.IsKnown(key))
    {
        key = KnownSettings.GetCanonicalForm(key);
    }

    var isFileNameScope = scope == ConfigFileScope.FileName && !string.IsNullOrEmpty(configFileName);
    var value = isFileNameScope
        ? _configStore.GetFromFileName(key, configFileName!)
        : _configStore.GetFromScope(key, scope);

    ConfigDisplayHelpers.DisplayConfigValue(key, value, showLocation: true);
    return 0;
}
```

**Evidence**: 
- Key is validated (line 28-31): Must not be null or whitespace
- Key is normalized (lines 34-37): If it matches a known setting, converted to canonical form
- Value is retrieved (lines 39-42): From either file-specific scope or standard scope
- Single key-value pair is displayed (line 44)

---

## Config List: Scope Filtering

### Parsing: Scope Options

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

```csharp
// Lines 212-256: Parsing config command scope options
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

**Evidence**: Scope options (`--global`, `--user`, `--local`, `--file`, `--any`) are parsed and set on `command.Scope`. The `--any` option is the default (see ConfigListCommand constructor).

### Application: Scope-Based Listing

**File**: `src/cycod/CommandLineCommands/ConfigCommands/ConfigListCommand.cs`

```csharp
// Lines 13-16: Default scope is Any
public ConfigListCommand() : base()
{
    Scope = ConfigFileScope.Any;
}

// Lines 18-26: ExecuteAsync with scope routing
public override async Task<object> ExecuteAsync(bool interactive)
{
    return await Task.Run(() => 
    {
        return Scope == ConfigFileScope.FileName
            ? ExecuteList(this.ConfigFileName!)
            : ExecuteList(Scope ?? ConfigFileScope.Any);
    });
}

// Lines 34-79: Scope-based listing with conditional display
private int ExecuteList(ConfigFileScope scope)
{
    var isAnyScope = scope == ConfigFileScope.Any;
    
    if (isAnyScope || scope == ConfigFileScope.Global)
    {
        DisplayConfigSettings(ConfigFileScope.Global);
        if (isAnyScope) ConsoleHelpers.WriteLine(overrideQuiet: true);
    }

    if (isAnyScope || scope == ConfigFileScope.User)
    {
        DisplayConfigSettings(ConfigFileScope.User);
        if (isAnyScope) ConsoleHelpers.WriteLine(overrideQuiet: true);
    }
    
    if (isAnyScope || scope == ConfigFileScope.Local)
    {
        DisplayConfigSettings(ConfigFileScope.Local);
        if (isAnyScope) ConsoleHelpers.WriteLine(overrideQuiet: true);
    }

    if (isAnyScope || scope == ConfigFileScope.FileName)
    {
        var fileNameToConfigValues = _configStore.ListFileNameScopeValues();
        foreach (var kvp in fileNameToConfigValues)
        {
            var location = $"{kvp.Key} (specified)";
            ConfigDisplayHelpers.DisplayConfigSettings(location, kvp.Value);
            ConsoleHelpers.WriteLine(overrideQuiet: true);
        }
    }

    if (isAnyScope)
    {
        var commandLineValues = _configStore.ListFromCommandLineSettings();
        if (commandLineValues.Count > 0)
        {
            var location = "Command line (specified)";
            ConfigDisplayHelpers.DisplayConfigSettings(location, commandLineValues);
            ConsoleHelpers.WriteLine(overrideQuiet: true);
        }
    }
    
    return 0;
}
```

**Evidence**: 
- Lines 36-42: If scope is `Any` or `Global`, display global settings
- Lines 44-48: If scope is `Any` or `User`, display user settings
- Lines 50-54: If scope is `Any` or `Local`, display local settings
- Lines 56-65: If scope is `Any` or `FileName`, display file-specific settings
- Lines 67-76: If scope is `Any`, also display command-line settings

This implements filtering by scope - only the requested scope(s) are displayed.

### Application: Retrieving Entries for Display

**File**: `src/cycod/CommandLineCommands/ConfigCommands/ConfigListCommand.cs`

```csharp
// Lines 87-91: Displaying config settings for a scope
private void DisplayConfigSettings(ConfigFileScope scope)
{
    var location = ConfigFileHelpers.GetLocationDisplayName(scope);
    ConfigDisplayHelpers.DisplayConfigSettings(location, _configStore.ListValuesFromKnownScope(scope));
}
```

**Evidence**: For each scope, `_configStore.ListValuesFromKnownScope(scope)` retrieves ALL key-value pairs from that scope. No additional filtering is applied at the content level (all entries within the scope are shown).

---

## Key Normalization

### Application: Known Settings Normalization

**File**: `src/cycod/CommandLineCommands/ConfigCommands/ConfigGetCommand.cs`

```csharp
// Lines 33-37: Key normalization in config get
// Normalize the key if it's a known setting
if (KnownSettings.IsKnown(key))
{
    key = KnownSettings.GetCanonicalForm(key);
}
```

**Evidence**: If the provided key matches a known setting (via `KnownSettings.IsKnown()`), it's transformed to its canonical form (via `KnownSettings.GetCanonicalForm()`).

**Purpose**: This is a content transformation that allows users to use shorthand or alternative key names, which are normalized to standard forms for retrieval.

**Note**: This normalization only occurs in `config get`. The `config list` command displays keys as they are stored without normalization.

---

## Write Commands: Key Targeting

### Parsing: Config Set Key and Value

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

```csharp
// Lines 109-121: Parsing config set arguments
else if (command is ConfigSetCommand configSetCommand)
{
    if (string.IsNullOrEmpty(configSetCommand.Key))
    {
        configSetCommand.Key = arg;
        parsedOption = true;
    }
    else if (string.IsNullOrEmpty(configSetCommand.Value))
    {
        configSetCommand.Value = arg;
        parsedOption = true;
    }
}
```

**Evidence**: Two positional arguments for `config set`:
1. First arg: Key
2. Second arg: Value

### Parsing: Config Clear Key

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

```csharp
// Lines 104-108: Parsing config clear argument
else if (command is ConfigClearCommand configClearCommand && string.IsNullOrEmpty(configClearCommand.Key))
{
    configClearCommand.Key = arg;
    parsedOption = true;
}
```

**Evidence**: Single positional argument for `config clear`: the key to clear.

### Parsing: Config Add Key and Value

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

```csharp
// Lines 122-134: Parsing config add arguments
else if (command is ConfigAddCommand configAddCommand)
{
    if (string.IsNullOrEmpty(configAddCommand.Key))
    {
        configAddCommand.Key = arg;
        parsedOption = true;
    }
    else if (string.IsNullOrEmpty(configAddCommand.Value))
    {
        configAddCommand.Value = arg;
        parsedOption = true;
    }
}
```

**Evidence**: Two positional arguments for `config add`:
1. First arg: Key
2. Second arg: Value to add to the list

### Parsing: Config Remove Key and Value

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

```csharp
// Lines 135-147: Parsing config remove arguments
else if (command is ConfigRemoveCommand configRemoveCommand)
{
    if (string.IsNullOrEmpty(configRemoveCommand.Key))
    {
        configRemoveCommand.Key = arg;
        parsedOption = true;
    }
    else if (string.IsNullOrEmpty(configRemoveCommand.Value))
    {
        configRemoveCommand.Value = arg;
        parsedOption = true;
    }
}
```

**Evidence**: Two positional arguments for `config remove`:
1. First arg: Key
2. Second arg: Value to remove from the list

---

## Summary

This proof document demonstrates that cycod config Layer 3 (Content Filter) is implemented through:

1. **Key-based filtering** (`config get`): Single key specified as positional argument, with optional key normalization for known settings
2. **Scope-based filtering** (`config list`): Scope options (`--global`, `--user`, `--local`, `--file`, `--any`) control which configuration scopes are displayed
3. **Key normalization**: Known settings are transformed to canonical form in `config get`
4. **Write command targeting**: `set`, `clear`, `add`, `remove` commands target specific keys via positional arguments

Config commands have minimal Layer 3 filtering because they operate on structured key-value data rather than free-form content. The primary filtering mechanism is scope-based (Layer 2), with Layer 3 providing key-specific targeting for read and write operations.
