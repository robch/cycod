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

### Authentication with `--azure-openai-api-key`

The `--azure-openai-api-key` option allows you to specify your Azure OpenAI API key directly in the command. This is useful for:

- Testing a new API key without changing your configuration
- Running one-off commands with a different key
- Scripts where you want to pass the key as an environment variable

```bash
# Using an API key directly in a command
chatx --use-azure --azure-openai-api-key "your-api-key" --question "What's the weather like today?"

# Using an environment variable to provide the API key (more secure for scripts)
chatx --use-azure --azure-openai-api-key "$AZURE_OPENAI_API_KEY" --question "What's the weather like today?"
```

For regular use, storing your API key in configuration (as shown above) is recommended rather than specifying it on every command.

### Specifying Endpoints with `--azure-openai-endpoint`

The `--azure-openai-endpoint` option allows you to specify the Azure OpenAI service endpoint URL directly in the command. This is useful for:

- Testing a new Azure OpenAI resource without changing your configuration
- Running commands against different endpoints for different regions
- Scripts where you need to target specific Azure resources dynamically

```bash
# Using a specific endpoint directly in a command
chatx --use-azure --azure-openai-endpoint "https://my-resource.openai.azure.com" --question "What is Azure OpenAI?"

# Targeting a different region or resource
chatx --use-azure --azure-openai-endpoint "https://my-backup-resource.openai.azure.com" --question "What are the benefits of Azure?"
```

Finding your endpoint:
1. Sign in to the Azure Portal
2. Navigate to your Azure OpenAI resource
3. Select "Keys and Endpoint" from the left menu
4. Copy the Endpoint value (typically in the format `https://your-resource.openai.azure.com`)

For regular use, storing your endpoint in configuration (as shown in the Configuration section) is recommended rather than specifying it on every command.

### Specifying Deployments with `--azure-openai-chat-deployment`

The `--azure-openai-chat-deployment` option specifies which deployment to use for chat completions:

```bash
# Using a specific deployment
chatx --use-azure --azure-openai-chat-deployment "gpt-4" --question "Can you help me design a system?"

# Switching between deployments for different tasks
chatx --use-azure --azure-openai-chat-deployment "gpt-35-turbo" --question "Simple question"
chatx --use-azure --azure-openai-chat-deployment "gpt-4" --question "Complex reasoning task"
```

Benefits of explicitly specifying deployments:

- Use different models for different types of tasks
- Test new model deployments without changing your configuration
- Control costs by selecting more economical deployments for simpler tasks
- Ensure critical tasks use your most capable models

For regular use with the same deployment, store this in your configuration or create profiles as shown in the "Working with Multiple Deployments" section.

## Example Usage

Basic query using Azure OpenAI:

```bash title="Basic query"
chatx --use-azure --question "Explain what Azure OpenAI is"
```

Interactive chat with Azure OpenAI:

```bash title="Interactive chat"
chatx --use-azure --interactive
```

## Understanding Azure OpenAI Deployments

In Azure OpenAI, a "deployment" is an instance of a specific model that you've deployed to your Azure OpenAI resource. Each deployment:

- Is associated with a specific model (like GPT-4, GPT-3.5-Turbo, etc.)
- Has a unique deployment name that you choose when creating it
- Has its own quota and rate limits 
- May have different configuration settings

When using the `--azure-openai-chat-deployment` option, you need to specify the name of a deployment you've created in your Azure OpenAI resource.

### Finding Your Deployment Names

To find your deployment names:

1. Sign in to the [Azure Portal](https://portal.azure.com)
2. Navigate to your Azure OpenAI resource
3. Select "Deployments" from the left menu
4. The "Name" column shows your deployment names

![Azure OpenAI Deployments](../assets/images/azure-openai-deployments.png)

Common deployment names are often the model name (like "gpt-4" or "gpt-35-turbo"), but you can choose any name when creating a deployment.

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
5. Avoid using `--azure-openai-api-key` in scripts that might be committed to version control
6. When using the key in scripts, read it from environment variables rather than hardcoding it
7. Use command history exclusion in your shell (e.g., HISTIGNORE in bash) to prevent API keys from being stored in command history

```bash
# Example of securely providing an API key via an environment variable
export AZURE_OPENAI_KEY="your-api-key"
chatx --use-azure --azure-openai-api-key "$AZURE_OPENAI_KEY" --question "Your question"
```

## Troubleshooting

If you encounter issues with Azure OpenAI, try these steps:

1. Verify your endpoint URL is correct and includes the full domain
2. Check that your deployment names match exactly what's in Azure
3. Ensure your API key has the correct permissions
4. Verify network connectivity to Azure services

For more detailed information, refer to the [Azure OpenAI Service documentation](https://learn.microsoft.com/en-us/azure/ai-services/openai/).