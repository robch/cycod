# cycodj search Command - Layer 5: Context Expansion - PROOF

This document provides source code evidence for the Layer 5 (Context Expansion) implementation in the `search` command.

## Command Line Option Parsing

### Location: CycoDjCommandLineOptions.cs

**File**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

**Method**: `TryParseSearchCommandOptions`

**Lines 469-478**: Context expansion option parsing

```csharp
469:         else if (arg == "--context" || arg == "-C")
470:         {
471:             var lines = i + 1 < args.Length ? args[++i] : null;
472:             if (string.IsNullOrWhiteSpace(lines) || !int.TryParse(lines, out var n))
473:             {
474:                 throw new CommandLineException($"Missing or invalid context lines for {arg}");
475:             }
476:             command.ContextLines = n;
477:             return true;
478:         }
```

**Evidence**:
- Line 469: Recognizes both `--context` and `-C` short form
- Line 471: Reads next argument as the number of context lines
- Line 472: Validates that argument is a parseable integer
- Line 474: Throws specific error if validation fails
- Line 476: Sets `ContextLines` property on `SearchCommand` object
- Line 477: Returns `true` to indicate option was successfully parsed

### Location: SearchCommand.cs

**File**: `src/cycodj/CommandLineCommands/SearchCommand.cs`

**Lines 14-17**: Property declaration and default value

```csharp
14:         public bool UseRegex { get; set; }
15:         public bool UserOnly { get; set; }
16:         public bool AssistantOnly { get; set; }
17:         public int ContextLines { get; set; } = 2;
```

**Evidence**:
- Line 17: `ContextLines` property with default value of `2`
- This is the property set by the command line parser (line 476 above)

## Context Expansion Implementation

### Search Text Method

**File**: `src/cycodj/CommandLineCommands/SearchCommand.cs`

**Method**: `SearchText`

**Lines 222-262**: Identifies matching lines

```csharp
222:         private List<(int lineNumber, string line, int matchStart, int matchLength)> SearchText(string text)
223:         {
224:             var matches = new List<(int lineNumber, string line, int matchStart, int matchLength)>();
225:             var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
226: 
227:             for (int lineNum = 0; lineNum < lines.Length; lineNum++)
228:             {
229:                 var line = lines[lineNum];
230:                 if (string.IsNullOrEmpty(line)) continue;
231: 
232:                 if (UseRegex)
233:                 {
234:                     try
235:                     {
236:                         var regexOptions = CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
237:                         var regex = new Regex(Query!, regexOptions);
238:                         var match = regex.Match(line);
239:                         if (match.Success)
240:                         {
241:                             matches.Add((lineNum, line, match.Index, match.Length));
242:                         }
243:                     }
244:                     catch (Exception ex)
245:                     {
246:                         ConsoleHelpers.WriteErrorLine($"Invalid regex pattern: {ex.Message}");
247:                         return matches;
248:                     }
249:                 }
250:                 else
251:                 {
252:                     var comparison = CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
253:                     var index = line.IndexOf(Query!, comparison);
254:                     if (index >= 0)
255:                     {
256:                         matches.Add((lineNum, line, index, Query!.Length));
257:                     }
258:                 }
259:             }
260: 
261:             return matches;
262:         }
```

**Evidence**:
- Line 222: Returns tuple including `lineNumber` which is critical for context expansion
- Line 225: Splits message text into individual lines
- Line 227-230: Iterates through all lines
- Lines 232-258: Performs matching (regex or plain text)
- Lines 241, 256: Records `lineNum` for each match - this enables context calculation
- Line 261: Returns all matched lines with their line numbers

### Display with Context Expansion

**File**: `src/cycodj/CommandLineCommands/SearchCommand.cs`

**Method**: `AppendConversationMatches`

**Lines 264-298**: Applies context expansion when displaying matches

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

**Evidence**:
- Line 280: Re-splits message content into lines for display
- Line 281: Builds a `HashSet` of matched line numbers for quick lookup
- Line 283: Iterates through ALL lines in the message
- Line 285: Determines if current line is a match
- **Line 286: THE CRITICAL LINE** - Context expansion logic:
  ```csharp
  var showContext = matchedLineNumbers.Any(ln => Math.Abs(ln - i) <= ContextLines);
  ```
  - Uses `Math.Abs(ln - i)` to calculate distance from current line `i` to each matched line `ln`
  - Checks if distance is `<= ContextLines`
  - If ANY matched line is within `ContextLines` distance, show this line
  - This creates symmetric before/after expansion
- Line 288: Shows line if it's a match OR within context distance
- Line 290: Uses different prefix for matched lines (`> `) vs context lines (` `)
- Line 292: Appends the line with appropriate prefix

## Data Flow Trace

### 1. User Input
```bash
cycodj search "error" --context 5
```

### 2. Parsing (CycoDjCommandLineOptions.cs, lines 469-478)
- `arg` = `"--context"`
- `lines` = `"5"`
- `n` = `5` (parsed integer)
- `command.ContextLines` = `5`

### 3. Search Execution (SearchCommand.cs, line 188-220)
- `SearchConversation` method finds matching messages
- `SearchText` method (lines 222-262) identifies matched line numbers
- Returns matches with line numbers: e.g., `[(5, "Found error", 6, 5)]`

### 4. Context Expansion (SearchCommand.cs, lines 264-298)
- Iterates through all lines in message (line 283)
- For each line, calculates: `Math.Abs(ln - i) <= 5`
- Example: If match is on line 10:
  - Line 5: `|10 - 5| = 5` ≤ 5 → **SHOW** (5 before)
  - Line 4: `|10 - 4| = 6` > 5 → hide
  - Line 10: `isMatch = true` → **SHOW** (the match)
  - Line 15: `|10 - 15| = 5` ≤ 5 → **SHOW** (5 after)
  - Line 16: `|10 - 16| = 6` > 5 → hide

### 5. Output
```
  [user] Message #3
      Line 5 (context)
      Line 6 (context)
      ...
  >   Line 10 with error (MATCH)
      ...
      Line 14 (context)
      Line 15 (context)
```

## SearchMatch Helper Class

**File**: `src/cycodj/CommandLineCommands/SearchCommand.cs`

**Lines 300-305**: Data structure for matched messages

```csharp
300:         private class SearchMatch
301:         {
302:             public int MessageIndex { get; set; }
303:             public Models.ChatMessage Message { get; set; } = null!;
304:             public List<(int lineNumber, string line, int matchStart, int matchLength)> MatchedLines { get; set; } = new();
305:         }
```

**Evidence**:
- Line 302: Stores which message in the conversation matched
- Line 303: Reference to the actual message object
- Line 304: List of matched lines WITH line numbers (critical for context expansion)
- The tuple includes `lineNumber` which is used by line 286 for distance calculation

## Mathematical Proof of Symmetry

The context expansion formula:
```csharp
Math.Abs(ln - i) <= ContextLines
```

Is symmetric because:
- `|ln - i|` = `|i - ln|` (absolute value is commutative)
- If match is at line M and context is N:
  - Line M-N: `|M - (M-N)| = N` ≤ N → SHOW
  - Line M+N: `|M - (M+N)| = N` ≤ N → SHOW
  - Lines M-N-1 and M+N+1: distance = N+1 > N → HIDE

Therefore, exactly N lines are shown before AND after each match.

## Edge Cases Handled

### Multiple Matches in Same Message

**File**: `src/cycodj/CommandLineCommands/SearchCommand.cs`, line 286

```csharp
286:                     var showContext = matchedLineNumbers.Any(ln => Math.Abs(ln - i) <= ContextLines);
```

**Evidence**:
- Uses `.Any()` to check distance from ALL matched line numbers
- If line is within context distance of ANY match, it's shown
- This handles overlapping contexts gracefully
- Example: Matches on lines 5 and 10, context=3
  - Lines 2-8 shown (context of line 5)
  - Lines 7-13 shown (context of line 10)
  - Lines 7-8 appear only once (no duplication)

### Match at Start/End of Message

The iteration (line 283) naturally handles boundaries:
```csharp
283:                 for (int i = 0; i < allLines.Length; i++)
```

- If match is at line 0: context before is empty (no negative indices)
- If match is at last line: context after is empty (loop ends naturally)
- No special boundary checking needed due to loop bounds

### Empty Lines

**File**: `src/cycodj/CommandLineCommands/SearchCommand.cs`, lines 229-230

```csharp
229:                 var line = lines[lineNum];
230:                 if (string.IsNullOrEmpty(line)) continue;
```

**Evidence**:
- Empty lines are skipped during search
- But they're still counted in line numbers (line 227 increments regardless)
- This means empty lines CAN appear in context if within range
- Maintains accurate line number tracking

## Related Command Line Options

While these options don't directly implement Layer 5, they affect what gets searched and thus what context is shown:

### Content Filtering (Layer 3)

**Lines 459-467**: Role-based filtering
```csharp
459:         else if (arg == "--user-only" || arg == "-u")
460:         {
461:             command.UserOnly = true;
462:             return true;
463:         }
464:         else if (arg == "--assistant-only" || arg == "-a")
465:         {
466:             command.AssistantOnly = true;
467:             return true;
468:         }
```

**Lines 449-457**: Search options
```csharp
449:         else if (arg == "--case-sensitive" || arg == "-c")
450:         {
451:             command.CaseSensitive = true;
452:             return true;
453:         }
454:         else if (arg == "--regex" || arg == "-r")
455:         {
456:             command.UseRegex = true;
457:             return true;
458:         }
```

These options affect which lines match, which in turn determines where context is expanded.

## Performance Characteristics

### Time Complexity

**Matching Phase** (SearchText, lines 227-259):
- O(L) where L = number of lines in message
- Each line checked once

**Context Expansion Phase** (AppendConversationMatches, lines 283-294):
- O(L × M) where:
  - L = number of lines in message
  - M = number of matched lines
- Line 286 checks distance to ALL matched lines for EACH line
- Could be optimized but M is typically small

### Space Complexity

**SearchMatch.MatchedLines** (line 304):
- O(M) where M = number of matched lines
- Stores tuples with line numbers for context calculation
- Minimal overhead (just integers and string references)

## Verification Methods

To verify this implementation:

1. **Test symmetric expansion**:
   ```bash
   cycodj search "test" --context 3
   ```
   Count lines before and after matches - should be equal (up to message boundaries)

2. **Test with context=0**:
   ```bash
   cycodj search "test" --context 0
   ```
   Should show ONLY matched lines, no surrounding lines

3. **Test overlapping contexts**:
   Create a message with matches close together and verify context areas merge properly

4. **Test edge cases**:
   - Match at first line (no "before" context possible)
   - Match at last line (no "after" context possible)
   - Single-line message (no context possible)

## Conclusion

The Layer 5 (Context Expansion) implementation in the `search` command is:

1. **Well-defined**: Clear command line options with validation
2. **Symmetric**: Uses `Math.Abs` for equal before/after expansion
3. **Efficient**: Simple distance calculation
4. **Robust**: Handles edge cases naturally
5. **Flexible**: User-controllable context amount (including 0 for no context)

The core logic (line 286) is elegant and mathematically sound, providing intuitive and useful context expansion for search results.

## Navigation

- [← Back to Layer 5 Documentation](cycodj-search-filtering-pipeline-catalog-layer-5.md)
- [↑ search Command Overview](cycodj-search-filtering-pipeline-catalog-README.md)
- [↑ cycodj Main Catalog](cycodj-filtering-pipeline-catalog-README.md)
