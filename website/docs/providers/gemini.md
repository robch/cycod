---
hide:
- toc
icon: material/google
---

The `cycod config set` command allows you to configure your Google Gemini API key and model name.

--8<-- "tips/tip-setup-prereqs.md"

```bash title="Set up Google Gemini configuration"
cycod config set Google.Gemini.ApiKey YOUR_API_KEY --user
cycod config set Google.Gemini.ModelId gemini-pro --user
```

## View Configuration

To view the persisted configuration, use the following commands:

```bash title="List all config values"
cycod config list
```

```bash title="View Gemini config values"
cycod config get Google.Gemini.ApiKey
cycod config get Google.Gemini.ModelId
```

