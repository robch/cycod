# --any

The `--any` option (or its shorthand `-a`) is a scope qualifier used to operate across all available scopes.

## Syntax

```bash
cycod [command] --any
# or
cycod [command] -a
```

## Description

CycoD uses a scoping system for configuration settings, aliases, prompts, and MCP servers that organizes items in three levels:

1. **Local scope**: Settings specific to the current directory
2. **User scope**: Settings specific to the current user
3. **Global scope**: Settings available to all users

The `--any` option instructs CycoD to consider all of these scopes when performing operations, which makes it useful when you want to:

- View all items across all scopes
- Find an item regardless of which scope it's defined in
- Operate on the first matching item found in any scope

## Behavior

The `--any` option behaves differently depending on the command type:

- **List commands**: Shows items from all scopes (local, user, and global)
- **Get commands**: Searches for items in all scopes, checking local first, then user, then global
- **Delete/Remove commands**: Deletes the item from the first scope it's found in

## Common Uses

The `--any` option is the default for many list and get commands, including:

- `cycod alias list`
- `cycod config list`
- `cycod config get`
- `cycod prompt list`
- `cycod prompt get`
- `cycod mcp list`
- `cycod mcp get`

## Examples

List all configuration settings from all scopes:

```bash
cycod config list --any
# or simply
cycod config list
```

Find a prompt definition regardless of its scope:

```bash
cycod prompt get translate --any
```

Delete an alias from the first scope it's found in:

```bash
cycod alias delete my-alias --any
```

## Scope Precedence

When searching with the `--any` option, CycoD uses this precedence order:

1. Local scope (current directory)
2. User scope (current user)
3. Global scope (all users)

This means that if the same item exists in multiple scopes, the one in the highest precedence scope will be used.

## Related Options

Other scope options that can be used instead of `--any`:

- `--local`, `-l`: Target only the local scope (current directory)
- `--user`, `-u`: Target only the user scope (current user)
- `--global`, `-g`: Target only the global scope (all users)