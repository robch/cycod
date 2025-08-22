## Tool Composition and Reusability

### Tool Composition

Tools can reference and use other tools:

```yaml
steps:
  - name: use-git-clone
    use-tool: git-clone             # Reference another tool
    with:                          # Parameters to pass to the referenced tool
      URL: "{REPO_URL}"
      DIRECTORY: "{WORKSPACE}"
```

### Tool Aliasing

Tools can be defined as aliases of other tools with preset parameters:

```yaml
name: clone-my-repo
type: alias                        # Define this tool as an alias
base-tool: github-repo-clone       # The tool being aliased
description: Clone my personal repository  # Custom description
default-parameters:                # Pre-configured parameters
  OWNER: "my-username"
  REPO: "my-project"
  OUTPUT_DIR: "./projects/{REPO}"
```

### Tool Versioning

Tools can specify version information for compatibility:

```yaml
name: tool-name
description: Tool description
version: 1.0.0                     # Tool version
min-cycod-version: "1.2.0"         # Minimum CYCOD version required
changelog:                         # Version history
  - version: 1.0.0
    changes: "Initial release"
  - version: 0.9.0
    changes: "Beta release with limited functionality"
```

### Tool Discovery

Tools can include metadata for better organization and discovery:

```yaml
metadata:
  category: file-management        # Primary category
  subcategory: search              # Subcategory
  tags: [files, search, find]      # Tags for filtering
  search-keywords: [locate, search, find, pattern]  # Additional search terms
  icon: "🔍"                       # Unicode icon for UI representation
  color: "#336699"                 # Color for UI representation
```