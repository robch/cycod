# --use-openai

The `--use-openai` option explicitly selects OpenAI API as the chat provider for your CYCOD session.

## Synopsis

```bash
cycod --use-openai [other options]
```

## Description

When you specify `--use-openai`, CYCOD will:

- Use OpenAI API for the chat session
- Override any default provider settings
- Expect proper OpenAI configuration to be available

This option is useful when you want to explicitly use OpenAI for a specific command, regardless of your default provider settings.

## Related OpenAI Options

The `--use-openai` option is typically used with the following OpenAI-specific options:

- `--openai-api-key`: Provides the API key for authentication with OpenAI
- `--openai-chat-model-name`: Specifies which model to use (default: gpt-4o)

## Examples

**Basic usage:**

```bash
cycod --use-openai --question "What is OpenAI?"
```

**Using with a specific model:**

```bash
cycod --use-openai \
      --openai-chat-model-name "gpt-4-turbo" \
      --question "Explain quantum computing"
```

**Interactive chat using OpenAI:**

```bash
cycod --use-openai --interactive
```

## Configuration

Instead of specifying this option on each command, you can set OpenAI as your default provider using:

```bash
cycod config set app.preferredProvider openai --user
```

Or create a profile for using OpenAI by creating a YAML file:

```yaml title="openai.yaml (in .cycod/profiles directory)"
app:
  preferredProvider: "openai"

openai:
  chatModelName: "gpt-4o"
```

Then use it with:

```bash
cycod --profile openai --question "Your question"
```

## Notes

- If both configuration and command-line options are present, the command-line option takes precedence.
- Make sure you have a valid OpenAI API key before using this option.
- This option requires either pre-configured OpenAI settings or the relevant OpenAI options to be specified in the same command.

## See Also

- [--use-azure-openai](./use-azure-openai.md) (for using Azure OpenAI API instead)
- [--use-copilot](./use-copilot.md) (for using GitHub Copilot instead)
- [OpenAI Provider Documentation](../../../providers/openai.md)
- [Provider Selection Guide](../../../providers/overview.md)