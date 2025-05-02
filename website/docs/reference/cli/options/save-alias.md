# --save-alias

The `--save-alias` option allows you to save the current command line options as a named alias in the local scope (current directory).

## Overview

The `--save-alias` option is a convenience alias for `--save-local-alias`. It allows you to create command shortcuts that save you from having to type the same options repeatedly. These aliases are stored in the local scope, meaning they are only available in the current directory.

## Syntax

```
--save-alias ALIAS_NAME
```

Where `ALIAS_NAME` is the name you want to give to your alias.

## Usage

The `--save-alias` option is typically used at the end of a CycoD command to save all the preceding options:

```bash
cycod [options] --save-alias ALIAS_NAME
```

All options specified before `--save-alias` will be saved as part of the alias, except for the `--save-alias` option itself.

## Examples

### Creating a project assistant alias

```bash
cycod --system-prompt "You are a project assistant familiar with our codebase." --save-alias project-helper
```

### Creating an alias with provider settings

```bash
cycod --use-openai --openai-chat-model-name gpt-4o --save-alias gpt4
```

### Using the created alias

After creating an alias, you can use it by prefixing the alias name with `--`:

```bash
cycod --project-helper --question "How should we structure this new feature?"
```

### Using an alias with additional options

```bash
cycod --gpt4 --question "Explain quantum computing" --output-trajectory quantum.md
```

## Storage Location

Aliases created with `--save-alias` are stored in the `.cycod/aliases` directory within your current working directory:

- **Windows**: `<current_directory>\.cycod\aliases\`
- **macOS/Linux**: `<current_directory>/.cycod/aliases/`

Each alias is stored as a separate file named `<ALIAS_NAME>.alias`.

## Notes

- `--save-alias` is equivalent to `--save-local-alias`
- Alias names are case-sensitive
- Alias names cannot contain spaces or special characters
- If an alias with the same name already exists in the local scope, it will be overwritten
- For user-wide aliases, use `--save-user-alias` instead
- For system-wide aliases, use `--save-global-alias` instead

## Team Collaboration

Since local aliases are stored within the project directory:

- They can be committed to version control
- This allows sharing common configurations with your team
- It's particularly useful for project-specific AI behaviors and settings

## Related

- [`--save-local-alias`](/reference/cli/options/save-local-alias.md): Same as --save-alias
- [`--save-user-alias`](/reference/cli/options/save-user-alias.md): Save alias to user scope
- [`--save-global-alias`](/reference/cli/options/save-global-alias.md): Save alias to global scope
- [`--local`](/reference/cli/options/local.md): Apply command at local scope