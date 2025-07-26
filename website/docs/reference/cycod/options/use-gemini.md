# --use-gemini

The `--use-gemini` option explicitly selects Google Gemini API as the chat provider for your CYCOD session.

## Synopsis

```bash
cycod --use-gemini [other options]
```

## Description

When you specify `--use-gemini`, CYCOD will:

- Use Google Gemini API for the chat session
- Override any default provider settings
- Expect proper Gemini API configuration to be available

This option is useful when you want to explicitly use Google Gemini for a specific command, regardless of your default provider settings.

## Related Gemini Options

The `--use-gemini` option is typically used with the following Gemini-specific options:

- [`--google-gemini-api-key`](./gemini-api-key.md): Provides the API key for the Gemini API
- [`--google-gemini-model-id`](./gemini-model.md): Specifies which Gemini model to use

## Examples

**Basic usage:**

```bash
cycod --use-gemini --question "What is Google Gemini?"
```

**Using with specific model:**

```bash
cycod --use-gemini \
      --google-gemini-model-id gemini-pro \
      --question "Explain quantum computing"
```

**Interactive chat using Gemini:**

```bash
cycod --use-gemini --interactive
```

**Using with API key:**

```bash
cycod --use-gemini \
      --google-gemini-api-key "YOUR_API_KEY" \
      --question "Help me write a Python function"
```

## Configuration

Instead of specifying this option on each command, you can set Google Gemini as your default provider using:

```bash
cycod config set app.preferredProvider gemini --user
```

Or create a profile for using Gemini by creating a YAML file:

```yaml title="gemini.yaml (in .cycod/profiles directory)"
app:
  preferredProvider: "gemini"
```

Then use it with:

```bash
cycod --profile gemini --question "What are the capabilities of Gemini models?"
```

## Notes

- Make sure you have a valid Google Gemini API key before using this option.
- This option requires either pre-configured Gemini settings or the relevant Gemini options to be specified in the same command.

## See Also

- [--use-openai](./use-openai.md) (for using the OpenAI API instead)
- [--use-azure-openai](./use-azure-openai.md) (for using the Azure OpenAI API instead)
- [--use-anthropic](./use-anthropic.md) (for using the Anthropic Claude API instead)
- [--use-copilot](./use-copilot.md) (for using GitHub Copilot instead)
- [Google Gemini Provider Documentation](../../../providers/gemini.md)
- [Provider Selection Guide](../../../providers/overview.md)