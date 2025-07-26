# --save-global-alias

The `--save-global-alias` option allows you to save the current command line options as a named alias in the global scope, making it available to all users on the system.

## Overview

Aliases in CycoD are saved shortcuts for command line options. By using `--save-global-alias`, you create an alias that is available system-wide to all users. This is useful for organization-wide configurations or standard setups that should be consistent across all users.

## Syntax

```
--save-global-alias ALIAS_NAME
```

Where `ALIAS_NAME` is the name you want to give to your alias.

## Usage

The `--save-global-alias` option is used at the end of a CycoD command to save all the preceding options as an alias:

```bash
cycod [options] --save-global-alias ALIAS_NAME
```

All options specified before `--save-global-alias` will be saved as part of the alias, except for the `--save-global-alias` option itself.

## Examples

### Creating a company-wide programming assistant alias

```bash
cycod --system-prompt "You are a coding assistant familiar with our organization's coding standards." --save-global-alias org-code-assistant
```

### Creating a global security review alias

```bash
cycod --use-openai --openai-chat-model-name gpt-4o --add-system-prompt "Always consider security implications. Follow our organization's security best practices." --save-global-alias org-security-review
```

### Using the created alias

After creating an alias, you can use it by prefixing the alias name with `--`:

```bash
cycod --org-code-assistant --question "How should I implement this feature according to our standards?"
```

### Combining an alias with additional options

You can combine an alias with additional options:

```bash
cycod --org-security-review --question "Review this authentication code" --output-trajectory security-review.md
```

## Storage Location

Global aliases are stored in a system-wide location:

- **Windows**: `%ProgramData%\cycod\aliases\`
- **macOS/Linux**: `/usr/local/share/cycod/aliases/` or `/etc/cycod/aliases/`

Each alias is stored as a separate file named `<ALIAS_NAME>.alias`.

## Administrator Rights

Creating global aliases typically requires administrative privileges:

- **Windows**: Run the command as Administrator
- **macOS/Linux**: Use `sudo` with the command

Without proper privileges, the command may fail with a permission error.

## Notes

- Alias names are case-sensitive
- Alias names cannot contain spaces or special characters
- If an alias with the same name already exists in the global scope, it will be overwritten
- You cannot create nested aliases (aliases that reference other aliases)
- Global aliases have the lowest precedence; local and user aliases with the same name will override global aliases

## Related

- [`--global`](/reference/cycod/options/global.md): Apply command at global scope
- [`--save-alias`](/reference/cycod/options/save-alias.md): Alias for --save-local-alias
- [`--save-local-alias`](/reference/cycod/options/save-local-alias.md): Save alias to local scope
- [`--save-user-alias`](/reference/cycod/options/save-user-alias.md): Save alias to user scope