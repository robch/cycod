## Examples

### Simple Single-Step Tool Example

```yaml
name: weather-lookup
description: Get weather for a location
version: 1.0.0
min-cycod-version: 1.0.0

bash: curl -s 'wttr.in/{LOCATION}?format={FORMAT}'

parameters:
  LOCATION:
    type: string
    description: City or airport code
    required: true
    examples: ["New York", "SFO", "London"]
  FORMAT:
    type: string
    description: Output format
    required: false
    default: "3"
    examples: ["1", "2", "3", "4"]
    detailed-help: |
      Format options:
      1 - Brief forecast
      2 - Compact forecast
      3 - Simple forecast (default)
      4 - Detailed forecast

timeout: 10000
tags: [weather, api, read]
platforms: [windows, linux, macos]

function-calling:
  schema-generation:
    include-descriptions: true
    include-defaults: true
    example-generation: true
```

### Multi-Step Tool with Output Capturing

```yaml
name: github-commit-stats
description: Get statistics for recent commits in a GitHub repository
version: 1.0.0

steps:
  - name: fetch-commits
    bash: curl -s "https://api.github.com/repos/{OWNER}/{REPO}/commits?per_page=10"
    output:
      mode: buffer
      buffer-limit: 5MB

  - name: count-authors
    bash: echo '{fetch-commits.output}' | jq 'group_by(.commit.author.name) | map({author: .[0].commit.author.name, count: length}) | sort_by(.count) | reverse'

  - name: format-output
    bash: echo '{count-authors.output}' | jq -r '.[] | "\(.author): \(.count) commits"'

parameters:
  OWNER:
    type: string
    description: Repository owner username
    required: true
  REPO:
    type: string
    description: Repository name
    required: true

security:
  execution-privilege: same-as-user
  required-permissions:
    - "network:external:api.github.com"
  justification: "Required for accessing GitHub API"

tags: [github, api, read]
timeout: 15000
```

### Tool with Conditional Steps and Error Handling

```yaml
name: github-repo-clone
description: Clone a GitHub repository with fallback methods
version: 1.0.0

steps:
  - name: try-https
    bash: git clone https://github.com/{OWNER}/{REPO}.git {OUTPUT_DIR}
    continue-on-error: true
    error-handling:
      retry:
        attempts: 3
        delay: 1000  # milliseconds

  - name: try-ssh
    bash: git clone git@github.com:{OWNER}/{REPO}.git {OUTPUT_DIR}
    run-condition: "{try-https.exit-code} != 0"
    continue-on-error: true

  - name: report-status
    bash: |
      if [ -d "{OUTPUT_DIR}/.git" ]; then
        echo "Successfully cloned {OWNER}/{REPO}"
      else
        echo "Failed to clone {OWNER}/{REPO} using both HTTPS and SSH"
        exit 1
      fi

parameters:
  OWNER:
    type: string
    description: Repository owner username
    required: true
  REPO:
    type: string
    description: Repository name
    required: true
  OUTPUT_DIR:
    type: string
    description: Output directory for the clone
    default: "{REPO}"

security:
  execution-privilege: same-as-user
  required-permissions:
    - "filesystem:write:{OUTPUT_DIR}"
    - "network:external:github.com"
  justification: "Required for cloning GitHub repositories"

tags: [github, git, clone, write]
platforms: [windows, linux, macos]

tests:
  - name: basic-test
    description: "Test basic cloning functionality"
    parameters:
      OWNER: "microsoft"
      REPO: "vscode"
      OUTPUT_DIR: "test-vscode"
    expected:
      exit-code: 0
      output-contains: "Successfully cloned"
      file-exists: "test-vscode/.git/config"
    cleanup:
      - "rm -rf test-vscode"
```

### Tool with Complex Parameter Handling

```yaml
name: search-code
description: Search for patterns in code files
version: 1.0.0

commands:
  default: grep -r {CASE_SENSITIVE} {WHOLE_WORD} "{PATTERN}" {DIRECTORY} --include="*.{FILE_TYPE}"
  platforms:
    windows: powershell -Command "Get-ChildItem -Path '{DIRECTORY}' -Filter '*.{FILE_TYPE}' -Recurse | Select-String -Pattern '{PATTERN}' {CASE_SENSITIVE} {WHOLE_WORD}"

parameters:
  PATTERN:
    type: string
    description: Pattern to search for
    required: true
    security:
      escape-shell: true

  DIRECTORY:
    type: string
    description: Directory to search in
    default: "."
    validation:
      pattern: "^[^<>:\"\\|?*]+$"  # Valid directory name

  FILE_TYPE:
    type: string
    description: File extension to search in
    default: "*"

  CASE_SENSITIVE:
    type: boolean
    description: Whether to use case-sensitive search
    default: false
    transform: "value ? '' : '-i'"  # Empty string for true, -i for false

  WHOLE_WORD:
    type: boolean
    description: Whether to search for whole words only
    default: false
    transform: "value ? '-w' : ''"  # -w for true, empty string for false

file-paths:
  normalize: true
  working-directory: "{WORKSPACE}"

metadata:
  category: development
  subcategory: code-search
  tags: [search, code, read]
  search-keywords: [find, grep, search, pattern]

tags: [search, code, read]
platforms: [windows, linux, macos]
```

### Tool Alias Example

```yaml
name: search-js
description: Search for patterns in JavaScript files
version: 1.0.0
type: alias
base-tool: search-code
default-parameters:
  FILE_TYPE: "js"
  DIRECTORY: "./src"

tags: [search, javascript, read]
```

### Tool Using Another Tool

```yaml
name: github-workflow
description: Run a complete GitHub workflow
version: 1.0.0

steps:
  - name: clone
    use-tool: github-repo-clone
    with:
      OWNER: "{OWNER}"
      REPO: "{REPO}"
      OUTPUT_DIR: "{WORKSPACE}"

  - name: install
    bash: cd {WORKSPACE} && npm install
    run-condition: "{clone.exit-code} == 0"

  - name: build
    bash: cd {WORKSPACE} && npm run build
    run-condition: "{install.exit-code} == 0"
    error-handling:
      retry:
        attempts: 2
        delay: 1000

  - name: test
    bash: cd {WORKSPACE} && npm test
    run-condition: "{build.exit-code} == 0"
    output:
      mode: stream
      stream-callback: console

  - name: cleanup
    bash: rm -rf {WORKSPACE}
    continue-on-error: true

parameters:
  OWNER:
    type: string
    description: Repository owner username
    required: true
  REPO:
    type: string
    description: Repository name
    required: true
  WORKSPACE:
    type: string
    description: Working directory
    default: "./workspace/{REPO}"

environment:
  variables:
    NODE_ENV: "development"
  inherit: true

resources:
  timeout: 300000  # 5 minutes
  max-memory: 1GB
  cleanup:
    - delete-temp-files: true
    - final-command: "rm -rf {WORKSPACE}"

security:
  execution-privilege: same-as-user
  required-permissions:
    - "filesystem:write:{WORKSPACE}"
    - "network:external:github.com"
    - "network:external:registry.npmjs.org"
  justification: "Required for GitHub workflow"

metadata:
  category: ci-cd
  subcategory: node
  tags: [github, npm, build, test]
  search-keywords: [workflow, pipeline, build, test]

tags: [github, npm, build, write, run]
platforms: [windows, linux, macos]

tests:
  - name: basic-test
    description: "Test basic workflow functionality"
    parameters:
      OWNER: "example"
      REPO: "sample-npm-project"
      WORKSPACE: "./test-workspace"
    expected:
      exit-code: 0
      output-contains: "All tests passed"
    cleanup:
      - "rm -rf ./test-workspace"

changelog:
  - version: 1.0.0
    changes: "Initial release"
```

### Parallel Execution Example

```yaml
name: parallel-build
description: Build multiple projects in parallel
version: 1.0.0

steps:
  - name: setup
    bash: mkdir -p {OUTPUT_DIR}

  - name: build-frontend
    bash: cd {FRONTEND_DIR} && npm run build && cp -r dist/* ../{OUTPUT_DIR}/
    parallel: true
    wait-for: [setup]

  - name: build-backend
    bash: cd {BACKEND_DIR} && mvn package && cp target/*.jar ../{OUTPUT_DIR}/
    parallel: true
    wait-for: [setup]

  - name: build-docs
    bash: cd {DOCS_DIR} && mkdocs build && cp -r site/* ../{OUTPUT_DIR}/docs/
    parallel: true
    wait-for: [setup]

  - name: report
    bash: |
      echo "Build completed:"
      ls -la {OUTPUT_DIR}
    wait-for: [build-frontend, build-backend, build-docs]

parameters:
  FRONTEND_DIR:
    type: string
    description: Frontend project directory
    default: "./frontend"
  BACKEND_DIR:
    type: string
    description: Backend project directory
    default: "./backend"
  DOCS_DIR:
    type: string
    description: Documentation directory
    default: "./docs"
  OUTPUT_DIR:
    type: string
    description: Output directory
    default: "./dist"

resources:
  timeout: 600000  # 10 minutes

tags: [build, parallel, write]
platforms: [linux, macos]
```

### Interactive Tool Example

```yaml
name: interactive-setup
description: Interactive project setup wizard
version: 1.0.0

steps:
  - name: prompt-project-name
    bash: |
      read -p "Enter project name: " PROJECT_NAME
      echo $PROJECT_NAME
    output:
      mode: buffer

  - name: prompt-language
    bash: |
      read -p "Select language (js/ts/python/go): " LANGUAGE
      echo $LANGUAGE
    output:
      mode: buffer

  - name: create-project
    bash: |
      PROJECT_NAME="{prompt-project-name.output}"
      LANGUAGE="{prompt-language.output}"
      
      mkdir -p $PROJECT_NAME
      
      case $LANGUAGE in
        js)
          echo "Creating JavaScript project..."
          cd $PROJECT_NAME && npm init -y
          ;;
        ts)
          echo "Creating TypeScript project..."
          cd $PROJECT_NAME && npm init -y && npm install typescript --save-dev
          ;;
        python)
          echo "Creating Python project..."
          cd $PROJECT_NAME && python -m venv venv && touch requirements.txt
          ;;
        go)
          echo "Creating Go project..."
          cd $PROJECT_NAME && go mod init $PROJECT_NAME
          ;;
        *)
          echo "Unsupported language: $LANGUAGE"
          exit 1
          ;;
      esac
      
      echo "Project created successfully!"

interactive: true
interactive-options:
  timeout: 60000  # 1 minute
  default-response: ""

tags: [setup, interactive, write]
platforms: [windows, linux, macos]
```