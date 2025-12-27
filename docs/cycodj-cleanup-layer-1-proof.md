# cycodj cleanup - Layer 1 Proof: SOURCE CODE EVIDENCE

## Overview

This document provides complete source code evidence for Layer 1 (TARGET SELECTION) implementation in the `cleanup` command, with exact line numbers and code excerpts.

---

## Parser Evidence

### Command Registration

**File**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

**Lines 46**: cleanup command registration
```csharp
46:         if (lowerCommandName.StartsWith("cleanup")) return new CleanupCommand();
```

### CleanupCommand Option Parsing

**File**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

**Lines 532-548**: Complete option parsing
```csharp
532:     private bool TryParseCleanupCommandOptions(CleanupCommand command, string[] args, ref int i, string arg)
533:     {
534:         if (arg == "--find-duplicates") { command.FindDuplicates = true; return true; }
535:         else if (arg == "--remove-duplicates") { command.RemoveDuplicates = true; command.FindDuplicates = true; return true; }
536:         else if (arg == "--find-empty") { command.FindEmpty = true; return true; }
537:         else if (arg == "--remove-empty") { command.RemoveEmpty = true; command.FindEmpty = true; return true; }
538:         else if (arg == "--older-than-days") 
539:         { 
540:             var days = i + 1 < args.Length ? args[++i] : null;
541:             if (string.IsNullOrWhiteSpace(days) || !int.TryParse(days, out var n))
542:                 throw new CommandLineException($"Missing or invalid days for {arg}");
543:             command.OlderThanDays = n;
544:             return true;
545:         }
546:         else if (arg == "--execute") { command.DryRun = false; return true; }
547:         return false;
548:     }
```

**Key Patterns**:
- **Line 534**: `--find-duplicates` sets `FindDuplicates = true`
- **Line 535**: `--remove-duplicates` sets BOTH `RemoveDuplicates` AND `FindDuplicates` (implies finding)
- **Line 536**: `--find-empty` sets `FindEmpty = true`
- **Line 537**: `--remove-empty` sets BOTH `RemoveEmpty` AND `FindEmpty` (implies finding)
- **Line 538-544**: `--older-than-days` requires integer argument, sets `OlderThanDays`
- **Line 546**: `--execute` sets `DryRun = false` (default is true)
- **NO calls to** `TryParseTimeOptions()` or `TryParseDisplayOptions()`

---

## Execution Evidence

### CleanupCommand Class Definition

**File**: `src/cycodj/CommandLineCommands/CleanupCommand.cs`

**Lines 9-16**: Properties for target selection
```csharp
9:     public class CleanupCommand : CommandLine.CycoDjCommand
10:     {
11:         public bool FindDuplicates { get; set; }
12:         public bool RemoveDuplicates { get; set; }
13:         public bool FindEmpty { get; set; }
14:         public bool RemoveEmpty { get; set; }
15:         public int? OlderThanDays { get; set; }
16:         public bool DryRun { get; set; } = true;
```

**Key Properties for Layer 1**:
- **Line 11**: `FindDuplicates` - Find duplicate conversations
- **Line 12**: `RemoveDuplicates` - Remove duplicates (implies finding)
- **Line 13**: `FindEmpty` - Find empty conversations
- **Line 14**: `RemoveEmpty` - Remove empty (implies finding)
- **Line 15**: `OlderThanDays` - Age threshold in days (nullable int)
- **Line 16**: `DryRun` - Default TRUE (safe mode)

### Criteria Validation

**Lines 18-29**: ExecuteAsync - Criteria validation
```csharp
18:         public override async Task<int> ExecuteAsync()
19:         {
20:             ConsoleHelpers.WriteLine("## Chat History Cleanup", ConsoleColor.Cyan, overrideQuiet: true);
21:             ConsoleHelpers.WriteLine(overrideQuiet: true);
22: 
23:             if (!FindDuplicates && !FindEmpty && !OlderThanDays.HasValue)
24:             {
25:                 ConsoleHelpers.WriteErrorLine("Please specify at least one cleanup operation:");
26:                 ConsoleHelpers.WriteLine("  --find-duplicates     Find duplicate conversations", overrideQuiet: true);
27:                 ConsoleHelpers.WriteLine("  --find-empty          Find empty conversations", overrideQuiet: true);
28:                 ConsoleHelpers.WriteLine("  --older-than-days N   Find conversations older than N days", overrideQuiet: true);
29:                 return 1;
30:             }
```

**Key Logic**:
- **Line 23**: Requires **at least one** criterion:
  - `FindDuplicates` OR
  - `FindEmpty` OR
  - `OlderThanDays.HasValue`
- **Lines 25-28**: Shows usage error if no criteria

### File Discovery

**Lines 32-36**: Find all files (no filtering)
```csharp
32:             var historyDir = CycoDj.Helpers.HistoryFileHelpers.GetHistoryDirectory();
33:             var files = CycoDj.Helpers.HistoryFileHelpers.FindAllHistoryFiles();
34: 
35:             ConsoleHelpers.WriteLine($"Scanning {files.Count} conversation files...", overrideQuiet: true);
36:             ConsoleHelpers.WriteLine(overrideQuiet: true);
```

**Note**: **NO time filtering** applied. All files are scanned.

### Target Collection

**Lines 38-56**: Collect files to remove
```csharp
38:             var toRemove = new List<string>();
39: 
40:             if (FindDuplicates || RemoveDuplicates)
41:             {
42:                 toRemove.AddRange(await FindDuplicateConversationsAsync(files));
43:             }
44: 
45:             if (FindEmpty || RemoveEmpty)
46:             {
47:                 toRemove.AddRange(FindEmptyConversations(files));
48:             }
49: 
50:             if (OlderThanDays.HasValue)
51:             {
52:                 toRemove.AddRange(FindOldConversations(files, OlderThanDays.Value));
53:             }
54: 
55:             // Remove duplicates from the list
56:             toRemove = toRemove.Distinct().ToList();
```

**Key Logic**:
- **Line 40-43**: If finding/removing duplicates, call `FindDuplicateConversationsAsync()`
- **Line 45-48**: If finding/removing empty, call `FindEmptyConversations()`
- **Line 50-53**: If age threshold set, call `FindOldConversations()`
- **Line 56**: Deduplicate the removal list (file might match multiple criteria)

---

## Duplicate Detection Implementation

### FindDuplicateConversationsAsync Method

**File**: `src/cycodj/CommandLineCommands/CleanupCommand.cs`

**Lines 121-186**: Complete duplicate detection logic
```csharp
121:         private async Task<List<string>> FindDuplicateConversationsAsync(List<string> files)
122:         {
123:             ConsoleHelpers.WriteLine("### Finding Duplicate Conversations", ConsoleColor.White, overrideQuiet: true);
124:             ConsoleHelpers.WriteLine(overrideQuiet: true);
125: 
126:             var duplicates = new List<string>();
127:             var conversationsByContent = new Dictionary<string, List<string>>();
128: 
129:             foreach (var file in files)
130:             {
131:                 try
132:                 {
133:                     var conversation = CycoDj.Helpers.JsonlReader.ReadConversation(file);
134:                     if (conversation == null) continue;
135: 
136:                     // Create a signature based on message content
137:                     var signature = string.Join("|", conversation.Messages
138:                         .Where(m => m.Role == "user" || m.Role == "assistant")
139:                         .Take(10) // First 10 messages
140:                         .Select(m => $"{m.Role}:{m.Content?.Length ?? 0}"));
141: 
142:                     if (!conversationsByContent.ContainsKey(signature))
143:                     {
144:                         conversationsByContent[signature] = new List<string>();
145:                     }
146:                     conversationsByContent[signature].Add(file);
147:                 }
148:                 catch (Exception ex)
149:                 {
150:                     Logger.Warning($"Failed to analyze {file}: {ex.Message}");
151:                 }
152:             }
153: 
154:             var duplicateGroups = conversationsByContent.Where(kv => kv.Value.Count > 1).ToList();
155: 
156:             if (duplicateGroups.Any())
157:             {
158:                 ConsoleHelpers.WriteLine($"Found {duplicateGroups.Count} group(s) of duplicates:", overrideQuiet: true);
159:                 ConsoleHelpers.WriteLine(overrideQuiet: true);
160: 
161:                 foreach (var group in duplicateGroups)
162:                 {
163:                     ConsoleHelpers.WriteLine($"  Duplicate group ({group.Value.Count} files):", ConsoleColor.Yellow, overrideQuiet: true);
164:                     
165:                     // Keep the newest, mark others for removal
166:                     var sorted = group.Value.OrderByDescending(f => CycoDj.Helpers.TimestampHelpers.ParseTimestamp(f)).ToList();
167:                     var keep = sorted.First();
168:                     var remove = sorted.Skip(1).ToList();
169: 
170:                     ConsoleHelpers.WriteLine($"    KEEP: {Path.GetFileName(keep)}", ConsoleColor.Green, overrideQuiet: true);
171:                     foreach (var file in remove)
172:                     {
173:                         ConsoleHelpers.WriteLine($"    remove: {Path.GetFileName(file)}", ConsoleColor.DarkGray, overrideQuiet: true);
174:                         duplicates.Add(file);
175:                     }
176:                     ConsoleHelpers.WriteLine(overrideQuiet: true);
177:                 }
178:             }
179:             else
180:             {
181:                 ConsoleHelpers.WriteLine("  No duplicates found.", ConsoleColor.Green, overrideQuiet: true);
182:             }
183: 
184:             ConsoleHelpers.WriteLine(overrideQuiet: true);
185:             return duplicates;
186:         }
```

**Key Algorithms**:
- **Lines 137-140**: Signature creation
  - Only user/assistant messages (not system/tool)
  - First 10 messages only
  - Format: `role:contentLength|role:contentLength|...`
- **Line 146**: Group files by signature
- **Line 154**: Find groups with count > 1 (duplicates)
- **Line 166**: Sort by timestamp DESC (newest first)
- **Line 167**: Keep newest file
- **Line 168**: Mark rest for removal

---

## Empty Detection Implementation

### FindEmptyConversations Method

**Lines 188-228**: Complete empty detection logic
```csharp
188:         private List<string> FindEmptyConversations(List<string> files)
189:         {
190:             ConsoleHelpers.WriteLine("### Finding Empty Conversations", ConsoleColor.White, overrideQuiet: true);
191:             ConsoleHelpers.WriteLine(overrideQuiet: true);
192: 
193:             var empty = new List<string>();
194: 
195:             foreach (var file in files)
196:             {
197:                 try
198:                 {
199:                     var conversation = CycoDj.Helpers.JsonlReader.ReadConversation(file);
200:                     if (conversation == null) continue;
201: 
202:                     var meaningfulMessages = conversation.Messages.Count(m => 
203:                         m.Role == "user" || m.Role == "assistant");
204: 
205:                     if (meaningfulMessages == 0)
206:                     {
207:                         empty.Add(file);
208:                         ConsoleHelpers.WriteLine($"  Empty: {Path.GetFileName(file)}", ConsoleColor.Yellow, overrideQuiet: true);
209:                     }
210:                 }
211:                 catch (Exception ex)
212:                 {
213:                     Logger.Warning($"Failed to analyze {file}: {ex.Message}");
214:                 }
215:             }
216: 
217:             if (empty.Any())
218:             {
219:                 ConsoleHelpers.WriteLine($"Found {empty.Count} empty conversation(s).", ConsoleColor.Yellow, overrideQuiet: true);
220:             }
221:             else
222:             {
223:                 ConsoleHelpers.WriteLine("  No empty conversations found.", ConsoleColor.Green, overrideQuiet: true);
224:             }
225: 
226:             ConsoleHelpers.WriteLine(overrideQuiet: true);
227:             return empty;
228:         }
```

**Key Algorithm**:
- **Lines 202-203**: Count "meaningful" messages
  - Only user OR assistant messages
  - System and tool messages don't count
- **Lines 205-208**: If count == 0, mark as empty

---

## Age Detection Implementation

### FindOldConversations Method

**Lines 230-267**: Complete age detection logic
```csharp
230:         private List<string> FindOldConversations(List<string> files, int olderThanDays)
231:         {
232:             ConsoleHelpers.WriteLine($"### Finding Conversations Older Than {olderThanDays} Days", ConsoleColor.White, overrideQuiet: true);
233:             ConsoleHelpers.WriteLine(overrideQuiet: true);
234: 
235:             var cutoffDate = DateTime.Now.AddDays(-olderThanDays);
236:             var old = new List<string>();
237: 
238:             foreach (var file in files)
239:             {
240:                 try
241:                 {
242:                     var timestamp = CycoDj.Helpers.TimestampHelpers.ParseTimestamp(file);
243:                     if (timestamp < cutoffDate)
244:                     {
245:                         old.Add(file);
246:                         ConsoleHelpers.WriteLine($"  Old: {Path.GetFileName(file)} ({timestamp:yyyy-MM-dd})", 
247:                             ConsoleColor.DarkGray, overrideQuiet: true);
248:                     }
249:                 }
250:                 catch (Exception ex)
251:                 {
252:                     Logger.Warning($"Failed to analyze {file}: {ex.Message}");
253:                 }
254:             }
255: 
256:             if (old.Any())
257:             {
258:                 ConsoleHelpers.WriteLine($"Found {old.Count} old conversation(s).", ConsoleColor.Yellow, overrideQuiet: true);
259:             }
260:             else
261:             {
262:                 ConsoleHelpers.WriteLine("  No old conversations found.", ConsoleColor.Green, overrideQuiet: true);
263:             }
264: 
265:             ConsoleHelpers.WriteLine(overrideQuiet: true);
266:             return old;
267:         }
```

**Key Algorithm**:
- **Line 235**: Calculate cutoff date (Now - N days)
- **Line 242**: Extract timestamp from filename
- **Line 243**: If timestamp < cutoff, mark as old

---

## Execution Flow Diagram

```
ExecuteAsync() [Line 18]
    ↓
Step 1: Validate criteria [Lines 23-29]
    IF (!FindDuplicates AND !FindEmpty AND !OlderThanDays)
        ERROR: "Please specify at least one cleanup operation"
        RETURN 1
    ↓
Step 2: Find all files [Lines 32-33]
    files = HistoryFileHelpers.FindAllHistoryFiles()
    (NO time filtering!)
    ↓
Step 3: Collect targets [Lines 38-53]
    toRemove = []
    ↓
    IF (FindDuplicates OR RemoveDuplicates)
        toRemove += FindDuplicateConversationsAsync(files)
    ↓
    IF (FindEmpty OR RemoveEmpty)
        toRemove += FindEmptyConversations(files)
    ↓
    IF (OlderThanDays.HasValue)
        toRemove += FindOldConversations(files, OlderThanDays.Value)
    ↓
    toRemove = toRemove.Distinct()  // Deduplicate
    ↓
Step 4: Check results [Lines 58-62]
    IF (toRemove.Count == 0)
        SUCCESS: "No files need cleanup!"
        RETURN 0
    ↓
Step 5: Display results [Lines 64-73]
    Show files to remove
    Show count and sizes
    ↓
Step 6: Execute or dry-run [Lines 75-116]
    IF (DryRun AND (RemoveDuplicates OR RemoveEmpty))
        Show: "DRY RUN - No files will be deleted"
        Show: "Add --execute to actually remove files"
    ELSE IF (!DryRun AND (RemoveDuplicates OR RemoveEmpty))
        Prompt: "Type 'DELETE' to confirm"
        IF confirmed
            Delete files
            Show results
```

---

## Data Flow Summary

### Input Options → Properties → Methods

| CLI Option | Property Set | Used In | Method Called |
|------------|--------------|---------|---------------|
| `--find-duplicates` | `FindDuplicates = true` | Line 40 | `FindDuplicateConversationsAsync()` |
| `--remove-duplicates` | `RemoveDuplicates = true`, `FindDuplicates = true` | Line 40 | Same + deletion |
| `--find-empty` | `FindEmpty = true` | Line 45 | `FindEmptyConversations()` |
| `--remove-empty` | `RemoveEmpty = true`, `FindEmpty = true` | Line 45 | Same + deletion |
| `--older-than-days 90` | `OlderThanDays = 90` | Line 50 | `FindOldConversations(90)` |
| `--execute` | `DryRun = false` | Line 82 | Enables actual deletion |
| (no options) | All false/null | Line 23 | Error: no criteria |

---

## Verification Tests

To verify this implementation, run:

```bash
# Test 1: No criteria (should error)
cycodj cleanup
# Expected: ERROR: Please specify at least one cleanup operation

# Test 2: Find duplicates (dry-run)
cycodj cleanup --find-duplicates
# Expected: Shows duplicates, no deletion

# Test 3: Remove duplicates (requires confirmation)
cycodj cleanup --remove-duplicates --execute
# Expected: Prompts for 'DELETE', removes files

# Test 4: Find empty (dry-run)
cycodj cleanup --find-empty
# Expected: Shows empty conversations, no deletion

# Test 5: Remove empty (requires confirmation)
cycodj cleanup --remove-empty --execute
# Expected: Prompts for 'DELETE', removes files

# Test 6: Find old conversations
cycodj cleanup --older-than-days 90
# Expected: Shows conversations 90+ days old

# Test 7: Combined criteria
cycodj cleanup --find-duplicates --find-empty --older-than-days 30
# Expected: Shows all matching files (union of criteria)

# Test 8: Time options don't work
cycodj cleanup --find-duplicates --today
# Expected: ERROR: Unknown option --today
```

---

## Conclusion

This proof document demonstrates:

1. ✅ **Type-based selection** - duplicates, empty, old conversations
2. ✅ **NO time filtering** - cleanup doesn't call `TryParseTimeOptions()`
3. ✅ **Required criteria** - at least one criterion must be specified
4. ✅ **Safe by default** - DryRun = true unless --execute
5. ✅ **Confirmation required** - prompts for 'DELETE' before removing
6. ✅ **Deduplication** - removes duplicate entries from removal list

**Unique Characteristics**:
- Only command with type-based selection (vs time-based)
- Only command that can delete files (Layer 9)
- Only command with dry-run/execute modes
- Only command requiring confirmation for destructive operations
- NO shared time filtering code (intentionally different)

**Source Files Analyzed**:
- `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs` (parser - Lines 532-548)
- `src/cycodj/CommandLineCommands/CleanupCommand.cs` (execution - Lines 9-267)
- `src/cycodj/Helpers/HistoryFileHelpers.cs` (file finding)
- `src/cycodj/Helpers/TimestampHelpers.cs` (timestamp extraction)

**Total Lines of Evidence**: 180+ lines across 4 files
