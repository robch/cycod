# --use-azure

The `--use-azure` option is an alias for [`--use-azure-openai`](./use-azure-openai.md) and explicitly selects Azure OpenAI API as the chat provider for your CYCOD session.

## Synopsis

```bash
cycod --use-azure [other options]
```

## Description

`--use-azure` is a shorter, more convenient alias for [`--use-azure-openai`](./use-azure-openai.md). Both options function identically, telling CYCOD to use Azure OpenAI API for the chat session.

When you specify `--use-azure`, CYCOD will:

- Use Azure OpenAI API for the chat session
- Override any default provider settings
- Expect proper Azure OpenAI configuration to be available

## Examples

**Basic usage:**

```bash
cycod --use-azure --question "What is Azure OpenAI?"
```

**Using with endpoint and deployment:**

```bash
cycod --use-azure \
      --azure-openai-endpoint "https://my-resource.openai.azure.com" \
      --azure-openai-chat-deployment "gpt-4" \
      --question "Explain quantum computing"
```

## Notes

- This is functionally identical to using `--use-azure-openai`
- This option requires either pre-configured Azure OpenAI settings or the relevant Azure OpenAI options to be specified in the same command

## See Also

- [--use-azure-openai](./use-azure-openai.md) (the full version of this alias)
- [--use-openai](./use-openai.md) (for using the OpenAI API instead)
- [--use-copilot](./use-copilot.md) (for using GitHub Copilot instead)
- [Azure OpenAI Provider Documentation](../../../providers/azure-openai.md)
- [Provider Selection Guide](../../../providers/overview.md)