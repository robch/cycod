# Getting Started with CHATX

CHATX is an AI-powered CLI tool that makes it easy to interact with large language models directly from your terminal. This guide will help you install CHATX and get started with its basic features.

## Installation

### Windows

You can install CHATX on Windows using one of the following methods:

#### Using winget (recommended)

```powershell
winget install RobChambers.CHATX
```

#### Manual Installation

1. Download the latest release from the [GitHub Releases page](https://github.com/robch/chatx/releases).
2. Extract the ZIP file to a directory of your choice.
3. Add the directory to your PATH environment variable.

### macOS

You can install CHATX on macOS using Homebrew:

```bash
brew install robch/tap/chatx
```

### Linux

You can install CHATX on Linux using the following commands:

```bash
curl -fsSL https://raw.githubusercontent.com/robch/chatx/main/install.sh | bash
```

## Verifying Installation

After installation, you can verify that CHATX is installed correctly by running:

```bash
chatx --version
```

You should see output similar to:

```
CHATX - AI-powered CLI, Version 1.0.0
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