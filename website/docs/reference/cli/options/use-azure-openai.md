# --use-azure-openai

The `--use-azure-openai` option explicitly selects Azure OpenAI API as the chat provider for your CHATX session.

## Synopsis

```bash
chatx --use-azure-openai [other options]
```

## Description

When you specify `--use-azure-openai`, CHATX will:

- Use Azure OpenAI API for the chat session
- Override any default provider settings
- Expect proper Azure OpenAI configuration to be available

This option is useful when you want to explicitly use Azure OpenAI for a specific command, regardless of your default provider settings.

## Alias

`--use-azure` is a shorter alias for `--use-azure-openai` and functions identically.

## Related Azure OpenAI Options

The `--use-azure-openai` option is typically used with the following Azure-specific options:

- [`--azure-openai-endpoint`](./azure-openai-endpoint.md): Specifies the Azure OpenAI service endpoint URL
- [`--azure-openai-api-key`](./azure-openai-api-key.md): Provides the authentication key for the Azure OpenAI service
- [`--azure-openai-chat-deployment`](./azure-openai-chat-deployment.md): Specifies which model deployment to use

## Examples

**Basic usage:**

```bash
chatx --use-azure-openai --question "What is Azure OpenAI?"
```

**Using with endpoint and deployment:**

```bash
chatx --use-azure-openai \
      --azure-openai-endpoint "https://my-resource.openai.azure.com" \
      --azure-openai-chat-deployment "gpt-4" \
      --question "Explain quantum computing"
```

**Using the shorter alias:**

```bash
chatx --use-azure --question "How does Azure OpenAI differ from standard OpenAI?"
```

**Interactive chat using Azure OpenAI:**

```bash
chatx --use-azure-openai --interactive
```

## Configuration

Instead of specifying this option on each command, you can set Azure OpenAI as your default provider using:

```bash
chatx config set app.preferredProvider azure-openai --user
```

Or create a profile for using Azure OpenAI by creating a YAML file:

```yaml title="azure.yaml (in .chatx/profiles directory)"
app:
  preferredProvider: "azure-openai"
```

Then use it with:

```bash
chatx --profile azure --question "Your question"
```

## Notes

- If both configuration and command-line options are present, the command-line option takes precedence.
- Make sure you have a valid Azure OpenAI deployment and credentials before using this option.
- This option requires either pre-configured Azure OpenAI settings or the relevant Azure OpenAI options to be specified in the same command.

## See Also

- [--use-azure](./use-azure.md) (alias for this option)
- [--use-openai](./use-openai.md) (for using the OpenAI API instead)
- [--use-copilot](./use-copilot.md) (for using GitHub Copilot instead)
- [Azure OpenAI Provider Documentation](../../../providers/azure-openai.md)
- [Provider Selection Guide](../../../providers/index.md)