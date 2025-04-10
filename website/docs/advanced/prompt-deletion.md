# Deleting Custom Prompts

ChatX lets you easily manage your custom prompts with the `prompt delete` command. This guide covers how to effectively remove prompts that are no longer needed.

## Basic Prompt Deletion

To delete a custom prompt, use the `prompt delete` command followed by the prompt name:

```bash
chatx prompt delete translate
```

This command will:
1. Search for a prompt named "translate" in all scopes (local, user, global)
2. Delete the first matching prompt it finds
3. Ask for confirmation before proceeding

## Understanding Scope-Specific Deletion

ChatX prompts can exist in multiple scopes (local, user, global), and you might want to remove a prompt from a specific scope without affecting others.

### Deleting from a Specific Scope

Use these options to target a specific scope:

```bash title="Delete from local scope"
chatx prompt delete code-review --local
```

```bash title="Delete from user scope"
chatx prompt delete translate --user
```

```bash title="Delete from global scope"
chatx prompt delete explain --global
```

### Deletion Search Order

When you use `prompt delete` without specifying a scope (or with `--any`), ChatX searches for prompts in this order:
1. Local scope (current directory)
2. User scope (user's home directory)
3. Global scope (system-wide)

It will delete the first matching prompt it finds.

## Bypassing Confirmation

By default, ChatX will ask for confirmation before deleting a prompt:

```
Are you sure you want to delete prompt 'translate' from local scope? [y/N]:
```

To skip this confirmation, use the `--yes` or `-y` option:

```bash
chatx prompt delete translate --yes
```

This is useful for automated scripts or when deleting multiple prompts.

## What Gets Deleted

When you delete a prompt, ChatX will:

1. Remove the prompt definition file from the appropriate scope directory
2. Remove any associated files referenced by the prompt
3. Display the paths of all deleted files

## Common Use Cases

### Cleaning Up Obsolete Prompts

Periodically review and remove prompts you no longer use:

```bash
# List all prompts first
chatx prompt list

# Delete those you don't need
chatx prompt delete outdated-prompt
```

### Making Way for Updates

To update a prompt, you typically need to delete it first, then create it again:

```bash
# Delete old version
chatx prompt delete translate --user

# Create updated version
chatx prompt create translate "Translate the following text to {language} and provide pronunciation tips:" --user
```

### Removing Duplicates

If you have the same prompt in multiple scopes, you might want to clean this up:

```bash
# Check where prompts exist
chatx prompt get translate --any

# Delete from specific scopes
chatx prompt delete translate --local
chatx prompt delete translate --user
```

## Best Practices

1. **List before deleting**: Use `chatx prompt list` to see what prompts exist before deleting
2. **Check prompt content**: Use `chatx prompt get` to verify you're deleting the right prompt
3. **Be specific about scope**: Use scope flags (--local, --user, --global) to avoid unintended deletions
4. **Back up important prompts**: Consider saving the text of important prompts before deletion
5. **Use confirmation**: Unless in scripts, avoid the --yes flag to prevent accidental deletions

## See Also

- [chatx prompt delete](../reference/cli/prompt/delete.md) - Command reference
- [chatx prompt create](../reference/cli/prompt/create.md) - Creating prompts
- [chatx prompt list](../reference/cli/prompt/list.md) - Listing prompts
- [chatx prompt get](../reference/cli/prompt/get.md) - Viewing prompts