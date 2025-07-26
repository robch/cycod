---
hide:
- toc
icon: material/microsoft-azure
---

The `cycod config set` command allows you to configure your OpenAI API key and model name.

--8<-- "tips/tip-setup-prereqs.md"

```bash title="Set up OpenAI configuration"
cycod config set openai.apiKey YOUR_API_KEY --user
cycod config set openai.chatDeployment YOUR_DEPLOYMENT_NAME --user
```

## View Configuration

To view the persisted configuration, use the following commands:

```bash title="List all config values"
cycod config list
```

```bash title="View OpenAI config values"
cycod config get openai.apiKey
cycod config get openai.chatModelName
```


