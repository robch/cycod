# CYCODEV CLI Public API Documentation

## Overview

CYCODEV (CycoD) is an AI-powered command-line interface that enables natural language interactions with AI models directly from the terminal. This document outlines the complete public API for implementing a Node.js/TypeScript version of the CLI.

## Core Command Structure

### Basic Invocation
```bash
cycod [options] [command]
```

## Primary CLI Options

### Input Options
- `--input <text>` - Single input line or question
- `--inputs <text1> <text2> ...` - Chain multiple questions/requests
- `--question <text>` or `-q <text>` - Shortcut for non-interactive, quiet mode
- `--interactive <boolean>` - Enable/disable interactive mode (default: true)
- `--quiet` - Suppress verbose output

### Chat History Management
- `--output-chat-history [filename]` - Save conversation (default: `chat-history-{timestamp}.jsonl`)
- `--input-chat-history <filename>` - Load previous conversation
- `--chat-history <filename>` - Use same filename for input and output
- `--continue` - Resume most recent chat history

### System Prompts
- `--system-prompt <text|@file>` - Completely replace default system behavior
- `--add-system-prompt <text|@file>` - Augment existing default system prompt

### Variables
- `--var <name>=<value>` - Define a single variable
- `--vars <name1>=<value1> <name2>=<value2> ...` - Define multiple variables

### Parallel Processing
- `--foreach var <name> in <value1> <value2> ...` - Run command with different variable values in parallel
- `--foreach var <name> in <start>..<end>` - Numeric range iteration
- `--threads <number>` - Specify maximum parallel processes

### Configuration
- `--profile <name>` - Load specific configuration profile

## Chat History File Format

Chat history is stored in JSONL format:
```jsonl
{"role":"system","content":"You are a helpful assistant."}
{"role":"user","content":"What is the capital of France?"}
{"role":"assistant","content":"The capital of France is Paris."}
```

## Configuration Management Commands

### Configuration Commands
```bash
cycod config list                    # Show all settings
cycod config get [setting]           # Retrieve specific setting
cycod config set [setting] [value]   # Configure a setting
cycod config clear [setting]         # Remove a setting
cycod config add [list-setting] [value]    # Add to list setting
cycod config remove [list-setting] [value] # Remove from list setting
```

### Configuration Scopes
1. **Local** - Directory-specific (`.cycod/` in current directory)
2. **User** - User-specific (`~/.cycod/` or `%USERPROFILE%\.cycod\`)
3. **Global** - System-wide

Configuration precedence: Local > User > Global

## Custom Prompts Management

### Prompt Commands
```bash
cycod prompt create <name> "<prompt text>"    # Create prompt
cycod prompt create <name> @<filename>        # Create from file
cycod prompt list                             # List all prompts
cycod prompt show <name>                      # Display specific prompt
cycod prompt delete <name>                    # Delete prompt
```

### Prompt Usage
- `--prompt <name>` - Use custom prompt
- Support for variable substitution in prompts using `{variable_name}` syntax

### Prompt Storage Locations
- **Windows**: `%USERPROFILE%\.cycod\prompts\`
- **macOS/Linux**: `~/.cycod/prompts/`

## Interactive Slash Commands

Available in interactive mode (`cycod` without arguments):

### Basic Commands
- `/save [filename]` - Save chat history
- `/clear` - Clear current chat history
- `/help` - Show available commands

### File Operations
- `/file <filename>` - Retrieve file contents
- `/files <pattern>` - List files matching pattern
- `/find <pattern>` - Search for pattern in files

### Command Execution
- `/run <command>` - Execute shell/system commands

### Web Operations
- `/search <query>` - Perform web searches
- `/get <url>` - Retrieve content from URL

### Custom Prompts
- `/prompts` - List available custom prompt templates
- `/<prompt-name>` - Use specific custom prompt

## Built-in Variables

### Date/Time Variables
- `{date}` - Current date (yyyy-MM-dd)
- `{time}` - Current time
- `{year}`, `{month}`, `{day}` - Date components

### System Information Variables
- `{os}` - Full OS information
- `{osname}` - OS platform name
- `{osversion}` - OS version

## Input Methods

### Direct Input
```bash
cycod --input "Your question here"
```

### File Input
```bash
cycod --input @filename.txt
cycod --system-prompt @prompt-file.txt
```

### Stdin Input
```bash
echo "Your question" | cycod --input @-
```

### Multiple File Input
```bash
cycod --add-system-prompt @file1.txt @file2.txt
```

## Configuration Categories

### Application Settings
- Provider selection
- Model preferences
- Default behaviors

### Provider-Specific Settings
- **GitHub Copilot Settings**
- **OpenAI Settings**  
- **Azure OpenAI Settings**
- API keys and endpoints

### Chat History Settings
- Default save locations
- History retention policies

## Advanced Features

### Conversation Branching
- Create multiple conversation paths from base conversations
- Support for trajectory output for human-readable review

### Alias Creation
```bash
cycod --var language=Python --add-system-prompt @template.txt --save-alias py
```

### Profile Management
- Store configuration sets in `.cycod/profiles/`
- Load with `--profile <name>` option

## Node.js/TypeScript Implementation Notes

### Core Architecture Requirements

1. **Command Parser**
   - Support for complex option parsing with multiple input methods
   - Handle file input (`@filename`) and stdin (`@-`) syntax
   - Variable substitution engine for `{variable}` syntax

2. **Chat History Management**
   - JSONL file format support
   - Timestamp-based filename generation
   - Conversation continuation logic
   - Branching support

3. **Configuration System**
   - Three-tier configuration (local/user/global)
   - YAML configuration file support
   - Environment variable integration
   - Profile management

4. **Interactive Shell**
   - REPL-style interface for slash commands
   - Command completion and help system
   - File system integration for `/file`, `/files`, `/find`
   - Web request capabilities for `/search`, `/get`

5. **Parallel Processing**
   - Thread pool management for `--foreach`
   - Variable iteration (ranges and lists)
   - Concurrent AI model requests

6. **Provider Integration**
   - Abstract provider interface
   - Support for multiple AI providers (OpenAI, Azure, GitHub Copilot)
   - Configurable API endpoints and authentication

7. **Variable System**
   - Built-in system variables (date, time, OS info)
   - User-defined variables
   - Variable substitution in templates

8. **Prompt Management**
   - Template storage and retrieval
   - Variable substitution in prompts
   - System vs. user prompt handling

### Key TypeScript Interfaces

```typescript
interface ChatMessage {
  role: 'system' | 'user' | 'assistant';
  content: string;
}

interface CommandOptions {
  input?: string | string[];
  inputs?: string[];
  interactive?: boolean;
  quiet?: boolean;
  systemPrompt?: string;
  addSystemPrompt?: string;
  outputChatHistory?: string;
  inputChatHistory?: string;
  chatHistory?: string;
  continue?: boolean;
  vars?: Record<string, string>;
  foreach?: {
    variable: string;
    values: string[];
  };
  threads?: number;
  profile?: string;
}

interface ConfigurationScope {
  local: Record<string, any>;
  user: Record<string, any>;
  global: Record<string, any>;
}
```

### File System Structure
```
~/.cycod/                    # User configuration directory
├── config.yaml             # User configuration
├── prompts/                 # Custom prompts
│   ├── prompt1.txt
│   └── prompt2.txt
├── profiles/                # Configuration profiles
│   ├── profile1.yaml
│   └── profile2.yaml
└── history/                 # Chat history files
    ├── chat-history-123.jsonl
    └── chat-history-456.jsonl

./.cycod/                    # Local configuration directory
└── config.yaml             # Local overrides
```

### Essential Dependencies for Node.js Implementation
- Command line parsing (e.g., `commander.js` or `yargs`)
- YAML parsing (`js-yaml`)
- File system utilities (`fs-extra`)
- HTTP client for web operations (`axios` or `node-fetch`)
- Interactive shell (`inquirer` or `readline`)
- Process management for parallel execution
- AI provider SDKs (OpenAI, Azure OpenAI, etc.)

### Error Handling Requirements
- Graceful handling of network failures
- Configuration validation
- File system permission errors
- AI provider rate limiting and errors
- Parallel processing error aggregation

This API documentation provides the complete foundation needed to implement a Node.js/TypeScript version of CYCODEV with full feature parity.