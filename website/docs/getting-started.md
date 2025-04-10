# Getting Started with ChatX

ChatX is an AI-powered CLI tool that makes it easy to interact with large language models directly from your terminal. This guide will help you install ChatX and get started with its basic features.

## Installation

### Prerequisites

- .NET 8.0 SDK or later

### Installing as a .NET Tool

You can install ChatX globally:

```bash
dotnet tool install --global ChatX --prerelease
```

Or locally in your current directory:

```bash
dotnet tool install --local ChatX --prerelease
```

### Building from Source

Alternatively, you can build ChatX from source:

1. Clone the repository:
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

## Verifying Installation

After installation, you can verify that ChatX is installed correctly by running:

```bash
chatx --version
```

You should see output showing the installed version:

```
ChatX - AI-powered CLI, Version 1.0.0
Copyright(c) 2025, Rob Chambers. All rights reserved.
```

## Setting Up AI Providers

CHATX supports multiple AI providers. You'll need to configure at least one provider before you can start using CHATX.

### OpenAI API

To use the OpenAI API with CHATX, you need an API key:

1. Get an API key from [OpenAI](https://platform.openai.com/api-keys).
2. Set up your OpenAI API key in CHATX:

```bash
chatx config set openai.apiKey YOUR_API_KEY --user
```

### Azure OpenAI API

To use the Azure OpenAI API with CHATX:

1. Create an Azure OpenAI resource in the [Azure Portal](https://portal.azure.com/).
2. Set up your Azure OpenAI credentials in CHATX:

```bash
chatx config set azure.openai.endpoint YOUR_ENDPOINT --user
chatx config set azure.openai.apiKey YOUR_API_KEY --user
chatx config set azure.openai.chatDeployment YOUR_DEPLOYMENT_NAME --user
```

### GitHub Copilot

To use GitHub Copilot with CHATX:

1. Ensure you have a GitHub account with Copilot subscription.
2. Authenticate with GitHub:

```bash
chatx github login
```

## Basic Usage

Now that you've installed CHATX and configured an AI provider, you can start using it. Here are some basic examples:

```bash
# Ask a simple question
chatx --question "What time is it?"

# Start an interactive chat session
chatx --interactive

# Save chat history and continue later
chatx --question "Tell me about AI" --output-chat-history chat.jsonl
chatx --input-chat-history chat.jsonl --question "Tell me more"

# Use a specific provider
chatx --use-openai --question "What is GPT-4?"
chatx --use-azure-openai --question "What are Azure OpenAI models?"
chatx --use-copilot --question "Explain GitHub Copilot"
```

For more detailed information on using CHATX, check out the [Chat Basics](/usage/basics.md) guide.