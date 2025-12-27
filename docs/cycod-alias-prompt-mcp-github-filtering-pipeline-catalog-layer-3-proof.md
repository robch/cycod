# cycod alias/prompt/mcp/github - Layer 3: Content Filter - PROOF

This document provides detailed source code evidence for all Layer 3 (Content Filter) mechanisms in the `cycod alias`, `prompt`, `mcp`, and `github` command groups.

## Table of Contents

1. [Name-Based Selection](#name-based-selection)
2. [Scope-Based Filtering](#scope-based-filtering)
3. [Alias Content Tokenization](#alias-content-tokenization)
4. [GitHub Models](#github-models)

---

## Name-Based Selection

### Parsing: Alias Name

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

```csharp
// Lines 148-152: Parsing alias get name
else if (command is AliasGetCommand aliasGetCommand && string.IsNullOrEmpty(aliasGetCommand.AliasName))
{
    aliasGetCommand.AliasName = arg;
    parsedOption = true;
}
```

**Evidence**: First positional argument for `alias get` is assigned to `AliasName`.

### Parsing: Alias Add Name and Content

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

```csharp
// Lines 153-165: Parsing alias add name and content
else if (command is AliasAddCommand aliasAddCommand)
{
    if (string.IsNullOrEmpty(aliasAddCommand.AliasName))
    {
        aliasAddCommand.AliasName = arg;
        parsedOption = true;
    }
    else if (string.IsNullOrEmpty(aliasAddCommand.Content))
    {
        // Just store the current argument as content
        // The proper handling of all content will be done in ExecuteAsync
        aliasAddCommand.Content = arg;
        parsedOption = true;
    }
}
```

**Evidence**: Two positional arguments for `alias add`:
1. First arg: Alias name
2. Second arg: Content (single string that will be tokenized later)

### Parsing: Alias Delete Name

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

```csharp
// Lines 166-170: Parsing alias delete name
else if (command is AliasDeleteCommand aliasDeleteCommand && string.IsNullOrEmpty(aliasDeleteCommand.AliasName))
{
    aliasDeleteCommand.AliasName = arg;
    parsedOption = true;
}
```

**Evidence**: Single positional argument for `alias delete`: the alias name.

### Parsing: Prompt Names

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

```csharp
// Lines 171-192: Parsing prompt command names
else if (command is PromptCreateCommand createCommand && string.IsNullOrEmpty(createCommand.PromptName))
{
    createCommand.PromptName = arg;
    parsedOption = true;
}
else if (command is PromptCreateCommand createCmd && string.IsNullOrEmpty(createCmd.PromptText))
{
    createCmd.PromptText = arg;
    parsedOption = true;
}
else if (command is PromptDeleteCommand deleteCommand && string.IsNullOrEmpty(deleteCommand.PromptName))
{
    deleteCommand.PromptName = arg;
    parsedOption = true;
}
else if (command is PromptGetCommand getCommand && string.IsNullOrEmpty(getCommand.PromptName))
{
    getCommand.PromptName = arg;
    parsedOption = true;
}
```

**Evidence**: Prompt commands use similar positional argument patterns:
- `prompt get <name>`: Single positional arg for name
- `prompt create <name> <text>`: Two positional args
- `prompt delete <name>`: Single positional arg

### Parsing: MCP Server Names

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

```csharp
// Lines 193-207: Parsing MCP command names
else if (command is McpGetCommand mcpGetCommand && string.IsNullOrEmpty(mcpGetCommand.Name))
{
    mcpGetCommand.Name = arg;
    parsedOption = true;
}
else if (command is McpAddCommand mcpAddCommand && string.IsNullOrEmpty(mcpAddCommand.Name))
{
    mcpAddCommand.Name = arg;
    parsedOption = true;
}
else if (command is McpRemoveCommand mcpRemoveCommand && string.IsNullOrEmpty(mcpRemoveCommand.Name))
{
    mcpRemoveCommand.Name = arg;
    parsedOption = true;
}
```

**Evidence**: MCP commands use similar positional argument patterns for server names.

### Application: Alias Get - Name-Based Retrieval

**File**: `src/cycod/CommandLineCommands/AliasCommands/AliasGetCommand.cs`

```csharp
// Lines 46-58: ExecuteAsync entry point
public override async Task<object> ExecuteAsync(bool interactive)
{
    return await Task.Run(() => 
    {
        if (string.IsNullOrWhiteSpace(AliasName))
        {
            ConsoleHelpers.WriteErrorLine("Error: Alias name is required.");
            return 1;
        }

        return ExecuteGet(AliasName, Scope ?? ConfigFileScope.Any);
    });
}

// Lines 66-90: Name-based retrieval logic
private int ExecuteGet(string aliasName, ConfigFileScope scope)
{
    ConsoleHelpers.WriteDebugLine($"ExecuteGet; aliasName: {aliasName}; scope: {scope}");

    var isAnyScope = scope == ConfigFileScope.Any;
    var aliasFilePath = isAnyScope
        ? AliasFileHelpers.FindAliasFile(aliasName)
        : AliasFileHelpers.FindAliasInScope(aliasName, scope);

    var fileNotFound = aliasFilePath == null || !File.Exists(aliasFilePath);
    if (fileNotFound)
    {
        ConsoleHelpers.WriteErrorLine(isAnyScope
            ? $"Error: Alias '{aliasName}' not found in any scope."
            : $"Error: Alias '{aliasName}' not found in specified scope.");
        return 1;
    }
    
    // Display the alias using the standardized method
    var content = FileHelpers.ReadAllText(aliasFilePath!);
    var foundInScope = ScopeFileHelpers.GetScopeFromPath(aliasFilePath!);
    AliasDisplayHelpers.DisplayAlias(aliasName, content, aliasFilePath!, foundInScope);

    return 0;
}
```

**Evidence**:
- Lines 50-54: Validates that alias name is provided
- Lines 71-73: Locates the alias file by name in the specified scope (or any scope)
- Lines 75-82: Returns error if not found
- Lines 84-87: Displays the single alias content

**Pattern**: Prompt get and MCP get commands follow identical structure with their respective helpers.

---

## Scope-Based Filtering

### Parsing: Scope Options

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

**Alias Commands**:
```csharp
// Lines 258-289: Parsing alias command scope options
private bool TryParseAliasCommandOptions(AliasBaseCommand? command, string[] args, ref int i, string arg)
{
    if (command == null)
    {
        return false;
    }

    bool parsed = true;

    if (arg == "--global" || arg == "-g")
    {
        command.Scope = ConfigFileScope.Global;
    }
    else if (arg == "--user" || arg == "-u")
    {
        command.Scope = ConfigFileScope.User;
    }
    else if (arg == "--local" || arg == "-l")
    {
        command.Scope = ConfigFileScope.Local;
    }
    else if (arg == "--any" || arg == "-a")
    {
        command.Scope = ConfigFileScope.Any;
    }
    else
    {
        parsed = false;
    }

    return parsed;
}
```

**Evidence**: Scope options are parsed identically for alias, prompt, and MCP commands (lines 258-322).

### Application: Alias List - Scope-Based Display

**File**: `src/cycod/CommandLineCommands/AliasCommands/AliasListCommand.cs`

```csharp
// Lines 15-18: Default scope is Any
public AliasListCommand() : base()
{
    Scope = ConfigFileScope.Any;
}

// Lines 34-37: ExecuteAsync with scope routing
public override async Task<object> ExecuteAsync(bool interactive)
{
    return await Task.Run(() => ExecuteList(Scope ?? ConfigFileScope.Any));
}

// Lines 44-66: Scope-based listing logic
private int ExecuteList(ConfigFileScope scope)
{
    var isAnyScope = scope == ConfigFileScope.Any;

    if (isAnyScope || scope == ConfigFileScope.Global)
    {
        AliasDisplayHelpers.DisplayAliases(ConfigFileScope.Global);
        if (isAnyScope) ConsoleHelpers.WriteLine(overrideQuiet: true);
    }

    if (isAnyScope || scope == ConfigFileScope.User)
    {
        AliasDisplayHelpers.DisplayAliases(ConfigFileScope.User);
        if (isAnyScope) ConsoleHelpers.WriteLine(overrideQuiet: true);
    }

    if (isAnyScope || scope == ConfigFileScope.Local)
    {
        AliasDisplayHelpers.DisplayAliases(ConfigFileScope.Local);
    }

    return 0;
}
```

**Evidence**:
- Lines 48-52: If scope is `Any` or `Global`, display global aliases
- Lines 54-58: If scope is `Any` or `User`, display user aliases
- Lines 60-63: If scope is `Any` or `Local`, display local aliases

This implements scope-based filtering - only the requested scope(s) are displayed.

**Pattern**: Prompt list and MCP list commands follow identical structure.

---

## Alias Content Tokenization

### Application: Tokenization in Alias Add

**File**: `src/cycod/CommandLineCommands/AliasCommands/AliasAddCommand.cs`

```csharp
// Lines 50-68: ExecuteAsync validation and preprocessing
public override async Task<object> ExecuteAsync(bool interactive)
{
    return await Task.Run(() =>
    {
        if (string.IsNullOrWhiteSpace(AliasName))
        {
            ConsoleHelpers.WriteErrorLine("Error: Alias name is required.");
            return 1;
        }

        if (string.IsNullOrWhiteSpace(Content))
        {
            ConsoleHelpers.WriteErrorLine("Error: Alias content is required. Provide it with as the next parameter after the alias name.");
            ConsoleHelpers.WriteLine("Example: cycod alias add myalias \"--system-prompt \\\"You are helpful\\\" --instruction\"");
            return 1;
        }

        return ExecuteAdd(AliasName, Content, Scope ?? ConfigFileScope.Local);
    });
}

// Lines 78-117: ExecuteAdd with tokenization
private int ExecuteAdd(string aliasName, string content, ConfigFileScope scope)
{
    ConsoleHelpers.WriteDebugLine($"ExecuteAdd; aliasName: {aliasName}; scope: {scope}");

    // Check if the alias already exists
    var existingAliasPath = AliasFileHelpers.FindAliasInScope(aliasName, scope);
    if (existingAliasPath != null)
    {
        ConsoleHelpers.WriteWarningLine($"Warning: Alias '{aliasName}' already exists in the {scope.ToString().ToLower()} scope and will be overwritten.");
    }

    // Remove any "cycod" executable name if the user accidentally included it
    if (content.TrimStart().StartsWith("cycod "))
    {
        content = content.TrimStart().Substring(6).TrimStart();
        ConsoleHelpers.WriteWarningLine("Note: 'cycod' prefix removed from alias content (not needed).");
    }

    var contentLines = TokenizeAliasValue(content);

    // Get the alias directory and ensure it exists
    var aliasDirectory = AliasFileHelpers.FindAliasDirectoryInScope(scope, create: true)!;
    var fileName = Path.Combine(aliasDirectory, $"{aliasName}.alias");

    try
    {
        // Write the content to the alias file
        File.WriteAllLines(fileName, contentLines);
        ConsoleHelpers.WriteLine($"Created alias '{aliasName}' in {scope.ToString().ToLower()} scope.");
        ConsoleHelpers.WriteLine($"Usage examples:");
        ConsoleHelpers.WriteLine($"  cycod --{aliasName} [additional arguments]");
        ConsoleHelpers.WriteLine($"  cycod [arguments] --{aliasName} [more arguments]");
        return 0;
    }
    catch (Exception ex)
    {
        ConsoleHelpers.LogException(ex, "Error creating alias");
        return 1;
    }
}
```

**Evidence**:
- Lines 89-94: Content undergoes preprocessing to remove "cycod" prefix if present
- Line 96: Content string is tokenized into individual lines via `TokenizeAliasValue()`
- Line 105: Tokenized lines are written to the alias file

### Implementation: Tokenization Logic

**File**: `src/cycod/CommandLineCommands/AliasCommands/AliasAddCommand.cs`

```csharp
// Lines 119-166: Tokenization implementation
public static string[] TokenizeAliasValue(string commandLine)
{
    if (string.IsNullOrWhiteSpace(commandLine))
        return Array.Empty<string>();

    var args = new List<string>();
    var currentArg = new StringBuilder();
    var inQuotes = false;

    for (int i = 0; i < commandLine.Length; i++)
    {
        char c = commandLine[i];

        if (c == '"')
        {
            inQuotes = !inQuotes;
        }
        else if (c == '\\' && i + 1 < commandLine.Length && commandLine[i + 1] == '"')
        {
            // Handle escaped quotes
            currentArg.Append('"');
            i++; // Skip the next character (the quote)
        }
        else if (char.IsWhiteSpace(c) && !inQuotes)
        {
            // End of current argument
            if (currentArg.Length > 0)
            {
                args.Add(currentArg.ToString());
                currentArg.Clear();
            }

            // Skip consecutive whitespace
            while (i + 1 < commandLine.Length && char.IsWhiteSpace(commandLine[i + 1]))
                i++;
        }
        else
        {
            currentArg.Append(c);
        }
    }

    // Add final argument if any
    if (currentArg.Length > 0)
        args.Add(currentArg.ToString());

    return args.ToArray();
}
```

**Evidence**: Tokenization logic:
- Lines 132-135: Toggle quote tracking when encountering `"`
- Lines 136-141: Handle escaped quotes (`\"`) by appending literal `"` and skipping next char
- Lines 142-154: Split on whitespace when not inside quotes, skipping consecutive whitespace
- Lines 156-159: Append characters to current argument
- Lines 162-163: Add final argument if present

This transforms a command-line string like:
```
--system-prompt "You are helpful" --instruction
```

Into three separate lines in the alias file:
```
--system-prompt
You are helpful
--instruction
```

---

## GitHub Models

### GitHub Login

**GitHub login** stores credentials in configuration scope (typically user scope). It has no Layer 3 content filtering - it's an interactive authentication flow that stores the result in config.

**Source**: `src/cycod/CommandLineCommands/GitHubCommands/GitHubLoginCommand.cs`

### GitHub Models

**GitHub models** lists available models from the GitHub Models service. There is no user-controlled content filtering at Layer 3 - all available models are displayed.

**Source**: `src/cycod/CommandLineCommands/GitHubCommands/GitHubModelsCommand.cs`

**Note**: The actual filtering of which models to display is determined by the GitHub Models API response, not by user-provided Layer 3 filters.

---

## Summary

This proof document demonstrates that Layer 3 (Content Filter) for alias/prompt/mcp/github commands is implemented through:

1. **Name-based selection**: Single positional arguments select specific resources by name
2. **Scope-based filtering**: Scope options (`--global`, `--user`, `--local`, `--any`) control which scopes are displayed in list commands
3. **Alias content tokenization**: Command-line strings are tokenized into individual arguments for proper alias file format
4. **No advanced filtering**: Unlike `chat` commands, these resource management commands don't have template substitution, pattern matching, or content removal

The commands operate on structured named resources (files with specific extensions like `.alias`, `.prompt`, `.mcp.json`) rather than free-form content, so Layer 3 filtering is minimal and focused on resource selection rather than content transformation.
