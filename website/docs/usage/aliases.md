---
hide:
- toc
---

--8<-- "snippets/ai-generated.md"

# Using Aliases in CycoD

Aliases in CycoD are powerful shortcuts that save you from typing the same command options repeatedly. This guide explains how to create, use, and manage aliases effectively.

## What Are Aliases?

Aliases are named shortcuts for CycoD command options. Instead of typing a long command with multiple options, you can save those options as an alias and use the alias name prefixed with `--`.

For example, instead of typing:

```bash
cycod --use-openai --openai-chat-model-name gpt-4o --add-system-prompt "You are an expert Python programmer."
```

You can create an alias:

```bash
cycod --use-openai --openai-chat-model-name gpt-4o --add-system-prompt "You are an expert Python programmer." --save-alias python-expert
```

And then use it simply as:

```bash
cycod --python-expert --question "How do I implement a decorator in Python?"
```

## Creating Aliases

CycoD provides several options for creating aliases at different scopes:

### Local Aliases (Project-Specific)

Local aliases are stored in the current directory and are ideal for project-specific configurations:

```bash
# Create a local alias (default behavior)
cycod [options] --save-alias my-alias

# Explicitly create a local alias
cycod [options] --save-local-alias my-alias
```

### User Aliases (Personal)

User aliases are available to the current user across all directories, making them ideal for personal preferences and specialized AI assistants that you want to access from any project:

```bash
cycod [options] --save-user-alias my-alias
```

For example, you could create personal assistants for different domains:

```bash
# Create a personal writing assistant
cycod --add-system-prompt "You are a writing assistant who helps improve clarity, flow, and grammar." --save-user-alias writer

# Create a personal data analysis assistant
cycod --add-system-prompt "You are a data analysis expert who helps interpret data, suggest visualization approaches, and identify meaningful patterns." --save-user-alias data-analyst
```

These aliases will be available to you in any directory on your system, making them perfect for cross-project needs. See the [Creating User Aliases Tutorial](/tutorials/creating-user-aliases.md) for more examples.

### Global Aliases (System-Wide)

Global aliases are available to all users on the system, making them ideal for organization-wide standards:

```bash
# Requires administrative privileges
cycod [options] --save-global-alias my-alias
```

## Using Aliases

To use an alias, prefix the alias name with `--`:

```bash
cycod --my-alias
```

You can also combine aliases with additional options:

```bash
cycod --my-alias --question "What is the capital of France?"
```

## Managing Aliases

CycoD provides commands to list, view, and delete aliases:

### List All Aliases

```bash
cycod alias list
```

By default, this lists aliases from all scopes. You can specify a particular scope:

```bash
cycod alias list --local
cycod alias list --user
cycod alias list --global
```

### View a Specific Alias

To see what options an alias contains:

```bash
cycod alias get my-alias
```

Like the list command, you can specify a scope:

```bash
cycod alias get my-alias --local
cycod alias get my-alias --user
cycod alias get my-alias --global
```

### Delete an Alias

To delete an alias:

```bash
cycod alias delete my-alias
```

By default, this will search for the alias in all scopes and delete the first occurrence found. You can specify a scope to ensure you're deleting the alias from the right location:

```bash
cycod alias delete my-alias --local   # Delete from local scope only
cycod alias delete my-alias --user    # Delete from user scope only
cycod alias delete my-alias --global  # Delete from global scope only
cycod alias delete my-alias --any     # Delete from any scope (default behavior)
```

When an alias is successfully deleted, the command will display the path of the deleted file:

```
Deleted: C:\Users\username\.cycod\aliases\my-alias.alias
```

If the specified alias isn't found in the requested scope, you'll see an error message:

```
Error: Alias 'my-alias' not found in specified scope.
```

## Storage Locations

Aliases are stored in different locations based on their scope:

### Local Scope

Located in the `.cycod/aliases` directory of your current working directory:
- Windows: `<current_directory>\.cycod\aliases\`
- macOS/Linux: `<current_directory>/.cycod/aliases/`

### User Scope

Located in your user profile:
- Windows: `%USERPROFILE%\.cycod\aliases\`
- macOS/Linux: `~/.cycod/aliases/`

### Global Scope

Located in system directories:
- Windows: `%ProgramData%\cycod\aliases\`
- macOS/Linux: `/usr/local/share/cycod/aliases/` or `/etc/cycod/aliases/`

Each alias is stored as a separate file named `<ALIAS_NAME>.alias`.

## Best Practices

### Choose the Right Scope

- Use **local aliases** for project-specific settings that might be shared with team members
- Use **user aliases** for your personal preferences that you want available in all directories
- Use **global aliases** for organization-wide standards that should be available to everyone

### Alias Naming Conventions

- Use descriptive names that indicate the purpose of the alias
- Use dashes instead of spaces in alias names
- Consider prefixing aliases with a category, like `py-` for Python-related aliases

### Examples of Effective Aliases

#### Project-Specific Aliases

```bash
# Create a project assistant familiar with your codebase
cycod --add-system-prompt "You're familiar with our project structure and coding standards." --save-local-alias project-assistant
```

#### Personal Workflow Aliases

```bash
# Create a user alias for quick translations
cycod --add-system-prompt "You are a translation expert. Translate between languages accurately and naturally." --save-user-alias translator
```

#### Organization-Wide Aliases

```bash
# Create a global alias for security reviews
cycod --use-openai --openai-chat-model-name gpt-4o --add-system-prompt "You are a security expert. Analyze code for security vulnerabilities following our organization's standards." --save-global-alias security-review
```

## Administrative Privileges

Creating global aliases typically requires administrative privileges:

- **Windows**: Run the command prompt or PowerShell as Administrator
- **macOS/Linux**: Use `sudo` with the command

Without proper privileges, the command may fail with a permission error.

## Alias Precedence

When an alias exists in multiple scopes with the same name, the precedence is:
1. Local aliases (highest)
2. User aliases
3. Global aliases (lowest)

This means that a local alias will override a user or global alias with the same name.

## Related Documentation

- [Understanding Scopes in CycoD](scopes.md)
- [Configuration](configuration.md)
- [Project-Specific Aliases Tutorial](/tutorials/project-aliases.md)
- [Creating User Aliases Tutorial](/tutorials/creating-user-aliases.md)
- [cycod alias delete Command](/reference/cycod/alias/delete.md)
- [cycod alias get Command](/reference/cycod/alias/get.md)
- [cycod alias list Command](/reference/cycod/alias/list.md)
- [--save-alias CLI Reference](/reference/cycod/options/save-alias.md)
- [--save-local-alias CLI Reference](/reference/cycod/options/save-local-alias.md)
- [--save-user-alias CLI Reference](/reference/cycod/options/save-user-alias.md)
- [--save-global-alias CLI Reference](/reference/cycod/options/save-global-alias.md)