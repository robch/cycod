---
hide:
- toc
icon: material/server-network
---

# Choosing a Provider

When deciding which provider to use, consider the following factors:

* **Model Availability**: Different providers offer different AI models. For example:
  * GitHub Copilot offers Claude models from Anthropic
  * Azure OpenAI offers Azure-hosted versions of OpenAI models
  * OpenAI offers models like GPT-4o and GPT-3.5-Turbo

* **Authentication Methods**: Providers use different authentication mechanisms:
  * GitHub Copilot uses GitHub tokens authentication
  * Azure OpenAI uses API keys and endpoints
  * OpenAI uses API keys

* **Cost Structure**: Each provider has its own pricing model:
  * GitHub Copilot is available as part of GitHub subscriptions
  * Azure OpenAI has capacity-based or consumption-based pricing
  * OpenAI charges per token

* **Compliance Requirements**: Organizations may have specific requirements:
  * Azure OpenAI offers regional data residency
  * Some providers offer dedicated enterprise offerings

## Provider-Specific Options

Each provider has its own set of options that can be configured:

### GitHub Copilot Options

* `--github-token`: GitHub authentication token
* `--copilot-model-name`: The model to use (default: claude-3.5-sonnet)

### Azure OpenAI Options

* `--azure-openai-api-key`: Your Azure OpenAI API key
* `--azure-openai-endpoint`: Your Azure OpenAI endpoint URL
* `--azure-openai-chat-deployment`: The deployment name to use

### OpenAI Options

* `--openai-api-key`: Your OpenAI API key
* `--openai-chat-model-name`: The model to use (default: gpt-4o)

## See Also

* [Provider Profiles](../../advanced/provider-profiles.md) - Using profiles for different providers
* [Configuration](../../usage/configuration.md) - Learn more about configuring CycoD
* [Command-Line Options](../../reference/cycod/index.md) - Reference for all command-line options