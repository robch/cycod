# CHATX CLI Reference

This section provides detailed reference documentation for all CHATX command-line interface (CLI) commands and options.

## Command Structure

CHATX commands follow this general structure:

```
chatx [global-options] [command] [command-options]
```

## Main Commands

| Command | Description |
|---------|-------------|
| `chatx` | Start a chat session |
| `chatx config` | Manage CHATX configuration |
| `chatx alias` | Manage command aliases |
| `chatx prompt` | Manage custom prompts |
| `chatx mcp` | Manage Model Context Protocol servers |
| `chatx github login` | Authenticate with GitHub for Copilot access |

## Global Options

These options apply to the main `chatx` command:

### Model Inputs

| Option | Description |
|--------|-------------|
| [`--system-prompt "PROMPT"`](options/system-prompt.md) | Replace system prompt given to AI model |
| [`--add-system-prompt "TEXT"`](options/add-system-prompt.md) | Add text to the system prompt |
| [`--add-user-prompt "TEXT"`](options/add-user-prompt.md) | Add user prompt(s), prepended to the first input |
| [`--input "LINE"`](options/input.md) | Provide input to the AI model |
| [`--question "LINE"`](options/question.md) | Alias for `--interactive false --quiet --input` |
| [`--instruction "LINE"`](options/instruction.md) | Alias for `--input` |
| [`--inputs "INPUT1" "INPUT2" ...`](options/inputs.md) | Provide multiple sequential inputs |
| [`--questions "Q1" "Q2" ...`](options/questions.md) | Alias for `--interactive false --quiet --inputs` |
| [`--instructions "I1" "I2" ...`](options/instructions.md) | Alias for `--inputs` |
| [`--use-templates TRUE/FALSE`](options/use-templates.md) | Control template processing in inputs (default: true) |
| [`--no-templates`](options/no-templates.md) | Alias for `--use-templates false` |

### Chat History

| Option | Description |
|--------|-------------|
| [`--continue`](options/continue.md) | Continue the most recent chat history |
| [`--chat-history [FILE]`](options/chat-history.md) | Load from and save to the same JSONL file |
| [`--input-chat-history [FILE]`](options/input-chat-history.md) | Load chat history from the specified JSONL file |
| [`--output-chat-history [FILE]`](options/output-chat-history.md) | Save chat history to the specified file |
| [`--output-trajectory [FILE]`](options/output-trajectory.md) | Save chat history in human readable format |

### Model Options

| Option | Description |
|--------|-------------|
| `--max-tokens TOKENS` | Limit AI output to specified number of tokens |
| [`--trim-token-target TOKENS`](options/trim-token-target.md) | Specify chat history maximum tokens target (default: 18000) |

### Model Providers

| Option | Description |
|--------|-------------|
| [`--use-copilot`](options/use-copilot.md) | Prefer use of GitHub Copilot |
| [`--use-openai`](options/use-openai.md) | Prefer use of OpenAI API |
| [`--use-azure`](options/use-azure.md) | Alias for `--use-azure-openai` |
| [`--use-azure-openai`](options/use-azure-openai.md) | Prefer use of Azure OpenAI API |

### Azure OpenAI Options

| Option | Description |
|--------|-------------|
| [`--azure-openai-api-key KEY`](options/azure-openai-api-key.md) | Use a specific authentication key |
| [`--azure-openai-endpoint URL`](options/azure-openai-endpoint.md) | Use a specific API endpoint |
| [`--azure-openai-chat-deployment NAME`](options/azure-openai-chat-deployment.md) | Use a specific chat model deployment |

### Copilot Options

| Option | Description |
|--------|-------------|
| [`--copilot-model-name NAME`](options/copilot-model-name.md) | Use a specific model by name (default: claude-3.7-sonnet) |
| [`--copilot-api-endpoint URL`](options/copilot-api-endpoint.md) | Use a specific API endpoint (default: https://api.githubcopilot.com) |
| [`--github-token TOKEN`](options/github-token.md) | Use a specific GitHub authentication token |

### OpenAI Options

| Option | Description |
|--------|-------------|
| [`--openai-api-key KEY`](options/openai-api-key.md) | Use a specific API key |
| [`--openai-chat-model-name NAME`](options/openai-chat-model-name.md) | Use a specific chat model (default: gpt-4o) |

### Configuration

| Option | Description |
|--------|-------------|
| [`--config FILE1 [FILE2 [...]]`](options/config.md) | Load configuration from YAML or INI files |
| [`--profile NAME`](options/profile.md) | Load a specific profile's configuration |

### Aliases

| Option | Description |
|--------|-------------|
| [`--save-alias ALIAS`](options/save-alias.md) | Same as `--save-local-alias` |
| [`--save-local-alias ALIAS`](options/save-local-alias.md) | Save current options as an alias in local scope |
| [`--save-user-alias ALIAS`](options/save-user-alias.md) | Save current options as an alias in user scope |
| [`--save-global-alias ALIAS`](options/save-global-alias.md) | Save current options as an alias in global scope |
| `--{ALIAS}` | Use options saved under the specified alias name |

### Variables

| Option | Description |
|--------|-------------|
| [`--var NAME=VALUE`](options/var.md) | Set a variable for template substitution |
| [`--vars NAME1=VALUE1 NAME2=VALUE2 ...`](options/vars.md) | Set multiple variables for template substitution |
| [`--foreach var NAME in VALUE1 [...]`](options/foreach.md) | Define a loop variable with multiple values |
| [`--foreach var NAME in @FILE`](options/foreach.md) | Define a loop variable with values from a file |
| [`--foreach var NAME in #..#`](options/foreach.md) | Define a loop variable with a numeric range |

### Additional Options

| Option | Description |
|--------|-------------|
| [`--interactive TRUE/FALSE`](options/interactive.md) | Allow interactive use (default: true) |
| [`--threads COUNT`](options/threads.md) | Number of parallel threads for non-interactive mode |
| [`--debug`](options/debug.md) | Turn on diagnostics/debug outputs |
| [`--quiet`](options/quiet.md) | Turn off all but the most critical console outputs |
| [`--verbose`](options/verbose.md) | Turn on additional diagnostics/debug outputs |

## Subcommands

CHATX includes several subcommands for managing different aspects of the tool:

### Config Commands

| Command | Description |
|---------|-------------|
| `chatx config list` | List configuration settings |
| `chatx config get KEY` | Get the value of a configuration setting |
| `chatx config set KEY VALUE` | Set the value of a configuration setting |
| `chatx config clear KEY` | Clear a configuration setting |
| `chatx config add KEY VALUE` | Add a value to a list setting |
| `chatx config remove KEY VALUE` | Remove a value from a list setting |

### Alias Commands

| Command | Description |
|---------|-------------|
| `chatx alias list` | List all available aliases |
| `chatx alias get ALIAS_NAME` | Display the content of a specific alias |
| `chatx alias delete ALIAS_NAME` | Delete an alias |

### Prompt Commands

| Command | Description |
|---------|-------------|
| `chatx prompt list` | List all available prompts |
| `chatx prompt get PROMPT_NAME` | Display the content of a specific prompt |
| `chatx prompt delete PROMPT_NAME` | Delete a prompt |
| `chatx prompt create PROMPT_NAME TEXT` | Create a new prompt |

### MCP Commands

| Command | Description |
|---------|-------------|
| `chatx mcp list` | List all available MCP servers |
| `chatx mcp get SERVER_NAME` | Display the details of a specific MCP server |
| `chatx mcp add SERVER_NAME --command CMD` | Create a new MCP server configuration |
| `chatx mcp remove SERVER_NAME` | Delete an MCP server configuration |

### GitHub Commands

| Command | Description |
|---------|-------------|
| `chatx github login` | Authenticate with GitHub for Copilot access |

## Scope Options

Many commands support different scopes for configuration, aliases, prompts, and MCP servers:

| Option | Description |
|--------|-------------|
| `--global`, `-g` | Use global scope (all users) |
| `--user`, `-u` | Use user scope (current user) |
| `--local`, `-l` | Use local scope (current directory, default for most commands) |
| [`--any`, `-a`](options/any.md) | Include items from all scopes (default for `list` and `get` commands) |