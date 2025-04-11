--8<-- "snippets/ai-generated.md"

# Troubleshooting ChatX

This guide will help you diagnose and resolve common issues when using ChatX.

## Diagnostic Options

ChatX provides several command-line options to help diagnose issues:

### Debug Mode

The `--debug` option provides detailed diagnostic information about what's happening inside ChatX:

```bash
chatx --debug --question "My question that isn't working"
```

Debug output appears in cyan text and includes information about:
- Configuration settings
- API connections
- Threading operations
- File operations
- Internal processing steps

When reporting issues, including debug output can be extremely helpful for developers to understand what's happening.

### Verbose Mode

For even more detailed diagnostic information, use the `--verbose` option:

```bash
chatx --verbose --question "My question that isn't working"
```

This option provides extensive details about every step of the execution process.

### Combining Options

For the most comprehensive diagnostic information, combine both options:

```bash
chatx --debug --verbose --question "My question that isn't working"
```

## Common Issues and Solutions

### API Connection Problems

If you're having trouble connecting to OpenAI, Azure OpenAI, or GitHub Copilot, try:

```bash
chatx --debug --use-openai --question "Test"
```

This will show the connection attempt details, including endpoints and authentication information (with sensitive details redacted).

### Chat History Issues

If chat history isn't loading or saving correctly:

```bash
chatx --debug --input-chat-history my-history.jsonl --question "Continue"
```

The debug output will show how the history is being processed and any issues encountered.

### Thread Management

If you're using multiple threads and encountering issues:

```bash
chatx --debug --threads 4 --foreach var i in 1..10 --question "Item {i}"
```

This will show how threads are being allocated and managed.

## Capturing Debug Output

For complex issues, it's often helpful to capture the full debug output to a file:

```bash
chatx --debug --question "My problematic command" > debug_log.txt 2>&1
```

This redirects both standard output and error messages to the file for later analysis.

## Getting Help

If you continue to experience issues after trying these troubleshooting steps:

1. Check the [GitHub Issues](https://github.com/robch/chatx/issues) to see if your problem has already been reported
2. Create a new issue with:
   - A clear description of the problem
   - Steps to reproduce the issue
   - The debug output from the command
   - Your system information (OS, .NET version, etc.)

## Environment Validation

To quickly check if your environment is properly configured:

```bash
chatx --debug --question "Echo back my environment information"
```

This will display details about your configuration including:
- Provider settings
- API endpoints
- Authentication status
- File paths