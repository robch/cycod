# GetMemoryLogs Improvements - 2025-01-09

**Purpose:** Fix self-referential search problem and improve usability

---

## 🎯 **Changes Made:**

### **1. Default `removeAllLines="GetMemoryLogs"` ($40)**

**Problem:** When searching for log content, matches would include my own GetMemoryLogs function calls, creating noise.

**Solution:** Change default value from `""` to `"GetMemoryLogs"` to automatically filter out self-referential logs.

**Impact:**
- ✅ Automatically excludes lines containing "GetMemoryLogs"
- ✅ Solves the most frustrating problem
- ✅ Can still override by setting `removeAllLines=""`

---

### **2. Add `useRegex` Parameter ($30)**

**Problem:** Unclear whether `lineContains` and `removeAllLines` use regex or literal text matching.

**Solution:** Add explicit `bool useRegex = true` parameter (following `ReplaceAllInFiles` pattern).

**Impact:**
- ✅ Makes behavior explicit
- ✅ `useRegex=true` (default): Pattern is regex
- ✅ `useRegex=false`: Pattern is literal text (special chars auto-escaped)
- ✅ Error messages now say "regex" or "text" based on mode

**Usage:**
```csharp
// Regex mode (default)
GetMemoryLogs(lineContains: "Test.*timeout")  // Matches "Test 1 timeout", "Testing timeout", etc.

// Literal mode
GetMemoryLogs(lineContains: "Test.*timeout", useRegex: false)  // Matches only "Test.*timeout" exactly
```

---

### **3. Better Feedback on Match Counts ($10)**

**Problem:** When getting too many matches, hard to know if I should refine the search.

**Solution:** Enhanced metadata to show match counts and truncation info.

**Before:**
```
[Showing lines 100-250 of 5000 total] [4750 lines remaining]
```

**After:**
```
[Matched 2000 lines, showing 150 lines with context, lines 100-250 (truncated: 1850 more matched lines not shown) of 5000 total] [4750 lines remaining]
```

**Impact:**
- ✅ Shows how many lines matched the pattern
- ✅ Shows if context lines were included
- ✅ Shows how many more matches were truncated
- ✅ Helps understand if search needs refinement

---

### **4. Show File Log Location ($20 - BONUS!)**

**Problem:** Memory logs are cleared on rebuild, losing all debugging context from previous sessions.

**Solution:** GetMemoryLogs now reports where the full logs are being saved to disk.

**Output:**
```
[Matched 5 lines, lines 100-105 of 500 total] [395 lines remaining]

💾 Full logs saved to: C:\dev\cycod\log-20250109-203000.log
   (Persists across rebuilds - use ViewFile to access)
```

**Impact:**
- ✅ I can now access logs from before rebuild!
- ✅ Can compare behavior across sessions
- ✅ Can verify fixes by looking at old logs
- ✅ File logging already existed, just wasn't surfaced
- ✅ Solves the "restart problem" completely!

---

## 📊 **Examples:**

### **Example 1: Default Behavior (Self-Filtering)**
```csharp
GetMemoryLogs(lineContains: "timeout")
// Automatically excludes lines with "GetMemoryLogs"
// Shows only actual timeout events, not my searches for them
```

### **Example 2: Literal Text Search**
```csharp
GetMemoryLogs(lineContains: "[000015]", useRegex: false)
// Searches for literal "[000015]" without needing to escape brackets
```

### **Example 3: Regex Search (Default)**
```csharp
GetMemoryLogs(lineContains: "\\[000015\\]")
// Regex mode - need to escape special characters
// Or just use useRegex=false!
```

### **Example 4: Include Own Calls**
```csharp
GetMemoryLogs(lineContains: "something", removeAllLines: "")
// Override default to see GetMemoryLogs calls
```

---

## 🔧 **Implementation Details:**

**Files Changed:**
- `src/cycod/FunctionCallingTools/MemoryLogHelperFunctions.cs`

**Key Changes:**
1. Parameter defaults:
   - `removeAllLines = "GetMemoryLogs"` (was `""`)
   - Added `useRegex = true`

2. Pattern escaping:
   ```csharp
   var pattern = useRegex ? inputPattern : Regex.Escape(inputPattern);
   ```

3. Enhanced feedback:
   ```csharp
   if (totalMatches > 0)
   {
       sb.Append($"[Matched {totalMatches} line{(totalMatches != 1 ? "s" : "")}");
       if (totalIncluded > totalMatches)
       {
           sb.Append($", showing {totalIncluded} lines with context");
       }
       // ...
   }
   ```

---

## ✅ **Testing:**

After rebuild, verify:
1. Default filtering works (GetMemoryLogs calls excluded)
2. `useRegex=false` allows literal text search
3. Match count feedback appears
4. Can override default with `removeAllLines=""`

---

## 📚 **Related:**

- Main investigation: `docs/timeout-investigation-findings.md`
- Call tree analysis: `docs/timeout-fix-call-tree.md`
- How to trace calls: `docs/how-to-trace-call-paths.md`
- Timeout code paths testing: `docs/timeout-code-paths-testing.md`
