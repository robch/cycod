---
hide:
- toc
---

--8<-- "snippets/ai-generated.md"

# ChatX Overview

ChatX is a powerful AI-powered CLI tool that enables natural language interactions with AI models directly from your terminal. By integrating seamlessly with your development environment, ChatX streamlines your workflow and enhances productivity without requiring complex setups or additional servers.

ChatX supports multiple AI providers, offers rich file and shell integration capabilities, and provides an extensible framework for custom workflows and tools.

ChatX operates directly in your terminal, understanding your inputs and performing real actions. It leverages the power of large language models to help you accomplish a wide range of tasks.

### From Questions to Solutions

```bash
# Ask a simple question
chatx --question "What time is it?"

# Process multiple inputs sequentially
chatx --inputs "What's today's date?" "Show me a calendar for this month"

# Continue a previous conversation
chatx --continue --question "What about next month?"

# Use expert roles
chatx --python-expert --question "How do I handle file I/O in Python?"
```

### File Operations

ChatX can interact with your local files, making it easy to analyze, create, and modify content:

```bash
# Analyze code in your project
chatx --instruction "Look at the files in this directory and explain what they do"

# Generate new files
chatx --instruction "Create a simple React component for a user profile"

# Transform content
chatx --instruction "Read data.json and convert it to a CSV format"
```

### Shell Command Integration

Execute and process commands directly from your ChatX session:

```bash
# Run system commands and analyze results
chatx --instruction "Show me the largest files in this directory"

# Perform complex operations
chatx --instruction "Find all TODO comments in the src directory"

# Automate workflows
chatx --instruction "Run the tests and explain any failures"
```

## Control ChatX with Commands

### CLI Commands and Options

ChatX offers numerous command-line options for customizing behavior:

| Category | Example | Purpose |
|----------|---------|---------|
| Input | `--question`, `--input`, `--instruction` | Provide input to the AI model |
| Provider | `--use-openai`, `--use-azure-openai`, `--use-copilot` | Select which AI provider to use |
| History | `--continue`, `--chat-history` | Manage conversation state |
| Variables | `--var`, `--foreach` | Define variables and loops |
| Configuration | `--profile`, `--config` | Load configuration settings |

For a complete list of options, use `chatx help options`.

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
chatx prompt create code-review "Please review this code and suggest improvements:
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
chatx --python-expert --add-system-prompt "Focus on debugging and performance optimization." --save-alias python-debug

# Use the alias
chatx --python-debug --question "Why is this function slow?"
```

## Configuration System

ChatX provides a flexible, scoped configuration system:

### Configuration Scopes

- **Local**: Settings for the current directory (`.chatx/`)
- **User**: Settings for the current user (`~/.chatx/`)
- **Global**: Settings for all users on the system

### Managing Configuration

```bash
# List all settings
chatx config list

# Get a specific setting
chatx config get openai-chat-model-name

# Set a value
chatx config set openai-chat-model-name gpt-4o

# Use specific scopes
chatx config set default-model gpt-4o --user  # User scope
```

## Advanced Features

### MCP Integration

ChatX supports the Model Context Protocol (MCP), enabling integration with external tools and data sources:

```bash
# Add an MCP server
chatx mcp add postgres-server --command /path/to/postgres-mcp-server --arg --connection-string --arg "postgresql://user:pass@localhost:5432/mydb"

# Use MCP capabilities in conversations
# "Look up the recent orders in our database"
```

### Variable Substitution and Templates

Use templates and variables for dynamic command execution:

```bash
# Use variables in a single command
chatx --var name=Alice --input "Hello, {name}!"

# Loop through multiple values
chatx --foreach var name in Alice Bob Charlie --input "Hello, {name}!"

# Use numeric ranges
chatx --foreach var day in 1..7 --input "What happened on day {day}?"
```

### Parallel Processing

Execute multiple operations concurrently:

```bash
# Process multiple topics in parallel
chatx --threads 4 --foreach var topic in "sorting" "searching" "hashing" "trees" --question "Explain {topic} algorithms"
```

## Next Steps

- See the [Getting Started](../usage/basics.md) guide for basic usage instructions
- Learn about [System Prompts](../tutorials/effective-system-prompts.md) to customize AI behavior
- Explore [Input Options](../usage/input-options.md) for different ways to interact with ChatX
- Check out [Provider Setup](../providers/overview.md) to configure your preferred AI service

By understanding these core features and workflows, you'll be well-equipped to leverage ChatX's capabilities for your development tasks, code analysis, content generation, and more.