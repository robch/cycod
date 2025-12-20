# Investigation Findings - Chat History JSONL Files

## File Structure Discovery

### Location
- User scope: `~/.cycod/history/` (or `c:\users\r\.cycod\history\` on Windows)
- Naming pattern: `chat-history-{timestamp}.jsonl`
- Timestamp: Unix epoch in milliseconds

### JSONL Format
Each line is a JSON object representing one message in the conversation:

```jsonl
{"role":"system","content":"...system prompt..."}
{"role":"user","content":"user typed this"}
{"role":"assistant","content":"response","tool_calls":[{"id":"toolu_xxx","function":{...}}]}
{"role":"tool","tool_call_id":"toolu_xxx","content":"tool output"}
```

### Message Roles
- **system**: Initial system prompt
- **user**: User's input/questions
- **assistant**: AI responses (includes tool_calls when calling functions)
- **tool**: Results from tool/function executions

## Branch Detection Discovery

### Key Finding: tool_call_id as Branch Signature

When conversations branch (user continues from an earlier point), the new file contains:
1. All the messages from the parent conversation up to the branch point
2. The new divergent messages

**Evidence:**
```
chat-history-1754437766985.jsonl:
  tool_call_id: toolu_vrtx_01HEpSDWBG6MSN9es1qX18ej
  tool_call_id: toolu_vrtx_01BBxwL7zWLpF5R1aEfL9gLb
  tool_call_id: toolu_vrtx_01BWxB4oA4eJUnnjZHsDDc4M
  
chat-history-1754437862035.jsonl:
  tool_call_id: toolu_vrtx_01HEpSDWBG6MSN9es1qX18ej  ← Same
  tool_call_id: toolu_vrtx_01BBxwL7zWLpF5R1aEfL9gLb  ← Same
  tool_call_id: toolu_vrtx_01BWxB4oA4eJUnnjZHsDDc4M  ← Same
  ... then diverges with different IDs
```

This means:
- Files sharing initial tool_call_ids are branches from the same conversation
- The longer the shared prefix, the later the branch point
- We can build a conversation tree by comparing tool_call_id sequences

### Tool Call ID Patterns

Different providers use different formats:
- Claude/Anthropic: `toolu_vrtx_01HEpSDWBG6MSN9es1qX18ej`
- Other: `tooluse_BJN3ZWdWSeyUC_zGZsl96Q`

But all are unique and consistent within a conversation lineage.

## Sample Message Flow

From `chat-history-1754437373970.jsonl`:

```
Line 1: system prompt (long, contains instructions)
Line 2: {"role":"user","content":"install git cli"}
Line 3: {"role":"assistant","content":"I'll help...","tool_calls":[{...RunPowershellCommandAsync...}]}
Line 4: {"role":"tool","tool_call_id":"toolu_vrtx_01BGur...", "content":"<exited with exit code 1>"}
Line 5: {"role":"assistant","content":"Let me install...","tool_calls":[{...winget install...}]}
...
```

Pattern:
1. User asks question
2. Assistant responds with text + tool_calls
3. Tool results returned with tool_call_id linking back
4. Assistant may respond again with more text/tool_calls

## File Statistics (Sample)

From user's history directory listing:
- Minimum: Several empty/small conversations
- Pattern: Many files from same day with similar timestamps (branching pattern)
- Timestamps show conversations clustered around active work periods

## Content Characteristics

### User Messages
- Generally short, concise requests
- Natural language questions or commands
- Example: "install git cli"

### Assistant Messages  
- Text explanations/responses
- Tool calls for executing functions
- Can be very short ("Let me check...") or longer explanations

### Tool Messages
- Can be VERY large (file contents, command outputs)
- Include escape sequences and formatting
- Often contain multi-line outputs
- Example: Full build output, file listings, error messages

### System Messages
- Very long initial prompt with instructions
- Contains OS info, helper methods, guidelines
- Appears at start of most/all conversations

## Related Code References

### File Discovery
`src/cycod/Helpers/ChatHistoryFileHelpers.cs`:
- `FindMostRecentChatHistoryFile()` - Searches user/local scopes
- `GroundAutoSaveChatHistoryFileName()` - Creates timestamped filenames
- Pattern: `chat-history-{time}.jsonl`

### JSONL Serialization
`src/cycod/Helpers/ChatMessageHelpers.cs`:
- `AsJsonl()` - Converts messages to JSONL format
- `SaveToFile()` - Writes JSONL with metadata
- Uses `JsonSerializerOptions` with `WriteIndented = false`

### Conversation Loading
`src/cycod/ChatClient/Conversation.cs`:
- `LoadFromFile()` - Reads JSONL and parses messages
- Handles metadata (title, etc) in first line
- Can trim history based on token targets

### Metadata Support
`src/cycod/Helpers/ConversationMetadataHelpers.cs`:
- First line of JSONL may contain conversation metadata
- Includes title, creation time, etc
- Format: `{"metadata":{...}}`

## Open Questions

1. **Metadata Prevalence**: 
   - How many files actually have metadata as first line?
   - Need to check if it's recent addition or always present

2. **Typed vs Piped Content**:
   - No obvious marker found in JSONL structure
   - May need heuristics (length, content patterns)
   - Or just focus on excluding large tool outputs

3. **Title Extraction**:
   - Check if metadata.title exists in files
   - If not, may need to generate from first user message
   - See TitleGenerationHelpers.cs for title generation logic

4. **Performance at Scale**:
   - User mentioned "huge" folder - need actual count
   - May need to benchmark reading all files
   - Consider incremental loading or indexing

5. **Exception Files**:
   - Pattern: `exception-chat-history*.jsonl`
   - Need to understand their structure/purpose
   - Should they be included in journal?

## Next Investigation Steps

1. Count total files and measure sizes
2. Sample files to check metadata presence
3. Analyze branching patterns with real data
4. Test branch detection algorithm
5. Measure tool output sizes vs user/assistant text
6. Check timezone handling of timestamps

## Useful Commands for Further Analysis

```bash
# Count all chat history files
ls -1 ~/.cycod/history/chat-history-*.jsonl | wc -l

# Total size
du -sh ~/.cycod/history/

# Analyze first lines for metadata
for f in ~/.cycod/history/chat-history-*.jsonl; do 
  head -1 "$f" | jq -r 'if .metadata then "HAS_META" else "NO_META" end'
done | sort | uniq -c

# Find files with shared tool_call_ids (branch detection)
for f in ~/.cycod/history/chat-history-*.jsonl; do
  echo "=== $(basename $f) ==="
  head -20 "$f" | grep -o '"tool_call_id":"[^"]*"' | head -5
done > tool_call_map.txt

# Count messages by role
for f in ~/.cycod/history/chat-history-*.jsonl; do
  cat "$f" | jq -r '.role'
done | sort | uniq -c
```
