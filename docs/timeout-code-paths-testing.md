# Timeout Detection - Testing All Code Paths

**Purpose:** Ensure all code paths that check for timeout work correctly after our fix

---

## 📋 **All Timeout Check Locations:**

### **1. ShellExecutionResults.ToAiString()** 
**File:** `src/common/ProcessExecution/ShellExecutionResults.cs:171`

**Check:** `if (TimedOut)`

**Special Message:**
```
<timed out; still running; use GetShellOrProcessOutput>
```

**How to Test:**
```csharp
RunShellCommand("sleep 10", expectedTimeout: 2000)
// Should return JSON with status="stillRunning" and the above message
```

---

### **2. ShellExecutionResults.FromProcessResult()**
**File:** `src/common/ProcessExecution/ShellExecutionResults.cs:202-206`

**Check:** `result.IsTimeout`

**Special Messages:**
```csharp
Success = result.ExitCode == 0 && !result.IsTimeout  // False when timeout
FriendlyErrorMessage = result.IsTimeout ? "Command timed out" : ...
```

**How to Test:**
```csharp
// Execute any shell command with timeout
// Check that Success=false and FriendlyErrorMessage="Command timed out"
```

---

### **3. GitHubSearchHelperFunctions.SearchGitHub()**
**File:** `src/cycod/FunctionCallingTools/GitHubSearchHelperFunctions.cs:77`

**Check:** `if (result.CompletionState == ProcessCompletionState.TimedOut)`

**Special Message:**
```
<cycodgr command timed out after 120 seconds>
```

**How to Test:**
```csharp
SearchGitHub("--contains 'some really slow search that takes forever'")
// Hard to test without actual slow search - might need mock
```

---

### **4. ShellCommandToolHelperFunctions (Legacy)**
**File:** `src/cycod/FunctionCallingTools/ShellCommandToolHelperFunctions.cs:32, 64, 96`

**Check:** `if (result.IsTimeout)`

**Special Message:**
```
<timed out and killed process - environment state has been reset>
```

**NOTE:** These are for singleton shells (BashShellSession.Instance, etc.) which DO get killed on timeout

**How to Test:**
```csharp
// These functions aren't AI tools anymore (no [KernelFunction])
// But they're still used internally - need to find callers
```

---

### **5. CycoDmdCliWrapper**
**File:** `src/cycod/FunctionCallingTools/CycoDmdCliWrapper.cs:124`

**Check:** `if (result.CompletionState == ProcessCompletionState.TimedOut)`

**Special Message:**
```
<Expected cycodmd to complete within {timeoutMs}ms for cycodmd to finish, but it exceeded the timeout>
```

**How to Test:**
```csharp
// Use ConvertFilesToMarkdown or other cycodmd function with very short timeout
// Or mock a slow cycodmd operation
```

---

### **6. CheckExpectInstructionsHelper (Testing Framework)**
**File:** `src/common/Helpers/CheckExpectInstructionsHelper.cs:57`

**Check:** `var timedoutOrKilled = !exitedNotKilled;`

**Special Message:**
```
"CheckExpectInstructionsHelper.CheckExpectations: WARNING: Timedout or killed!"
```

**How to Test:**
```yaml
# Create cycodt test with timeout
- name: "Test timeout detection in cycodt"
  command: |
    sleep 5
  timeout: 2000
  expect-timeout: true
```

---

## 🧪 **Test Plan:**

### **Test 1: RunShellCommand Timeout (Path 1)**
**Goal:** Verify ShellExecutionResults.ToAiString() message

```bash
# Test command
RunShellCommand("echo 'Starting'; sleep 10; echo 'Done'", expectedTimeout: 2000)

# Expected output
{
  "status": "stillRunning",
  "shellName": "auto-bash-XXXXXX",
  "outputSoFar": "Starting\n<timed out; still running; use GetShellOrProcessOutput>",
  ...
}
```

**How to verify:**
- Look for the special message `<timed out; still running; use GetShellOrProcessOutput>`
- Check that shell was promoted
- Verify shell is still usable

---

### **Test 2: ExecuteInShell Timeout (Path 2)**
**Goal:** Verify ShellCommandResult.FromProcessResult()

```bash
# Test command
CreateNamedShell("test-timeout")
ExecuteInShell("test-timeout", "sleep 10", timeout: 2000)

# Expected
Success: false
FriendlyErrorMessage: "Command timed out"
TimedOut: true
```

**How to verify:**
- Check result has TimedOut=true
- Check FriendlyErrorMessage="Command timed out"
- Check Success=false

---

### **Test 3: Legacy Shell Functions (Path 4)**
**Goal:** Verify singleton shell timeout message

```bash
# These aren't exposed as AI functions anymore
# Need to check if anything still calls them
```

**Search for callers:**
```
SearchInFiles: "RunBashCommand\(|RunCmdCommand\(|RunPowershellCommand\("
```

---

### **Test 4: CycoDmdCliWrapper Timeout (Path 5)**
**Goal:** Verify cycodmd timeout message

```bash
# Create a cycodmd operation that times out
# Difficult to test naturally - might need to modify timeout for testing
```

**Option A:** Create huge markdown file to process with very short timeout
**Option B:** Mock/modify timeout temporarily

---

### **Test 5: cycodt Timeout Detection (Path 6)**
**Goal:** Verify cycodt test framework detects timeouts

```yaml
# Create test file: tests/timeout-detection-test.yaml
tests:
  - name: "Verify cycodt detects timeout"
    command: |
      sleep 5
    timeout: 2000
    expect-regex:
      - "WARNING: Timedout or killed"
```

---

## ✅ **Quick Verification Tests:**

### **Minimal Test Suite:**

```bash
# Test 1: RunShellCommand timeout
echo "Test RunShellCommand timeout"
RunShellCommand("sleep 5", expectedTimeout: 1000)
# Verify: Output contains "<timed out; still running; use GetShellOrProcessOutput>"

# Test 2: ExecuteInShell timeout  
echo "Test ExecuteInShell timeout"
CreateNamedShell("test-shell")
ExecuteInShell("test-shell", "sleep 5", timeout: 1000)
# Verify: FriendlyErrorMessage="Command timed out", Success=false

# Test 3: Shell still usable after timeout
echo "Test shell survives timeout"
ExecuteInShell("test-shell", "echo 'Still alive'", timeout: 5000)
# Verify: Output contains "Still alive"

# Test 4: cycodt timeout test
echo "Test cycodt timeout detection"
cycodt run --file timeout-detection-test.yaml
# Verify: Test passes and detects timeout
```

---

## 🎯 **What We're Testing:**

1. ✅ **Timeout flag is set** (`TimedOut=true`, `IsTimeout=true`)
2. ✅ **Special messages appear** (each code path has unique message)
3. ✅ **Shell survives timeout** (can execute more commands)
4. ✅ **Shell promotion works** (auto-promoted shells are usable)
5. ✅ **All code paths reached** (each check location is exercised)

---

## 📝 **Test Results Template:**

```markdown
## Test Results - Timeout Code Paths

| Path | Location | Special Message | Status | Notes |
|------|----------|----------------|--------|-------|
| ToAiString() | ShellExecutionResults.cs:171 | `<timed out; still running...>` | ⬜ | |
| FromProcessResult() | ShellExecutionResults.cs:206 | `"Command timed out"` | ⬜ | |
| SearchGitHub() | GitHubSearchHelperFunctions.cs:77 | `<cycodgr command timed out...>` | ⬜ | |
| Legacy Shells | ShellCommandToolHelperFunctions.cs:32 | `<timed out and killed process...>` | ⬜ | |
| CycoDmdCliWrapper | CycoDmdCliWrapper.cs:124 | `<Expected cycodmd to complete...>` | ⬜ | |
| CheckExpect | CheckExpectInstructionsHelper.cs:57 | `WARNING: Timedout or killed!` | ⬜ | |
```

---

## 🚨 **Important Notes:**

1. **Legacy shell functions** (Path 4) DO kill the shell on timeout - this is expected behavior for singleton shells
2. **RunShellCommand** (Path 1) should NOT kill shell - should promote it
3. **ExecuteInShell** (Path 2) on named shells should NOT kill shell
4. **CycoDmdCliWrapper** (Path 5) is for one-shot processes - uses RunnableProcess (different code path)
5. **GitHubSearchHelperFunctions** (Path 3) also uses RunnableProcess with 120s timeout

---

## 🔍 **How to Run All Tests:**

```bash
# Option 1: Manual testing (run commands above)
# Option 2: Create automated test script
# Option 3: Create cycodt test file covering all paths
```

**Recommended:** Create `tests/cycodt-yaml/timeout-code-paths-test.yaml` covering all scenarios

---

## 📚 **Related:**

- Main findings: `docs/timeout-investigation-findings.md`
- Call tree: `docs/timeout-fix-call-tree.md`
- How to trace: `docs/how-to-trace-call-paths.md`
