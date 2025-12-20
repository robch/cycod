# TODO - Chat Journal Investigation

## Investigate JSONL File Patterns

### Tool Call ID Analysis
- [x] Confirmed files with same tool_call_ids at beginning = branched conversations
- [ ] Determine: Do ALL branched conversations share tool_call_ids, or just some?
- [ ] Check: How far into a file do we need to read to identify branch point?
- [ ] Find: What's the maximum common prefix we've seen in real data?

### Content Differentiation  
- [ ] **PRIORITY**: Figure out how to differentiate typed vs piped content
  - Look for markers in the JSONL structure
  - Check if there are metadata fields we missed
  - Analyze user message patterns (length, structure, etc)
  - May need to add this feature to cycod if it doesn't exist

### Conversation Metadata
- [ ] Check: Do files have metadata line as first line?
  - See `ConversationMetadataHelpers.cs` references
- [ ] Find: What metadata fields are available?
  - Title
  - Created date
  - Parent conversation ID?
  - Branch point indicator?

### Tool Output Handling
- [ ] Measure: Average size of tool outputs vs user/assistant text
- [ ] Identify: Which tools produce the largest outputs?
- [ ] Determine: Best strategy for abbreviating
  - First N lines?
  - Summary of file operations?
  - Just show tool name + result status?

### File Size & Performance
User mentioned "huge" history folder:
- [ ] Count: How many files are we dealing with?
- [ ] Measure: Average file size
- [ ] Measure: Largest file size
- [ ] Test: How long does it take to read all files?
- [ ] Decide: Do we need indexing/caching?

### Timestamp Analysis
Filenames use Unix epoch milliseconds:
- [ ] Verify: Are these UTC or local time?
- [ ] Check: Are there gaps in timestamp sequences?
- [ ] Understand: How does conversation branching affect timestamps?
  - Does branched file get new timestamp?
  - Or does it reference original timestamp?

## Code Investigation

### Existing Conversation Code
- [ ] Read: `src/cycod/ChatClient/Conversation.cs` completely
  - Understand: How LoadFromFile works
  - Check: What metadata gets parsed
  - Find: Any existing branch tracking?

### Existing Helpers
- [ ] Study: `ChatMessageHelpers.cs`
  - Serialization format
  - Any helper methods we can reuse
  
- [ ] Study: `ConversationMetadataHelpers.cs`
  - What metadata exists
  - How to extract it
  
- [ ] Study: `TitleGenerationHelpers.cs`
  - How are titles generated?
  - Can we use this logic?

### Message Types
- [ ] Investigate: OpenAI.Chat.ChatMessage structure
  - What fields are available?
  - Any hidden/undocumented fields?
  - Role types beyond user/assistant/system/tool?

## Real Data Sampling

### Sample Various File Types
From `c:\users\r\.cycod\history\`:
- [ ] Sample: File with no tool calls (pure chat)
- [ ] Sample: File with many tool calls
- [ ] Sample: Very long conversation
- [ ] Sample: Short conversation
- [ ] Sample: Exception chat history files
- [ ] Sample: Files from different dates/times

### Branching Patterns
- [ ] Find: Clear example of 3+ files branching from same parent
- [ ] Document: The exact tool_call_ids at branch points
- [ ] Visualize: One complete conversation tree with all branches

### Conversation Topics
- [ ] Sample: What topics appear most frequently?
- [ ] Check: Are there patterns in when branching occurs?
  - Errors/exceptions?
  - Trying different approaches?
  - Saving before risky operations?

## Questions for User

1. **Typed vs Piped Content**
   - Do you care about differentiating piped-in files from typed text?
   - Or just want to exclude large file contents from summaries?
   - What about code blocks that you copy-pasted?

2. **Summary Detail Level**
   - How much detail do you want in daily journal?
   - Just topic headlines?
   - Include your questions/commands?
   - Include AI summaries of actions taken?

3. **Branch Handling**
   - How do you want branches displayed?
   - Inline in timeline? Separate section?
   - Collapsed by default?

4. **Date Ranges**
   - Most interested in "today"?
   - Or reviewing past conversations?
   - Want weekly/monthly summaries?

5. **Future Features**
   - Interest in searching conversations?
   - Want to "continue" from a journal entry?
   - Export to other formats?

## Experiments to Run

### Parsing Performance
```bash
# Count all chat-history files
ls -1 ~/.cycod/history/chat-history-*.jsonl | wc -l

# Total size
du -sh ~/.cycod/history/

# Average file size
ls -l ~/.cycod/history/chat-history-*.jsonl | awk '{total += $5; count++} END {print total/count/1024 " KB"}'

# Largest files
ls -lhS ~/.cycod/history/chat-history-*.jsonl | head -10
```

### Branch Detection Algorithm Test
```bash
# Find all unique tool_call_ids across first 10 lines of each file
for file in ~/.cycod/history/chat-history-*.jsonl; do
  echo "=== $file ==="
  head -10 "$file" | grep -o '"tool_call_id":"[^"]*"' | sort -u
done > tool_call_analysis.txt

# Group files by shared tool_call_ids
# (This is the core of branch detection)
```

### Content Analysis
```bash
# Count message types across all files
for file in ~/.cycod/history/chat-history-*.jsonl; do
  cat "$file" | jq -r '.role' 
done | sort | uniq -c

# Average message length by role
for file in ~/.cycod/history/chat-history-*.jsonl; do
  cat "$file" | jq -r 'select(.role=="user") | .content | length'
done | awk '{sum+=$1; count++} END {print "User avg: " sum/count " chars"}'

# Similar for assistant, tool
```

## Next Actions

Priority order:
1. ✅ Create worktree and branch
2. ✅ Create this TODO and planning document
3. Run performance/sizing experiments
4. Sample diverse JSONL files to understand full structure
5. Implement basic JSONL reader
6. Test branch detection algorithm with real data
7. Prototype simple daily listing
8. Get user feedback on format/approach
