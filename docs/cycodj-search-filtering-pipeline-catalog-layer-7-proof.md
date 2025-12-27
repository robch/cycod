# cycodj search - Layer 7: Output Persistence - PROOF

[← Back to Layer 7 Catalog](cycodj-search-filtering-pipeline-catalog-layer-7.md)

## Implementation Identity

**IMPORTANT**: The `search` command uses the EXACT SAME implementation for Layer 7 as all other cycodj commands. The implementation is provided by the `CycoDjCommand` base class.

For complete implementation details of the shared components, see: [cycodj-list-filtering-pipeline-catalog-layer-7-proof.md](cycodj-list-filtering-pipeline-catalog-layer-7-proof.md)

---

## Search-Specific Implementation

### Execution Flow

**Location**: `src/cycodj/CommandLineCommands/SearchCommand.cs`

**Lines 23-40**: Standard execution pattern:

```csharp
23:         public override async System.Threading.Tasks.Task<int> ExecuteAsync()
24:         {
25:             var output = GenerateSearchOutput();
26:             
27:             // Apply instructions if provided
28:             var finalOutput = ApplyInstructionsIfProvided(output);
29:             
30:             // Save to file if --save-output was provided
31:             if (SaveOutputIfRequested(finalOutput))
32:             {
33:                 return await System.Threading.Tasks.Task.FromResult(0);
34:             }
35:             
36:             // Otherwise print to console
37:             ConsoleHelpers.WriteLine(finalOutput);
38:             
39:             return await System.Threading.Tasks.Task.FromResult(0);
40:         }
```

---

## Output Generation

**Location**: `src/cycodj/CommandLineCommands/SearchCommand.cs`

**Lines 42-186**: The `GenerateSearchOutput` method creates search-specific output:

```csharp
42:         private string GenerateSearchOutput()
43:         {
44:             var sb = new System.Text.StringBuilder();
45:             
46:             if (string.IsNullOrWhiteSpace(Query))
47:             {
48:                 sb.AppendLine("ERROR: Search query is required.");
49:                 return sb.ToString();
50:             }
51: 
52:             sb.AppendLine($"## Searching conversations for: \"{Query}\"");
53:             sb.AppendLine();
```

**Search Results Structure**:

1. **Header** (lines 52-53): Search query display
2. **Filtering Info** (lines 59-94): Time range and date filters
3. **Search Execution** (lines 112-132): Find matches in conversations
4. **Results Display** (lines 135-147): List of matching conversations
5. **Match Details** (lines 144-147): Detailed match information per conversation
6. **Summary** (lines 150): Total match count
7. **Statistics** (lines 154-183): Optional statistics section

**Key Output Features**:

**Lines 264-298** - Context display around matches:
```csharp
264:         private void AppendConversationMatches(System.Text.StringBuilder sb, Models.Conversation conversation, List<SearchMatch> matches)
265:         {
266:             var title = conversation.Metadata?.Title ?? $"conversation-{conversation.Id}";
267:             var timestamp = conversation.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
268: 
269:             sb.AppendLine($"### {timestamp} - {title}");
270:             sb.AppendLine($"    File: {conversation.FilePath}");
271:             sb.AppendLine($"    Matches: {matches.Count}");
272:             sb.AppendLine();
273: 
274:             foreach (var match in matches)
275:             {
276:                 var role = match.Message.Role;
277:                 sb.AppendLine($"  [{role}] Message #{match.MessageIndex + 1}");
278: 
279:                 // Show matched lines with context
280:                 var allLines = match.Message.Content.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
281:                 var matchedLineNumbers = match.MatchedLines.Select(m => m.lineNumber).Distinct().ToHashSet();
282: 
283:                 for (int i = 0; i < allLines.Length; i++)
284:                 {
285:                     var isMatch = matchedLineNumbers.Contains(i);
286:                     var showContext = matchedLineNumbers.Any(ln => Math.Abs(ln - i) <= ContextLines);
287: 
288:                     if (showContext || isMatch)
289:                     {
290:                         var prefix = isMatch ? "  > " : "    ";
291:                         var line = allLines[i];
292:                         sb.AppendLine(prefix + line);
293:                     }
294:                 }
295: 
296:                 sb.AppendLine();
297:             }
298:         }
```

**Analysis**:
- Line 286: Uses `ContextLines` property (from `--context` option) to determine how many lines before/after to show
- Lines 288-293: Shows matched lines with `>` prefix, context lines with plain prefix
- This context is part of the output that gets saved when `--save-output` is used

---

## Search-Specific Display Options

**Location**: `src/cycodj/CommandLineCommands/SearchCommand.cs`

**Lines 10-20**: Properties that affect output:

```csharp
10:         public string? Query { get; set; }
11:         public string? Date { get; set; }
12:         public int? Last { get; set; }
13:         public bool CaseSensitive { get; set; }
14:         public bool UseRegex { get; set; }
15:         public bool UserOnly { get; set; }
16:         public bool AssistantOnly { get; set; }
17:         public int ContextLines { get; set; } = 2;
18:         public bool ShowBranches { get; set; } = false;
19:         public int? MessageCount { get; set; } = null; // null = use default (3)
20:         public bool ShowStats { get; set; } = false;
```

**How They Affect Saved Output**:

1. **Query, CaseSensitive, UseRegex** - Determine what matches are found
2. **UserOnly, AssistantOnly** - Filter which messages are searched
3. **ContextLines** - Controls how many lines around matches are included in output (line 286)
4. **ShowStats** - Adds statistics section to output (lines 154-183)
5. **ShowBranches, MessageCount** - Display options (though less relevant for search)

All these properties affect what goes into the output string before it's saved.

---

## Call Stack Summary

```
1. Command Line Parsing
   └─ CycoDjCommandLineOptions.Parse(args)
      └─ TryParseSearchCommandOptions() + TryParseDisplayOptions()
         ├─ Parses --save-output [lines 171-180]
         ├─ Parses --context [lines 319-326]
         ├─ Parses --user-only, --assistant-only [lines 289-299]
         └─ Parses --regex, --case-sensitive [lines 283-287]

2. Command Execution
   └─ SearchCommand.ExecuteAsync() [lines 23-40]
      ├─ Generates output: GenerateSearchOutput() [line 25]
      │  ├─ Searches conversations [lines 112-132]
      │  ├─ Applies content filters (UserOnly, AssistantOnly, regex, etc.)
      │  ├─ Formats matches with context [lines 264-298]
      │  │  └─ Uses ContextLines property [line 286]
      │  └─ Adds statistics if requested [lines 154-183]
      │
      ├─ Applies AI processing: ApplyInstructionsIfProvided() [line 28]
      └─ Saves if requested: SaveOutputIfRequested(finalOutput) [line 31]
         └─ Inherited save logic [CycoDjCommand lines 58-75]
      
      If saved: Returns immediately [line 33]
      Otherwise: Prints to console [line 37]
```

---

## Key Differences from Other Commands

| Feature | List/Show/Branches/Stats | Search |
|---------|-------------------------|--------|
| **Output Type** | Listing/details | Search results with matches |
| **Context Display** | N/A | Uses `ContextLines` property (line 286) |
| **Content Filtering** | Limited | Rich (UserOnly, AssistantOnly, regex, case-sensitive) |
| **Layer 7 Implementation** | Identical | Identical |

The search command has unique **input** (query, filters) and unique **output format** (matches with context), but the **save mechanism** (Layer 7) is identical to all other commands.

---

## Summary

The `search` command implements Layer 7 (Output Persistence) through:

1. **Shared option parsing** at lines 171-180 in `CycoDjCommandLineOptions.cs`
2. **Shared property** at line 17 in `CycoDjCommand.cs`
3. **Shared save logic** at lines 58-75 in `CycoDjCommand.cs`
4. **Standard execution flow** at lines 23-40 in `SearchCommand.cs`
5. **Search-specific output generation** at lines 42-298 in `SearchCommand.cs`
   - Includes unique match formatting with context expansion (lines 264-298)

The Layer 7 implementation is architecturally identical to all other cycodj commands. The uniqueness is in the search results format and context display, not in how output is saved.
