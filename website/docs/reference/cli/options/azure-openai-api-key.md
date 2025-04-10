# --azure-openai-api-key

The `--azure-openai-api-key` option allows you to specify an authentication key for Azure OpenAI API when using CHATX.

## Syntax

```bash
--azure-openai-api-key KEY
```

## Description

This option sets the API key used to authenticate with Azure OpenAI services. When provided, CHATX will use this key to authenticate requests to your Azure OpenAI deployment, overriding any keys stored in your configuration.

The API key is a sensitive credential that grants access to your Azure OpenAI resources. It should be handled securely and never shared publicly.

## Obtaining an API Key

To get an Azure OpenAI API key:

1. Sign in to the [Azure Portal](https://portal.azure.com)
2. Navigate to your Azure OpenAI resource
3. Select "Keys and Endpoint" from the left menu
4. Copy one of the two available keys (either Key 1 or Key 2)

## Examples

### Example 1: Basic usage with Azure OpenAI

```bash
chatx --use-azure-openai --azure-openai-api-key "your-api-key" --question "What is Azure OpenAI?"
```

### Example 2: Complete Azure OpenAI configuration

```bash
chatx --use-azure-openai \
      --azure-openai-endpoint "https://your-resource.openai.azure.com" \
      --azure-openai-api-key "your-api-key" \
      --azure-openai-chat-deployment "your-deployment-name" \
      --question "Explain how Azure OpenAI works"
```

### Example 3: Interactive chat with Azure OpenAI

```bash
chatx --use-azure-openai \
      --azure-openai-api-key "your-api-key" \
      --azure-openai-endpoint "https://your-resource.openai.azure.com" \
      --interactive
```

### Example 4: Using an environment variable (recommended for scripts)

```bash
# Set the key in an environment variable
export AZURE_OPENAI_API_KEY="your-api-key"

# Use the environment variable in your command
chatx --use-azure-openai --azure-openai-api-key "$AZURE_OPENAI_API_KEY" --question "What is Azure OpenAI?"
```

## Configuration Alternative

For regular use, it's recommended to store your API key in the CHATX configuration instead of passing it on the command line:

```bash
chatx config set azure.openai.apiKey "your-api-key" --user
```

This stores the API key in your user configuration, making it available for all CHATX sessions without having to specify it on the command line.

## Configuration Precedence

When CHATX looks for the Azure OpenAI API key, it checks sources in this order:

1. Command-line option (`--azure-openai-api-key`)
2. Environment variable (`AZURE_OPENAI_API_KEY`)
3. Configuration files (local, then user, then global scope)

Using the command-line option will override any value set in environment variables or configuration files.

## Security Considerations

- Avoid including the API key directly in scripts that might be shared or stored in version control
- Consider using environment variables for scripts and CI/CD pipelines
- Regularly rotate API keys according to your security policies
- Use the `--user` scope when storing API keys in configuration to keep them private to your user account
- When using the key in scripts, consider reading it from a secure location like a secret manager
- Be aware that API keys in command history might be accessible to other users on the system

## Related Options

| Option | Description |
|--------|-------------|
| `--use-azure-openai` | Explicitly select Azure OpenAI as the provider |
| `--azure-openai-endpoint` | Specify the Azure OpenAI service endpoint URL |
| `--azure-openai-chat-deployment` | Specify the deployment name for chat completions |

## See Also

- [Azure OpenAI Provider](../../../providers/azure-openai.md)
- [Configuration](../../../usage/configuration.md)