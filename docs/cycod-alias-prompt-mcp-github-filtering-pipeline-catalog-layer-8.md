# cycod `alias`, `prompt`, `mcp`, `github` Commands - Layer 8: AI Processing

## Overview

**Layer 8** focuses on AI-assisted analysis and processing of content. The following commands are **administrative/resource management commands** that **do not involve AI processing**:

- `alias list`, `alias get`, `alias add`, `alias delete`
- `prompt list`, `prompt get`, `prompt create`, `prompt delete`
- `mcp list`, `mcp get`, `mcp add`, `mcp remove`
- `github login`, `github models`

## Commands

### Alias Commands
```bash
cycod alias list [--global|--user|--local|--any]
cycod alias get <name> [--global|--user|--local|--any]
cycod alias add <name> <content> [--global|--user|--local]
cycod alias delete <name> [--global|--user|--local]
```

### Prompt Commands
```bash
cycod prompt list [--global|--user|--local|--any]
cycod prompt get <name> [--global|--user|--local|--any]
cycod prompt create <name> <text> [--global|--user|--local]
cycod prompt delete <name> [--global|--user|--local]
```

### MCP Commands
```bash
cycod mcp list [--global|--user|--local|--any]
cycod mcp get <name> [--global|--user|--local|--any]
cycod mcp add <name> --command <cmd> [--arg <arg>]... [--env <var>]... [--global|--user|--local]
cycod mcp add <name> --url <url> [--global|--user|--local]
cycod mcp remove <name> [--global|--user|--local]
```

### GitHub Commands
```bash
cycod github login [--global|--user|--local]
cycod github models
```

## Purpose

These commands perform administrative operations for resource management:

- **alias**: Command-line shortcuts (expand to full command-line arguments)
- **prompt**: Saved prompt templates (text files for reuse)
- **mcp**: MCP server configurations (enable AI tools, but don't run AI themselves)
- **github**: GitHub authentication and model discovery (setup for AI access)

## Layer 8 Applicability

**Layer 8 (AI Processing) is NOT APPLICABLE** to these commands because:

1. **No AI model interaction** - Commands manage files/configurations, no AI calls
2. **No content analysis** - Simple CRUD operations, no interpretation needed
3. **No tool calling** - No functions or external tools invoked (MCP commands *configure* tools but don't *call* them)
4. **No conversation context** - Single-operation commands with no history
5. **No template processing** - Resources stored/retrieved verbatim (templates expanded when *used*, not when *managed*)
6. **No multi-modal input** - Text-only operations
7. **Deterministic output** - No generative or probabilistic processing

## What These Commands DO

### Alias Commands

#### Purpose
Manage command-line shortcuts.

#### Operations
- **list**: Display all aliases from specified scope
- **get**: Show content of specific alias
- **add**: Create or update alias
- **delete**: Remove alias

#### Storage
- Individual text files: `.cycod/alias/{name}` (per scope)
- Content: Raw command-line arguments (one per line or space-separated)

### Prompt Commands

#### Purpose
Manage reusable prompt templates.

#### Operations
- **list**: Display all prompts from specified scope
- **get**: Show content of specific prompt
- **create**: Create or update prompt
- **delete**: Remove prompt

#### Storage
- Individual text files: `.cycod/prompt/{name}` (per scope)
- Content: Plain text (can include template variables like `{VAR}`)

### MCP Commands

#### Purpose
Manage MCP (Model Context Protocol) server configurations.

#### Operations
- **list**: Display all MCP servers from specified scope
- **get**: Show configuration of specific MCP server
- **add**: Register MCP server (stdio or SSE transport)
- **remove**: Unregister MCP server

#### Storage
- JSON configuration: `.cycod/config.json` under `mcp.servers` key
- Structure: `{ "name": { "command": "...", "args": [...], "env": {...}, "transport": "stdio" } }`

#### MCP Options
- `--command <cmd>`: Command to execute (stdio transport)
- `--url <url>`: URL to connect to (SSE transport)
- `--arg <arg>`: Argument to pass to command (repeatable)
- `--args <arg1> <arg2> ...`: Multiple arguments at once
- `--env <VAR=VALUE>`, `-e <VAR=VALUE>`: Environment variable (repeatable)

### GitHub Commands

#### Purpose
Authenticate with GitHub and discover available AI models.

#### Operations
- **login**: Obtain and store GitHub API token for model access
- **models**: List available AI models from GitHub

#### Storage
- Token stored in config: `github.api_token` or `copilot.token`
- Scope options: `--global`, `--user`, `--local`

## Relationship to AI Processing

While these commands don't use AI themselves, they **enable** AI processing in other commands:

### Aliases Enable Workflows
```bash
cycod alias add analyze-cs "--input 'Analyze {FILE}' --cs-file-contains 'async'"
cycod alias get analyze-cs  # Just retrieves text, no AI
cycod --analyze-cs FILE=Program.cs  # Uses AI when alias is expanded
```

### Prompts Provide AI Instructions
```bash
cycod prompt create code-review "You are a code reviewer. Focus on: {ASPECTS}"
cycod prompt get code-review  # Just retrieves text, no AI
cycod --prompt /code-review --var ASPECTS="security, performance"  # Uses AI with prompt
```

### MCP Servers Extend AI Capabilities
```bash
cycod mcp add myserver --command "node mcp-server.js" --arg "--port" --arg "8080"
cycod mcp get myserver  # Just shows config, no AI
cycod --use-mcps myserver --input "Use myserver tools"  # AI uses MCP tools
```

### GitHub Provides AI Model Access
```bash
cycod github login --global  # Interactive OAuth, stores token
cycod github models  # Lists models, no AI calls
cycod --use-copilot --input "Hello"  # AI chat using GitHub models
```

## Source Code Evidence

### Command-Line Parsing

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

#### Alias/Prompt Options (Lines 259-322)
```csharp
private bool TryParseAliasCommandOptions(AliasBaseCommand? command, string[] args, ref int i, string arg)
{
    if (command == null) return false;
    
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

// TryParsePromptCommandOptions is identical (different command type check only)
```

**Evidence**: Only scope options. No AI-related options.

#### MCP Options (Lines 324-395)
```csharp
private bool TryParseMcpCommandOptions(McpBaseCommand? command, string[] args, ref int i, string arg)
{
    // ... scope options (same as above) ...
    
    else if (command is McpAddCommand cmdAddCommand && arg == "--command")
    {
        var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
        var cmdValue = ValidateString(arg, max1Arg.FirstOrDefault(), "command");
        cmdAddCommand.Command = cmdValue;
        cmdAddCommand.Transport = "stdio";
        i += max1Arg.Count();
    }
    else if (command is McpAddCommand urlAddCommand && arg == "--url")
    {
        var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
        var urlValue = ValidateString(arg, max1Arg.FirstOrDefault(), "url");
        urlAddCommand.Url = urlValue;
        urlAddCommand.Transport = "sse";
        i += max1Arg.Count();
    }
    else if (command is McpAddCommand argAddCommand && arg == "--arg")
    {
        var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
        var argValue = ValidateString(arg, max1Arg.FirstOrDefault(), "argument");
        argAddCommand.Args.Add(argValue!);
        i += max1Arg.Count();
    }
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
    else if (command is McpAddCommand envAddCommand && (arg == "--env" || arg == "-e"))
    {
        var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
        var env = ValidateString(arg, max1Arg.FirstOrDefault(), "environment variable");
        envAddCommand.EnvironmentVars.Add(env!);
        i += max1Arg.Count();
    }
    // ...
}
```

**Evidence**: MCP options configure server parameters (command, URL, args, env). No AI interaction, just configuration storage.

#### GitHub Options (Lines 681-725)
```csharp
private bool TryParseGitHubLoginCommandOptions(GitHubLoginCommand? command, string[] args, ref int i, string arg)
{
    if (command == null) return false;
    
    bool parsed = true;
    
    if (arg == "--config")
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
    // ... other scope options ...
    else
    {
        parsed = false;
    }
    
    return parsed;
}
```

**Evidence**: GitHub commands only have scope options for token storage location. No AI processing options.

### Command Execution Patterns

All these commands follow a simple pattern:

```csharp
public override async Task<object> ExecuteAsync(bool interactive)
{
    // 1. Read from file/config
    var content = LoadResourceFromScope(Scope, Name);
    
    // 2. Perform operation (list/get/add/delete)
    switch (Operation)
    {
        case "list": DisplayAllResources(); break;
        case "get": DisplayResource(content); break;
        case "add": SaveResource(content); break;
        case "delete": DeleteResource(); break;
    }
    
    // 3. Exit
    return 0;
}
```

**No AI calls**, **no conversation loop**, **no streaming**, **no function calling**.

## Layer Usage Summary

| Command Group | Layer 1 | Layer 2 | Layer 3 | Layer 4 | Layer 5 | Layer 6 | Layer 7 | Layer 8 | Layer 9 |
|---------------|---------|---------|---------|---------|---------|---------|---------|---------|---------|
| **alias** | ✅ Name | ⚪ N/A | ⚪ N/A | ⚪ N/A | ⚪ N/A | ✅ Display | ✅ File I/O | ⚪ N/A | ✅ CRUD |
| **prompt** | ✅ Name | ⚪ N/A | ⚪ N/A | ⚪ N/A | ⚪ N/A | ✅ Display | ✅ File I/O | ⚪ N/A | ✅ CRUD |
| **mcp** | ✅ Name | ⚪ N/A | ⚪ N/A | ⚪ N/A | ⚪ N/A | ✅ Display | ✅ Config | ⚪ N/A | ✅ CRUD |
| **github** | ⚪ N/A | ⚪ N/A | ⚪ N/A | ⚪ N/A | ⚪ N/A | ✅ Display | ✅ Config | ⚪ N/A | ✅ Auth |

**Legend**:
- ✅ = Implemented
- ⚪ = Not applicable
- N/A = Not applicable to this command type

**Contrast with chat command**:
- **chat**: Uses all 9 layers, especially Layer 8 (extensive AI processing)

## Related Documentation

- [Chat Layer 8](cycod-chat-filtering-pipeline-catalog-layer-8.md) - Real AI processing example
- [Config Layer 8](cycod-config-filtering-pipeline-catalog-layer-8.md) - Similar administrative pattern
- [Main Catalog](cycod-filter-pipeline-catalog-README.md)

## Proof Document

See [Layer 8 Proof](cycod-alias-prompt-mcp-github-filtering-pipeline-catalog-layer-8-proof.md) for confirmation that no AI processing code exists in these commands.
