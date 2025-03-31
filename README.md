# ChatX - AI-powered CLI

ChatX is a command-line interface (CLI) application that provides a chat-based interaction with AI assistants. Built in C#, it leverages AI chat models from multiple providers with function calling capabilities to create a powerful tool for AI-assisted command-line operations.

## Features

- **Interactive AI Chat**: Have conversations with an AI assistant directly in your terminal
- **Multiple AI Providers**: Support for OpenAI, Azure OpenAI, and GitHub Copilot APIs
- **Provider Selection**: Easily switch between different AI providers or use profiles
- **Function Calling**: Allow the AI assistant to execute various operations:
  - Run shell commands (Bash, CMD, PowerShell) with persistent sessions
  - Manipulate files (view, create, edit, replace text)
  - Access date and time information
- **Comprehensive Configuration System**: 
  - Multiple configuration scopes (global, user, local)
  - Profile support for different configurations
  - Full configuration CLI with get, set, list, clear, add, remove commands
- **Customizable Experience**: Configure the AI's behavior with system prompts and other options
- **Chat History**: Load and save chat histories for later reference
- **Command Aliases**: Create shortcuts for frequently used command configurations
- **Token Management**: Automatically manages token usage for long conversations
- **Chat Commands**: Special commands like `/clear` during chat sessions

## Installation

### Prerequisites

- .NET 8.0 SDK or later

### Installing as a .NET Tool

Once published to NuGet, ChatX can be installed globally:

```bash
dotnet tool install --global ChatX
```

Or locally in your current directory:

```bash
dotnet tool install --local ChatX
```

After installation, you can run ChatX directly from your terminal:

```bash
chatx --input "Hello, how can you help me?"
```

### Building from Source

1. Clone this repository:
   ```
   git clone https://github.com/robch/chatx.git
   cd chatx
   ```

2. Build the project:
   ```
   dotnet build
   ```

3. Run the application:
   ```
   dotnet run
   ```

## Usage

Basic usage:

```
chatx [options]
```

### Common Options

- `--system-prompt <prompt>`: Set a custom system prompt for the AI
- `--input <text>` or `--question <text>`: Provide input or questions to the AI
- `--inputs <text...>` or `--questions <text...>`: Provide multiple inputs to process sequentially
- `--input-chat-history <file>`: Load previous chat history from a file
- `--output-chat-history <file>`: Save chat history to a file
- `--trim-token-target <n>`: Set a target for trimming chat history when it gets too large
- `--interactive`: Control whether to enter interactive mode (default: true)
- `--save-alias <name>`: Save the current command options as a named alias
- `--help`: Display help information

### Examples

Start a chat session:
```
chatx
```

Ask a specific question:
```
chatx --input "How do I find the largest files in a directory using bash?"
```

Use a custom system prompt:
```
chatx --system-prompt "You are an expert Linux system administrator who gives concise answers."
```

Save and load chat history:
```
chatx --output-chat-history "linux-help-session.jsonl"
chatx --input-chat-history "linux-help-session.jsonl"
```

## Environment Variables and Configuration

ChatX supports a flexible configuration system with multiple scopes (global, user, local) and formats (YAML, INI). You can:

1. Use environment variables
2. Use configuration files at:
   - Global: `%ProgramData%\.chatx\config` (Windows) or `/etc/.chatx/config` (Linux/macOS)
   - User: `%AppData%\.chatx\config` (Windows) or `~/.chatx/config` (Linux/macOS)
   - Local: `.chatx\config` in the current directory

### Common Configuration Settings

#### OpenAI API
- `OPENAI_API_KEY`: Your OpenAI API key
- `OPENAI_CHAT_MODEL_NAME`: Model name to use (default: gpt-4o)

#### Azure OpenAI API
- `AZURE_OPENAI_API_KEY`: Your Azure OpenAI API key
- `AZURE_OPENAI_ENDPOINT`: Your Azure OpenAI endpoint
- `AZURE_OPENAI_CHAT_DEPLOYMENT`: Your Azure OpenAI deployment name

#### GitHub Copilot API
- `GITHUB_TOKEN`: Your GitHub personal access token for Copilot API (preferred method)
- `COPILOT_API_ENDPOINT`: Copilot API endpoint (default: https://api.githubcopilot.com)
- `COPILOT_MODEL_NAME`: Model name to use (default: claude-3.7-sonnet)
- `COPILOT_INTEGRATION_ID`: Your Copilot integration ID (required for both auth methods)

*Alternative HMAC authentication:*
- `COPILOT_HMAC_KEY`: Your Copilot HMAC key

#### App Configuration
- `CHATX_PREFERRED_PROVIDER`: Set default AI provider (openai, azure-openai, copilot, copilot-hmac)

### Configuration Profiles

You can create named profiles to store different provider configurations:

1. Create YAML files at `.chatx/profiles/<name>.yaml`
2. Use `--profile <name>` to load that profile

Example profile at `.chatx/profiles/work.yaml`:
```yaml
app:
  preferredProvider: "azure-openai"

azure:
  openai:
    endpoint: "https://my-work-endpoint.openai.azure.com"
    chatDeployment: "gpt-4"
```

### Configuration Management

Manage configurations from the command line:

```bash
chatx config list              # List all settings in current scope
chatx config list --any        # List all settings from all scopes
chatx config get KEY           # Get a configuration value
chatx config set KEY VALUE     # Set a configuration value
chatx config clear KEY         # Clear a configuration value
```

## Documentation

For more detailed documentation, see:
- [Getting Started](docs/getting-started.md)
- [Command Line Options](docs/cli-options.md)
- [Function Calling](docs/function-calling.md)
- [Creating Aliases](docs/aliases.md)
- [Chat History](docs/chat-history.md)

## License

Copyright(c) 2025, Rob Chambers. All rights reserved.