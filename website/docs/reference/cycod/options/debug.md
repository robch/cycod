# --debug

The `--debug` option enables diagnostic output during the execution of CycoD commands, providing insight into the application's internal operations.

## Syntax

```bash
cycod --debug [other options]
```

## Description

When troubleshooting issues or developing scripts that use CycoD, it's often useful to see what's happening behind the scenes. The `--debug` option displays detailed diagnostic information about the execution of your command, such as:

- Configuration values being used
- Threading and execution flow
- File operations
- API interactions
- Internal decision-making processes

Debug messages appear in cyan text to distinguish them from regular output, making them easy to identify in the command console.

The `--debug` option is designed for troubleshooting and development purposes. For normal usage, this option should be omitted unless you're diagnosing a specific issue.

## Examples

### Basic Usage

View debug information when asking a simple question:

```bash
cycod --debug --question "What time is it?"
```

### Troubleshooting Chat History Issues

Enable debug output when loading a chat history file to see how it's processed:

```bash
cycod --debug --input-chat-history previous-chat.jsonl --question "Continue our discussion"
```

### Diagnosing API Connection Problems

See detailed information about API connection attempts:

```bash
cycod --debug --use-openai --question "Why isn't my connection working?"
```

### Understanding Thread Management

When using multiple threads for batch processing, see how they're allocated:

```bash
cycod --debug --threads 4 --foreach var i in 1..10 --question "Processing item {i}"
```

## Best Practices

1. **Isolate Issues**: When troubleshooting, start with the minimal command that reproduces your issue and add `--debug` to see what's happening.

2. **Capture Output**: For complex issues, redirect the debug output to a file for analysis:
   ```bash
   cycod --debug --question "My complex question" > debug_log.txt 2>&1
   ```

3. **Combine with Verbose**: For even more detailed information, use both `--debug` and `--verbose`:
   ```bash
   cycod --debug --verbose --question "Tell me what's happening"
   ```

4. **Avoid in Production**: The `--debug` option can generate significant output that might interfere with scripts or automation. Use it only during development and troubleshooting.

## Related Options

- [`--verbose`](verbose.md): Provides even more detailed diagnostic information than `--debug`
- [`--quiet`](quiet.md): Suppresses most console output except for critical messages and the AI responses (debug messages will still appear)