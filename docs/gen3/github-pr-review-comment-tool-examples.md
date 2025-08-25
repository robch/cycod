# GitHub PR Review Comment Tool Examples

These example tools demonstrate how to use the CYCOD Custom Tools feature to create specialized tools for GitHub PR reviews, based on our experience using the GitHub CLI to add different types of comments.

## 1. PR Summary Comment Tool

This tool adds a top-level comment to a GitHub Pull Request.

```yaml
name: gh-pr-summary-comment
description: Add a summary comment to a GitHub Pull Request

bash: gh pr comment {PR_NUMBER} --body "{COMMENT}"

parameters:
  PR_NUMBER:
    type: string
    description: The PR number to comment on
    required: true
  
  COMMENT:
    type: string
    description: The comment content (supports Markdown)
    required: true

  REPO:
    type: string
    description: Repository in format 'owner/repo'
    required: false
    default: ""

working-directory: "{REPO_DIR}"
timeout: 15000
tags: [github, pr, comment, write]
platforms: [windows, linux, macos]

# Enhanced features based on our critique recommendations
environment:
  variables:
    GH_REPO: "{REPO}"
  inherit: true

error-handling:
  retry:
    attempts: 2
    delay: 1000
```

### Usage Examples

```bash
# Comment on PR in current repository
cycod gh-pr-summary-comment --PR_NUMBER 41 --COMMENT "# Comprehensive Review\n\nThis PR looks great overall, with a few suggestions for improvement."

# Comment on PR in specific repository
cycod gh-pr-summary-comment --PR_NUMBER 41 --REPO "robch/cycod" --COMMENT "Great work on this specification!"
```

## 2. PR Section Comment Tool

This tool adds a comment that references a specific section of code in a PR.

```yaml
name: gh-pr-section-comment
description: Add a section-specific comment to a GitHub Pull Request

steps:
  - name: create-comment
    bash: |
      if [ -n "{REPO}" ]; then
        gh repo set-current {REPO}
      fi
      
      cat << EOF > section_comment.md
      ## {SECTION_TITLE} (Lines {START_LINE}-{END_LINE})
      
      {COMMENT}
      
      ${EXAMPLE_BLOCK}
      EOF
      
      gh pr comment {PR_NUMBER} --body-file section_comment.md
      rm section_comment.md

parameters:
  PR_NUMBER:
    type: string
    description: The PR number to comment on
    required: true
  
  SECTION_TITLE:
    type: string
    description: Title for the section you're commenting on
    required: true
    
  START_LINE:
    type: number
    description: Starting line number of the section
    required: true
    
  END_LINE:
    type: number
    description: Ending line number of the section
    required: true
  
  COMMENT:
    type: string
    description: The comment content (supports Markdown)
    required: true
    
  EXAMPLE_CODE:
    type: string
    description: Example code to include (optional)
    required: false
    default: ""
    
  REPO:
    type: string
    description: Repository in format 'owner/repo'
    required: false
    default: ""

working-directory: "{REPO_DIR}"
timeout: 20000
tags: [github, pr, comment, section, write]
platforms: [windows, linux, macos]

# Enhanced features based on our critique recommendations
environment:
  variables:
    EXAMPLE_BLOCK: "$(if [ -n \"{EXAMPLE_CODE}\" ]; then echo -e \"\\n\`\`\`yaml\\n{EXAMPLE_CODE}\\n\`\`\`\"; fi)"
  inherit: true
```

### Usage Examples

```bash
# Comment on a specific section
cycod gh-pr-section-comment \
  --PR_NUMBER 41 \
  --SECTION_TITLE "Parameter Handling Enhancement Opportunities" \
  --START_LINE 54 \
  --END_LINE 75 \
  --COMMENT "Consider adding validation rules for parameters and enhanced documentation."

# With example code
cycod gh-pr-section-comment \
  --PR_NUMBER 41 \
  --SECTION_TITLE "Error Handling Enhancement" \
  --START_LINE 138 \
  --END_LINE 153 \
  --COMMENT "Consider adding more sophisticated recovery options like retries and fallbacks." \
  --EXAMPLE_CODE "error-handling:\n  retry:\n    attempts: 3\n    delay: 1000  # milliseconds\n  fallback: alternative-command {PARAM}"
```

## 3. PR Line Comment Tool

This tool adds a line-specific comment to a GitHub Pull Request.

```yaml
name: gh-pr-line-comment
description: Add a line-specific comment to a GitHub Pull Request

steps:
  - name: get-commit-id
    bash: gh pr view {PR_NUMBER} --json commits | jq -r '.commits[-1].oid'
    
  - name: create-json
    bash: |
      cat << EOF > line_comment.json
      {
        "commit_id": "{get-commit-id.output}",
        "body": "Line-specific suggestion for GitHub PR",
        "event": "COMMENT",
        "comments": [
          {
            "path": "{FILE_PATH}",
            "position": {LINE_NUMBER},
            "body": "{COMMENT}"
          }
        ]
      }
      EOF
      
  - name: submit-comment
    bash: |
      if [ -n "{REPO}" ]; then
        REPO_PATH="repos/{REPO}"
      else
        REPO_PATH="repos/$(gh repo view --json nameWithOwner -q .nameWithOwner)"
      fi
      
      cat line_comment.json | gh api --method POST $REPO_PATH/pulls/{PR_NUMBER}/reviews --input -
      rm line_comment.json

parameters:
  PR_NUMBER:
    type: string
    description: The PR number to comment on
    required: true
  
  FILE_PATH:
    type: string
    description: Path to the file you want to comment on
    required: true
    
  LINE_NUMBER:
    type: number
    description: Line number to attach the comment to
    required: true
  
  COMMENT:
    type: string
    description: The comment content (supports Markdown)
    required: true
    
  REPO:
    type: string
    description: Repository in format 'owner/repo'
    required: false
    default: ""

working-directory: "{REPO_DIR}"
timeout: 30000
tags: [github, pr, comment, line, write]
platforms: [windows, linux, macos]

# Enhanced features based on our critique recommendations
tests:
  - name: basic-test
    parameters:
      PR_NUMBER: "1"
      FILE_PATH: "README.md"
      LINE_NUMBER: 10
      COMMENT: "Test comment"
    expected:
      exit-code: 0

resources:
  timeout: 30000
  cleanup:
    - delete-temp-files: true
```

### Usage Examples

```bash
# Add a line-specific comment
cycod gh-pr-line-comment \
  --PR_NUMBER 41 \
  --FILE_PATH "docs/gen3/custom-tools-spec-draft-1.md" \
  --LINE_NUMBER 126 \
  --COMMENT "**Parameter Types Enhancement**: Consider adding validation rules for each parameter type. For example:\n\`\`\`yaml\nparameter-types:\n  string: \n    validation: [pattern, min-length, max-length]\n  number:\n    validation: [minimum, maximum, multiple-of]\n\`\`\`"

# Multiple line-specific comments
# Note: This would require multiple tool invocations or enhancing the tool to accept multiple comments
```

## 4. Advanced PR Multi-Comment Tool

This tool combines the abilities of the previous tools, allowing you to add multiple types of comments in a single batch.

```yaml
name: gh-pr-multi-comment
description: Add multiple comments to a GitHub Pull Request with a single command

steps:
  - name: create-comment-file
    bash: |
      cat << EOF > comments.json
      {
        "summary": "{SUMMARY_COMMENT}",
        "sections": {SECTION_COMMENTS_JSON},
        "lines": {LINE_COMMENTS_JSON}
      }
      EOF
  
  - name: get-commit-id
    bash: gh pr view {PR_NUMBER} --json commits | jq -r '.commits[-1].oid'
    
  - name: add-summary-comment
    bash: |
      if [ -n "{SUMMARY_COMMENT}" ]; then
        gh pr comment {PR_NUMBER} --body "{SUMMARY_COMMENT}"
      fi
    continue-on-error: true
    
  - name: add-section-comments
    bash: |
      if [ -n "{SECTION_COMMENTS_JSON}" ] && [ "{SECTION_COMMENTS_JSON}" != "null" ]; then
        cat comments.json | jq -r '.sections[] | "## " + .title + " (Lines " + (.start_line|tostring) + "-" + (.end_line|tostring) + ")\n\n" + .comment' | while read -r content; do
          gh pr comment {PR_NUMBER} --body "$content"
        done
      fi
    continue-on-error: true
    
  - name: add-line-comments
    bash: |
      if [ -n "{LINE_COMMENTS_JSON}" ] && [ "{LINE_COMMENTS_JSON}" != "null" ]; then
        REPO_PATH="repos/$(gh repo view --json nameWithOwner -q .nameWithOwner)"
        
        cat comments.json | jq '{
          "commit_id": "'{get-commit-id.output}'",
          "body": "Line-specific comments for GitHub PR",
          "event": "COMMENT",
          "comments": [.lines[] | {
            "path": .file_path,
            "position": .line_number,
            "body": .comment
          }]
        }' > line_comments.json
        
        cat line_comments.json | gh api --method POST $REPO_PATH/pulls/{PR_NUMBER}/reviews --input -
      fi
    continue-on-error: true
    
  - name: cleanup
    bash: |
      rm -f comments.json line_comments.json

parameters:
  PR_NUMBER:
    type: string
    description: The PR number to comment on
    required: true
  
  SUMMARY_COMMENT:
    type: string
    description: Top-level summary comment for the PR
    required: false
    default: ""
    
  SECTION_COMMENTS_JSON:
    type: string
    description: JSON array of section comments (format below)
    required: false
    default: "[]"
    
  LINE_COMMENTS_JSON:
    type: string
    description: JSON array of line-specific comments (format below)
    required: false
    default: "[]"
    
  REPO:
    type: string
    description: Repository in format 'owner/repo'
    required: false
    default: ""

working-directory: "{REPO_DIR}"
timeout: 60000
tags: [github, pr, comment, advanced, write]
platforms: [windows, linux, macos]

# Enhanced features based on our critique recommendations
detailed-help: |
  This tool allows adding multiple types of comments to a PR in one operation.
  
  Format for SECTION_COMMENTS_JSON:
  ```json
  [
    {
      "title": "Section Title",
      "start_line": 10,
      "end_line": 20,
      "comment": "Comment text with **markdown** support"
    }
  ]
  ```
  
  Format for LINE_COMMENTS_JSON:
  ```json
  [
    {
      "file_path": "path/to/file.md",
      "line_number": 42,
      "comment": "Line-specific comment with **markdown** support"
    }
  ]
  ```
```

### Usage Examples

```bash
# Add multiple types of comments in one command
cycod gh-pr-multi-comment \
  --PR_NUMBER 41 \
  --SUMMARY_COMMENT "# Comprehensive Review\n\nHere are my thoughts on this PR." \
  --SECTION_COMMENTS_JSON '[{"title":"Parameter Handling","start_line":54,"end_line":75,"comment":"Consider adding validation."}]' \
  --LINE_COMMENTS_JSON '[{"file_path":"docs/file.md","line_number":126,"comment":"Specific suggestion here."}]'

# Using a file for complex JSON
cycod gh-pr-multi-comment --PR_NUMBER 41 --LINE_COMMENTS_JSON "$(cat line_comments.json)"
```

## Additional Features

These tools could be enhanced with the following features, based on our critique recommendations:

1. **Parameter Validation**:
   - Validate PR numbers are numeric
   - Ensure file paths exist in the repository
   - Check that line numbers are within range

2. **Error Handling**:
   - Add retry mechanisms for network failures
   - Provide fallback options for comment submission
   - Include detailed error reporting

3. **Interactivity**:
   - Add interactive mode for confirming comments before posting
   - Allow editing comments in-place

4. **Tool Composition**:
   - Allow these tools to be used together in workflows
   - Support referencing outputs between tools

These examples demonstrate how the Custom Tools specification could be used to create powerful, reusable workflows for GitHub PR reviews, making the review process more efficient and consistent.