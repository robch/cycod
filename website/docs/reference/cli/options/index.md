# Command Line Options

This section provides detailed information about all available command line options in ChatX.

## Option Categories

ChatX offers a wide range of command line options that are grouped into the following categories:

### Model Input Options

These options control the input provided to the AI model:

| Option | Description |
|--------|-------------|
| [`--system-prompt`](system-prompt.md) | Replace system prompt given to AI model |
| [`--add-system-prompt`](add-system-prompt.md) | Add text to the system prompt |
| [`--add-user-prompt`](add-user-prompt.md) | Add user prompt(s), prepended to the first input |
| [`--input`](input.md) | Provide input to the AI model |
| [`--question`](question.md) | Alias for `--interactive false --quiet --input` |
| [`--instruction`](instruction.md) | Alias for `--input` |
| [`--inputs`](inputs.md) | Provide multiple sequential inputs |
| [`--questions`](questions.md) | Alias for `--interactive false --quiet --inputs` |
| [`--instructions`](instructions.md) | Alias for `--inputs` |
| [`--use-templates`](use-templates.md) | Control template processing in inputs |
| [`--no-templates`](no-templates.md) | Alias for `--use-templates false` |

### Chat History Options

These options manage chat history and conversations:

| Option | Description |
|--------|-------------|
| [`--continue`](continue.md) | Continue the most recent chat history |
| [`--chat-history`](chat-history.md) | Load from and save to the same JSONL file |
| [`--input-chat-history`](input-chat-history.md) | Load chat history from the specified JSONL file |
| [`--output-chat-history`](output-chat-history.md) | Save chat history to the specified file |
| [`--output-trajectory`](output-trajectory.md) | Save chat history in human readable format |

### Model Provider Options

These options control which AI provider to use:

| Option | Description |
|--------|-------------|
| [`--use-copilot`](use-copilot.md) | Prefer use of GitHub Copilot |
| [`--use-openai`](use-openai.md) | Prefer use of OpenAI API |
| [`--use-azure-openai`](use-azure-openai.md) | Prefer use of Azure OpenAI API |
| [`--use-azure`](use-azure.md) | Alias for `--use-azure-openai` |

### Azure OpenAI Options

These options configure Azure OpenAI API:

| Option | Description |
|--------|-------------|
| [`--azure-openai-api-key`](azure-openai-api-key.md) | Use a specific authentication key |
| [`--azure-openai-endpoint`](azure-openai-endpoint.md) | Use a specific API endpoint |
| [`--azure-openai-chat-deployment`](azure-openai-chat-deployment.md) | Use a specific chat model deployment |

### Copilot Options

These options configure GitHub Copilot API:

| Option | Description |
|--------|-------------|
| [`--copilot-model-name`](copilot-model-name.md) | Use a specific model by name |
| [`--copilot-api-endpoint`](copilot-api-endpoint.md) | Use a specific API endpoint |
| [`--github-token`](github-token.md) | Use a specific GitHub authentication token |

### OpenAI Options

These options configure OpenAI API:

| Option | Description |
|--------|-------------|
| [`--openai-api-key`](openai-api-key.md) | Use a specific API key |
| [`--openai-chat-model-name`](openai-chat-model-name.md) | Use a specific chat model |

### Configuration and Profile Options

These options manage configurations and profiles:

| Option | Description |
|--------|-------------|
| [`--config`](config.md) | Load configuration from YAML or INI files |
| [`--profile`](profile.md) | Load a specific profile's configuration |

### Alias Options

These options manage command aliases:

| Option | Description |
|--------|-------------|
| [`--save-alias`](save-alias.md) | Same as `--save-local-alias` |
| [`--save-local-alias`](save-local-alias.md) | Save current options as an alias in local scope |
| [`--save-user-alias`](save-user-alias.md) | Save current options as an alias in user scope |
| [`--save-global-alias`](save-global-alias.md) | Save current options as an alias in global scope |

### Variable Options

These options manage template variables:

| Option | Description |
|--------|-------------|
| [`--var`](var.md) | Set a variable for template substitution |
| [`--vars`](vars.md) | Set multiple variables for template substitution |
| [`--foreach`](foreach.md) | Define a loop variable with multiple values |

### Command Execution Options

These options control execution behavior:

| Option | Description |
|--------|-------------|
| [`--command`](command.md) | Execute a command with the AI response |
| [`--working-dir`](working-dir.md) | Set the working directory for command execution |
| [`--url`](url.md) | Open a URL with the AI response |
| [`--env`](env.md) | Set environment variables for command execution |
| [`--arg`](arg.md) | Set command arguments for MCP servers |

### Debugging and Output Options

These options control logging and console output:

| Option | Description |
|--------|-------------|
| [`--interactive`](interactive.md) | Allow interactive use |
| [`--threads`](threads.md) | Number of parallel threads for non-interactive mode |
| [`--debug`](debug.md) | Turn on diagnostics/debug outputs |
| [`--quiet`](quiet.md) | Turn off all but the most critical console outputs |
| [`--verbose`](verbose.md) | Turn on additional diagnostics/debug outputs |

### Scope Options

These options control the scope for various commands:

| Option | Description |
|--------|-------------|
| [`--global`](global.md) | Use global scope (all users) |
| [`--user`](user.md) | Use user scope (current user) |
| [`--local`](local.md) | Use local scope (current directory) |
| [`--any`](any.md) | Include items from all scopes |

## Common Usage Patterns

Many options can be combined to create powerful command patterns:

```bash
# Using a specific model with custom prompt
chatx --use-openai --openai-chat-model-name gpt-4 --add-system-prompt "You are a helpful assistant"

# Running in non-interactive mode with multiple questions
chatx --questions "What is AI?" "How does it work?" --output-trajectory answers.md

# Using variables in templates
chatx --var name=John --var age=30 --question "Generate a bio for {name}, who is {age} years old"

# Continuing a previous conversation
chatx --continue --question "Explain more about the last topic"
```

For more examples and usage patterns, see the [Chat Basics](../../../basics/chat.md) section.