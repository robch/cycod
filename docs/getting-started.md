# Getting Started with CycoD

This guide will help you get up and running with CycoD quickly.

## Prerequisites

Before you begin, ensure you have:

- **.NET 8.0 SDK** or later installed
- An **OpenAI API key** if you plan to use OpenAI's models
- An **Azure OpenAI API key** if you plan to use Azure OpenAI
- A **GitHub token** if you plan to use GitHub Copilot

## Installation

### Installing as a .NET Tool

The easiest way to install CycoD is as a .NET global tool:

```bash
dotnet tool install --global CycoD --prerelease
```

This will make the `cycod` command available in your terminal. If you prefer a local installation that's only available in the current directory:

```bash
dotnet tool install --local CycoD --prerelease
```

Note: Local tools require you to run them through the .NET CLI: `dotnet cycod`

### Building from Source

1. Clone the repository:
   ```bash
   git clone https://github.com/robch/cycod.git
   cd cycod
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

CycoD uses environment variables to configure API access. You can set these in your shell or add them to a `.cycod/config` file in your home or project directory.

### Configuration File

Create a `.cycod/config` file with your variables:

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
cycod github login
```

This will guide you through the GitHub device flow authentication process and save the token to your `.cycod/config` file.

Alternatively, you can manually set the following environment variables:

```bash
# Windows
set GITHUB_TOKEN=your_github_token_here
set COPILOT_INTEGRATION_ID=your_integration_id_here
set COPILOT_API_ENDPOINT=https://api.githubcopilot.com
set COPILOT_MODEL_NAME=claude-sonnet-4

# Linux/macOS
export GITHUB_TOKEN=your_github_token_here
export COPILOT_INTEGRATION_ID=your_integration_id_here
export COPILOT_API_ENDPOINT=https://api.githubcopilot.com
export COPILOT_MODEL_NAME=claude-sonnet-4
```


## Your First Chat

Start a basic conversation with:

```bash
cycod
```

This will start an interactive session where you can talk with the AI assistant.

### Asking a Direct Question

To ask a single question without entering interactive mode:

```bash
cycod --input "What is the capital of France?"
```

### Using a Custom System Prompt

You can customize the AI's behavior with a system prompt:

```bash
cycod --system-prompt "You are an expert programmer who gives concise code examples."
```

### Working with Files

CycoD can help you work with files and code:

```bash
cycod --input "Can you help me understand the file at src/Program.cs?"
```

The AI will analyze the file and provide explanations or suggestions.

### Executing Shell Commands

The AI can execute shell commands to help with tasks:

```bash
cycod --input "How much disk space do I have left?"
```

The AI might use `RunBashCommand` to execute `df -h` and explain the results.

## Using Speech Recognition

CycoD supports speech-to-text input using Azure Cognitive Services. This allows you to speak your prompts instead of typing them.

### Quick Start

1. Set up Azure Speech Service credentials (see [Speech Setup Guide](speech-setup.md) for details)
2. Start cycod with the `--speech` flag:

```bash
cycod --speech
```

3. Press ENTER on an empty line and select "Speech input" from the menu
4. Speak your prompt clearly into the microphone
5. The recognized text will appear and be sent to the AI

### Example Session

```bash
$ cycod --speech

You> [Press Enter]
  Continue chatting
  Speech input       ← Select this
  Reset conversation
  Exit

(listening)...
Interim: "tell me about"
Interim: "tell me about python"

You> tell me about python

AI> Python is a high-level, interpreted programming language...
```

For complete setup instructions, see the [Speech Setup Guide](speech-setup.md).

## Managing Chat History

To save your conversation for later reference:

```bash
cycod --output-chat-history "my-chat.jsonl"
```

To continue a previous conversation:

```bash
cycod --input-chat-history "my-chat.jsonl"
```

## Creating and Using Aliases

If you frequently use the same options, you can create an alias:

```bash
cycod --system-prompt "You are an expert Python programmer." --save-alias python-expert
```

Then use it later:

```bash
cycod --python-expert --input "How do I work with async functions in Python?"
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