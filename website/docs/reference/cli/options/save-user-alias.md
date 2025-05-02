# --save-user-alias

The `--save-user-alias` option allows you to save the current command line options as a named alias in the user scope, making it available to the current user across all directories.

## Overview

Aliases in CycoD are saved shortcuts for command line options. By using `--save-user-alias`, you create an alias that is available to the current user in any directory. This is useful for personal configurations that should follow you across different projects.

## Syntax

```
--save-user-alias ALIAS_NAME
```

Where `ALIAS_NAME` is the name you want to give to your alias.

## Usage

The `--save-user-alias` option is used at the end of a CycoD command to save all the preceding options as an alias:

```bash
cycod [options] --save-user-alias ALIAS_NAME
```

All options specified before `--save-user-alias` will be saved as part of the alias, except for the `--save-user-alias` option itself.

## Examples

### Creating a personal Python assistant alias

```bash
cycod --add-system-prompt "You are an expert Python programmer specializing in clean, efficient code. Always explain your reasoning and include error handling." --save-user-alias python-expert
```

### Creating a translator alias

```bash
cycod --use-openai --openai-chat-model-name gpt-4o --add-system-prompt "You are a translation expert. Translate text accurately between languages while preserving tone and cultural context." --save-user-alias translator
```

### Creating a code reviewer alias

```bash
cycod --add-system-prompt "You are a code review expert. Analyze code for bugs, security issues, performance problems, and maintainability concerns. Suggest specific improvements and explain their benefits." --save-user-alias code-reviewer
```

### Using the created alias

After creating an alias, you can use it by prefixing the alias name with `--`:

```bash
cycod --python-expert --question "How should I implement a binary search tree in Python?"
```

### Combining an alias with additional options

You can combine an alias with additional options:

```bash
cycod --translator --question "Translate the following to Spanish: 'Hello, how are you?'"
```

### Using an alias in a workflow

```bash
# First, create a code sample
echo "function processData(data) { return data.map(x => x*2); }" > sample.js

# Then use your code reviewer alias to check it
cycod --code-reviewer --input "Review this JavaScript code:" @sample.js
```

## Storage Location

User aliases are stored in the `.cycod/aliases` directory within your user profile:

- **Windows**: `%USERPROFILE%\.cycod\aliases\`
- **macOS/Linux**: `~/.cycod/aliases/`

Each alias is stored as a separate file named `<ALIAS_NAME>.alias`.

## When to Use User Aliases

User aliases are ideal for:

- Personal AI assistants that you want to use across all projects
- Specialized tools that match your workflow (regardless of which project you're in)
- Configurations that reflect your personal preferences for interacting with AI

If you need project-specific aliases, use [`--save-local-alias`](/reference/cli/options/save-local-alias.md) instead.

## Notes

- Alias names are case-sensitive
- Alias names cannot contain spaces or special characters
- If an alias with the same name already exists in the user scope, it will be overwritten
- You cannot create nested aliases (aliases that reference other aliases)
- User aliases have medium precedence; they override global aliases but are overridden by local aliases with the same name

## Related

- [`--user`](/reference/cli/options/user.md): Apply command at user scope
- [`--save-alias`](/reference/cli/options/save-alias.md): Alias for --save-local-alias
- [`--save-local-alias`](/reference/cli/options/save-local-alias.md): Save alias to local scope
- [`--save-global-alias`](/reference/cli/options/save-global-alias.md): Save alias to global scope
- [Creating User Aliases Tutorial](/tutorials/creating-user-aliases.md)
- [Using Aliases in CycoD](/usage/aliases.md)