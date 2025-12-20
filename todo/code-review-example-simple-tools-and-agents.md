# Code Review Example: Simple Tools and Agents

This document shows how to build up a complete code review system using the simple tools specification and markdown-based agents.

## Building Blocks: Basic Git Tools

### changed-files.yaml
```yaml
name: changed-files
description: Get list of files changed since master

bash: git diff --name-only {BASE_BRANCH}..HEAD

parameters:
  BASE_BRANCH:
    type: string
    description: Branch to compare against
    default: "master"

tags: [git, read]
platforms: [windows, linux, macos]
```

### file-diff.yaml
```yaml
name: file-diff  
description: Show diff for a specific file since master

bash: git diff {BASE_BRANCH}..HEAD -- "{FILE}"

parameters:
  FILE:
    type: string
    description: Path to the file to show diff for
    required: true
  BASE_BRANCH:
    type: string
    description: Branch to compare against  
    default: "master"

tags: [git, read]
platforms: [windows, linux, macos]
```

### all-diffs.yaml
```yaml
name: all-diffs
description: Show all diffs since master

bash: git diff {BASE_BRANCH}..HEAD

parameters:
  BASE_BRANCH:
    type: string
    description: Branch to compare against
    default: "master"

tags: [git, read]  
platforms: [windows, linux, macos]
```

## Analysis Tools

### analyze-file.yaml
```yaml
name: analyze-file
description: Analyze a single file for code quality issues

bash: |
  echo "=== File Analysis: {FILE} ==="
  echo "Lines of code:"
  wc -l "{FILE}"
  echo ""
  echo "File type:"
  file "{FILE}"
  echo ""
  echo "Recent changes:"
  git log --oneline -5 -- "{FILE}" || echo "No git history"

parameters:
  FILE:
    type: string
    description: Path to file to analyze
    required: true

tags: [analysis, read]
platforms: [windows, linux, macos]
```

### analyze-diff.yaml
```yaml
name: analyze-diff
description: Analyze diff content for patterns and complexity

python: |
  import sys
  diff_content = """{DIFF_CONTENT}"""
  
  lines = diff_content.split('\n')
  added_lines = [l for l in lines if l.startswith('+') and not l.startswith('+++')]
  removed_lines = [l for l in lines if l.startswith('-') and not l.startswith('---')]
  
  print(f"Lines added: {len(added_lines)}")
  print(f"Lines removed: {len(removed_lines)}")
  print(f"Net change: {len(added_lines) - len(removed_lines)}")
  
  # Look for potential issues
  issues = []
  for line in added_lines:
      if 'TODO' in line: issues.append(f"TODO found: {line.strip()}")
      if 'console.log' in line: issues.append(f"Debug code: {line.strip()}")
      if 'password' in line.lower(): issues.append(f"Password reference: {line.strip()}")
  
  if issues:
      print("\nPotential issues:")
      for issue in issues: print(f"  - {issue}")

parameters:
  DIFF_CONTENT:
    type: string
    description: The diff content to analyze
    required: true

tags: [analysis, read]
platforms: [windows, linux, macos]
```

## Composition Tools

### review-single-file.yaml
```yaml
name: review-single-file
description: Complete review of one file - both current state and recent changes

steps:
  - name: get-file-diff
    tool: file-diff
    with:
      FILE: "{FILE}"
      BASE_BRANCH: "{BASE_BRANCH}"

  - name: analyze-current-file
    tool: analyze-file
    with:
      FILE: "{FILE}"

  - name: analyze-the-diff
    tool: analyze-diff
    with:
      DIFF_CONTENT: "{get-file-diff.output}"

  - name: show-summary
    bash: |
      echo "=== REVIEW SUMMARY FOR {FILE} ==="
      echo ""
      echo "Current file analysis:"
      echo "{analyze-current-file.output}"
      echo ""
      echo "Diff analysis:"
      echo "{analyze-the-diff.output}"
      echo ""
      echo "Git diff:"
      echo "{get-file-diff.output}"

parameters:
  FILE:
    type: string
    description: File to review
    required: true
  BASE_BRANCH:
    type: string
    description: Branch to compare against
    default: "master"

tags: [review, analysis, read]
platforms: [windows, linux, macos]
```

### loop-review-files.yaml
```yaml
name: loop-review-files
description: Loop through files and review each one individually

steps:
  - name: get-changed-files
    tool: changed-files
    with:
      BASE_BRANCH: "{BASE_BRANCH}"

  - name: review-each-file
    bash: |
      echo "=== REVIEWING ALL CHANGED FILES ==="
      echo ""
      echo "Changed files:"
      echo "{get-changed-files.output}"
      echo ""
      for file in {get-changed-files.output}; do
        if [ -f "$file" ]; then
          echo "========================================"
          cycod tool run review-file --param FILE="$file" --param BASE_BRANCH="{BASE_BRANCH}"
          echo ""
        fi
      done

parameters:
  BASE_BRANCH:
    type: string
    description: Branch to compare against
    default: "master"

tags: [review, analysis, read]
platforms: [windows, linux, macos]
```

## Agent Wrapper Tools

### review-file.yaml
```yaml
name: review-file
description: Review a specific file using the file-reviewer agent

file-reviewer:
  FILE: "{FILE}"
  BASE_BRANCH: "{BASE_BRANCH}"

parameters:
  FILE:
    type: string
    description: File to review
    required: true
  BASE_BRANCH:
    type: string
    description: Branch to compare against
    default: "master"

tags: [review, agent-wrapper]
platforms: [windows, linux, macos]
```

### review-changes.yaml
```yaml
name: review-changes
description: Review all changed files using code-reviewer agent

steps:
  - name: get-files
    tool: changed-files
    with:
      BASE_BRANCH: "{BASE_BRANCH}"
      
  - name: invoke-reviewer
    code-reviewer:
      CHANGED_FILES: "{get-files.output}"
      BASE_BRANCH: "{BASE_BRANCH}"

parameters:
  BASE_BRANCH:
    type: string
    description: Branch to compare against
    default: "master"

tags: [review, agent-wrapper]
platforms: [windows, linux, macos]
```

## Agents

### file-reviewer.md
```markdown
---
uses:
  tools: [analyze-file, analyze-diff, file-diff]
---

# File Reviewer

You are a senior software engineer who can review files for quality, security, and best practices.

When asked to review a file, you should:

1. **Use available tools** - gather technical data about the file and its changes
2. **Analyze current state** - code quality, structure, readability
3. **Examine recent changes** - what was added, removed, or modified  
4. **Identify issues** - bugs, security concerns, style violations
5. **Suggest improvements** - specific, actionable recommendations
6. **Prioritize findings** - focus on the most impactful issues first

Be constructive and specific. Reference line numbers when possible. 
Explain WHY something is an issue, not just WHAT the issue is.

You have access to file analysis tools - use them to inform your review.
```

### code-reviewer.md
```markdown
---
uses:
  tools: [changed-files, all-diffs]
---

# Code Reviewer

You are a lead engineer who can conduct comprehensive code reviews.

When asked to review changes, you should:

1. **Get the big picture** - understand what files changed and why
2. **Use available tools** - gather technical data about the changes
3. **Analyze each file** - review individual files for quality and issues
4. **Consider cross-file impact** - how changes interact across the codebase
5. **Provide summary** - prioritize findings and suggest next steps

Focus on:
- Code quality and maintainability
- Potential bugs or regressions  
- Security implications
- Performance considerations
- Team coding standards
- Architecture and design patterns

Provide both detailed feedback and a high-level summary for stakeholders.

You have access to git tools - use them to understand the scope and nature of changes.
```

## Usage Examples

### Review a single file (via tool that wraps agent)
```bash
cycod tool run review-file --param FILE=src/main.py
```

### Review all changes since master (via tool that wraps agent)
```bash
cycod tool run review-changes --param BASE_BRANCH=master
```

### Loop through and review each file individually
```bash
cycod tool run loop-review-files --param BASE_BRANCH=develop
```

### Direct agent usage (if you want to interact with the agent directly)
```bash
cycod agent run file-reviewer --param FILE=src/main.py
cycod agent run code-reviewer --param BASE_BRANCH=master
```

## Architecture Demonstration

This example shows:

- **Simple tools** (changed-files, file-diff, analyze-file) that do one thing well
- **Composition tools** (review-single-file, loop-review-files) that combine simple tools  
- **Agent wrapper tools** (review-file, review-changes) that invoke agents cleanly
- **Agents** (file-reviewer, code-reviewer) that provide intelligence and can use tools
- **The neat interplay**: 
  - Tools can wrap agents (`review-file` → `file-reviewer`)
  - Agents can use tools (`file-reviewer` → `analyze-file`)
  - Clean agent invocation syntax (`file-reviewer: { params }`)
- **Consistent patterns** - same `uses:`/`with:` dependency system across tools and agents

The result is a complete code review system built from simple, reusable components that demonstrate the power of tools and agents working together through clean composition patterns.