---
hide:
- toc
icon: material/download
---

# Setting Up AI Providers

CycoD supports multiple AI providers. You'll need to configure at least one provider before you can start using CycoD.

Below are quick setup examples for each supported provider:

### OpenAI API

To use the OpenAI API with CycoD, you need an API key:

1. Get an API key from [OpenAI](https://platform.openai.com/api-keys).
2. Set up your OpenAI API key in CycoD:

```bash
cycod config set openai.apiKey YOUR_API_KEY --user
```

### Azure OpenAI API

To use the Azure OpenAI API with CycoD:

1. Create an Azure OpenAI resource in the [Azure Portal](https://portal.azure.com/).
2. Set up your Azure OpenAI credentials in CYCOD:

```bash
cycod config set azure.openai.endpoint YOUR_ENDPOINT --user
cycod config set azure.openai.apiKey YOUR_API_KEY --user
cycod config set azure.openai.chatDeployment YOUR_DEPLOYMENT_NAME --user
```

### GitHub Copilot

To use GitHub Copilot with CycoD:

1. Ensure you have a GitHub account with Copilot subscription.
2. Authenticate with GitHub:

```bash
cycod github login
```
