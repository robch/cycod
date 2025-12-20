# Chat Journal Tool - Project Plan

## Overview
A standalone executable tool to analyze JSONL chat history files stored in the user scope (`~/.cycod/history/`) and generate daily journal summaries of conversations.

## Problem Statement
Currently, chat history files accumulate in the history folder with:
- Lots of duplication at the beginning of files (due to conversation branching)
- No easy way to see "what did I work on today?"
- No visualization of how conversations branched from each other
- Difficult to extract just the meaningful user input and assistant responses (not huge tool outputs)

## Goals
1. **Extract meaningful content**: Focus on actual user-typed text and concise assistant responses
2. **Daily journal view**: Generate a summary of "what happened today" across all conversations
3. **Deduplicate branched conversations**: Identify and handle conversation branches intelligently
4. **Tree-like visualization**: Show how conversations relate to each other via branching
5. **Filter noise**: Exclude or minimize large tool outputs, piped content, etc.

## JSONL File Structure
Based on analysis of `chat-history-*.jsonl` files:

```jsonl
{"role":"system","content":"...system prompt..."}
{"role":"user","content":"user typed this"}
{"role":"assistant","content":"response text","tool_calls":[{"id":"toolu_xxx","function":{...}}]}
{"role":"tool","tool_call_id":"toolu_xxx","content":"tool output"}
```

### Key Fields
- **role**: system, user, assistant, tool
- **content**: The actual message text
- **tool_calls**: Array of tool invocations by assistant
- **tool_call_id**: Links tool responses to tool calls (useful for tracking conversation lineage)

### Branching Detection
Files that share the same `tool_call_id` values at the beginning are branches from the same conversation:
```
chat-history-1754437766985.jsonl: toolu_vrtx_01HEpSDWBG6MSN9es1qX18ej, ...
chat-history-1754437862035.jsonl: toolu_vrtx_01HEpSDWBG6MSN9es1qX18ej, ...
                                  ^ Same ID = they branched from same conversation
```

## Tool Name Options
- `cycodj` (Chat Journal)
- `cycod-journal`
- `chat-journal`
- `cycodh` (Chat History)

**Recommendation**: `cycodj` - follows the pattern of cycodmd, cycodt

## Features (MVP)

### 1. List Conversations
```bash
cycodj list
cycodj list --date 2024-12-20
cycodj list --last 10
```
Output:
```
2024-12-20
  10:15 AM - conversation-1754437373970 "Installing git cli"
  10:22 AM - conversation-1754437766985 "Fixing build errors"
  10:25 AM   ↳ conversation-1754437862035 (branch)
  11:30 AM - conversation-1754438564559 "Testing framework updates"
```

### 2. Show Conversation Summary
```bash
cycodj show conversation-1754437373970
cycodj show --file chat-history-1754437373970.jsonl
```
Output:
```
Conversation: Installing git cli
Started: 2024-12-20 10:15:33 AM
File: chat-history-1754437373970.jsonl
Branches: 1 (see: conversation-1754437786268)

Messages: 15 total (4 user, 6 assistant, 5 tool)

User:
  > install git cli
  > check if it worked
  > show me the version
  
Assistant (summary):
  - Checked for existing git installation
  - Installed Git using winget
  - Verified installation with git --version
```

### 3. Daily Journal
```bash
cycodj journal
cycodj journal --date 2024-12-20
cycodj journal --date today
cycodj journal --last 7d
```
Output:
```
# Journal for 2024-12-20

## Morning (6 conversations)

### 10:15 AM - Installing git cli
> install git cli

Installed Git using winget, verified version.

### 10:22 AM - Fixing build errors
> the build is failing with error CS0103

Fixed missing using statement for System.Linq.

  ↳ 10:25 AM - Tried alternative approach with IEnumerable
  
...
```

### 4. Detect and Show Branches
```bash
cycodj branches
cycodj branches --conversation conversation-1754437373970
```
Output:
```
Conversation tree:

conversation-1754437766985 (10:22 AM) "Fixing build errors"
├─ conversation-1754437862035 (10:25 AM) "Tried alternative approach"
└─ conversation-1754438012345 (10:30 AM) "Added more tests"
```

## Technical Architecture

### Project Structure
```
src/cycodj/
  ├── cycodj.csproj
  ├── Program.cs
  ├── Commands/
  │   ├── ListCommand.cs
  │   ├── ShowCommand.cs
  │   ├── JournalCommand.cs
  │   └── BranchesCommand.cs
  ├── Models/
  │   ├── ChatMessage.cs
  │   ├── Conversation.cs
  │   └── ConversationTree.cs
  ├── Analyzers/
  │   ├── ConversationAnalyzer.cs
  │   ├── BranchDetector.cs
  │   └── ContentSummarizer.cs
  └── Helpers/
      ├── JsonlReader.cs
      └── HistoryFileHelpers.cs
```

### Key Classes

#### ChatMessage
```csharp
public class ChatMessage
{
    public string Role { get; set; }
    public string Content { get; set; }
    public List<ToolCall>? ToolCalls { get; set; }
    public string? ToolCallId { get; set; }
}
```

#### Conversation
```csharp
public class Conversation
{
    public string Id { get; set; }
    public string FilePath { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Title { get; set; }
    public List<ChatMessage> Messages { get; set; }
    public List<string> ToolCallIds { get; set; } // For branch detection
}
```

#### BranchDetector
```csharp
public class BranchDetector
{
    public Dictionary<string, List<Conversation>> DetectBranches(List<Conversation> conversations)
    {
        // Group conversations by shared tool_call_ids
        // Return parent -> children mapping
    }
    
    public int FindCommonPrefixLength(Conversation a, Conversation b)
    {
        // Compare tool_call_ids to find where they diverge
    }
}
```

#### ContentSummarizer
```csharp
public class ContentSummarizer
{
    public string SummarizeUserMessages(Conversation conv)
    {
        // Extract and format user messages
    }
    
    public string SummarizeAssistantMessages(Conversation conv, int maxLength = 500)
    {
        // Extract assistant text responses (not tool output)
        // Abbreviate if needed
    }
    
    public bool IsLargeToolOutput(ChatMessage msg)
    {
        // Detect if this is a large tool output to exclude/minimize
    }
}
```

## Implementation Phases

**Prerequisites:** Before starting implementation, review [adding-new-cli-tool.md](adding-new-cli-tool.md) which documents the exact infrastructure changes needed based on how `cycodgr` was added to the project.

### Phase 0: Project Setup (Foundation)
- [x] Create project structure following cycod patterns
- [x] Set up cycodj.csproj with proper PackageId and tool settings
- [x] Add to solution file (cycod.sln)
- [x] Update CI/CD workflows (.github/workflows/*.yml)
- [x] Update build scripts (scripts/_functions.sh)
- [x] Create CycoDjProgramInfo class
- [x] Test project builds and integrates with CI

### Phase 1: Core Reading & Parsing
- [x] Create project structure
- [x] Implement JsonlReader to parse chat-history files
- [x] Create ChatMessage and Conversation models
- [x] Read all files from history directory
- [x] Parse timestamps from filenames
- [x] Basic list command

### Phase 2: Branch Detection
- [x] Implement tool_call_id extraction
- [x] Build BranchDetector algorithm
- [x] Create ConversationTree structure
- [x] Test with real branched conversations

### Phase 3: Content Analysis
- [ ] Filter user vs assistant vs tool messages
- [ ] Implement content summarization
- [ ] Detect and handle large tool outputs
- [ ] Extract conversation titles (from metadata or content)

### Phase 4: Commands & Output
- [ ] Implement show command
- [ ] Implement journal command with date filtering
- [ ] Implement branches command
- [ ] Add output formatting (colors, indentation)

### Phase 5: Advanced Features (Future)
- [ ] Search across conversations
- [ ] Export to markdown
- [ ] Statistics (messages per day, tool usage, etc)
- [ ] Interactive mode (TUI)
- [ ] Conversation merging/cleanup tools

## Questions to Resolve

1. **Differentiating piped vs typed input**: 
   - Currently no clear marker in JSONL
   - May need to use heuristics (length, formatting, etc)
   - Or accept that we can't perfectly distinguish

2. **Tool output handling**:
   - Show abbreviated version?
   - Show count ("15 tool outputs")?
   - Configurable verbosity?

3. **Title extraction**:
   - Some files may have metadata with titles
   - Fall back to first user message?
   - Generate title from content?

4. **Performance**:
   - User mentioned history folder is "huge"
   - May need lazy loading or indexing
   - Consider SQLite cache for metadata?

5. **Date/time handling**:
   - Timestamps in filenames are Unix epoch milliseconds
   - Need to convert to readable dates
   - Handle timezone properly

## Success Criteria
- Can generate a readable daily journal from chat history
- Accurately detects and visualizes conversation branches
- Runs fast enough on large history folders (100+ files)
- Provides useful summaries without overwhelming detail
- Helps answer "what did I work on today?"

## Related Code to Reference
- `src/cycod/Helpers/ChatHistoryFileHelpers.cs` - File discovery
- `src/cycod/Helpers/ChatMessageHelpers.cs` - JSONL serialization
- `src/cycod/ChatClient/Conversation.cs` - Conversation loading
- `src/cycod/Helpers/ConversationMetadataHelpers.cs` - Metadata handling

## Next Steps
1. Set up basic project with CommandLineParser
2. Implement JSONL reading for chat-history files
3. Create simple list command to verify we can parse files
4. Start branch detection algorithm with real data
