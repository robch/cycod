# cycod github models

The `github models` command lists all available models in GitHub Copilot.

## Usage

```
cycod github models
```

## Description

This command displays all available AI models in GitHub Copilot, including:
- Model name and ID
- Vendor information (OpenAI, Anthropic, Google, etc.)
- Model capabilities and limits
- Preview status
- Feature support (streaming, tool calls, vision, etc.)

Models are grouped by vendor for easier browsing.

## Authentication

You must be authenticated with GitHub Copilot to use this command. If you're not authenticated, run:

```
cycod github login
```

## Examples

### List all available models

```
cycod github models
```

## Related Commands

- `cycod github login` - Authenticate with GitHub to use Copilot services