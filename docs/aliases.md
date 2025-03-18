# Creating Aliases

Aliases in ChatX allow you to save frequently used command configurations as shortcuts, making it easier to reuse complex or common configurations without having to type them out each time.

## How Aliases Work

Aliases are stored as plain text files in a `.chatx/aliases` directory, either in your home directory or in the current working directory. Each alias file contains the command line options that should be applied when the alias is used.

## Creating an Alias

To create an alias, use the `--save-alias` option followed by a name for your alias:

```bash
chatx --system-prompt "You are an expert Python programmer who gives concise code examples." --save-alias python-helper
```

This will save all the command line options (except `--save-alias` itself) to an alias named `python-helper`. The alias will be stored in a file in the `.chatx/aliases` directory.

## Using an Alias

To use a previously saved alias, simply prefix the alias name with `--`:

```bash
chatx --python-helper --input "Write a function that sorts a list of dictionaries by a given key"
```

This will apply all the options stored in the `python-helper` alias (in this case, setting the system prompt) and then apply any additional options provided in the command.

## Examples

### Creating a Role-Specific Alias

```bash
chatx --system-prompt "You are an expert Linux system administrator. Provide clear and concise answers to technical questions about Linux systems." --save-alias linux-admin
```

### Creating a Workflow Alias

```bash
chatx --system-prompt "You are an assistant focused on helping with Git operations and best practices." --save-alias git-helper
```

### Combining Aliases with Additional Options

```bash
# First create the alias
chatx --system-prompt "You are a technical documentation writer who creates clear and thorough explanations." --save-alias tech-writer

# Later, use the alias with additional options
chatx --tech-writer --input "Write documentation for a REST API endpoint that creates user accounts" --output-chat-history "api-docs.jsonl"
```

## Managing Aliases

Aliases are stored as plain text files in the `.chatx/aliases` directory. You can edit these files directly if needed, or simply create a new alias with the same name to overwrite an existing one.

## Technical Details

- Alias files are stored in `.chatx/aliases/`
- Each alias is stored in a file named `<alias-name>.alias`
- Multi-line inputs in aliases are handled properly
- Aliases cannot be nested (an alias cannot reference another alias)