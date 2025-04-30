# Creating Aliases

Aliases in CycoD allow you to save frequently used command configurations as shortcuts, making it easier to reuse complex or common configurations without having to type them out each time.

## How Aliases Work

Aliases are stored as plain text files in an `aliases` directory within the CycoD configuration folders. Aliases can be stored in three different scopes:

1. **Local scope** - stored in the `.cycod/aliases` directory of the current working directory
2. **User scope** - stored in the user's CycoD configuration directory (`~/.cycod/aliases` on Unix, `%USERPROFILE%\.cycod\aliases` on Windows)
3. **Global scope** - stored in the system-wide CycoD configuration directory

When looking for an alias, CycoD searches in the local scope first, then the user scope, and finally the global scope.

## Creating an Alias

To create an alias, use one of the following options followed by a name for your alias:

- `--save-alias` or `--save-local-alias` - Save in the local scope (current directory)
- `--save-user-alias` - Save in the user scope (available to the current user in any directory)
- `--save-global-alias` - Save in the global scope (available to all users)

Example:

```bash
cycod --system-prompt "You are an expert Python programmer who gives concise code examples." --save-alias python-helper
```

This will save all the command line options (except the save-alias option itself) to an alias named `python-helper` in the local scope.

To save an alias in the user scope:

```bash
cycod --system-prompt "You are an expert Python programmer who gives concise code examples." --save-user-alias python-helper
```

## Using an Alias

To use a previously saved alias, simply prefix the alias name with `--`:

```bash
cycod --python-helper --input "Write a function that sorts a list of dictionaries by a given key"
```

This will apply all the options stored in the `python-helper` alias (in this case, setting the system prompt) and then apply any additional options provided in the command.

## Examples

### Creating a Role-Specific Alias

```bash
cycod --system-prompt "You are an expert Linux system administrator. Provide clear and concise answers to technical questions about Linux systems." --save-alias linux-admin
```

### Creating a User-Scoped Workflow Alias

```bash
cycod --system-prompt "You are an assistant focused on helping with Git operations and best practices." --save-user-alias git-helper
```

### Creating a Global Alias for All Users

```bash
cycod --system-prompt "You are a technical documentation writer who creates clear and thorough explanations." --save-global-alias tech-writer
```

### Combining Aliases with Additional Options

```bash
# First create the alias
cycod --system-prompt "You are a technical documentation writer who creates clear and thorough explanations." --save-alias tech-writer

# Later, use the alias with additional options
cycod --tech-writer --input "Write documentation for a REST API endpoint that creates user accounts" --output-chat-history "api-docs.jsonl"
```

## Managing Aliases

Aliases are stored as plain text files in the appropriate `aliases` directory based on the scope where they were saved. You can edit these files directly if needed, or simply create a new alias with the same name to overwrite an existing one.

## Technical Details

- Alias files are stored in `aliases/` within the appropriate configuration directory for the selected scope
- Each alias is stored in a file named `<alias-name>.alias`
- Multi-line inputs in aliases are handled properly
- Aliases cannot be nested (an alias cannot reference another alias)