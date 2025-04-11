---
hide:
- toc
---

--8<-- "snippets/ai-generated.md"

# Understanding Scopes in ChatX

ChatX uses a powerful scoping system that allows you to apply settings, configurations, aliases, and prompts at different levels. Understanding how scopes work is essential for effectively customizing and managing your ChatX environment.

## The Three-Tier Scoping System

ChatX uses three scope levels for all configuration features:

1. **Local Scope**: Settings applied only to the current directory
2. **User Scope**: Settings applied to the current user across all directories
3. **Global Scope**: Settings applied to all users on the system

## Scope Hierarchy and Precedence

When ChatX looks for a setting, alias, prompt, or MCP server, it follows this search order:

1. First checks the **local scope** (current directory)
2. If not found, checks the **user scope** (current user)
3. If still not found, checks the **global scope** (all users)

This means that local settings override user settings, which override global settings.

## Using Scope Options

You can explicitly specify which scope to use with these options:

| Option | Alias | Description |
|--------|-------|-------------|
| `--local` | `-l` | Apply at local scope (current directory) |
| `--user` | `-u` | Apply at user scope (current user) |
| `--global` | `-g` | Apply at global scope (all users) |
| `--any` | `-a` | Search across all scopes |

### Default Scopes

If no scope option is specified:

- Most **set/create/add/delete** operations use **local scope** by default
- Most **list/get** operations use **any scope** by default (search all scopes)

## Example: Configuring ChatX at Different Scopes

### Local Scope (Project-Specific)

```bash
# Set a project-specific configuration
chatx config set app.preferredProvider openai --local

# Create a project-specific alias
chatx --use-openai --openai-chat-model-name gpt-4o --save-alias project-assistant

# List local settings only
chatx config list --local
```

### User Scope (Personal)

```bash
# Set a user-specific API key
chatx config set OPENAI_API_KEY sk-yourapikeyhere --user

# Create a personal prompt
chatx prompt create explain "Explain this concept in simple terms:" --user

# List user settings only
chatx config list --user

# Save a frequently-used command as a user alias
chatx --use-openai --openai-chat-model-name gpt-4o --save-user-alias my-assistant

# Add an MCP server for personal use across all projects
chatx mcp add personal-tools --command "~/bin/tools-server" --user

# Delete a user-level prompt
chatx prompt delete old-prompt --user

# Disable auto-saving chat history for all your projects
chatx config set App.AutoSaveChatHistory false --user
```

### Global Scope (System-Wide)

```bash
# Set a system-wide default model
chatx config set OPENAI_CHAT_MODEL_NAME gpt-3.5-turbo --global

# Create a system-wide MCP server
chatx mcp add shared-service --command "/usr/local/bin/service" --global

# List global settings only
chatx config list --global
```

## Storage Locations

ChatX stores configuration data in different locations based on the scope:

### Local Scope

Located in the `.chatx` directory of your current working directory:
- Windows: `<current_directory>\.chatx\`
- macOS/Linux: `<current_directory>/.chatx/`

### User Scope

Located in your user profile:
- Windows: `%USERPROFILE%\.chatx\`
- macOS/Linux: `~/.chatx/`

### Global Scope

Located in system directories:
- Windows: `C:\ProgramData\.chatx\`
- macOS/Linux: `/usr/local/share/.chatx/` or similar system-wide location

## When to Use Each Scope

### Local Scope

Best for:
- Project-specific settings
- Team-shared settings (when in a shared repository)
- Temporary configurations for specific tasks

### User Scope

Best for:
- Personal preferences that apply to all your projects
- Security-sensitive information like API keys
- Your frequently used aliases and prompts
- Settings you want to persist across different machines (when synced)
- Configuration that shouldn't be shared with a team via version control
- Consistent default behavior regardless of which directory you're working in
- Personal customizations to the ChatX experience

### Global Scope

Best for:
- Organization-wide standards and defaults
- Shared resources for all users on a system
- Common utilities and tools

## Practical Guide to User Scope

The user scope (`--user` or `-u`) is particularly valuable because it provides a balance between convenience and security. Here are some practical scenarios where user scope is the ideal choice:

### API Keys and Authentication

Always store your API keys and authentication tokens at the user scope for security:

```bash
# Store your OpenAI API key
chatx config set OPENAI_API_KEY sk-yourapikeyhere --user

# Store your GitHub token
chatx config set GITHUB_TOKEN ghp_yourtokenhere --user

# Configure Azure OpenAI credentials
chatx config set AZURE_OPENAI_ENDPOINT https://your-resource.openai.azure.com --user
chatx config set AZURE_OPENAI_API_KEY your-api-key-here --user
```

### Personal Workflow Preferences

Configure your personal workflow preferences that should apply regardless of project:

```bash
# Set your preferred AI model
chatx config set app.preferredProvider azure-openai --user
chatx config set OPENAI_CHAT_MODEL_NAME gpt-4-turbo --user

# Configure auto-saving behavior
chatx config set App.AutoSaveTrajectory true --user
chatx config set App.HistoryDirectory "~/Documents/ChatX/History" --user
```

### Reusable Prompts and Aliases

Create personal prompts and aliases you'll use across different projects:

```bash
# Create a code review prompt you can use in any project
chatx prompt create code-review "Review the following code for:
1. Security issues
2. Performance optimizations
3. Code style improvements" --user

# Create aliases for common tasks
chatx --use-openai --openai-chat-model-name gpt-4o --save-user-alias expert
chatx --use-azure-openai --azure-openai-chat-deployment gpt-35-turbo --save-user-alias fast
```

### MCP Tools Configuration

Configure Model Context Protocol servers that you'll use across different projects:

```bash
# Set up your personal database connection tool
chatx mcp add my-database --command "~/tools/db-connector" --env DB_PASSWORD=securepassword --user

# Configure a personal API integration
chatx mcp add personal-api --url "https://api.example.com/personal" --user
```

### Migrating Between Machines

User scope settings are perfect for migrating your configuration between machines:

1. On your first machine: `chatx config list --user > my-chatx-config.txt`
2. Transfer the file to a new machine
3. On the new machine, use this file to recreate your settings

## Security Considerations

- **API Keys**: Store in user scope, not local or global scope
- **Sensitive Data**: Avoid storing sensitive information in global scope
- **Permissions**: Modifying global settings may require administrative privileges
- **Version Control**: Add `.chatx/` to your `.gitignore` to avoid accidentally sharing user settings

## Related Commands

- [chatx config](/reference/cli/config/index.md): Manage configuration settings
- [chatx alias](/reference/cli/alias/index.md): Manage command aliases
- [chatx prompt](/reference/cli/prompt/index.md): Manage custom prompts
- [chatx mcp](/reference/cli/mcp/index.md): Manage MCP servers