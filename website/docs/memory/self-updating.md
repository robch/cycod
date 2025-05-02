---
hide:
- toc
---

--8<-- "snippets/ai-generated.md"

# Self-Updating Memories

A truly powerful memory system should evolve over time, capturing new insights and refining existing knowledge. CycoD's approach to memory enables several strategies for creating self-updating memories.

## Instructing the AI to Suggest Updates

The simplest approach is to instruct the AI to suggest updates to your memory files:

```bash
cycod --input "/files .memories/**/*.md" \
      --add-system-prompt "As we discuss topics related to the information in memory, suggest updates or additions to these memory files. Format your suggestions as:
      MEMORY UPDATE [filename]: The content to add or update." \
      --save-local-alias memory-writer
```

When you use this alias, the AI will proactively suggest memory updates:

```
> Tell me about ways we could improve our API authentication.

Based on the information in memory, your API currently uses basic JWT authentication. 
I can suggest several improvements:

1. Add refresh token functionality to avoid frequent re-authentication
2. Implement token revocation capabilities
3. Add rate limiting to prevent brute force attacks

MEMORY UPDATE [.memories/security/authentication.md]: 
## Potential Improvements
- Implement refresh token functionality
- Add token revocation capabilities for security incidents
- Implement rate limiting on authentication endpoints
```

You can then manually apply these suggested updates to your memory files.

## Creating a Memory Update Helper

To streamline the update process, you can create a simple script that watches for the update pattern and offers to apply the changes:

```bash
# Create a memory-update.sh script
echo '#!/bin/bash
MEMORY_DIR=".memories"

# Watch for the update pattern in cycod output
cycod "$@" | tee /dev/tty | while IFS= read -r line; do
  if [[ $line == "MEMORY UPDATE"* ]]; then
    # Extract filename and content
    filename=$(echo "$line" | sed -n "s/MEMORY UPDATE \[\(.*\)\]:.*/\1/p")
    
    # Read the content until we find a blank line or another command
    content=""
    while IFS= read -r content_line && [[ -n $content_line ]] && [[ ! $content_line == "MEMORY UPDATE"* ]]; do
      content="$content
$content_line"
    done
    
    # Ask user if they want to apply the update
    echo -e "\n\033[1;33mUpdate detected for: $filename\033[0m"
    echo -e "$content\n"
    read -p "Apply this update? (y/n) " choice
    
    if [[ $choice == "y" ]]; then
      mkdir -p "$(dirname "$filename")"
      if [[ -f "$filename" ]]; then
        echo -e "$content" >> "$filename"
        echo "✅ Updated $filename"
      else
        echo -e "$content" > "$filename"
        echo "✅ Created $filename"
      fi
    fi
  fi
done' > memory-update.sh

# Make it executable
chmod +x memory-update.sh
```

Now you can use this script:

```bash
./memory-update.sh --memory-writer --question "What improvements should we make to our API authentication?"
```

The script will detect MEMORY UPDATE suggestions and prompt you to apply them.

## Creating Memory-Writing Aliases

Create specialized aliases for different types of memory updates:

### Documenting Decisions

```bash
cycod --input "/files .memories/decisions/*.md" \
      --add-system-prompt "After discussing a decision, summarize it and suggest adding it to the decisions memory. Format as:
      MEMORY UPDATE [.memories/decisions/YYYY-MM-DD-topic.md]:
      # Decision: [Brief Title]
      
      ## Context
      [The situation that required a decision]
      
      ## Decision
      [The decision that was made]
      
      ## Rationale
      [Why this decision was chosen]
      
      ## Alternatives Considered
      [Other options that were evaluated]
      
      ## Date
      YYYY-MM-DD" \
      --save-local-alias decision-memory
```

### Capturing Meeting Notes

```bash
cycod --input "/files .memories/meetings/*.md" \
      --add-system-prompt "When discussing a meeting, help capture notes and suggest adding them to the meetings memory. Format as:
      MEMORY UPDATE [.memories/meetings/YYYY-MM-DD-topic.md]:
      # Meeting: [Brief Title]
      
      ## Date
      YYYY-MM-DD
      
      ## Attendees
      [List of people present]
      
      ## Agenda
      [Topics discussed]
      
      ## Decisions
      [Decisions made]
      
      ## Action Items
      [Tasks assigned]
      
      ## Notes
      [Additional details]" \
      --save-local-alias meeting-memory
```

## Automated Memory Extraction

For more advanced setups, you can create a workflow that periodically processes conversations to extract memory-worthy content:

```bash
# Create a script to extract memories from chat history
echo '#!/bin/bash
# Process recent chat history files to extract memory-worthy content
HISTORY_DIR="$HOME/.cycod/history"
NEWEST_HISTORY=$(ls -t $HISTORY_DIR/chat-history-*.jsonl | head -1)

echo "Processing: $NEWEST_HISTORY"

# Use CycoD to analyze the conversation and suggest memory updates
cycod --input-chat-history "$NEWEST_HISTORY" \
      --question "Review this conversation and extract any important information that should be added to our memory system. Format your suggestions as: MEMORY UPDATE [filename]: content" \
      --output-chat-history /dev/null

echo "Memory extraction complete"' > extract-memories.sh

chmod +x extract-memories.sh
```

You could run this script manually or schedule it to run periodically:

```bash
# Add to crontab to run daily
(crontab -l 2>/dev/null; echo "0 0 * * * $PWD/extract-memories.sh") | crontab -
```

## Memory Refinement Prompts

Beyond adding new information, you can instruct the AI to help refine existing memories:

```bash
cycod --input "/files .memories/**/*.md" \
      --add-system-prompt "Review the information in memory and suggest refinements to make it more accurate, concise, and useful. Format suggestions as:
      MEMORY REFINEMENT [filename]:
      CURRENT: [existing text to replace]
      REFINED: [improved version]" \
      --save-local-alias memory-refiner
```

## Semantic Memory Organization

You can periodically reorganize your memories based on semantic relationships:

```bash
cycod --input "/files .memories/**/*.md" \
      --add-system-prompt "Analyze the current memory organization and suggest improvements to the structure and categorization. Suggest moving related information into cohesive files and creating new categories as needed. Format as:
      MEMORY REORGANIZATION:
      [detailed plan for reorganizing memories]" \
      --save-local-alias memory-organizer
```

## Best Practices for Self-Updating Memories

1. **Regular review**: Schedule time to review and apply memory updates
2. **Clear formatting**: Use consistent patterns for update suggestions
3. **Verification**: Always verify AI-suggested updates before applying them
4. **Version control**: Keep your memory files in version control when possible
5. **Selective updates**: Not everything needs to be remembered; focus on valuable insights
6. **Metadata**: Include dates and sources for important information

## Next Steps

Now that you understand how to create self-updating memories, explore advanced [memory patterns and examples](patterns.md) for specific workflows.