# --save-local-alias

The `--save-local-alias` option allows you to save the current command line options as a named alias in the local scope (current directory).

## Overview

Aliases in ChatX are saved shortcuts for command line options. By using `--save-local-alias`, you create an alias that is available only in the current directory. This is useful for project-specific configurations that don't need to be available globally.

## Syntax

```
--save-local-alias ALIAS_NAME
```

Where `ALIAS_NAME` is the name you want to give to your alias.

## Usage

The `--save-local-alias` option is used at the end of a ChatX command to save all the preceding options as an alias:

```bash
chatx [options] --save-local-alias ALIAS_NAME
```

All options specified before `--save-local-alias` will be saved as part of the alias, except for the `--save-local-alias` option itself.

## Examples

### Creating a project-specific Python assistant alias

```bash
chatx --system-prompt "You are an expert Python programmer familiar with our codebase." --save-local-alias python-helper
```

Command output:
```
Alias 'python-helper' created at: C:\project\.chatx\aliases\python-helper.alias
```

### Creating an alias with multiple options

```bash
chatx --use-openai --openai-chat-model-name gpt-4o --add-system-prompt "Focus on security best practices." --save-local-alias security-review
```

Command output:
```
Alias 'security-review' created at: C:\project\.chatx\aliases\security-review.alias
```

### Using the created alias

After creating an alias, you can use it by prefixing the alias name with `--`:

```bash
chatx --python-helper --question "How should we implement this feature?"
```

### Combining an alias with additional options

You can combine an alias with additional options:

```bash
chatx --security-review --question "Review this authentication code" --output-trajectory security-review.md
```

## Storage Location

Local aliases are stored in the `.chatx/aliases` directory within your current working directory:

- **Windows**: `<current_directory>\.chatx\aliases\`
- **macOS/Linux**: `<current_directory>/.chatx/aliases/`

Each alias is stored as a separate file named `<ALIAS_NAME>.alias`.

## Default Behavior

For convenience, the standard `--save-alias` option is equivalent to `--save-local-alias`:

```bash
chatx [options] --save-alias ALIAS_NAME
# is the same as
chatx [options] --save-local-alias ALIAS_NAME
```

## Version Control Considerations

Since local aliases are stored in the project directory:

- They can be committed to version control and shared with a team
- This makes them ideal for project-specific configurations
- For personal preferences, use `--save-user-alias` instead
- Consider adding `.chatx/aliases/` to your `.gitignore` file if you don't want to share aliases

## Permissions

When creating local aliases:
- You must have write permissions for the current directory
- In shared project directories, all team members with write access can create and modify local aliases
- In read-only directories, you won't be able to create local aliases (use `--save-user-alias` instead)

## Error Handling

Common error scenarios:

- **Alias already exists**: If you try to create an alias with a name that already exists in the local scope, the existing alias will be overwritten without warning.
- **Invalid characters**: Alias names should not contain spaces or special characters. If you use invalid characters, you may receive an error.
- **Permission denied**: If you don't have write permissions to the current directory, you'll receive a permission error.

Example error:
```
Error: Cannot create alias file at 'C:\read-only-dir\.chatx\aliases\my-alias.alias'. Access denied.
```

## Notes

- Alias names are case-sensitive
- Alias names cannot contain spaces or special characters
- If an alias with the same name already exists in the local scope, it will be overwritten
- You cannot create nested aliases (aliases that reference other aliases)

## Related

- [`--local`](/reference/cli/options/local.md): Apply command at local scope
- [`--save-alias`](/reference/cli/options/save-alias.md): Alias for --save-local-alias
- [`--save-user-alias`](/reference/cli/options/save-user-alias.md): Save alias to user scope
- [`--save-global-alias`](/reference/cli/options/save-global-alias.md): Save alias to global scope