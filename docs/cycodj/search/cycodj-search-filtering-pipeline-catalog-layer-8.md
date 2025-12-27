# cycodj search - Layer 8: AI Processing

## Overview

Layer 8 (AI Processing) enables AI-assisted analysis of search results. Users can apply custom instructions to process, summarize, or transform search output using AI capabilities.

## Implementation Status

✅ **IMPLEMENTED** - The search command inherits AI processing capabilities from the base `CycoDjCommand` class.

## CLI Options

### From `CycoDjCommand` Base Class

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `--instructions <text>` | string | null | AI instructions to apply to output |
| `--use-built-in-functions` | flag | false | Enable AI to use built-in functions |
| `--save-chat-history <file>` | string | null | Save AI interaction history to file |

### Command-Line Parser Support

**Source**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

All cycodj commands share the same AI processing options parser.

## Implementation Details

### Execution Flow

**Source**: `src/cycodj/CommandLineCommands/SearchCommand.cs`

Lines 23-40: The search command applies AI instructions after generating results:

```csharp
public override async System.Threading.Tasks.Task<int> ExecuteAsync()
{
    var output = GenerateSearchOutput();
    
    // Apply instructions if provided
    var finalOutput = ApplyInstructionsIfProvided(output);
    
    // Save to file if --save-output was provided
    if (SaveOutputIfRequested(finalOutput))
    {
        return await System.Threading.Tasks.Task.FromResult(0);
    }
    
    // Otherwise print to console
    ConsoleHelpers.WriteLine(finalOutput);
    
    return await System.Threading.Tasks.Task.FromResult(0);
}
```

### AI Processing Logic

**Source**: `src/cycodj/CommandLine/CycoDjCommand.cs`

The base class `ApplyInstructionsIfProvided()` method handles AI processing:
- Checks if Instructions property is set
- If yes: calls `AiInstructionProcessor.ApplyInstructions()`
- If no: returns original output unchanged

## Data Flow

```
1. GenerateSearchOutput() creates search results with matches
   ↓
2. ApplyInstructionsIfProvided() checks if Instructions is set
   ↓
3. If Instructions present:
   - Calls AiInstructionProcessor.ApplyInstructions()
   - Passes: instructions, output, built-in functions flag, chat history path
   - Returns: AI-processed output
   ↓
4. If no Instructions:
   - Returns original output unchanged
   ↓
5. Output proceeds to Layer 7 (Output Persistence)
```

## Usage Examples

### Example 1: Summarize Search Results

```bash
cycodj search "authentication" --instructions "Summarize the key findings about authentication implementations"
```

This will:
1. Search for "authentication" in conversations
2. Generate search results with context
3. AI summarizes the findings
4. Return summarized output

### Example 2: Extract Code Patterns

```bash
cycodj search "async.*Task" --regex --instructions "Extract all unique async/await patterns found and explain them" --save-output patterns.md
```

This will:
1. Search using regex pattern
2. Find matches in conversations
3. AI extracts and explains patterns
4. Save analysis to file

### Example 3: Analyze Search Results with Functions

```bash
cycodj search "error" --last 20 --instructions "Use SearchFiles to find related error logs and correlate with conversation errors" --use-built-in-functions
```

This will:
1. Search last 20 conversations for "error"
2. AI has access to built-in functions
3. Can call SearchFiles or other tools to enhance analysis
4. Return correlated findings

### Example 4: Create Structured Output

```bash
cycodj search "feature request" --instructions "Create a JSON array of all feature requests mentioned with: conversation_id, date, request_summary, priority (if mentioned)" --save-output requests.json
```

This will:
1. Search for "feature request" mentions
2. AI extracts structured data
3. Returns JSON format
4. Saves to file

## Integration Points

### With Layer 5 (Context Expansion)

AI processing happens AFTER context expansion:
- `--context` option determines how much surrounding text is shown
- AI receives the full search output including context lines
- Larger context gives AI more information for analysis

### With Layer 6 (Display Control)

AI processing happens AFTER display formatting:
- `--show-stats` affects what statistics are included in output
- AI receives the formatted search results
- AI-processed output maintains or transforms the structure

### With Layer 7 (Output Persistence)

- If both `--instructions` and `--save-output` are used:
  - AI processes the search results first
  - Processed output is saved to file
- If `--save-chat-history` is also used:
  - Separate file contains AI conversation history
  - Useful for debugging AI processing of complex searches

## Behavioral Notes

1. **Optional Processing**: AI processing only occurs if `--instructions` is provided
2. **Pass-through**: If no instructions, search results are unchanged
3. **Post-search**: AI sees the complete search results with matches and context
4. **Function Access**: `--use-built-in-functions` enables AI to perform additional searches or file operations
5. **History Persistence**: `--save-chat-history` saves the AI interaction for review

## Limitations

1. **No Query Modification**: AI cannot change the search query retroactively
2. **No Re-search**: AI cannot trigger additional searches (unless using built-in functions)
3. **String-based**: AI receives markdown string, not structured search result objects
4. **Single Pass**: Instructions are applied once, not iteratively

## Performance Considerations

- AI processing adds latency (typically 1-5 seconds)
- Complex searches with many results may take longer to process
- Consider using `--save-output` for large result sets
- Use `--save-chat-history` for debugging to avoid re-running expensive searches

## Use Cases

### Analysis Use Cases

- **Theme Extraction**: "What are the main themes in these search results?"
- **Pattern Recognition**: "Identify common patterns or anti-patterns in the code snippets found"
- **Trend Analysis**: "How has the discussion of this topic evolved over time?"
- **Issue Correlation**: "Are there related issues or topics in these conversations?"

### Transformation Use Cases

- **Format Conversion**: "Convert these search results to a structured JSON/CSV format"
- **Summarization**: "Create an executive summary of these findings"
- **Report Generation**: "Generate a markdown report with sections for each conversation"
- **Data Extraction**: "Extract all mentioned file paths, function names, or error codes"

### Enhancement Use Cases

- **Context Enhancement**: "For each match, explain what problem was being solved"
- **Cross-referencing**: "Use SearchFiles to find related code or documentation"
- **Gap Analysis**: "What information is missing or would be helpful to find?"
- **Action Items**: "Extract action items or TODOs from these conversations"

## See Also

- [Layer 7: Output Persistence](cycodj-search-layer-7.md) - Where AI-processed output is saved
- [Layer 6: Display Control](cycodj-search-layer-6.md) - What AI receives as input
- [Layer 5: Context Expansion](cycodj-search-layer-5.md) - Context lines included in AI input
- [Layer 8 Proof](cycodj-search-layer-8-proof.md) - Detailed source code evidence
