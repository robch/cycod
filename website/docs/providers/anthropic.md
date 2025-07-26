---
hide:
- toc
icon: material/robot
---

The `cycod config set` command allows you to configure your Anthropic API key and model name.

--8<-- "tips/tip-setup-prereqs.md"

```bash title="Set up Anthropic configuration"
cycod config set Anthropic.ApiKey YOUR_API_KEY --user
cycod config set Anthropic.ModelName claude-3-7-sonnet-20250219 --user
```

## View Configuration

To view the persisted configuration, use the following commands:

```bash title="List all config values"
cycod config list
```

```bash title="View Anthropic config values"
cycod config get Anthropic.ApiKey
cycod config get Anthropic.ModelName
```
