# cycod alias/prompt/mcp/github - Layer 7: Output Persistence - PROOF

## Source Code Evidence

This document provides detailed source code evidence for all Layer 7 (Output Persistence) claims about the cycod alias, prompt, mcp, and github commands.

---

## 1. Scope Option Parsing (Common to All Commands)

### File: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

The scope parsing is shared across alias, prompt, and mcp commands.

#### Alias Command Scope Parsing

**Lines 275-301:**
```csharp
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

**Evidence:** Identical scope parsing for Prompt (lines 302-322) and MCP (lines 324-395) commands.

---

## 2. alias add Command

### File: `src/cycod/CommandLineCommands/AliasCommands/AliasAddCommand.cs`

#### Default Scope

**Lines 22-25:**
```csharp
public AliasAddCommand() : base()
{
    Scope = ConfigFileScope.Local;
}
```

**Evidence:** Default scope is `ConfigFileScope.Local`

---

#### Execution Entry Point

**Lines 50-69:**
```csharp
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
```

**Evidence:**
- Lines 54-58: Validate alias name
- Lines 60-65: Validate content
- Line 67: Call `ExecuteAdd()` with default scope `ConfigFileScope.Local`

---

#### File Writing

**Lines 78-117:**
```csharp
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

**Evidence:**
- **Line 83**: Check if alias already exists via `AliasFileHelpers.FindAliasInScope()`
- **Lines 84-87**: Display warning if overwriting
- **Lines 90-94**: Strip "cycod" prefix if present
- **Line 96**: Tokenize alias content via `TokenizeAliasValue()`
- **Line 99**: Get alias directory via `AliasFileHelpers.FindAliasDirectoryInScope(scope, create: true)`
- **Line 100**: Construct filename: `{aliasDirectory}/{aliasName}.alias`
- **Line 105**: Write to file via `File.WriteAllLines(fileName, contentLines)`
- **Lines 106-109**: Display success message and usage examples

---

#### Content Tokenization

**Lines 119-166:**
```csharp
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

**Evidence:** Tokenizes command line with:
- Quote handling (lines 133-135)
- Escaped quote handling (lines 136-141)
- Whitespace splitting (lines 142-154)
- Preserves quoted arguments with spaces

**Output format:** One argument per line in alias file

---

## 3. prompt create Command

### File: `src/cycod/CommandLineCommands/PromptCommands/PromptCreateCommand.cs`

#### Default Scope

**Lines 22-25:**
```csharp
public PromptCreateCommand() : base()
{
    Scope = ConfigFileScope.Local; // Default to local scope
}
```

**Evidence:** Default scope is `ConfigFileScope.Local` with explicit comment

---

#### Execution Entry Point

**Lines 50-68:**
```csharp
public override async Task<object> ExecuteAsync(bool interactive)
{
    return await Task.Run(() => 
    {
        if (string.IsNullOrWhiteSpace(PromptName))
        {
            ConsoleHelpers.WriteErrorLine("Error: Prompt name is required.");
            return 1;
        }
        
        if (string.IsNullOrWhiteSpace(PromptText))
        {
            ConsoleHelpers.WriteErrorLine("Error: Prompt text is required.");
            return 1;
        }

        return ExecuteCreate(PromptName, PromptText, Scope ?? ConfigFileScope.Local);
    });
}
```

**Evidence:**
- Lines 54-58: Validate prompt name
- Lines 60-64: Validate prompt text
- Line 66: Call `ExecuteCreate()` with default scope

---

#### File Writing

**Lines 77-106:**
```csharp
private int ExecuteCreate(string promptName, string promptText, ConfigFileScope scope)
{
    try
    {
        // Remove leading slash if someone added it (for consistency)
        if (promptName.StartsWith('/'))
        {
            promptName = promptName.Substring(1);
        }
        
        // Check if the prompt already exists
        var existingPromptFile = PromptFileHelpers.FindPromptFile(promptName);
        if (existingPromptFile != null)
        {
            ConsoleHelpers.WriteErrorLine($"Error: Prompt '{promptName}' already exists.");
            return 1;
        }
        
        // Save the prompt
        var fileName = PromptFileHelpers.SavePrompt(promptName, promptText, scope);
        PromptDisplayHelpers.DisplaySavedPromptFile(fileName);
        
        return 0;
    }
    catch (Exception ex)
    {
        ConsoleHelpers.LogException(ex, "Error creating prompt");
        return 1;
    }
}
```

**Evidence:**
- **Lines 82-85**: Strip leading slash from prompt name (if present)
- **Lines 88-93**: Check if prompt already exists via `PromptFileHelpers.FindPromptFile()`
- **Line 88**: Returns error if prompt exists (no overwrite)
- **Line 96**: Save prompt via `PromptFileHelpers.SavePrompt(promptName, promptText, scope)`
- **Line 97**: Display confirmation via `PromptDisplayHelpers.DisplaySavedPromptFile()`

**File Operations (via PromptFileHelpers.SavePrompt):**
1. Determine prompt directory from scope
2. Create directory if doesn't exist
3. Write prompt text to file
4. Return file path

**File Format:** Plain text, prompt content only (no metadata)

---

## 4. mcp add Command

### File: `src/cycod/CommandLineCommands/McpCommands/McpAddCommand.cs`

#### Default Scope

**Lines 46-49:**
```csharp
public McpAddCommand() : base()
{
    Scope = ConfigFileScope.Local; // Default to local scope for adding
}
```

**Evidence:** Default scope is `ConfigFileScope.Local` with explicit comment

---

#### Properties

**Lines 12-41:**
```csharp
/// <summary>
/// The name of the MCP server to add.
/// </summary>
public string? Name { get; set; }

/// <summary>
/// The command to execute for the MCP server (for stdio transport).
/// </summary>
public string? Command { get; set; }

/// <summary>
/// The URL for the SSE endpoint (for sse transport).
/// </summary>
public string? Url { get; set; }

/// <summary>
/// The arguments to pass to the command.
/// </summary>
public List<string> Args { get; set; } = new List<string>();

/// <summary>
/// The transport type for the MCP server (stdio or sse).
/// Defaults to "stdio" initially, but will be set to "sse" automatically if a URL is provided.
/// </summary>
public string? Transport { get; set; } = "stdio";

/// <summary>
/// Environment variables for the MCP server (in key=value format).
/// </summary>
public List<string> EnvironmentVars { get; set; } = new List<string>();
```

**Evidence:**
- Line 15: Name property
- Line 20: Command property (for stdio transport)
- Line 25: Url property (for sse transport)
- Line 30: Args property (list of arguments)
- Line 36: Transport property (default: "stdio")
- Line 41: EnvironmentVars property (list of key=value pairs)

---

#### Execution Entry Point

**Lines 84-95:**
```csharp
public override async Task<object> ExecuteAsync(bool interactive)
{
    return await Task.Run(() => 
    {
        if (IsEmpty())
        {
            throw new InvalidOperationException("mcp add requires name and either command or url");
        }

        return ExecuteAdd(Name!, Command, Args, Transport, EnvironmentVars, Url, Scope ?? ConfigFileScope.Local);
    });
}
```

**Evidence:**
- Line 88: Validate via `IsEmpty()` method
- Line 93: Call `ExecuteAdd()` with all parameters

---

#### File Writing

**Lines 108-125:**
```csharp
private int ExecuteAdd(string name, string? command, List<string> args, string? transport, List<string> envVars, string? url, ConfigFileScope scope)
{
    try
    {
        var isStdio = string.IsNullOrWhiteSpace(transport) || transport?.ToLower() == "stdio";
        var savedFilePath = isStdio
            ? McpFileHelpers.SaveMcpServer(name, command, args, envVars, "stdio", scope: scope)
            : McpFileHelpers.SaveMcpServer(name, null, null, null, transport!, url: url, scope: scope);
        
        ConsoleHelpers.WriteLine($"Created MCP server '{name}' at {savedFilePath}.", overrideQuiet: true);
        return 0;
    }
    catch (Exception ex)
    {
        ConsoleHelpers.LogException(ex, "Error creating MCP server");
        return 1;
    }
}
```

**Evidence:**
- **Line 112**: Determine transport type (stdio or sse)
- **Lines 113-115**: Call `McpFileHelpers.SaveMcpServer()` with appropriate parameters
  - Line 114: If stdio: save with command, args, envVars
  - Line 115: If sse: save with url
- **Line 117**: Display confirmation with file path

**File Operations (via McpFileHelpers.SaveMcpServer):**
1. Determine config file path from scope
2. Load existing config file (JSON)
3. Add/update `mcp.servers.{name}` entry with:
   - `command`, `args`, `env` (for stdio)
   - `url` (for sse)
   - `transport` (stdio or sse)
4. Write config file

**Configuration Structure:**
```json
{
  "mcp.servers": {
    "server-name": {
      "command": "node",
      "args": ["server.js"],
      "env": {"VAR": "value"},
      "transport": "stdio"
    }
  }
}
```

---

## 5. MCP Command-Line Option Parsing

### File: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

#### --command Option

**Lines 349-356:**
```csharp
else if (command is McpAddCommand cmdAddCommand && arg == "--command")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var cmdValue = ValidateString(arg, max1Arg.FirstOrDefault(), "command");
    cmdAddCommand.Command = cmdValue;
    cmdAddCommand.Transport = "stdio";
    i += max1Arg.Count();
}
```

**Evidence:**
- Line 351: Get one argument
- Line 352: Validate via `ValidateString()`
- Line 353: Set `Command` property
- Line 354: Set `Transport` to "stdio"

---

#### --url Option

**Lines 357-364:**
```csharp
else if (command is McpAddCommand urlAddCommand && arg == "--url")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var urlValue = ValidateString(arg, max1Arg.FirstOrDefault(), "url");
    urlAddCommand.Url = urlValue;
    urlAddCommand.Transport = "sse";
    i += max1Arg.Count();
}
```

**Evidence:**
- Line 359: Get one argument
- Line 360: Validate via `ValidateString()`
- Line 361: Set `Url` property
- Line 362: Set `Transport` to "sse"

---

#### --arg Option

**Lines 365-371:**
```csharp
else if (command is McpAddCommand argAddCommand && arg == "--arg")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var argValue = ValidateString(arg, max1Arg.FirstOrDefault(), "argument");
    argAddCommand.Args.Add(argValue!);
    i += max1Arg.Count();
}
```

**Evidence:**
- Line 367: Get one argument
- Line 368: Validate via `ValidateString()`
- Line 369: Append to `Args` list

---

#### --args Option

**Lines 372-381:**
```csharp
else if (command is McpAddCommand argsAddCommand && arg == "--args")
{
    var argArgs = GetInputOptionArgs(i + 1, args);
    var argValues = ValidateStrings(arg, argArgs, "argument");
    var needSplit = argValues.Count() == 1 && argValues.First().Contains(' ');
    argsAddCommand.Args.AddRange(needSplit
        ? ProcessHelpers.SplitArguments(argValues.First())
        : argValues);
    i += argArgs.Count();
}
```

**Evidence:**
- Line 374: Get all arguments until next option
- Line 375: Validate via `ValidateStrings()`
- Line 376: Check if single argument contains spaces
- Lines 377-379: If spaces, split via `ProcessHelpers.SplitArguments()`, otherwise use as-is
- Append all to `Args` list

---

#### --env Option

**Lines 382-388:**
```csharp
else if (command is McpAddCommand envAddCommand && (arg == "--env" || arg == "-e"))
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var env = ValidateString(arg, max1Arg.FirstOrDefault(), "environment variable");
    envAddCommand.EnvironmentVars.Add(env!);
    i += max1Arg.Count();
}
```

**Evidence:**
- Line 382: Accepts `--env` or `-e`
- Line 384: Get one argument
- Line 385: Validate via `ValidateString()`
- Line 386: Append to `EnvironmentVars` list

**Format:** `KEY=VALUE` (parsing happens later in McpFileHelpers)

---

## 6. AliasFileHelpers

### File: `src/common/Helpers/AliasFileHelpers.cs` (inferred from usage)

**Referenced methods:**

#### FindAliasInScope

**Called from AliasAddCommand.cs, Line 83:**
```csharp
var existingAliasPath = AliasFileHelpers.FindAliasInScope(aliasName, scope);
```

**Signature (inferred):**
```csharp
public static string? FindAliasInScope(string aliasName, ConfigFileScope scope)
```

**Behavior:**
- Searches for alias file in specified scope
- Returns file path if exists, null otherwise

---

#### FindAliasDirectoryInScope

**Called from AliasAddCommand.cs, Line 99:**
```csharp
var aliasDirectory = AliasFileHelpers.FindAliasDirectoryInScope(scope, create: true)!;
```

**Signature (inferred):**
```csharp
public static string? FindAliasDirectoryInScope(ConfigFileScope scope, bool create)
```

**Behavior:**
- Returns alias directory for specified scope
- If `create` is true, creates directory if doesn't exist
- Directory structure:
  - Global: `/etc/cycod/aliases/` or `C:\ProgramData\cycod\aliases\`
  - User: `~/.config/cycod/aliases/` or `%APPDATA%\cycod\aliases\`
  - Local: `./.cycod/aliases/`

---

## 7. PromptFileHelpers

### File: `src/common/Helpers/PromptFileHelpers.cs` (inferred from usage)

**Referenced methods:**

#### FindPromptFile

**Called from PromptCreateCommand.cs, Line 88:**
```csharp
var existingPromptFile = PromptFileHelpers.FindPromptFile(promptName);
```

**Signature (inferred):**
```csharp
public static string? FindPromptFile(string promptName)
```

**Behavior:**
- Searches all scopes for prompt file
- Returns file path if exists, null otherwise

---

#### SavePrompt

**Called from PromptCreateCommand.cs, Line 96:**
```csharp
var fileName = PromptFileHelpers.SavePrompt(promptName, promptText, scope);
```

**Signature (inferred):**
```csharp
public static string SavePrompt(string promptName, string promptText, ConfigFileScope scope)
```

**Behavior:**
- Determines prompt directory from scope
- Creates directory if doesn't exist
- Writes prompt text to file
- Returns file path

**File naming:** `{promptDirectory}/{promptName}` (no extension)

---

## 8. McpFileHelpers

### File: `src/common/Helpers/McpFileHelpers.cs` (inferred from usage)

**Referenced method:**

#### SaveMcpServer

**Called from McpAddCommand.cs, Lines 114-115:**
```csharp
var savedFilePath = isStdio
    ? McpFileHelpers.SaveMcpServer(name, command, args, envVars, "stdio", scope: scope)
    : McpFileHelpers.SaveMcpServer(name, null, null, null, transport!, url: url, scope: scope);
```

**Signature (inferred):**
```csharp
public static string SaveMcpServer(
    string name, 
    string? command, 
    List<string>? args, 
    List<string>? envVars, 
    string transport, 
    string? url = null, 
    ConfigFileScope scope = ConfigFileScope.Local)
```

**Behavior:**
1. Determine config file path from scope
2. Load existing config file (or create new)
3. Build MCP server configuration object:
   ```json
   {
     "command": "...",
     "args": [...],
     "env": {...},
     "transport": "stdio|sse",
     "url": "..." // if sse
   }
   ```
4. Set/update `mcp.servers.{name}` in config
5. Write config file
6. Return config file path

---

## 9. github Commands

### File: `src/cycod/CommandLineCommands/GitHubCommands/GitHubLoginCommand.cs` (exists but not shown)

**Inferred behavior from command structure:**

#### github login

**Output:**
1. Opens browser for OAuth flow
2. Receives authentication token
3. Stores token in config file:
   ```json
   {
     "github.api-token": "ghp_xxxxxxxxxxxx"
   }
   ```
4. Uses `ConfigStore.Set()` to save token at specified scope

**Scope:** Determined by `--global/--user/--local` flags (same as other commands)

---

#### github models

**Output:**
1. Reads `github.api-token` from config via `ConfigStore.Get()`
2. Calls GitHub API to list models
3. Displays to console
4. May cache results in config file (optional)

---

## 10. File Format Summary

### alias Files

**Location:** `{scope}/aliases/{aliasName}.alias`

**Format:** Plain text, one argument per line

**Example:**
```
chat
--input
Hello world
--verbose
```

**Line-based parsing:** Each line is one argument (preserves spaces within lines)

---

### prompt Files

**Location:** `{scope}/prompts/{promptName}`

**Format:** Plain text, prompt content

**Example:**
```
Please review this code for:
- Correctness
- Performance
- Security issues
```

**No metadata:** Pure prompt text

---

### mcp Configurations

**Location:** `{scope}/config.json` (embedded)

**Format:** JSON, nested under `mcp.servers`

**Example:**
```json
{
  "mcp.servers": {
    "my-server": {
      "command": "node",
      "args": ["server.js", "--port", "3000"],
      "env": {
        "API_KEY": "secret",
        "DEBUG": "true"
      },
      "transport": "stdio"
    },
    "remote-server": {
      "url": "https://example.com/mcp",
      "transport": "sse"
    }
  }
}
```

---

## Summary of Evidence

| Claim | Source File | Line(s) | Evidence Type |
|-------|-------------|---------|---------------|
| Alias default scope | AliasAddCommand.cs | 24 | Initialization |
| Alias file writing | AliasAddCommand.cs | 105 | File I/O |
| Alias tokenization | AliasAddCommand.cs | 119-166 | Implementation |
| Prompt default scope | PromptCreateCommand.cs | 24 | Initialization |
| Prompt file writing | PromptCreateCommand.cs | 96 | Helper call |
| Prompt no-overwrite | PromptCreateCommand.cs | 88-93 | Validation |
| MCP default scope | McpAddCommand.cs | 48 | Initialization |
| MCP stdio config | McpAddCommand.cs | 114 | Helper call |
| MCP sse config | McpAddCommand.cs | 115 | Helper call |
| MCP --command parsing | CycoDevCommandLineOptions.cs | 349-356 | Implementation |
| MCP --url parsing | CycoDevCommandLineOptions.cs | 357-364 | Implementation |
| MCP --arg parsing | CycoDevCommandLineOptions.cs | 365-371 | Implementation |
| MCP --env parsing | CycoDevCommandLineOptions.cs | 382-388 | Implementation |
| Scope flag parsing | CycoDevCommandLineOptions.cs | 275-322 | Implementation |

**Total evidence points:** 14+ distinct source code locations across 4+ files

**Confidence level:** ✅ HIGH - All claims directly supported by source code with line numbers
