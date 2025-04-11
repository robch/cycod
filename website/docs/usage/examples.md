--8<-- "snippets/ai-generated.md"

# CLI Usage Examples

This page provides practical examples of using ChatX across different operating systems. Choose your platform from the tabs below to see platform-specific examples.

## Basic Usage

These examples cover the most common ChatX commands and options.

=== "Windows"
    
    --8<-- "snippets/cli-examples/basic/windows-examples.md"

=== "macOS"
    
    --8<-- "snippets/cli-examples/basic/macos-examples.md"

=== "Linux"
    
    --8<-- "snippets/cli-examples/basic/linux-examples.md"

## Advanced Usage

These examples demonstrate more advanced features and configurations.

=== "Windows"
    
    --8<-- "snippets/cli-examples/advanced/windows-examples.md"

=== "macOS"
    
    --8<-- "snippets/cli-examples/advanced/macos-examples.md"

=== "Linux"
    
    --8<-- "snippets/cli-examples/advanced/linux-examples.md"

## Using Multiple Providers

You can specify which AI provider to use for each command:

```bash
# Use OpenAI
chatx --use-openai --question "What is GPT-4?"

# Use Azure OpenAI
chatx --use-azure-openai --question "What is Azure OpenAI?"

# Use GitHub Copilot
chatx --use-copilot --question "What is GitHub Copilot?"
```

## Working with Chat History

ChatX allows you to save and reuse chat history:

```bash
# Save chat history to a file
chatx --question "Tell me about AI" --output-chat-history chat.jsonl

# Continue the conversation from a saved chat history
chatx --input-chat-history chat.jsonl --question "Tell me more about machine learning"

# Output conversation trajectory for analysis
chatx --question "Explain quantum computing" --output-trajectory trajectory.md
```

## Environment-Specific Tips

=== "Windows"

    - On Windows, you can use PowerShell, Command Prompt, or Windows Terminal
    - File paths use backslashes (e.g., `C:\Users\username\Documents\file.txt`)
    - Environment variables are accessed using `%VARIABLE%` in CMD or `$env:VARIABLE` in PowerShell

=== "macOS"

    - On macOS, you can use Terminal.app or iTerm2
    - File paths use forward slashes (e.g., `/Users/username/Documents/file.txt`)
    - Use `~/` as a shortcut to your home directory
    - Environment variables are accessed using `$VARIABLE`

=== "Linux"

    - On Linux, you can use your distribution's terminal emulator
    - File paths use forward slashes (e.g., `/home/username/Documents/file.txt`)
    - Use `~/` as a shortcut to your home directory
    - Environment variables are accessed using `$VARIABLE`

## See Also

- [Chat Basics](/usage/basics.md) - Learn more about basic ChatX usage
- [Configuration](/usage/configuration.md) - How to configure ChatX
- [Templates and Variables](/usage/templates-and-variables.md) - Working with templates