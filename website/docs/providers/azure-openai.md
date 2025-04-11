---
hide:
- toc
icon: material/microsoft-azure
---

The `chatx config set` command allows you to configure your Azure OpenAI endpoint, API key, and deployment name.

--8<-- "tips/tip-setup-prereqs.md"

```bash title="Set up Azure OpenAI configuration"
chatx config set azure.openai.apiKey YOUR_API_KEY --user
chatx config set azure.openai.endpoint YOUR_ENDPOINT --user
chatx config set azure.openai.chatDeployment YOUR_DEPLOYMENT_NAME --user
```

## View Configuration

To view the persisted configuration, use the following commands:

```bash title="List all config values"
chatx config list
```

```bash title="View OpenAI config values"
chatx config get azure.openai.apiKey
chatx config get azure.openai.endpoint
chatx config get azure.openai.chatDeployment
```
