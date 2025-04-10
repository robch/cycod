# AI Providers

ChatX supports multiple AI providers, allowing you to choose the service that best fits your needs, requirements, and budget. This flexibility makes ChatX adaptable to various use cases and constraints.

## Available Providers

ChatX currently supports the following AI providers:

| Provider | Description | Option Flag | Configuration Setting |
|----------|-------------|-------------|------------------------|
| [OpenAI](openai.md) | Direct integration with OpenAI API | `--use-openai` | `app.preferredProvider=openai` |
| [Azure OpenAI](azure-openai.md) | Microsoft's Azure-based OpenAI service | `--use-azure-openai` or `--use-azure` | `app.preferredProvider=azure-openai` |
| [GitHub Copilot](github-copilot.md) | GitHub's AI coding assistant service | `--use-copilot` | `app.preferredProvider=copilot` |

## Choosing a Provider

When deciding which provider to use, consider the following factors:

* **Model Availability**: Different providers offer different AI models. For example:
  * OpenAI offers models like GPT-4o and GPT-3.5-Turbo
  * Azure OpenAI offers Azure-hosted versions of OpenAI models
  * GitHub Copilot offers Claude models from Anthropic

* **Authentication Methods**: Providers use different authentication mechanisms:
  * OpenAI uses API keys
  * Azure OpenAI uses API keys and endpoints
  * GitHub Copilot uses GitHub tokens or HMAC authentication

* **Cost Structure**: Each provider has its own pricing model:
  * OpenAI charges per token
  * Azure OpenAI has capacity-based or consumption-based pricing
  * GitHub Copilot is available as part of GitHub subscriptions

* **Compliance Requirements**: Organizations may have specific requirements:
  * Azure OpenAI offers regional data residency
  * Some providers offer dedicated enterprise offerings

## Setting a Default Provider

You can set your default AI provider in the ChatX configuration:

```bash
# Set OpenAI as default
chatx config set app.preferredProvider openai --user

# Set Azure OpenAI as default
chatx config set app.preferredProvider azure-openai --user

# Set GitHub Copilot as default
chatx config set app.preferredProvider copilot --user
```

## Overriding the Provider for a Specific Command

You can override your default provider for a specific command using the appropriate flag:

```bash
# Use OpenAI for this command
chatx --use-openai --question "What is ChatX?"

# Use Azure OpenAI for this command
chatx --use-azure-openai --question "What is ChatX?"

# Use GitHub Copilot for this command
chatx --use-copilot --question "What is ChatX?"
```

## Provider-Specific Options

Each provider has its own set of options that can be configured:

### OpenAI Options

* `--openai-api-key`: Your OpenAI API key
* `--openai-chat-model-name`: The model to use (default: gpt-4o)

### Azure OpenAI Options

* `--azure-openai-api-key`: Your Azure OpenAI API key
* `--azure-openai-endpoint`: Your Azure OpenAI endpoint URL
* `--azure-openai-chat-deployment`: The deployment name to use

### GitHub Copilot Options

* `--github-token`: GitHub authentication token
* `--copilot-model-name`: The model to use (default: claude-3.5-sonnet)
* `--copilot-api-endpoint`: The API endpoint
* `--copilot-hmac-key`: HMAC authentication key
* `--copilot-integration-id`: Integration ID for HMAC auth

## Creating Provider-Specific Profiles

You can create dedicated profiles for different providers:

```bash
# Create an OpenAI profile
chatx --use-openai --openai-chat-model-name gpt-4o --save-profile openai-profile

# Create an Azure OpenAI profile
chatx --use-azure --azure-openai-chat-deployment gpt-4 --save-profile azure-profile

# Create a GitHub Copilot profile
chatx --use-copilot --copilot-model-name claude-3-opus --save-profile copilot-profile
```

Then use them with:

```bash
chatx --profile openai-profile --question "What is ChatX?"
```

## See Also

* [Configuration](../usage/configuration.md) - Learn more about configuring ChatX
* [Command-Line Options](../reference/cli/index.md) - Reference for all command-line options
* [Profiles](../advanced/profiles.md) - Using profiles to manage different configurations