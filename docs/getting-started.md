# Getting Started with ChatX

This guide will help you get up and running with ChatX quickly.

## Prerequisites

Before you begin, ensure you have:

- **.NET 6.0 SDK** or later installed
- An **OpenAI API key** if you plan to use OpenAI's models
- An **Azure OpenAI API key** if you plan to use Azure OpenAI
- A **GitHub token** or **Copilot HMAC key** if you plan to use GitHub Copilot

## Installation

### Building from Source

1. Clone the repository:
   ```bash
   git clone https://github.com/username/chatx.git
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

ChatX uses environment variables to configure API access. Set up the following based on which AI service you want to use:

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

```bash
# Windows
set GITHUB_TOKEN=your_github_token_here
set COPILOT_API_ENDPOINT=https://api.githubcopilot.com
set COPILOT_MODEL_NAME=claude-3.7-sonnet

# Linux/macOS
export GITHUB_TOKEN=your_github_token_here
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

## Next Steps

- Learn about [Command Line Options](cli-options.md)
- Explore [Function Calling](function-calling.md) capabilities
- See how to create [Aliases](aliases.md) for common commands