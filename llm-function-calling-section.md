## LLM Function Calling Integration

Custom Tools are designed to be directly accessible to LLMs through function calling. Each tool is automatically represented as a callable function with appropriate schema.

### Schema Generation

Tools generate function calling schemas based on their definition:

```yaml
function-calling:
  enabled: true                    # Whether to expose this tool to LLMs
  schema-generation:
    name: "{tool-name}"            # Override the function name
    description: "{description}"   # Use tool description or override
    parameter-mapping:             # How tool parameters map to schema types
      string: "string"
      number: "number"
      boolean: "boolean"
      array: "array"
      object: "object"
    include-descriptions: true     # Include parameter descriptions in schema
    include-defaults: true         # Include default values in schema
    example-generation: true       # Generate example values for parameters
    include-validation: true       # Include validation rules in schema
```

### Example Schema

A tool like `weather-lookup` would generate the following function calling schema:

```json
{
  "name": "weather-lookup",
  "description": "Get weather for a location",
  "parameters": {
    "type": "object",
    "properties": {
      "LOCATION": {
        "type": "string",
        "description": "City or airport code"
      },
      "FORMAT": {
        "type": "string",
        "description": "Output format",
        "default": "3"
      }
    },
    "required": ["LOCATION"]
  }
}
```

### Schema Customization

Schemas can be customized to provide better information to LLMs:

```yaml
name: github-repo-clone
description: Clone a GitHub repository with fallback methods

function-calling:
  schema-generation:
    description: "Clone a GitHub repository using HTTPS or SSH with automatic fallback"
    example-generation:
      OWNER: ["microsoft", "google", "openai"]
      REPO: ["vscode", "tensorflow", "gpt-4"]
    parameter-mapping:
      # Custom parameter mappings for special cases
      OUTPUT_DIR: "string"
```

### Invocation Context

When tools are invoked through function calling, they receive additional context:

```yaml
function-calling:
  context:
    provide-conversation-history: false   # Include conversation history
    provide-tool-description: true        # Include detailed tool description
    provide-system-info: false            # Include system information
```