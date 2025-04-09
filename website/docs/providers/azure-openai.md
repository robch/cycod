# Azure OpenAI Provider

CHATX can connect directly to Azure OpenAI services, allowing you to use your organization's Azure deployments. This guide will help you set up and use Azure OpenAI with CHATX.

## Prerequisites

1. An Azure account with access to Azure OpenAI resources
2. An Azure OpenAI endpoint with deployed models
3. Authentication credentials for the Azure OpenAI service

## Configuration

You can configure the Azure OpenAI provider in CHATX using the `config` command:

```bash
chatx config set azure.openai.endpoint YOUR_ENDPOINT --user
chatx config set azure.openai.apiKey YOUR_API_KEY --user
chatx config set azure.openai.chatDeployment YOUR_DEPLOYMENT_NAME --user
```

These commands store your Azure OpenAI settings in the user-level configuration, making them available for all your CHATX sessions.

## Configuration Values

| Setting | Description | Example Value |
|---------|-------------|---------------|
| `azure.openai.endpoint` | Your Azure OpenAI service endpoint | `https://myresource.openai.azure.com` |
| `azure.openai.apiKey` | The API key for authentication | `00000000000000000000000000000000` |
| `azure.openai.chatDeployment` | The deployment name for chat completions | `gpt-4` |

## Command-Line Options

You can also provide Azure OpenAI settings directly in your commands:

```bash
chatx --use-azure-openai --azure-openai-endpoint YOUR_ENDPOINT --azure-openai-api-key YOUR_API_KEY --azure-openai-chat-deployment YOUR_DEPLOYMENT --question "What is Azure OpenAI?"
```

Using the `--use-azure-openai` flag (or just `--use-azure`) explicitly tells CHATX to use Azure OpenAI as the provider.

## Example Usage

Basic query using Azure OpenAI:

```bash title="Basic query"
chatx --use-azure --question "Explain what Azure OpenAI is"
```

Interactive chat with Azure OpenAI:

```bash title="Interactive chat"
chatx --use-azure --interactive
```

## Working with Multiple Azure OpenAI Deployments

If you have multiple Azure OpenAI deployments, you can set up different profiles for each:

1. Create a profile for each deployment:

```bash
# Create a profile for GPT-4 deployment
chatx --use-azure --azure-openai-chat-deployment gpt-4 --save-profile azure-gpt4

# Create a profile for GPT-3.5 deployment
chatx --use-azure --azure-openai-chat-deployment gpt-35-turbo --save-profile azure-gpt35
```

2. Use the profiles as needed:

```bash
# Use GPT-4 deployment
chatx --profile azure-gpt4 --question "Complex question requiring GPT-4"

# Use GPT-3.5 deployment
chatx --profile azure-gpt35 --question "Simple question for GPT-3.5"
```

## Security Considerations

When working with Azure OpenAI API keys:

1. Use the `--user` scope to keep credentials isolated to your user account
2. Consider using Azure AD authentication when available
3. Regularly rotate your API keys following your organization's security policies
4. Never share configuration files containing API keys

## Troubleshooting

If you encounter issues with Azure OpenAI, try these steps:

1. Verify your endpoint URL is correct and includes the full domain
2. Check that your deployment names match exactly what's in Azure
3. Ensure your API key has the correct permissions
4. Verify network connectivity to Azure services

For more detailed information, refer to the [Azure OpenAI Service documentation](https://learn.microsoft.com/en-us/azure/ai-services/openai/).