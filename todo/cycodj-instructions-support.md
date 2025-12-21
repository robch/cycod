# ✅ IMPLEMENTED - Add --instructions Support to cycodj

**Status:** Completed and pushed to robch/2512-dec20-chat-journal

## Implementation Summary

Successfully added --instructions support to `cycodj journal` command following the pattern from cycodmd and cycodgr.

### What Was Implemented

1. **Command Line Options** - Added three new options to JournalCommand:
   - `--instructions <text>` - AI instructions for processing journal output
   - `--use-built-in-functions` - Enable built-in AI functions
   - `--save-chat-history <path>` - Save the AI processing chat history

2. **Prompt Templates** - Created embedded prompt files:
   - `src/cycodj/assets/prompts/system.md` - System prompt for chat history analysis
   - `src/cycodj/assets/prompts/user.md` - User prompt template

3. **Refactored Output Generation** - Modified JournalCommand to:
   - Generate journal output to string first (GenerateJournalOutput method)
   - Optionally process through AI with AiInstructionProcessor
   - Maintain backward compatibility (works without --instructions)

4. **Leveraged Common Library** - Uses existing AiInstructionProcessor from common library

### Examples That Now Work

```bash
# List conversation titles as bullets
cycodj journal --instructions "List all conversation titles as a bulleted list"

# Summarize accomplishments
cycodj journal --detailed --instructions "Summarize what I accomplished today in 3-5 bullet points"

# Focus on specific aspects
cycodj journal --instructions "Show only conversations where files were modified"
cycodj journal --instructions "Create a status report for stakeholders"
cycodj journal --instructions "List all TODO items mentioned"
```

## Original Context
Following the pattern established in `cycodmd` and `cycodgr`, add `--instructions` support to `cycodj` for AI-powered customization of journal output.

## Use Cases

### End-of-Day Review
```bash
cycodj journal --instructions "Focus on code changes and technical decisions made"
cycodj journal --instructions "Show only conversations where tests were run"
cycodj journal --instructions "Summarize what I accomplished today in bullet points"
```

### Filtering and Focus
```bash
cycodj journal --instructions "Only show conversations about the cycodj project"
cycodj journal --instructions "Highlight conversations where errors occurred"
cycodj journal --instructions "Show conversations where new files were created"
```

### Custom Summaries
```bash
cycodj journal --instructions "Create a status report for stakeholders"
cycodj journal --instructions "List all TODO items mentioned in conversations"
cycodj journal --instructions "Summarize what files were modified and why"
```

## Implementation Approach

1. **Add --instructions parameter** to JournalCommand (and possibly ListCommand, ExportCommand)

2. **Wrap output through cycod** when --instructions is provided:
   - Collect the normal journal output
   - Pass it to cycod with instructions like:
     ```
     Given this chat history journal output, [user instructions]
     
     [journal output]
     ```

3. **Consider caching** - instructions-based summaries could be expensive, might want to cache results

4. **Template support** - might want common instruction templates:
   - `--instructions-template status-report`
   - `--instructions-template file-changes`
   - `--instructions-template decisions-made`

## Related Code
- `src/cycodj/CommandLineCommands/JournalCommand.cs`
- `src/cycodj/CommandLineCommands/ListCommand.cs`
- `src/cycodj/CommandLineCommands/ExportCommand.cs`
- Reference: `src/cycodmd/` for --instructions pattern
- Reference: `src/cycodgr/` for --instructions pattern

## Benefits
- Flexible, powerful end-user customization
- No need to hardcode every possible view/filter
- Leverages AI for intelligent summarization
- Consistent with other tools in the suite

## Challenges
- API costs (each --instructions call = API request)
- Response time (might be slow for large journals)
- Caching strategy needed
- Need good default/example instructions

## Examples from Other Tools

### cycodmd
```bash
cycodmd README.md --instructions "Format as a concise bullet list"
```

### cycodgr
```bash
cycodgr --instructions "Use conventional commits format"
```

## Next Steps
1. Review how cycodmd and cycodgr implement --instructions
2. Design the prompt template for journal instructions
3. Add parameter to command line options
4. Implement cycod integration
5. Add tests
6. Document common instruction patterns
