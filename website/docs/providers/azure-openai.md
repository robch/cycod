---
hide:
- toc
icon: material/microsoft-azure
---

The `cycod config set` command allows you to configure your Azure OpenAI endpoint, API key, and deployment name.

--8<-- "tips/tip-setup-prereqs.md"

```bash title="Set up Azure OpenAI configuration"
cycod config set azure.openai.apiKey YOUR_API_KEY --user
cycod config set azure.openai.endpoint YOUR_ENDPOINT --user
cycod config set azure.openai.chatDeployment YOUR_DEPLOYMENT_NAME --user
```

## View Configuration

To view the persisted configuration, use the following commands:

```bash title="List all config values"
cycod config list
```

```bash title="View OpenAI config values"
cycod config get azure.openai.apiKey
cycod config get azure.openai.endpoint
cycod config get azure.openai.chatDeployment
```
