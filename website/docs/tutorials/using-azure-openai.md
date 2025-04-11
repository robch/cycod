--8<-- "snippets/ai-generated.md"

# Using Azure OpenAI with ChatX

This tutorial will guide you through setting up and using Azure OpenAI with ChatX, including how to configure and work with different model deployments.

## Prerequisites

Before you begin, ensure you have:

- An Azure account with access to Azure OpenAI resources
- An Azure OpenAI resource with at least one model deployment
- The ChatX CLI tool installed on your machine

## Setting Up Azure OpenAI with ChatX

### Step 1: Gather Your Azure OpenAI Information

First, gather the necessary information from your Azure OpenAI resource:

1. Sign in to the [Azure Portal](https://portal.azure.com)
2. Navigate to your Azure OpenAI resource
3. For the endpoint:
   - Select "Keys and Endpoint" from the left menu
   - Copy the Endpoint value (e.g., `https://your-resource.openai.azure.com`)
4. For the API key:
   - In the same "Keys and Endpoint" page
   - Copy either KEY1 or KEY2
5. For the deployment name(s):
   - Select "Deployments" from the left menu
   - Note the names of the deployments you want to use

### Step 2: Configure ChatX with Azure OpenAI Settings

You can set up Azure OpenAI as your provider in several ways:

#### Option A: Set Up Using Configuration Commands

```bash
# Set the endpoint
chatx config set azure.openai.endpoint https://your-resource.openai.azure.com --user

# Set the API key
chatx config set azure.openai.apiKey your-api-key --user

# Set the default chat deployment
chatx config set azure.openai.chatDeployment your-deployment-name --user

# Set Azure OpenAI as the preferred provider
chatx config set app.preferredProvider azure-openai --user
```

#### Option B: Configure Using Command-Line Parameters

```bash
chatx --use-azure-openai \
      --azure-openai-endpoint https://your-resource.openai.azure.com \
      --azure-openai-api-key your-api-key \
      --azure-openai-chat-deployment your-deployment-name \
      --question "Hello! Can you confirm you're using Azure OpenAI?"
```

### Step 3: Test Your Connection

Run a simple test to ensure your Azure OpenAI connection is working:

```bash
chatx --question "What's your name and which model are you using?"
```

If configured correctly, the AI should respond and may indicate it's using Azure OpenAI.

## Working with Azure OpenAI Deployments

In Azure OpenAI, a "deployment" is an instance of a specific model that you've deployed to your Azure OpenAI resource. The `--azure-openai-chat-deployment` parameter is used to specify which deployment ChatX should use.

### Understanding Azure OpenAI Deployments

Each deployment in Azure OpenAI:

- Is associated with a specific model version (e.g., GPT-4, GPT-3.5-Turbo)
- Has a unique name that you chose when creating it
- Has its own quota and rate limits
- May have different configuration settings (like content filtering levels)

### Finding Your Deployment Names

To list your available deployments:

1. Go to your Azure OpenAI resource in the Azure Portal
2. Select "Deployments" from the left menu
3. The "Name" column shows your deployment names

Common naming patterns include:
- The model name (e.g., "gpt-4", "gpt-35-turbo")
- The model + purpose (e.g., "gpt4-documentation", "gpt35-customerservice")
- A custom naming scheme specific to your organization

### Specifying Deployments with ChatX

The `--azure-openai-chat-deployment` option lets you select which deployment to use:

```bash
chatx --use-azure-openai --azure-openai-chat-deployment your-deployment-name --question "Hello!"
```

### Example: Comparing Different Model Deployments

This example shows how to compare responses from different deployments:

```bash
# Using GPT-4 deployment for a complex task
chatx --use-azure-openai \
      --azure-openai-chat-deployment gpt-4 \
      --question "Design a system architecture for a high-scale e-commerce platform"

# Using GPT-3.5 Turbo deployment for a simpler task
chatx --use-azure-openai \
      --azure-openai-chat-deployment gpt-35-turbo \
      --question "Give me 5 ideas for dinner tonight"
```

## Creating Profiles for Different Deployments

To make it easier to switch between deployments, you can create profile YAML files:

```yaml title="azure-gpt4.yaml (in .chatx/profiles/ directory)"
app:
  preferredProvider: "azure-openai"

azure:
  openai:
    endpoint: "https://your-resource.openai.azure.com"
    apiKey: "your-api-key" 
    chatDeployment: "gpt-4"
```

```yaml title="azure-gpt35.yaml (in .chatx/profiles/ directory)"
app:
  preferredProvider: "azure-openai"

azure:
  openai:
    endpoint: "https://your-resource.openai.azure.com"
    apiKey: "your-api-key"
    chatDeployment: "gpt-35-turbo"
```

Then use the profiles as needed:

```bash
# Use the GPT-4 profile for complex reasoning
chatx --profile azure-gpt4 --question "Explain quantum computing to me"

# Use the GPT-3.5 profile for simpler tasks
chatx --profile azure-gpt35 --question "What's the capital of France?"
```

## Best Practices

### Choosing the Right Deployment

- Use more powerful deployments (like GPT-4) for:
  - Complex reasoning tasks
  - Code generation and review
  - Creative content generation
  - Technical problem-solving

- Use more economical deployments (like GPT-3.5-Turbo) for:
  - Simple questions and answers
  - Basic information retrieval
  - Short summaries
  - Less complex tasks

### Deployment Naming Conventions

When creating your own deployments in Azure OpenAI:

- Use consistent, descriptive names
- Consider including the model version in the name
- Consider including the purpose if deployments are task-specific

Example naming scheme:
- `gpt4-general` - General purpose GPT-4 deployment
- `gpt35-support` - GPT-3.5 Turbo deployment for customer support tasks
- `gpt4-coding` - GPT-4 deployment tuned for programming tasks

## Troubleshooting

### Common Issues with Azure OpenAI Deployments

1. **"Deployment not found" error**
   - Verify the deployment name exists in your Azure resource
   - Check that the name is spelled correctly (deployment names are case-sensitive)
   - Ensure the deployment is in the same region as your endpoint

   ```bash
   # List deployments in your configuration
   chatx config get azure.openai.chatDeployment --any
   ```

2. **Quota exceeded errors**
   - Your deployment may have reached its token quota limit
   - Check the "Quotas & usage" section in your Azure OpenAI resource
   - Consider increasing your quota or waiting until it resets

3. **Invalid authentication**
   - Ensure your API key is valid and has not expired
   - Try regenerating your API key in the Azure Portal

4. **Inconsistent responses**
   - Different deployments (even of the same model) may have different configurations
   - Verify that your deployments have the same content filter settings if consistency is important

### Testing Deployments

You can quickly test if a deployment is working with:

```bash
chatx --use-azure-openai \
      --azure-openai-chat-deployment your-deployment-name \
      --question "Please respond with 'Hello from [deployment name]'"
```

## Conclusion

You've now learned how to:
- Set up ChatX to use Azure OpenAI
- Configure and specify different Azure OpenAI deployments
- Create profiles for different deployments
- Choose the right deployment for different tasks
- Troubleshoot common issues

By mastering the use of the `--azure-openai-chat-deployment` option, you can effectively leverage your organization's Azure OpenAI deployments for various tasks while optimizing for both performance and cost.

## Next Steps

- Learn about [Azure OpenAI content filtering](https://learn.microsoft.com/en-us/azure/ai-services/openai/concepts/content-filter)
- Explore [ChatX profiles](../advanced/profiles.md) for more advanced configuration
- Check out how to use [ChatX with GitHub Copilot](github-copilot-auth.md) as an alternative provider