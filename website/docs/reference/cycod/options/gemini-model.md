# --google-gemini-model-id

The `--google-gemini-model-id` option allows you to specify which Google Gemini model to use for AI chat when using the Gemini provider.

## Syntax

```bash
cycod --google-gemini-model-id MODEL_NAME
```

## Parameters

| Parameter | Description |
|-----------|-------------|
| `MODEL_NAME` | The name of the Gemini model to use (e.g., `gemini-pro`, `gemini-pro-vision`) |

## Description

This option lets you select a specific Gemini model for your AI interactions. Different models have different capabilities and performance characteristics. When provided, CYCOD will use the specified model instead of the default one configured in your settings.

The Google Gemini API offers several models:

- `gemini-pro`: General-purpose text model optimized for text-based tasks
- `gemini-pro-vision`: Multimodal model that can process both text and images
- `gemini-ultra`: Most powerful Gemini model with advanced reasoning capabilities

## Examples

### Example 1: Basic usage with Gemini

```bash
cycod --use-gemini --google-gemini-model-id gemini-pro --question "What is Google Gemini?"
```

### Example 2: Using the vision model with an image

```bash
cycod --use-gemini --google-gemini-model-id gemini-pro-vision --file-path image.jpg --question "Describe what's in this image"
```

### Example 3: Specifying a different model for complex tasks

```bash
cycod --use-gemini --google-gemini-model-id gemini-ultra --question "Can you explain quantum computing in simple terms?"
```

### Example 4: Interactive chat with specific model

```bash
cycod --use-gemini --google-gemini-model-id gemini-pro --interactive
```

## Configuration Alternative

For regular use, it's recommended to store your preferred model in the CYCOD configuration:

```bash
# Set your preferred Gemini model in user configuration
cycod config set Google.Gemini.ModelId gemini-pro --user
```

## Configuration Precedence

When CYCOD looks for the Gemini model to use, it checks sources in this order:

1. Command-line option (`--google-gemini-model-id`)
2. Environment variable (`GOOGLE_GEMINI_MODEL_ID`)
3. Configuration files (local, then user, then global scope)

Using the command-line option will override any value set in environment variables or configuration files.

## Notes

- You must have a valid API key and proper configuration for the Google Gemini API.
- Some models may have different rate limits or pricing.
- Not all models are available in all regions or to all users.
- For multimodal capabilities like image processing, make sure to use a model that supports it (like gemini-pro-vision).

## Related Options

| Option | Description |
|--------|-------------|
| `--use-gemini` | Explicitly select Google Gemini as the provider |
| `--google-gemini-api-key` | Specify the API key for Google Gemini |

## See Also

- [Google Gemini Provider](../../../providers/gemini.md)
- [Configuration](../../../usage/configuration.md)