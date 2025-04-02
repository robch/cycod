# Getting Started with ChatX

This guide will help you get up and running with ChatX quickly.

## Prerequisites

Before you begin, ensure you have:

- **.NET 8.0 SDK** or later installed
- An **OpenAI API key** if you plan to use OpenAI's models
- An **Azure OpenAI API key** if you plan to use Azure OpenAI
- A **GitHub token** or **Copilot HMAC key** if you plan to use GitHub Copilot

## Installation

### Installing as a .NET Tool

The easiest way to install ChatX is as a .NET global tool:

```bash
dotnet tool install --global ChatX --prerelease
```

This will make the `chatx` command available in your terminal. If you prefer a local installation that's only available in the current directory:

```bash
dotnet tool install --local ChatX --prerelease
```

Note: Local tools require you to run them through the .NET CLI: `dotnet chatx`

### Building from Source

1. Clone the repository:
   ```bash
   git clone https://github.com/robch/chatx.git
   cd chatx
   ```

2. Build the project:
   ```bash
   dotnet build
   ```

3. Run the application:
   ```bash
   dotnet run
   ```

## Setting Up Environment Variables

ChatX uses environment variables to configure API access. You can set these in your shell or add them to a `.chatx/config` file in your home or project directory.

### Configuration File

Create a `.chatx/config` file with your variables:

```
OPENAI_API_KEY=your_api_key_here
OPENAI_CHAT_MODEL_NAME=gpt-4o
```

### OpenAI API

```bash
# Windows
set OPENAI_API_KEY=your_api_key_here
set OPENAI_CHAT_MODEL_NAME=gpt-4o

# Linux/macOS
export OPENAI_API_KEY=your_api_key_here
export OPENAI_CHAT_MODEL_NAME=gpt-4o
```

### Azure OpenAI API

```bash
# Windows
set AZURE_OPENAI_API_KEY=your_api_key_here
set AZURE_OPENAI_ENDPOINT=your_endpoint_here
set AZURE_OPENAI_CHAT_DEPLOYMENT=your_deployment_name

# Linux/macOS
export AZURE_OPENAI_API_KEY=your_api_key_here
export AZURE_OPENAI_ENDPOINT=your_endpoint_here
export AZURE_OPENAI_CHAT_DEPLOYMENT=your_deployment_name
```

### GitHub Copilot API

GitHub Copilot API can be accessed using two authentication methods:

#### GitHub Token Authentication (Recommended)

The easiest way to authenticate with GitHub Copilot is to use the built-in login command:

```bash
chatx github login
```

This will guide you through the GitHub device flow authentication process and save the token to your `.chatx/config` file.

Alternatively, you can manually set the following environment variables:

```bash
# Windows
set GITHUB_TOKEN=your_github_token_here
set COPILOT_INTEGRATION_ID=your_integration_id_here
set COPILOT_API_ENDPOINT=https://api.githubcopilot.com
set COPILOT_MODEL_NAME=claude-3.7-sonnet

# Linux/macOS
export GITHUB_TOKEN=your_github_token_here
export COPILOT_INTEGRATION_ID=your_integration_id_here
export COPILOT_API_ENDPOINT=https://api.githubcopilot.com
export COPILOT_MODEL_NAME=claude-3.7-sonnet
```

#### HMAC Authentication

```bash
# Windows
set COPILOT_HMAC_KEY=your_hmac_key_here
set COPILOT_INTEGRATION_ID=your_integration_id_here
set COPILOT_API_ENDPOINT=https://api.githubcopilot.com
set COPILOT_MODEL_NAME=claude-3.7-sonnet

# Linux/macOS
export COPILOT_HMAC_KEY=your_hmac_key_here
export COPILOT_INTEGRATION_ID=your_integration_id_here
export COPILOT_API_ENDPOINT=https://api.githubcopilot.com
export COPILOT_MODEL_NAME=claude-3.7-sonnet
```

## Your First Chat

Start a basic conversation with:

```bash
chatx
```

This will start an interactive session where you can talk with the AI assistant.

### Asking a Direct Question

To ask a single question without entering interactive mode:

```bash
chatx --input "What is the capital of France?"
```

### Using a Custom System Prompt

You can customize the AI's behavior with a system prompt:

```bash
chatx --system-prompt "You are an expert programmer who gives concise code examples."
```

### Working with Files

ChatX can help you work with files and code:

```bash
chatx --input "Can you help me understand the file at src/Program.cs?"
```

The AI will analyze the file and provide explanations or suggestions.

### Executing Shell Commands

The AI can execute shell commands to help with tasks:

```bash
chatx --input "How much disk space do I have left?"
```

The AI might use `RunBashCommandAsync` to execute `df -h` and explain the results.

## Managing Chat History

To save your conversation for later reference:

```bash
chatx --output-chat-history "my-chat.jsonl"
```

To continue a previous conversation:

```bash
chatx --input-chat-history "my-chat.jsonl"
```

## Creating and Using Aliases

If you frequently use the same options, you can create an alias:

```bash
chatx --system-prompt "You are an expert Python programmer." --save-alias python-expert
```

Then use it later:

```bash
chatx --python-expert --input "How do I work with async functions in Python?"
```

## Troubleshooting

If you encounter issues:

1. Use the `--debug` flag to see detailed logs
2. Ensure your API keys and environment variables are correct
3. Check that you're using .NET 8.0 SDK or later
4. See if you're hitting rate limits on the APIs

## Next Steps

- Learn about [Command Line Options](cli-options.md)
- Explore [Function Calling](function-calling.md) capabilities
- See how to create [Aliases](aliases.md) for common commands
- Learn how to manage [Chat History](chat-history.md)