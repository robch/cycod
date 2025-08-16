# CycoD - AI-powered CLI

CycoD is a command-line interface (CLI) application that provides a chat-based interaction with AI assistants. Built in C#, it leverages AI chat models from multiple providers with function calling capabilities to create a powerful tool for AI-assisted command-line operations.

## Features

- **Interactive AI Chat**: Have conversations with an AI assistant directly in your terminal
- **Image Input Support**: Include images in conversations for AI analysis and vision tasks
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
- **Chat Commands**: Special commands like `/clear`, `/save`, `/cost` during chat sessions
  - CYCODMD integration commands: `/file`, `/files`, `/find`, `/search`, `/get`, `/run`
  - Image commands: `/image` to add images to conversations

## Installation

### Prerequisites

- .NET 8.0 SDK or later

### Installing as a .NET Tool

Once published to NuGet, CycoD can be installed globally:

```bash
dotnet tool install --global CycoD --prerelease
```

Or locally in your current directory:

```bash
dotnet tool install --local CycoD --prerelease
```

After installation, you can run CycoD directly from your terminal:

```bash
cycod --input "Hello, how can you help me?"
```

### Building from Source

1. Clone this repository:
   ```
   git clone https://github.com/robch/cycod.git
   cd cycod
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
cycod [options]
```

### Common Options

- `--system-prompt <prompt>`: Set a custom system prompt for the AI
- `--input <text>` or `--question <text>`: Provide input or questions to the AI
- `--inputs <text...>` or `--questions <text...>`: Provide multiple inputs to process sequentially
- `--input-chat-history <file>`: Load previous chat history from a file
- `--output-chat-history <file>`: Save chat history to a file
- `--max-chat-tokens <n>`: Set a target for trimming chat history when it gets too large
- `--interactive`: Control whether to enter interactive mode (default: true)
- `--save-alias <name>`: Save the current command options as a named alias
- `--foreach var <name> in <values>`: Define a loop variable with multiple values
- `--foreach var <name> in <start>..<end>`: Define a loop variable with a numeric range
- `--help`: Display help information

### Examples

Start a chat session:
```
cycod
```

Ask a specific question:
```
cycod --input "How do I find the largest files in a directory using bash?"
```

Use a custom system prompt:
```
cycod --system-prompt "You are an expert Linux system administrator who gives concise answers."
```

Analyze images with AI:
```
cycod --image screenshot.png --input "What's shown in this screenshot?"
cycod --image "*.jpg" "diagrams/*.png" --input "Analyze these images"
```

Save and load chat history:
```
cycod --output-chat-history "linux-help-session.jsonl"
cycod --input-chat-history "linux-help-session.jsonl"
```

## Environment Variables and Configuration

CycoD supports a flexible configuration system with multiple scopes (global, user, local) and formats (YAML, INI). You can:

1. Use environment variables
2. Use configuration files at:
   - Global: `%ProgramData%\.cycod\config` (Windows) or `/etc/.cycod/config` (Linux/macOS)
   - User: `%AppData%\.cycod\config` (Windows) or `~/.cycod/config` (Linux/macOS)
   - Local: `.cycod\config` in the current directory

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

#### App Configuration
- `CYCOD_PREFERRED_PROVIDER`: Set default AI provider (openai, azure-openai, copilot)

### Configuration Profiles

You can create named profiles to store different provider configurations:

1. Create YAML files at `.cycod/profiles/<name>.yaml`
2. Use `--profile <name>` to load that profile

Example profile at `.cycod/profiles/work.yaml`:
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
cycod config list              # List all settings in current scope
cycod config list --any        # List all settings from all scopes
cycod config get KEY           # Get a configuration value
cycod config set KEY VALUE     # Set a configuration value
cycod config clear KEY         # Clear a configuration value
```

## Documentation

For more detailed documentation, see:
- [Getting Started](docs/getting-started.md)
- [Command Line Options](docs/cli-options.md)
- [Function Calling](docs/function-calling.md)
- [Creating Aliases](docs/aliases.md)
- [Chat History](docs/chat-history.md)
- [Slash Commands](#chat-commands) - Special commands during chat sessions

## License

Copyright(c) 2025, Rob Chambers. All rights reserved.

## Chat Commands

CycoD supports several slash commands that can be used during interactive chat sessions:

### Basic Commands
- `/clear` - Clear the current chat history
- `/save` - Save the current chat history to a file
- `/cost` - Show token usage and estimated cost of the session

### CYCODMD Integration Commands
- `/file <pattern>` - Search files matching pattern
- `/files <pattern>` - List files matching pattern
- `/find <pattern>` - Find occurrences of pattern in files
- `/search <query>` - Search the web for the given query
- `/get <url>` - Get and display content from a URL
- `/run <command>` - Run a command and display the result

These CYCODMD integration commands require the [CYCODMD](https://github.com/robch/cycodmd) tool to be installed and available on your system.

### Image Commands
- `/image <pattern>` - Add image files matching the pattern to the conversation

Examples:
```
/image screenshot.png
/image *.jpg
/image photos/*.png docs/diagram.svg
```

Supported image formats: JPEG, PNG, GIF, BMP, WebP, SVG, TIFF. The image command supports glob patterns for selecting multiple files.