## Parameter Enhancements

### Parameter Validation

Parameters can include validation rules to ensure inputs meet requirements:

```yaml
parameters:
  COUNT:
    type: number
    description: Count of items
    validation:
      minimum: 1                 # Minimum value
      maximum: 100               # Maximum value
      pattern: "^[0-9]+$"        # Regex pattern (for strings)
      enum: [10, 20, 50, 100]    # Allowed values
      custom: "value % 10 === 0" # Custom validation expression
```

### Parameter Transformation

Parameters can be transformed before substitution:

```yaml
parameters:
  URL:
    type: string
    description: URL to access
    transform: "encodeURIComponent(value)"  # JavaScript expression to transform value
    format: "--url={value}"                 # How parameter appears in command
```

### Enhanced Documentation

Parameters can include examples and detailed help information:

```yaml
parameters:
  PATTERN:
    type: string
    description: File pattern to match
    examples: ["*.txt", "log*.log"]         # Example values
    detailed-help: |
      A glob pattern that follows standard shell conventions.
      Use * as a wildcard for any characters.
      Examples: *.txt, log-*.log, data-????.csv
```

### Complex Parameter Types

Tools can use complex parameter types for structured data:

```yaml
parameters:
  CONFIG:
    type: object                           # Object type for structured data
    description: Server configuration
    properties:                            # Object properties
      server:
        type: string
        description: Server hostname
      port:
        type: number
        description: Server port
        default: 8080
    required: ["server"]                   # Required properties
```

### Dynamic Parameter References

Parameters can reference other parameters in default values:

```yaml
parameters:
  REPO:
    type: string
    description: Repository name
    required: true
  
  OUTPUT_DIR:
    type: string
    description: Output directory
    default: "./checkout/{REPO}"           # References another parameter
```