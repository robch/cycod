# Getting Started with ChatX

This guide will help you get up and running with ChatX quickly.

## Prerequisites

Before you begin, ensure you have:

- **.NET 6.0 SDK** or later installed
- An **OpenAI API key** if you plan to use OpenAI's models

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

ChatX uses environment variables to configure API access. Set up the following:

### OpenAI API

```bash
# Windows
set OPENAI_API_KEY=your_api_key_here

# Linux/macOS
export OPENAI_API_KEY=your_api_key_here
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