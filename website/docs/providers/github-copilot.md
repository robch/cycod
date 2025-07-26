---
hide:
- toc
icon: material/github
---

The `cycod github login` command allows you to authenticate with GitHub using device code authentication.

--8<-- "tips/tip-setup-prereqs.md"

```bash title="Login to GitHub"
cycod github login
```

**STEP 1**: ⇛ Launch the browser  
**STEP 2**: ⇛ Enter the device code

Alternatively, you can manually obtain and [set your GitHub token](./github-copilot-manual-config.md).

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


