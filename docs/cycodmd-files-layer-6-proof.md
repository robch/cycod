# cycodmd Files Command - Layer 6 Proof: Display Control

## Overview

This document provides **source code evidence** for all Display Control (Layer 6) functionality in the cycodmd Files command. Each section includes file paths, line numbers, and explanations of how the code implements display features.

---

## Command Line Parsing

### File: `src/cycodmd/CommandLine/CycoDmdCommandLineOptions.cs`

#### Line Numbers Parsing

**Lines 161-164**:
```csharp
else if (arg == "--line-numbers")
{
    command.IncludeLineNumbers = true;
}
```

**Explanation**:
- Checks if argument is `"--line-numbers"`
- Sets `FindFilesCommand.IncludeLineNumbers` property to `true`
- No argument value required (boolean flag)
- Default value is `false` (set in constructor)

**Data Flow**:
1. User specifies `--line-numbers` on command line
2. Parser recognizes flag in `TryParseFindFilesCommandOptions`
3. Sets property on `FindFilesCommand` instance
4. Property read during execution in `HandleFindFileCommand`
5. Passed to `GetCheckSaveFileContent` for formatting

---

#### Highlight Matches Parsing

**Lines 165-168**:
```csharp
else if (arg == "--highlight-matches")
{
    command.HighlightMatches = true;
}
```

**Explanation**:
- Checks if argument is `"--highlight-matches"`
- Sets `FindFilesCommand.HighlightMatches` property to `true`
- Explicitly enables highlighting, overriding auto-detection
- Property type is `bool?` (nullable) to support tri-state logic

**Tri-State Values**:
- `true`: Explicitly enabled by `--highlight-matches`
- `false`: Explicitly disabled by `--no-highlight-matches`
- `null`: Auto-decide (default from constructor)

---

#### No-Highlight Matches Parsing

**Lines 169-172**:
```csharp
else if (arg == "--no-highlight-matches")
{
    command.HighlightMatches = false;
}
```

**Explanation**:
- Checks if argument is `"--no-highlight-matches"`
- Sets `FindFilesCommand.HighlightMatches` property to `false`
- Explicitly disables highlighting, even if auto-enable conditions met
- Overrides auto-detection logic

**Use Case**:
When user wants context and line numbers but NOT highlighting:
```bash
cycodmd "**/*.cs" --line-contains "async" --line-numbers --lines 3 --no-highlight-matches
```

---

#### Files-Only Parsing

**Lines 173-176**:
```csharp
else if (arg == "--files-only")
{
    command.FilesOnly = true;
}
```

**Explanation**:
- Checks if argument is `"--files-only"`
- Sets `FindFilesCommand.FilesOnly` property to `true`
- Enables special mode: output file paths only, no content
- Short-circuits most processing

---

## Command Properties

### File: `src/cycodmd/CommandLineCommands/FindFilesCommand.cs`

#### Property Declarations

**Lines 99-103**:
```csharp
public int IncludeLineCountBefore;
public int IncludeLineCountAfter;
public bool IncludeLineNumbers;
public bool? HighlightMatches;
public bool FilesOnly;
```

**Explanation**:
- `IncludeLineCountBefore`: Context lines before match (from Layer 5)
- `IncludeLineCountAfter`: Context lines after match (from Layer 5)
- `IncludeLineNumbers`: Display line numbers (Layer 6)
- `HighlightMatches`: Highlight matched lines (Layer 6, nullable tri-state)
- `FilesOnly`: Suppress content, show only paths (Layer 6)

**Type Significance**:
- `bool?` for `HighlightMatches` enables tri-state logic:
  - Distinguish between "not set" (null) and "explicitly disabled" (false)
  - Allows auto-enable heuristic when null
  - Respects explicit user choice when non-null

---

#### Constructor Initialization

**Lines 19-23**:
```csharp
IncludeLineCountBefore = 0;
IncludeLineCountAfter = 0;
IncludeLineNumbers = false;
HighlightMatches = null;
FilesOnly = false;
```

**Explanation**:
- **Default values** for display control:
  - No line numbers
  - Auto-decide highlighting (null)
  - Full content display (not files-only)
- `HighlightMatches = null` critical for auto-detection

---

#### IsEmpty Check

**Lines 54-58**:
```csharp
IncludeLineCountBefore == 0 &&
IncludeLineCountAfter == 0 &&
IncludeLineNumbers == false &&
HighlightMatches == null &&
FilesOnly == false &&
```

**Explanation**:
- Checks if command has any display control settings
- Part of overall emptiness check for command validation
- Display settings are optional (have sensible defaults)

---

## Execution Flow

### File: `src/cycodmd/Program.cs`

#### Main Handler Entry Point

**Line 104**:
```csharp
FindFilesCommand findFilesCommand => await HandleFindFileCommand(commandLineOptions, findFilesCommand, processor, delayOutputToApplyInstructions),
```

**Explanation**:
- Pattern match in main command dispatcher
- Routes `FindFilesCommand` instances to handler method
- Passes command with all property values set by parser

---

#### Files-Only Mode Shortcut

**Lines 194-206**:
```csharp
// If FilesOnly mode, return just the file paths
if (findFilesCommand.FilesOnly)
{
    var fileListOutput = string.Join(Environment.NewLine, files);
    var task = Task.FromResult(fileListOutput);
    
    if (!delayOutputToApplyInstructions)
    {
        ConsoleHelpers.WriteLineIfNotEmpty(fileListOutput);
    }
    
    return new List<Task<string>> { task };
}
```

**Explanation**:
1. **Check**: If `FilesOnly` is `true`
2. **Join**: Combine file paths with newlines
3. **Wrap**: Create completed task with result
4. **Output**: Print immediately if not delaying
5. **Return**: Short-circuit, skip all content processing

**Performance Impact**:
- Avoids reading file contents
- Avoids line-level filtering
- Avoids markdown formatting
- Significantly faster for large file sets

**Output Format**:
```
path/to/file1.cs
path/to/file2.cs
path/to/file3.cs
```

---

#### Auto-Highlight Logic

**Lines 219-224**:
```csharp
// Derive actual highlight matches value from tri-state:
// - If explicitly set (true/false), use that value
// - If null (not specified), auto-enable when we have line numbers AND context lines
var actualHighlightMatches = findFilesCommand.HighlightMatches ?? 
    (findFilesCommand.IncludeLineNumbers && 
     (findFilesCommand.IncludeLineCountBefore > 0 || findFilesCommand.IncludeLineCountAfter > 0));
```

**Explanation**:
1. **Null coalescing**: `findFilesCommand.HighlightMatches ?? <auto-logic>`
2. **If not null**: Use explicit user-specified value (`true` or `false`)
3. **If null (auto-detect)**: Enable if ALL conditions met:
   - `IncludeLineNumbers` is `true` (user wants line numbers), AND
   - Context is shown (either `IncludeLineCountBefore > 0` OR `IncludeLineCountAfter > 0`)

**Rationale**:
- **Line numbers** indicate user wants precise line references
- **Context lines** indicate user is comparing matched vs. context lines
- **Highlighting** helps visually distinguish which lines matched

**Truth Table**:

| HighlightMatches | LineNumbers | Context > 0 | Result | Reason |
|------------------|-------------|-------------|--------|--------|
| `true` | any | any | `true` | Explicit enable |
| `false` | any | any | `false` | Explicit disable |
| `null` | `true` | `true` | `true` | Auto-enable |
| `null` | `true` | `false` | `false` | No context to highlight |
| `null` | `false` | `true` | `false` | No line numbers reference |
| `null` | `false` | `false` | `false` | Neither condition met |

---

#### Markdown Wrapping Decision

**Lines 229-231**:
```csharp
var onlyOneFile = files.Count == 1 && commandLineOptions.Commands.Count == 1;
var skipMarkdownWrapping = onlyOneFile && FileConverters.CanConvert(file);
var wrapInMarkdown = !skipMarkdownWrapping;
```

**Explanation**:
1. **Check**: Is exactly one file and one command?
2. **Check**: Can `FileConverters` convert the file? (images, PDFs, etc.)
3. **Decide**: Skip markdown wrapping if both conditions true
4. **Result**: `wrapInMarkdown` flag passed to content processor

**Logic Table**:

| File Count | Command Count | CanConvert | wrapInMarkdown | Reason |
|------------|---------------|------------|----------------|--------|
| 1 | 1 | `true` | `false` | Single convertible file (e.g., image) |
| 1 | 1 | `false` | `true` | Single non-convertible file |
| >1 | 1 | any | `true` | Multiple files need headers |
| 1 | >1 | any | `true` | Multiple commands need separation |

**FileConverters Examples**:
- Images: `.png`, `.jpg`, `.gif`
- Documents: `.pdf`
- These get converted to markdown representations, already formatted

---

#### Content Processing Call

**Lines 233-246**:
```csharp
return await GetCheckSaveFileContentAsync(
    file,
    wrapInMarkdown,
    findFilesCommand.IncludeLineContainsPatternList,
    findFilesCommand.IncludeLineCountBefore,
    findFilesCommand.IncludeLineCountAfter,
    findFilesCommand.IncludeLineNumbers,
    findFilesCommand.RemoveAllLineContainsPatternList,
    actualHighlightMatches,
    findFilesCommand.FileInstructionsList,
    findFilesCommand.UseBuiltInFunctions,
    findFilesCommand.SaveChatHistory,
    findFilesCommand.SaveFileOutput);
```

**Explanation**:
- **Parameters passed**:
  - `wrapInMarkdown`: Markdown wrapping decision (computed)
  - `IncludeLineNumbers`: Display line numbers flag
  - `actualHighlightMatches`: Resolved highlighting flag (explicit or auto-detected)
  - Other params: From previous layers (filtering, context, etc.)

**Display Control Parameters**:
- Line 239: `IncludeLineNumbers` (Layer 6)
- Line 241: `actualHighlightMatches` (Layer 6, resolved)
- Line 235: `wrapInMarkdown` (Layer 6, computed)

---

#### Task Execution Wrapper

**Lines 472-488**:
```csharp
private static Task<string> GetCheckSaveFileContentAsync(string fileName, bool wrapInMarkdown, List<Regex> includeLineContainsPatternList, int includeLineCountBefore, int includeLineCountAfter, bool includeLineNumbers, List<Regex> removeAllLineContainsPatternList, bool highlightMatches, List<Tuple<string, string>> fileInstructionsList, bool useBuiltInFunctions, string? saveChatHistory, string? saveFileOutput)
{
    return Task.Run(() => 
        GetCheckSaveFileContent(
            fileName,
            wrapInMarkdown,
            includeLineContainsPatternList,
            includeLineCountBefore,
            includeLineCountAfter,
            includeLineNumbers,
            removeAllLineContainsPatternList,
            highlightMatches,
            fileInstructionsList,
            useBuiltInFunctions,
            saveChatHistory,
            saveFileOutput));
}
```

**Explanation**:
- Async wrapper around synchronous content processing
- Runs on thread pool for parallelism
- Forwards all display parameters unchanged

---

#### Content Processing Implementation

**Lines 490-532**:
```csharp
private static string GetCheckSaveFileContent(string fileName, bool wrapInMarkdown, List<Regex> includeLineContainsPatternList, int includeLineCountBefore, int includeLineCountAfter, bool includeLineNumbers, List<Regex> removeAllLineContainsPatternList, bool highlightMatches, List<Tuple<string, string>> fileInstructionsList, bool useBuiltInFunctions, string? saveChatHistory, string? saveFileOutput)
{
    try
    {
        ConsoleHelpers.DisplayStatus($"Processing: {fileName} ...");
        
        if (includeLineContainsPatternList.Count > 0)
        {
            Logger.Info($"Using {includeLineContainsPatternList.Count} include regex patterns on '{fileName}':");
            foreach (var pattern in includeLineContainsPatternList)
            {
                Logger.Info($"  Include pattern: '{pattern}'");
            }
        }
        
        if (removeAllLineContainsPatternList.Count > 0)
        {
            Logger.Info($"Using {removeAllLineContainsPatternList.Count} exclude regex patterns on '{fileName}':");
            foreach (var pattern in removeAllLineContainsPatternList)
            {
                Logger.Info($"  Exclude pattern: '{pattern}'");
            }
        }

        var finalContent = GetFinalFileContent(
            fileName,
            wrapInMarkdown,
            includeLineContainsPatternList,
            includeLineCountBefore,
            includeLineCountAfter,
            includeLineNumbers,
            removeAllLineContainsPatternList,
            highlightMatches,
            fileInstructionsList,
            useBuiltInFunctions,
            saveChatHistory);

        if (!string.IsNullOrEmpty(saveFileOutput))
        {
            var saveFileName = FileHelpers.GetFileNameFromTemplate(fileName, saveFileOutput)!;
            FileHelpers.WriteAllText(saveFileName, finalContent);
            ConsoleHelpers.DisplayStatus($"Saving to: {saveFileName} ... Done!");
        }

        return finalContent;
    }
    catch (Exception ex)
    {
        Logger.Error($"Error processing file '{fileName}': {ex.Message}");
        return $"## Error processing {fileName}\n\n{ex.Message}\n\n{ex.StackTrace}";
    }
}
```

**Explanation**:
1. **Status Display**: Show progress to user
2. **Debug Logging**: Log filtering patterns (for troubleshooting)
3. **Content Processing**: Call `GetFinalFileContent` with all display parameters
4. **File Output**: Optionally save formatted content (Layer 7)
5. **Return**: Formatted content string for console/further processing
6. **Error Handling**: Catch and format errors as markdown

**Display Parameters Forwarded**:
- Line 520: `includeLineNumbers`
- Line 522: `highlightMatches`
- Line 516: `wrapInMarkdown`

**Note**: Actual line number formatting and highlighting implementation is in `GetFinalFileContent` (not shown here but invoked with these parameters).

---

#### Console Output

**Lines 254-260**:
```csharp
// Handle output based on whether we're delaying for final instructions
if (!delayOutputToApplyInstructions)
{
    var outputTasks = tasks.Select(task => task.ContinueWith(t =>
    {
        ConsoleHelpers.WriteLineIfNotEmpty(t.Result);
        return t.Result;
    })).ToList();
    
    return outputTasks;
}
```

**Explanation**:
1. **Check**: If not delaying for AI instructions
2. **Attach continuation**: For each task, add console output step
3. **Output**: Call `ConsoleHelpers.WriteLineIfNotEmpty` with formatted content
4. **Return**: Tasks now include console output side effect

**Console Output Function**:
- `ConsoleHelpers.WriteLineIfNotEmpty(string)`: Writes to console if content is non-empty
- Respects global `--quiet` flag (suppresses output if set)

---

## Data Flow Summary

### Parsing Phase

```
Command Line: cycodmd "**/*.cs" --line-numbers --highlight-matches
                                      ↓
CycoDmdCommandLineOptions.TryParseFindFilesCommandOptions()
    Lines 161-164: Parse --line-numbers → FindFilesCommand.IncludeLineNumbers = true
    Lines 165-168: Parse --highlight-matches → FindFilesCommand.HighlightMatches = true
                                      ↓
FindFilesCommand object created with display properties set
```

### Execution Phase

```
Program.Main()
    Line 104: Dispatch to HandleFindFileCommand()
                                      ↓
HandleFindFileCommand()
    Line 195: Check FilesOnly (shortcut if true)
    Lines 219-224: Resolve actualHighlightMatches (explicit or auto-detect)
    Lines 229-231: Decide wrapInMarkdown
    Lines 233-246: Call GetCheckSaveFileContentAsync()
                                      ↓
GetCheckSaveFileContentAsync()
    Lines 472-488: Wrap in Task.Run() for parallelism
                                      ↓
GetCheckSaveFileContent()
    Line 494: Display status
    Lines 514-525: Call GetFinalFileContent() with display parameters
                                      ↓
GetFinalFileContent() [Not shown - in FileHelpers or similar]
    - Apply includeLineNumbers formatting
    - Apply highlightMatches formatting
    - Apply wrapInMarkdown formatting
    - Return formatted string
                                      ↓
Back to GetCheckSaveFileContent()
    Lines 527-531: Optionally save to file (Layer 7)
    Line 534: Return formatted content
                                      ↓
Back to HandleFindFileCommand()
    Line 258: ConsoleHelpers.WriteLineIfNotEmpty() - output to console
```

---

## Related Components

### ConsoleHelpers (Output)

**Functions Used**:
- `WriteLineIfNotEmpty(string)`: Output formatted content
- `DisplayStatus(string)`: Show progress messages
- `DisplayStatusErase()`: Clear progress messages

**Configuration**:
- Respects `--quiet`, `--verbose`, `--debug` flags
- Set via `ConsoleHelpers.Configure(debug, verbose, quiet)` in `Program.Main()` (line 45)

### FileHelpers (Content Formatting)

**Functions Used**:
- `GetFinalFileContent(...)`: Apply display formatting to file content
  - Parameters include: `includeLineNumbers`, `highlightMatches`, `wrapInMarkdown`
  - Implementation details not shown here (would be in FileHelpers or similar)

### FileConverters (Format Detection)

**Functions Used**:
- `CanConvert(string fileName)`: Check if file can be converted (images, PDFs, etc.)
- Affects markdown wrapping decision (line 230)

---

## Testing Scenarios

### Scenario 1: Default Display (No Options)
```bash
cycodmd "**/*.cs" --line-contains "async"
```

**Expected State**:
- `IncludeLineNumbers`: `false`
- `HighlightMatches`: `null` → Auto-detect → `false` (no line numbers)
- `FilesOnly`: `false`
- `wrapInMarkdown`: `true` (assuming multiple files)

**Output**: Markdown-wrapped content, no line numbers, no highlighting.

### Scenario 2: Line Numbers Only
```bash
cycodmd "**/*.cs" --line-contains "async" --line-numbers
```

**Expected State**:
- `IncludeLineNumbers`: `true`
- `HighlightMatches`: `null` → Auto-detect → `false` (no context expansion)
- `FilesOnly`: `false`

**Output**: Markdown-wrapped content WITH line numbers, no highlighting (no context to highlight).

### Scenario 3: Line Numbers + Context (Auto-Highlight)
```bash
cycodmd "**/*.cs" --line-contains "async" --line-numbers --lines 3
```

**Expected State**:
- `IncludeLineNumbers`: `true`
- `IncludeLineCountBefore`: `3`
- `IncludeLineCountAfter`: `3`
- `HighlightMatches`: `null` → Auto-detect → `true` (line numbers + context)

**Output**: Markdown-wrapped content WITH line numbers AND highlighting (auto-enabled).

### Scenario 4: Explicit No-Highlighting
```bash
cycodmd "**/*.cs" --line-contains "async" --line-numbers --lines 3 --no-highlight-matches
```

**Expected State**:
- `IncludeLineNumbers`: `true`
- `IncludeLineCountBefore`: `3`
- `IncludeLineCountAfter`: `3`
- `HighlightMatches`: `false` (explicit)
- `actualHighlightMatches`: `false` (explicit overrides auto-detect)

**Output**: Markdown-wrapped content WITH line numbers, NO highlighting (explicit disable).

### Scenario 5: Files-Only Mode
```bash
cycodmd "**/*.cs" --file-contains "async" --files-only
```

**Expected State**:
- `FilesOnly`: `true`
- Shortcut at line 195 (skip all content processing)

**Output**: Plain text list of file paths, no content, no markdown.

---

## Summary

### Key Implementation Points

1. **Display properties** stored in `FindFilesCommand` class (lines 101-103)
2. **Parsing logic** in `CycoDmdCommandLineOptions.TryParseFindFilesCommandOptions` (lines 161-176)
3. **Auto-highlight logic** in `Program.HandleFindFileCommand` (lines 219-224)
4. **Markdown wrapping decision** in `Program.HandleFindFileCommand` (lines 229-231)
5. **Files-only shortcut** in `Program.HandleFindFileCommand` (lines 194-206)
6. **Content processing** in `Program.GetCheckSaveFileContent` (lines 490-532)
7. **Console output** in `Program.HandleFindFileCommand` (lines 254-260)

### Cross-References

- **Layer 5 (Context Expansion)**: Provides `IncludeLineCountBefore/After` values used for auto-highlight decision
- **Layer 7 (Output Persistence)**: Receives formatted content for file saving
- **FileHelpers**: Implements actual formatting (`GetFinalFileContent`)
- **ConsoleHelpers**: Handles console output and progress display
- **FileConverters**: Determines if files need markdown wrapping

---

## Verification

To verify this layer is working correctly:

1. **Line Numbers**: Run with `--line-numbers`, check output has line prefixes
2. **Highlighting**: Run with context + line numbers, verify matched lines are highlighted
3. **No-Highlighting**: Run with `--no-highlight-matches`, verify NO highlighting despite auto-enable conditions
4. **Files-Only**: Run with `--files-only`, verify only file paths appear (no content)
5. **Markdown Wrapping**: Run single file vs. multiple files, verify wrapping behavior differs
