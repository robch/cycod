---
hide:
- toc
icon: material/information-outline
---

--8<-- "snippets/ai-generated.md"

# CycoD Overview

CycoD is a powerful AI-powered CLI tool that enables natural language interactions with AI models directly from your terminal. By integrating seamlessly with your development environment, CycoD streamlines your workflow and enhances productivity without requiring complex setups or additional servers.

CycoD supports multiple AI providers, offers rich file and shell integration capabilities, and provides an extensible framework for custom workflows and tools.

CycoD operates directly in your terminal, understanding your inputs and performing real actions. It leverages the power of large language models to help you accomplish a wide range of tasks.

### From Questions to Solutions

```bash
# Ask a simple question
cycod --question "What time is it?"

# Process multiple inputs sequentially
cycod --inputs "What's today's date?" "Show me a calendar for this month"

# Continue a previous conversation
cycod --continue --question "What about next month?"

# Use expert roles
cycod --python-expert --question "How do I handle file I/O in Python?"
```

### File Operations

CycoD can interact with your local files, making it easy to analyze, create, and modify content:

```bash
# Analyze code in your project
cycod --instruction "Look at the files in this directory and explain what they do"

# Generate new files
cycod --instruction "Create a simple React component for a user profile"

# Transform content
cycod --instruction "Read data.json and convert it to a CSV format"
```

### Shell Command Integration

Execute and process commands directly from your CycoD session:

```bash
# Run system commands and analyze results
cycod --instruction "Show me the largest files in this directory"

# Perform complex operations
cycod --instruction "Find all TODO comments in the src directory"

# Automate workflows
cycod --instruction "Run the tests and explain any failures"
```

## Control CycoD with Commands

### CLI Commands and Options

CycoD offers numerous command-line options for customizing behavior:

| Category | Example | Purpose |
|----------|---------|---------|
| Input | `--question`, `--input`, `--instruction` | Provide input to the AI model |
| Provider | `--use-openai`, `--use-azure-openai`, `--use-copilot` | Select which AI provider to use |
| History | `--continue`, `--chat-history` | Manage conversation state |
| Variables | `--var`, `--foreach` | Define variables and loops |
| Configuration | `--profile`, `--config` | Load configuration settings |

For a complete list of options, use `cycod help options`.

### Interactive Slash Commands

When in interactive mode, use slash commands for quick access to functionality:

| Command | Purpose |
|---------|---------|
| `/clear` | Clear the current chat history |
| `/save` | Save the current chat history |
| `/file <pattern>` | Search for files matching pattern |
| `/find <pattern>` | Find occurrences of pattern in files |
| `/run <command>` | Execute a command and show results |
| `/<promptname>` | Insert a custom prompt template |

## Manage Custom Prompts and Aliases

### Custom Prompts

Create reusable prompt templates to streamline common interactions:

```bash
# Create a prompt template
cycod prompt create code-review "Please review this code and suggest improvements:
1. Identify any bugs or edge cases
2. Suggest performance optimizations
3. Comment on style and readability"

# Use it in a conversation
/code-review
# [paste code here]
```

### Command Aliases

Save complex command combinations as simple aliases:

```bash
# Create an alias for Python debugging
cycod --python-expert --add-system-prompt "Focus on debugging and performance optimization." --save-alias python-debug

# Use the alias
cycod --python-debug --question "Why is this function slow?"
```

## Configuration System

CycoD provides a flexible, scoped configuration system:

### Configuration Scopes

- **Local**: Settings for the current directory (`.cycod/`)
- **User**: Settings for the current user (`~/.cycod/`)
- **Global**: Settings for all users on the system

### Managing Configuration

```bash
# List all settings
cycod config list

# Get a specific setting
cycod config get openai-chat-model-name

# Set a value
cycod config set openai-chat-model-name gpt-4o

# Use specific scopes
cycod config set default-model gpt-4o --user  # User scope
```

## Advanced Features

### MCP Integration

CycoD supports the Model Context Protocol (MCP), enabling integration with external tools and data sources:

```bash
# Add an MCP server
cycod mcp add postgres-server --command /path/to/postgres-mcp-server --arg --connection-string --arg "postgresql://user:pass@localhost:5432/mydb"

# Use MCP capabilities in conversations
# "Look up the recent orders in our database"
```

### Variable Substitution and Templates

Use templates and variables for dynamic command execution:

```bash
# Use variables in a single command
cycod --var name=Alice --input "Hello, {name}!"

# Loop through multiple values
cycod --foreach var name in Alice Bob Charlie --input "Hello, {name}!"

# Use numeric ranges
cycod --foreach var day in 1..7 --input "What happened on day {day}?"
```

### Parallel Processing

Execute multiple operations concurrently:

```bash
# Process multiple topics in parallel
cycod --threads 4 --foreach var topic in "sorting" "searching" "hashing" "trees" --question "Explain {topic} algorithms"
```

## Next Steps

- See the [Chat Basics](../basics/chat.md) guide for basic usage instructions
- Learn about [System Prompts](../tutorials/effective-system-prompts.md) to customize AI behavior
- Explore [Input Options](../usage/input-options.md) for different ways to interact with CycoD
- Check out [Provider Setup](../providers/overview.md) to configure your preferred AI service

By understanding these core features and workflows, you'll be well-equipped to leverage CycoD's capabilities for your development tasks, code analysis, content generation, and more.