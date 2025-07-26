# --use-copilot

The `--use-copilot` option explicitly selects GitHub Copilot as the chat provider for your CYCOD session.

## Synopsis

```bash
cycod --use-copilot [other options]
```

## Description

When you specify `--use-copilot`, CYCOD will:

- Use GitHub Copilot for the chat session
- Override any default provider settings
- Default to using the `claude-3.7-sonnet` model unless otherwise specified

This option is useful when you want to explicitly use GitHub Copilot for a specific command, regardless of your default provider settings.

## Related Copilot Options

The `--use-copilot` option can be used with the following GitHub Copilot specific options:

- `--copilot-model-name`: Specifies which model to use (default: claude-3.7-sonnet)
- `--copilot-api-endpoint`: Specifies a custom API endpoint (default: https://api.githubcopilot.com)
- `--github-token`: Provides a specific GitHub authentication token

## Authentication

Before using `--use-copilot`, you need to authenticate with GitHub using `cycod github login`. This will open a browser for you to authorize CYCOD with GitHub.

## Examples

**Basic usage:**

```bash
cycod --use-copilot --question "What is GitHub Copilot?"
```

**Specifying a different model:**

```bash
cycod --use-copilot --copilot-model-name claude-3-opus --question "Explain quantum computing"
```

**Using with custom API endpoint:**

```bash
cycod --use-copilot \
      --copilot-api-endpoint "https://custom-endpoint.example.com" \
      --question "How does GitHub Copilot work?"
```

## Configuration

Instead of specifying this option on each command, you can set GitHub Copilot as your default provider using:

```bash
cycod config set app.preferredProvider copilot --user
```

Or create a profile for using GitHub Copilot by creating a YAML file:

```yaml title="copilot.yaml (in .cycod/profiles directory)"
app:
  preferredProvider: "copilot"

copilot:
  modelName: "claude-3.7-sonnet"
```

Then use it with:

```bash
cycod --profile copilot --question "Your question"
```

## Notes

- If both configuration and command-line options are present, the command-line option takes precedence.
- Make sure you have a valid GitHub Copilot subscription and have authenticated before using this option.

## See Also

- [--use-openai](./use-openai.md) (for using the OpenAI API instead)
- [--use-azure-openai](./use-azure-openai.md) (for using the Azure OpenAI API instead)
- [GitHub Copilot Provider Documentation](../../../providers/github-copilot.md)
- [Provider Selection Guide](../../../providers/overview.md)