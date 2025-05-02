To configure Azure OpenAI as your provider:

1. Create an Azure OpenAI resource in the [Azure Portal](https://portal.azure.com/)
2. Configure CycoD with your credentials:

```bash
# Set your Azure OpenAI endpoint
cycod config set azure.openai.endpoint YOUR_ENDPOINT --user

# Set your Azure OpenAI API key
cycod config set azure.openai.apiKey YOUR_API_KEY --user

# Set your deployment name
cycod config set azure.openai.chatDeployment YOUR_DEPLOYMENT_NAME --user

# Set Azure OpenAI as your default provider (optional)
cycod config set app.preferredProvider azure-openai --user
```

You can also provide your credentials directly in commands:

```bash
cycod --use-azure-openai --azure-openai-endpoint YOUR_ENDPOINT --azure-openai-api-key YOUR_API_KEY --azure-openai-chat-deployment YOUR_DEPLOYMENT --question "What is CycoD?"
```