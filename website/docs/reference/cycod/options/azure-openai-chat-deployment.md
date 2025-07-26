# --azure-openai-chat-deployment

Specifies the deployment name to use for chat completions when using Azure OpenAI.

## Syntax

```
--azure-openai-chat-deployment NAME
```

## Parameters

| Parameter | Description |
|-----------|-------------|
| `NAME` | The deployment name configured in your Azure OpenAI resource |

## Description

In Azure OpenAI, models are made available as "deployments." When you create a deployment in your Azure OpenAI resource, you assign it a name. This option allows you to specify which deployment to use for chat completions.

Each deployment is associated with a specific model (like GPT-4, GPT-3.5-Turbo, etc.) and has its own quota and rate limits. The deployment name is case-sensitive and must exactly match the name you created in the Azure Portal.

## Examples

### Example 1: Use with other Azure OpenAI options

```bash
cycod --use-azure-openai \
      --azure-openai-endpoint "https://your-resource.openai.azure.com" \
      --azure-openai-api-key "your-api-key" \
      --azure-openai-chat-deployment "your-deployment-name" \
      --question "Explain how Azure OpenAI works"
```

### Example 2: Use different model deployments for different tasks

```bash
# Use a more powerful deployment for complex coding tasks
cycod --use-azure --azure-openai-chat-deployment gpt-4 --question "Explain how to implement a B-tree in Python"

# Use a more economical deployment for simpler tasks
cycod --use-azure --azure-openai-chat-deployment gpt-35-turbo --question "What's the weather like today?"
```

### Example 3: Set as configuration value

```bash
# Set as a configuration value for future use
cycod config set Azure.OpenAI.ChatDeployment your-deployment-name --user
```

### Example 4: Choosing deployments based on task complexity

```bash
# Use a more powerful deployment for complex coding tasks
cycod --use-azure --azure-openai-chat-deployment gpt-4 --question "Explain how to implement a B-tree in Python"

# Use a more economical deployment for simpler tasks
cycod --use-azure --azure-openai-chat-deployment gpt-35-turbo --question "What's the weather like today?"
```

### Example 5: Verify a deployment is accessible

```bash
# Test if the deployment responds correctly
cycod --use-azure --azure-openai-chat-deployment your-deployment-name --question "Respond with 'Hello' if you can hear me"
```

## Notes

- You must specify a valid deployment name that exists in your Azure OpenAI resource.
- This option is only applicable when using Azure OpenAI as the provider.
- Common deployment names are often based on the model they're associated with, like "gpt-4" or "gpt-35-turbo".
- You can find your deployment names in the Azure portal under your Azure OpenAI resource → Deployments.
- The deployment name is case-sensitive and must match exactly what's in the Azure Portal.

## Troubleshooting

If you encounter errors related to the chat deployment:

1. **"Deployment not found" errors**: Verify the deployment name exists in your Azure resource and the spelling matches exactly (including case).
2. **Quota errors**: Ensure your deployment has available quota. Check the Azure Portal for quota usage.
3. **Permission errors**: Confirm your API key has access to the specified deployment.
4. **Rate limit errors**: Your requests may be exceeding the deployment's rate limits. Consider increasing limits or distributing requests over time.

## Environment Variables

You can also set this value using the environment variable:

```
AZURE_OPENAI_CHAT_DEPLOYMENT=your-deployment-name
```

## See Also

| Option | Description |
|--------|-------------|
| `--use-azure-openai` | Explicitly select Azure OpenAI as the provider |
| `--azure-openai-endpoint` | Specify the Azure OpenAI service endpoint URL |
| `--azure-openai-api-key` | Specify the authentication key for Azure OpenAI |