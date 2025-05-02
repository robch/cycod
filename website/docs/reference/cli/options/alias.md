# --ALIAS Option

The `--ALIAS` option allows you to use previously saved command aliases. It's a shorthand way to invoke a set of pre-configured options.

## Syntax

```bash
cycod --ALIAS [additional options]
```

Where `ALIAS` is the name of a previously saved alias.

## Description

When you use the `--ALIAS` syntax (where `ALIAS` is the name of your saved alias), CycoD will:

1. Search for the specified alias in local, user, and global scopes (in that order)
2. Load the saved command options from the alias file
3. Apply those options to your current command
4. Continue processing any additional options you provide

This feature allows you to create shortcuts for frequently used commands and option combinations.

## Locating Aliases

When looking for an alias, CycoD searches in this order:

1. Local scope (`.cycod/aliases` in the current directory)
2. User scope (`.cycod/aliases` in the user's home directory)
3. Global scope (system-wide directory available to all users)

This means a local alias takes precedence over a user alias with the same name, which takes precedence over a global alias.

## Examples

### Basic Usage

```bash
# Using an alias named "python-expert"
cycod --python-expert
```

This would apply all the options that were saved when the alias was created, such as model settings, system prompts, etc.

### Combining with Additional Options

```bash
# Using the alias and adding a specific question
cycod --python-expert --question "How do I read JSON files in Python?"
```

The options specified in the `python-expert` alias will be applied first, and then the additional `--question` option will be added.

## Related Commands

- [`--save-alias`](/reference/cli/options/save-alias.md) - Save current options as an alias in local scope
- [`--save-local-alias`](/reference/cli/options/save-local-alias.md) - Save current options as an alias in local scope
- [`--save-user-alias`](/reference/cli/options/save-user-alias.md) - Save current options as an alias in user scope
- [`--save-global-alias`](/reference/cli/options/save-global-alias.md) - Save current options as an alias in global scope

## See Also

- [alias command](/reference/cli/alias/index.md)
- [Managing aliases](/advanced/aliases.md)