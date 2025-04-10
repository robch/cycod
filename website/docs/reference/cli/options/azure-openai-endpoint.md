# --azure-openai-endpoint

This option allows you to specify the Azure OpenAI service endpoint URL to use when connecting to Azure OpenAI services.

## Syntax

```bash
chatx --azure-openai-endpoint URL
```

Where `URL` is your Azure OpenAI service endpoint, typically in the format `https://your-resource-name.openai.azure.com`.

## Description

When using Azure OpenAI as your provider, you need to specify the service endpoint that hosts your Azure OpenAI deployments. This endpoint is unique to your Azure resource and is required to connect to your Azure OpenAI API.

The `--azure-openai-endpoint` option allows you to:

1. Specify the URL endpoint directly in the command line
2. Override any endpoint that may be configured in your profile or configuration files
3. Target specific Azure OpenAI resources for different workloads

## Examples

### Basic Usage

```bash
chatx --use-azure-openai \
      --azure-openai-endpoint "https://my-resource.openai.azure.com" \
      --question "What is the capital of France?"
```

### Complete Azure OpenAI Configuration

```bash
chatx --use-azure-openai \
      --azure-openai-endpoint "https://my-resource.openai.azure.com" \
      --azure-openai-api-key "your-api-key" \
      --azure-openai-chat-deployment "gpt-4" \
      --question "Explain quantum computing"
```

### Using with Interactive Mode

```bash
chatx --use-azure-openai \
      --azure-openai-endpoint "https://my-resource.openai.azure.com" \
      --interactive
```

## Setting in Configuration

Instead of specifying the endpoint in each command, you can set it in your configuration:

```bash
# Set in user configuration (recommended)
chatx config set azure.openai.endpoint https://my-resource.openai.azure.com --user

# Set in local configuration (current directory only)
chatx config set azure.openai.endpoint https://my-resource.openai.azure.com --local
```

## Finding Your Azure OpenAI Endpoint

You can find your Azure OpenAI endpoint in the Azure portal:

1. Sign in to the [Azure Portal](https://portal.azure.com)
2. Navigate to your Azure OpenAI resource
3. Select "Keys and Endpoint" from the left menu
4. Copy the Endpoint value

## Related Options

| Option | Description |
|--------|-------------|
| `--use-azure-openai` | Explicitly select Azure OpenAI as the provider |
| `--use-azure` | Alias for `--use-azure-openai` |
| `--azure-openai-api-key` | Specify the authentication key for Azure OpenAI |
| `--azure-openai-chat-deployment` | Specify the deployment name for chat completions |

## See Also

- [Azure OpenAI Provider](../../providers/azure-openai.md)
- [`--azure-openai-api-key`](azure-openai-api-key.md)
- [`--azure-openai-chat-deployment`](azure-openai-chat-deployment.md)
- [`--use-azure-openai`](use-azure-openai.md)