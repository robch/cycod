# --quiet

The `--quiet` option suppresses non-critical console output during the execution of CycoD commands, providing cleaner output that focuses only on the AI's responses and essential messages.

## Syntax

```bash
cycod --quiet [other options]
```

## Description

The `--quiet` option minimizes the console output produced by CycoD. When enabled, it:

- Suppresses informational messages and status updates
- Hides program banners and progress indicators
- Preserves error messages and warnings (which are shown regardless of this setting)
- Keeps the AI model's responses visible

This option is particularly useful when:
- Incorporating CycoD into scripts or automated workflows
- Piping output to other commands or files
- Creating cleaner output for readability
- Using CycoD in non-interactive environments

Note that the `--question` and `--questions` options automatically apply the `--quiet` flag, as these are typically used in scenarios where minimal output is desired.

## Examples

### Basic Usage

Get a clean response without any extra output:

```bash
cycod --quiet --input "What is the capital of France?"
```

### Integrating with Scripts

Use quiet mode when capturing output for a script:

```bash
result=$(cycod --quiet --input "Translate 'hello' to Spanish")
echo "The translation is: $result"
```

### Piping to Other Commands

Send the output through a pipeline with minimal noise:

```bash
cycod --quiet --input "List 10 prime numbers" | grep -E "^[0-9]+"
```

### Combined with Input File

Process a file without extra console output:

```bash
cycod --quiet --input @report.txt --question "Summarize this report"
```

### Combined with Output Files

When saving results to a file, suppress status messages:

```bash
cycod --quiet --question "Write a short poem" --output-trajectory poem.md
```

## Best Practices

1. **Use for Automation**: Always include the `--quiet` flag in scripts and automated processes.

2. **Prefer `--question` for Single Queries**: The `--question` option already includes `--quiet` behavior.

3. **Combine with Redirection**: For completely silent operation except for the result:
   ```bash
   cycod --quiet --question "Calculate 15 * 24" > result.txt 2>/dev/null
   ```

4. **Error Handling**: Remember that errors and warnings will still be shown even in quiet mode, so you can still detect issues.

## Related Options

- [`--debug`](debug.md): Enables diagnostic output (overrides `--quiet` for debug messages)
- [`--verbose`](verbose.md): Increases the verbosity of console output (opposite of `--quiet`)
- [`--question`](question.md): An alias that combines `--interactive false`, `--quiet`, and `--input`
- [`--questions`](questions.md): An alias that combines `--interactive false`, `--quiet`, and `--inputs`