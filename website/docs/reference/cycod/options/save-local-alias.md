# --save-local-alias

The `--save-local-alias` option allows you to save the current command line options as a named alias in the local scope (current directory).

## Overview

Aliases in CycoD are saved shortcuts for command line options. By using `--save-local-alias`, you create an alias that is available only in the current directory. This is useful for project-specific configurations that don't need to be available globally.

## Syntax

```
--save-local-alias ALIAS_NAME
```

Where `ALIAS_NAME` is the name you want to give to your alias.

## Usage

The `--save-local-alias` option is used at the end of a CycoD command to save all the preceding options as an alias:

```bash
cycod [options] --save-local-alias ALIAS_NAME
```

All options specified before `--save-local-alias` will be saved as part of the alias, except for the `--save-local-alias` option itself.

## Examples

### Creating a project-specific Python assistant alias

```bash
cycod --system-prompt "You are an expert Python programmer familiar with our codebase." --save-local-alias python-helper
```

Command output:
```
Alias 'python-helper' created at: C:\project\.cycod\aliases\python-helper.alias
```

### Creating an alias with multiple options

```bash
cycod --use-openai --openai-chat-model-name gpt-4o --add-system-prompt "Focus on security best practices." --save-local-alias security-review
```

Command output:
```
Alias 'security-review' created at: C:\project\.cycod\aliases\security-review.alias
```

### Using the created alias

After creating an alias, you can use it by prefixing the alias name with `--`:

```bash
cycod --python-helper --question "How should we implement this feature?"
```

### Combining an alias with additional options

You can combine an alias with additional options:

```bash
cycod --security-review --question "Review this authentication code" --output-trajectory security-review.md
```

## Storage Location

Local aliases are stored in the `.cycod/aliases` directory within your current working directory:

- **Windows**: `<current_directory>\.cycod\aliases\`
- **macOS/Linux**: `<current_directory>/.cycod/aliases/`

Each alias is stored as a separate file named `<ALIAS_NAME>.alias`.

## Default Behavior

For convenience, the standard `--save-alias` option is equivalent to `--save-local-alias`:

```bash
cycod [options] --save-alias ALIAS_NAME
# is the same as
cycod [options] --save-local-alias ALIAS_NAME
```

## Version Control Considerations

Since local aliases are stored in the project directory:

- They can be committed to version control and shared with a team
- This makes them ideal for project-specific configurations
- For personal preferences, use `--save-user-alias` instead
- Consider adding `.cycod/aliases/` to your `.gitignore` file if you don't want to share aliases

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
Error: Cannot create alias file at 'C:\read-only-dir\.cycod\aliases\my-alias.alias'. Access denied.
```

## Notes

- Alias names are case-sensitive
- Alias names cannot contain spaces or special characters
- If an alias with the same name already exists in the local scope, it will be overwritten
- You cannot create nested aliases (aliases that reference other aliases)

## Related

- [`--local`](/reference/cycod/options/local.md): Apply command at local scope
- [`--save-alias`](/reference/cycod/options/save-alias.md): Alias for --save-local-alias
- [`--save-user-alias`](/reference/cycod/options/save-user-alias.md): Save alias to user scope
- [`--save-global-alias`](/reference/cycod/options/save-global-alias.md): Save alias to global scope