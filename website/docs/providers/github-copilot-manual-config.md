---
hide:
- toc
icon: material/github
---

# Setup GitHub Copilot manually

The `cycod config set` command allows you to configure your GitHub Copilot settings.

--8<-- "tips/tip-setup-prereqs.md"

```bash title="Setup GitHub configuration"
cycod config set github.token YOUR_GITHUB_TOKEN --user
cycod config set copilot.modelName YOUR_MODEL_NAME --user
```

## View Configuration

To view the persisted configuration, use the following commands:

```bash title="List all config values"
cycod config list
```

```bash title="View Copilot config values"
cycod config get github.token
cycod config get copilot.apiEndpoint
cycod config get copilot.modelName
```
