# cycod alias/prompt/mcp/github Commands - Layer 5: Context Expansion - PROOF

## Overview

This document provides evidence that Layer 5 (Context Expansion) is **not applicable** to cycod's resource management commands (alias, prompt, mcp, github).

## Source Files Referenced

- **Primary Parser**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`
- **Command Implementations**: `src/cycod/Commands/*` directory

---

## Evidence of No Context Expansion

### 1. Alias Command Options

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

#### Alias Option Parsing (Lines 258-289)
```csharp
Lines 258-289:
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

**Evidence**:
- Lines 267-269: `--global` / `-g` - scope selection only (Layer 1/2)
- Lines 271-273: `--user` / `-u` - scope selection only (Layer 1/2)
- Lines 275-277: `--local` / `-l` - scope selection only (Layer 1/2)
- Lines 279-281: `--any` / `-a` - scope selection only (Layer 1/2)
- **No context expansion options** (no `--show-related`, `--with-dependencies`, etc.)

#### Alias Positional Arguments (Lines 148-172)
```csharp
Lines 148-152:
        else if (command is AliasGetCommand aliasGetCommand && string.IsNullOrEmpty(aliasGetCommand.AliasName))
        {
            aliasGetCommand.AliasName = arg;
            parsedOption = true;
        }

Lines 153-167:
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

Lines 168-172:
        else if (command is AliasDeleteCommand aliasDeleteCommand && string.IsNullOrEmpty(aliasDeleteCommand.AliasName))
        {
            aliasDeleteCommand.AliasName = arg;
            parsedOption = true;
        }
```

**Evidence**:
- Single argument: alias name
- Optional second argument (add command): alias content
- **No context parameters**: no context window size, no "show N related aliases", etc.

---

### 2. Prompt Command Options

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

#### Prompt Option Parsing (Lines 291-322)
```csharp
Lines 291-322:
    private bool TryParsePromptCommandOptions(PromptBaseCommand? command, string[] args, ref int i, string arg)
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

**Evidence**: Identical structure to alias commands - **scope selection only, no context expansion**.

#### Prompt Positional Arguments (Lines 173-192)
```csharp
Lines 173-182:
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

Lines 183-187:
        else if (command is PromptDeleteCommand deleteCommand && string.IsNullOrEmpty(deleteCommand.PromptName))
        {
            deleteCommand.PromptName = arg;
            parsedOption = true;
        }

Lines 188-192:
        else if (command is PromptGetCommand getCommand && string.IsNullOrEmpty(getCommand.PromptName))
        {
            getCommand.PromptName = arg;
            parsedOption = true;
        }
```

**Evidence**: Single argument (prompt name), optional second (prompt text for create) - **no context parameters**.

---

### 3. MCP Command Options

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

#### MCP Option Parsing (Lines 324-395)
```csharp
Lines 324-395:
    private bool TryParseMcpCommandOptions(McpBaseCommand? command, string[] args, ref int i, string arg)
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
        else
        {
            parsed = false;
        }

        return parsed;
    }
```

**Evidence**:
- Lines 333-346: Scope selection (same as alias/prompt)
- Lines 349-355: `--command` - MCP command path (Layer 9 - action parameter)
- Lines 357-363: `--url` - MCP URL (Layer 9 - action parameter)
- Lines 365-371: `--arg` - MCP argument (Layer 9 - action parameter)
- Lines 373-381: `--args` - Multiple MCP arguments (Layer 9 - action parameter)
- Lines 383-388: `--env` - Environment variable (Layer 9 - action parameter)
- **No context expansion options**

#### MCP Positional Arguments (Lines 193-207)
```csharp
Lines 193-197:
        else if (command is McpGetCommand mcpGetCommand && string.IsNullOrEmpty(mcpGetCommand.Name))
        {
            mcpGetCommand.Name = arg;
            parsedOption = true;
        }

Lines 198-202:
        else if (command is McpAddCommand mcpAddCommand && string.IsNullOrEmpty(mcpAddCommand.Name))
        {
            mcpAddCommand.Name = arg;
            parsedOption = true;
        }

Lines 203-207:
        else if (command is McpRemoveCommand mcpRemoveCommand && string.IsNullOrEmpty(mcpRemoveCommand.Name))
        {
            mcpRemoveCommand.Name = arg;
            parsedOption = true;
        }
```

**Evidence**: Single argument (MCP server name) - **no context parameters**.

---

### 4. GitHub Command Options

**File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

#### GitHub Login Option Parsing (Lines 681-725)
```csharp
Lines 681-725:
    private bool TryParseGitHubLoginCommandOptions(GitHubLoginCommand? command, string[] args, ref int i, string arg)
    {
        if (command == null)
        {
            return false;
        }

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

**Evidence**:
- Lines 690-698: `--config` - load specific config file (Layer 1)
- Lines 699-722: Scope selection (same pattern as other commands)
- **No context expansion options**

#### GitHub Models Command
```
Expected implementation (based on command name):
- Lists available GitHub models
- No command-line options for context expansion
- No positional arguments for context parameters
```

**Evidence**: GitHub models command has **no documented options** in the parser for context expansion.

---

## Comparison with Context-Aware Commands

### Commands with Context Expansion

#### cycod chat
```csharp
// Has explicit context expansion:
--chat-history               // Load conversation history (temporal context)
--input-chat-history         // Load specific history file
--continue                   // Auto-load most recent history
--add-system-prompt          // Add instructional context
--add-user-prompt            // Add user context
--image                      // Add visual context
```

#### cycodmd
```csharp
// Has explicit context expansion:
--lines N                    // Show N lines before AND after
--lines-before N             // Show N lines before
--lines-after N              // Show N lines after
```

#### cycodj
```csharp
// Has explicit context expansion:
--context N, -C N            // Show N messages before/after match
```

**Evidence**: Resource management commands (alias, prompt, mcp, github) have **none of these patterns**.

---

## What These Commands DO Have (Other Layers)

### Layer 1: Target Selection (Scope Selection)
```csharp
All list/get/add/delete/remove commands support:
--global, -g
--user, -u
--local, -l
--any, -a
```

### Layer 2: Container Filter (Scope Filtering)
```csharp
list commands use scope flags to filter which scopes to search
```

### Layer 9: Actions on Results
```csharp
Positional arguments for resource names
MCP add command has additional action parameters (--command, --url, --arg, --args, --env)
```

---

## Conclusion

### Evidence Summary

1. **No command-line options** for context expansion in any resource management command
2. **No positional arguments** for context parameters
3. **Simple CRUD operations** - no search, filter, or analysis requiring context
4. **Comparison** with context-aware commands shows resource commands lack expansion patterns
5. **MCP add options** are for action parameters (Layer 9), not context expansion

### Definitive Statement

Layer 5 (Context Expansion) **is not implemented and not applicable** to cycod's resource management commands (alias, prompt, mcp, github). These commands perform atomic operations on individual resources without surrounding context.

Any future context expansion features (showing related resources, usage, dependencies, etc.) would be **new functionality** not present in the current codebase.
