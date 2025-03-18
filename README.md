# ChatX - AI-powered CLI

ChatX is a command-line interface (CLI) application that provides a chat-based interaction with AI assistants. Built in C#, it leverages OpenAI's chat completion APIs with function calling capabilities to create a powerful tool for AI-assisted command-line operations.

## Features

- **Interactive AI Chat**: Have conversations with an AI assistant directly in your terminal
- **Function Calling**: Allow the AI assistant to execute various operations:
  - Run shell commands (Bash, CMD, PowerShell)
  - Manipulate files (view, create, edit)
  - Access date and time information
- **Customizable Experience**: Configure the AI's behavior with system prompts and other options
- **Chat History**: Load and save chat histories for later reference
- **Command Aliases**: Create shortcuts for frequently used command configurations

## Installation

### Prerequisites

- .NET 6.0 SDK or later

### Building from Source

1. Clone this repository:
   ```
   git clone https://github.com/username/chatx.git
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
- `--input-chat-history <file>`: Load previous chat history from a file
- `--output-chat-history <file>`: Save chat history to a file
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

## Environment Variables

The application uses the following environment variables:

- `OPENAI_API_KEY`: Your OpenAI API key
- `OPENAI_SYSTEM_PROMPT`: Default system prompt if not specified

## License

Copyright(c) 2025, Rob Chambers. All rights reserved.