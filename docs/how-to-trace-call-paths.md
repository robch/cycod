# How to Trace Call Paths in Codebase

**Quick reference for finding all callers of a method**

---

## 🎯 **Basic Strategy:**

1. Start with the method you changed
2. Find all callers of that method
3. For each caller, find their callers
4. Continue up the chain until you reach public APIs or entry points
5. Document the tree
6. Test each major path

---

## 🔍 **Using SearchInFiles to Find Callers:**

### **Pattern 1: Find calls to a specific method**
```
SearchInFiles:
  filePatterns: ["**/*.cs"]
  excludePatterns: ["**/bin/**", "**/obj/**"]
  lineContains: "MethodName\("
  linesBeforeAndAfter: 2
```

**Example:** Finding calls to `WaitForMarkerAsync`:
```
lineContains: "WaitForMarkerAsync\("
```

**Result:** Shows all lines that call this method with context

---

### **Pattern 2: Find where a class is instantiated**
```
lineContains: "new ClassName"
```

**Example:** Finding where `PersistentShellCommandBuilder` is created:
```
lineContains: "new PersistentShellCommandBuilder"
```

---

### **Pattern 3: Find method implementations**
```
lineContains: "public.*async.*Task.*MethodName"
```

**Example:** Finding where `ExecuteCommandAsync` is implemented:
```
lineContains: "public.*async.*Task.*ExecuteCommandAsync"
```

---

## 📋 **Step-by-Step Process:**

### **Step 1: Identify what you changed**
```
Example:
- Modified: PersistentShellProcess.WaitForMarkerAsync()
- Removed: TimeoutException catch block
```

### **Step 2: Find direct callers**
```bash
SearchInFiles for "WaitForMarkerAsync("
```

**Record results:**
- `RunCommandInternalAsync()` calls it (line 236)
- `VerifyShellAsync()` calls it (line 83)

### **Step 3: Find callers of those callers**
```bash
SearchInFiles for "RunCommandInternalAsync("
```

**Record results:**
- `RunCommandAsync()` calls it (line 143)
- `RunCommandWithInputAsync()` calls it (line 384)

### **Step 4: Continue up the tree**
Repeat for each level until you reach:
- Public API methods
- AI function tools ([KernelFunction])
- Entry points (Main, handlers)
- Test code

### **Step 5: Document the tree**
Create a document showing:
```
Level 1: MethodYouChanged
  ↑ Called by
Level 2: DirectCaller1
  ↑ Called by
Level 3: DirectCaller2
  ↑ Called by
Level 4: PublicAPI
```

### **Step 6: Test each major path**
For each public API / entry point:
- Create a test that exercises that path
- Verify it still works correctly
- Document the test result

---

## 🎨 **Example: Complete Analysis**

### **Scenario:** Modified `WaitForMarkerAsync()` in `PersistentShellProcess`

### **Step 1: Find direct callers**
```bash
Search: "WaitForMarkerAsync\("
```

**Results:**
```
File: PersistentShellProcess.cs
  Line 83:  await WaitForMarkerAsync(...)     # VerifyShellAsync
  Line 236: await WaitForMarkerAsync(...)     # RunCommandInternalAsync
```

### **Step 2: Find callers of `RunCommandInternalAsync`**
```bash
Search: "RunCommandInternalAsync\("
```

**Results:**
```
File: PersistentShellProcess.cs
  Line 124: await RunCommandInternalAsync(...)  # RunCommandAsync
  Line 143: await RunCommandInternalAsync(...)  # RunCommandAsync (timeout overload)
```

### **Step 3: Find callers of `RunCommandAsync`**
```bash
Search: "RunCommandAsync\("
```

**Results:**
```
File: PersistentShellCommandBuilder.cs
  Line 105: await _shellProcess.RunCommandAsync(...)  # RunAsync
```

### **Step 4: Find callers of builder's `RunAsync`**
```bash
Search: "commandBuilder.*RunAsync\("
```

**Results:**
```
File: ShellSession.cs
  Line 176: await commandBuilder.RunAsync()  # ExecuteCommandAsync
```

### **Step 5: Find callers of `ShellSession.ExecuteCommandAsync`**
```bash
Search: "ExecuteCommandAsync\("
```

**Results:**
```
File: NamedShellProcessManager.cs
  Line 413: await shell.ExecuteCommandAsync(...)  # ExecuteInShellAsync

File: ShellAndProcessHelperFunctions.cs
  Line 76: await ...ExecuteInShellAsync(...)  # RunShellCommand [KernelFunction]
```

### **Step 6: Document the call tree**
```
WaitForMarkerAsync (MODIFIED)
  ↑
RunCommandInternalAsync
  ↑
RunCommandAsync
  ↑
PersistentShellCommandBuilder.RunAsync
  ↑
ShellSession.ExecuteCommandAsync (MODIFIED)
  ↑
NamedShellProcessManager.ExecuteInShellAsync
  ↑
RunShellCommand [AI Function] ← PUBLIC API
```

### **Step 7: Test the path**
```bash
# Test the public API
RunShellCommand("echo test")  # Should work
RunShellCommand("sleep 10", timeout=2000)  # Should timeout correctly
```

---

## 🚨 **Common Pitfalls:**

1. **Missing overloads:** Methods often have multiple overloads - search for each
2. **Interface implementations:** Search for interface name too
3. **Base class calls:** Check if method is virtual and has overrides
4. **Reflection/dynamic calls:** Won't show up in search - check tests
5. **Lambda expressions:** May call methods inline - look for `=>` patterns

---

## 🛠️ **Useful Search Patterns:**

### Find all methods that could throw an exception type:
```
lineContains: "throw new TimeoutException"
```

### Find all catch blocks for an exception:
```
lineContains: "catch.*TimeoutException"
```

### Find all async methods:
```
lineContains: "public.*async.*Task"
```

### Find all AI function tools:
```
lineContains: "\[KernelFunction"
```

### Find all public APIs:
```
lineContains: "public class.*Manager|public.*static.*async"
```

---

## ✅ **Verification Checklist:**

When you've completed the analysis:

- [ ] Documented all call paths from bottom (modified method) to top (public API)
- [ ] Identified all public APIs that could be affected
- [ ] Created tests for each major path
- [ ] Ran existing tests related to the feature
- [ ] Documented the results
- [ ] Verified no regressions
- [ ] Created call tree diagram/document

---

## 📚 **Tools & Commands:**

### **SearchInFiles** (AI function):
```
SearchInFiles(
  filePatterns: ["**/*.cs"],
  lineContains: "MethodName\(",
  linesBeforeAndAfter: 2
)
```

### **git grep** (command line):
```bash
git grep "MethodName(" -- "*.cs"
```

### **Visual Studio** (if using IDE):
```
Right-click method → Find All References
```

### **Rider** (if using IDE):
```
Ctrl+Shift+Alt+F7 → Find Usages
```

---

## 🎓 **Best Practices:**

1. **Start from the bottom** (lowest level change) and work up
2. **Document as you go** - don't rely on memory
3. **Test each major path** - don't assume it works
4. **Look for patterns** - similar methods likely have similar call paths
5. **Check tests** - existing tests often reveal important call paths
6. **Consider edge cases** - error paths, timeouts, cancellations
7. **Update documentation** - create a call tree document for future reference

---

## 📖 **Related:**

- Example analysis: `docs/timeout-fix-call-tree.md`
- Investigation findings: `docs/timeout-investigation-findings.md`
