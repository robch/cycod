To configure Azure OpenAI as your provider:

1. Create an Azure OpenAI resource in the [Azure Portal](https://portal.azure.com/)
2. Configure ChatX with your credentials:

```bash
# Set your Azure OpenAI endpoint
chatx config set azure.openai.endpoint YOUR_ENDPOINT --user

# Set your Azure OpenAI API key
chatx config set azure.openai.apiKey YOUR_API_KEY --user

# Set your deployment name
chatx config set azure.openai.chatDeployment YOUR_DEPLOYMENT_NAME --user

# Set Azure OpenAI as your default provider (optional)
chatx config set app.preferredProvider azure-openai --user
```

You can also provide your credentials directly in commands:

```bash
chatx --use-azure-openai --azure-openai-endpoint YOUR_ENDPOINT --azure-openai-api-key YOUR_API_KEY --azure-openai-chat-deployment YOUR_DEPLOYMENT --question "What is ChatX?"
```