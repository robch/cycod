## Implementation Considerations

### Parameter Substitution and Escaping

When substituting parameters into commands, the system:
1. Replaces `{PARAM}` placeholders with the parameter value
2. Applies any transformations defined for the parameter
3. Escapes values according to the shell and security settings
4. Formats according to any specified format strings

### Security Implementation

The security model works by:
1. Checking tool tags against user-configured auto-approve/auto-deny settings
2. Validating required permissions against user permissions
3. Setting up isolation boundaries based on the tool's requirements
4. Applying parameter validation and escaping to prevent injection attacks

### Cross-Platform Compatibility

Tools ensure cross-platform compatibility by:
1. Using platform-specific commands when available
2. Converting file paths to the appropriate format for the current platform
3. Using shell-agnostic syntax when possible
4. Handling environment variables consistently across platforms

### Output Handling

Output from commands is processed by:
1. Capturing stdout and stderr separately
2. Applying any size limits or streaming settings
3. Making output available to subsequent steps as needed
4. Formatting output for return to the caller

### Resource Management

Resources are managed by:
1. Setting appropriate timeouts for commands
2. Monitoring memory and CPU usage
3. Executing cleanup commands on completion or failure
4. Properly terminating any spawned processes

## Best Practices

### Tool Design Principles

1. **Single Responsibility**: Tools should do one thing and do it well
2. **Clear Interface**: Parameters should have clear names and descriptions
3. **Robust Error Handling**: Tools should gracefully handle errors and provide useful messages
4. **Cross-Platform Awareness**: Consider compatibility across different operating systems
5. **Security First**: Apply the principle of least privilege

### Parameter Naming Conventions

1. Use UPPER_CASE for parameter names
2. Use descriptive, specific names that indicate purpose
3. Be consistent with parameter names across related tools
4. Use standard names for common parameters (e.g., INPUT, OUTPUT, DIRECTORY)
5. Document parameters thoroughly with descriptions and examples

### Error Handling Patterns

1. Check for prerequisites before executing commands
2. Use appropriate exit codes to indicate specific failure types
3. Provide informative error messages that suggest how to fix the problem
4. Consider retry logic for operations that might fail transiently
5. Clean up any temporary resources even if the tool fails

### Cross-Platform Compatibility Techniques

1. Use platform-specific commands when necessary
2. Be aware of differences in path separators, line endings, and shell behavior
3. Test tools on all target platforms
4. Use environment variables for system-specific paths
5. Avoid hard-coding absolute paths or assuming specific directory structures

## Advanced Examples

### Tool with Comprehensive Error Handling

```yaml
name: robust-file-processor
description: Process files with comprehensive error handling

steps:
  - name: check-prerequisites
    bash: |
      if ! command -v jq &> /dev/null; then
        echo "Error: jq is not installed. Please install it first."
        exit 1
      fi
      if [ ! -f "{INPUT_FILE}" ]; then
        echo "Error: Input file '{INPUT_FILE}' does not exist."
        exit 2
      fi
    continue-on-error: false

  - name: process-file
    bash: jq '.items[]' "{INPUT_FILE}" > "{OUTPUT_FILE}"
    error-handling:
      retry:
        attempts: 3
        delay: 1000
      fallback: cp "{INPUT_FILE}" "{OUTPUT_FILE}" && echo "Warning: Processing failed, copied original file"
      on-error-output: "Error processing file: {error}"

  - name: validate-output
    bash: |
      if [ ! -s "{OUTPUT_FILE}" ]; then
        echo "Error: Output file is empty."
        exit 1
      fi
      echo "Successfully processed {INPUT_FILE} to {OUTPUT_FILE}"

parameters:
  INPUT_FILE:
    type: string
    description: Input file to process
    required: true
    validation:
      pattern: ".*\\.json$"
      custom: "fs.existsSync(value)"

  OUTPUT_FILE:
    type: string
    description: Output file
    default: "{INPUT_FILE}.processed"

tags: [file, process, read, write]
platforms: [windows, linux, macos]
```

### Cross-Platform Tool Example

```yaml
name: find-files
description: Find files with cross-platform support

commands:
  default: find "{DIRECTORY}" -name "{PATTERN}" -type f
  platforms:
    windows: powershell -Command "Get-ChildItem -Path '{DIRECTORY}' -Filter '{PATTERN}' -File -Recurse | Select-Object -ExpandProperty FullName"
    unix: find "{DIRECTORY}" -name "{PATTERN}" -type f

parameters:
  DIRECTORY:
    type: string
    description: Directory to search in
    default: "."
    transform: "path.normalize(value)"

  PATTERN:
    type: string
    description: File pattern to match
    required: true
    examples: ["*.txt", "*.log", "data*.csv"]

file-paths:
  normalize: true
  cross-platform:
    auto-convert: true

tags: [files, search, read]
platforms: [windows, linux, macos]
```

### Tool with Complex Parameter Validation

```yaml
name: deploy-service
description: Deploy a service with sophisticated parameter validation

bash: ./deploy.sh --name "{SERVICE_NAME}" --port {PORT} --env {ENVIRONMENT} --replicas {REPLICAS}

parameters:
  SERVICE_NAME:
    type: string
    description: Name of the service to deploy
    required: true
    validation:
      pattern: "^[a-z0-9-]+$"
      custom: "value.length >= 3 && value.length <= 63"
    examples: ["web-frontend", "api-service", "database"]
    detailed-help: |
      Service name must:
      - Contain only lowercase letters, numbers, and hyphens
      - Start and end with a letter or number
      - Be between 3 and 63 characters long

  PORT:
    type: number
    description: Port to expose the service on
    default: 8080
    validation:
      minimum: 1024
      maximum: 65535
      custom: "![80, 443].includes(value)"
    examples: [3000, 8080, 9000]

  ENVIRONMENT:
    type: string
    description: Deployment environment
    default: "development"
    validation:
      enum: ["development", "staging", "production"]
    examples: ["development", "staging", "production"]

  REPLICAS:
    type: number
    description: Number of service replicas
    default: 1
    validation:
      minimum: 1
      maximum: 10
      custom: "ENVIRONMENT === 'production' ? value >= 2 : true"

tags: [deployment, kubernetes, run]
platforms: [linux, macos]
```

### Tool with Integration to Other Systems

```yaml
name: jira-create-issue
description: Create a Jira issue

steps:
  - name: authenticate
    bash: echo '{API_TOKEN}' > .token-temp && chmod 600 .token-temp
    continue-on-error: false

  - name: create-issue
    bash: |
      curl -s -X POST \
        -H "Content-Type: application/json" \
        -H "Authorization: Bearer $(cat .token-temp)" \
        -d '{
          "fields": {
            "project": { "key": "{PROJECT_KEY}" },
            "summary": "{SUMMARY}",
            "description": "{DESCRIPTION}",
            "issuetype": { "name": "{ISSUE_TYPE}" },
            "priority": { "name": "{PRIORITY}" }
          }
        }' \
        "{JIRA_URL}/rest/api/2/issue"
    output:
      mode: buffer

  - name: cleanup
    bash: rm -f .token-temp
    continue-on-error: true

parameters:
  JIRA_URL:
    type: string
    description: Jira instance URL
    required: true
    examples: ["https://company.atlassian.net"]

  API_TOKEN:
    type: string
    description: Jira API token
    required: true
    security:
      sensitive: true

  PROJECT_KEY:
    type: string
    description: Jira project key
    required: true
    examples: ["PROJ", "DEV", "OPS"]

  SUMMARY:
    type: string
    description: Issue summary (title)
    required: true

  DESCRIPTION:
    type: string
    description: Issue description
    default: ""

  ISSUE_TYPE:
    type: string
    description: Type of issue
    default: "Task"
    validation:
      enum: ["Bug", "Task", "Story", "Epic"]

  PRIORITY:
    type: string
    description: Issue priority
    default: "Medium"
    validation:
      enum: ["Highest", "High", "Medium", "Low", "Lowest"]

environment:
  variables:
    CURL_SSL_VERIFY: "true"

security:
  required-permissions:
    - "network:external:{JIRA_URL}"
  justification: "Required for creating Jira issues"

tags: [jira, integration, write]
```