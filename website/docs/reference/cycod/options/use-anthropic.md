# --use-anthropic

The `--use-anthropic` option explicitly selects Anthropic Claude as the chat provider for your CYCOD session.

## Synopsis

```bash
cycod --use-anthropic [other options]
```

## Description

When you specify `--use-anthropic`, CYCOD will:

- Use Anthropic Claude API for the chat session
- Override any default provider settings
- Expect proper Anthropic configuration to be available

This option is useful when you want to explicitly use Claude models for a specific command, regardless of your default provider settings.

## Related Anthropic Options

The `--use-anthropic` option is typically used with the following Claude-specific options:

- [`--anthropic-api-key`](./anthropic-api-key.md): Provides the authentication key for the Anthropic API
- [`--anthropic-model-name`](./anthropic-model.md): Specifies which Claude model to use

## Examples

**Basic usage:**

```bash
cycod --use-anthropic --question "What is Claude?"
```

**Using with specific model:**

```bash
cycod --use-anthropic \
      --anthropic-model-name claude-3-7-sonnet-20250219 \
      --question "Explain quantum computing"
```


## Configuration

Instead of specifying this option on each command, you can set Anthropic as your default provider using:

```bash
cycod config set app.preferredProvider anthropic --user
```

Or create a profile for using Claude by creating a YAML file:

```yaml title="claude.yaml (in .cycod/profiles directory)"
app:
  preferredProvider: "anthropic"
```

Then use it with:

```bash
cycod --profile claude --question "What is Claude?"
```

## Notes

- Make sure you have a valid Anthropic API key before using this option.
- This option requires either pre-configured Anthropic settings or the relevant Anthropic options to be specified in the same command.

## See Also

- [--use-openai](./use-openai.md) (for using the OpenAI API instead)
- [--use-gemini](./use-gemini.md) (for using Google Gemini instead)
- [--use-azure-openai](./use-azure-openai.md) (for using Azure OpenAI API instead)
- [--use-copilot](./use-copilot.md) (for using GitHub Copilot instead)
- [Anthropic Provider Documentation](../../../providers/anthropic.md)
- [Provider Selection Guide](../../../providers/overview.md)