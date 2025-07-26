# --verbose

The `--verbose` option enables highly detailed diagnostic output during the execution of CycoD commands, providing comprehensive insight into all operations and internal processes.

## Syntax

```bash
cycod --verbose [other options]
```

## Description

The `--verbose` option provides the maximum level of diagnostic information about CycoD's execution. It expands on the `--debug` option by showing additional details that are normally hidden, such as:

- Complete API request and response data
- Detailed token calculations and management
- Full configuration resolution process
- Internal state transitions and decision points
- Memory usage and performance metrics
- Dependency loading and initialization

Verbose messages appear in magenta text, while standard debug messages appear in cyan, making them easy to distinguish in the console output.

This option is primarily intended for:
- Developers working on CycoD itself
- Users experiencing complex issues that require deep inspection
- Advanced users wanting to understand CycoD's internal workings

## Examples

### Examining API Interactions

View complete details of API communications:

```bash
cycod --verbose --question "What's happening in the API layer?"
```

### Troubleshooting Configuration Issues

See the complete configuration resolution process:

```bash
cycod --verbose --config my-config.yml --question "How is my configuration loaded?"
```

### Tracking Token Usage

Monitor token calculations in detail:

```bash
cycod --verbose --input-chat-history previous-chat.jsonl --question "Continue our discussion"
```

### Debugging Template Processing

See how variables and templates are processed:

```bash
cycod --verbose --var name=World --question "Hello {name}, how does this work?"
```

## Best Practices

1. **Capture to File**: Verbose output can be extensive. Save it to a file for analysis:
   ```bash
   cycod --verbose --question "My question" > verbose_log.txt 2>&1
   ```

2. **Isolate Components**: When troubleshooting a specific component, combine with relevant options:
   ```bash
   cycod --verbose --use-azure-openai --question "Test Azure connection"
   ```

3. **Gradual Diagnosis**: Start with `--debug` first, and if more information is needed, escalate to `--verbose`.

4. **Development Use**: Use `--verbose` during development to understand how your changes affect the system.

5. **Avoid in Production**: The `--verbose` option generates significant output that will interfere with normal operation. Use only for troubleshooting.

## Related Options

- [`--debug`](debug.md): Provides standard diagnostic information (less detailed than `--verbose`)
- [`--quiet`](quiet.md): Suppresses most console output except for critical messages and AI responses