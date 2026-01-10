# RunShellCommand Timeout Investigation - Findings & Fix

**Date:** 2025-01-09  
**Issue:** Commands sometimes behave strangely with timeouts - "weird stuff happened"

---

## 🔍 **What We Discovered**

### **Two Timeout Mechanisms Racing**

`PersistentShellProcess.WaitForMarkerAsync()` has **TWO different timeout mechanisms**:

1. **CancellationToken mechanism** (Modern, correct)
   - Created in `RunCommandAsync` line 139: `new CancellationTokenSource(timeout)`
   - Passed to `Task.Delay(50, cancellationToken)` 
   - Throws `OperationCanceledException` when timeout fires
   - Shell stays alive, gets promoted correctly ✅

2. **Manual timeout check** (Old, problematic)
   - Lines 511-533 in `WaitForMarkerAsync`
   - Checks `if (elapsed > actualTimeoutMs)`
   - Throws `TimeoutException`
   - Caught in `RunCommandInternalAsync` line 268
   - Calls `ForceShutdown()` - **KILLS THE SHELL** ❌
   - Shell promotion fails

---

## 🎯 **The Race Condition**

**Normal case (works fine):**
```
Loop iteration:
1. Read output (5ms)
2. Check regex (1ms)
3. Manual timeout check (passes, we're at 1950ms < 2000ms)
4. Task.Delay(50, cancellationToken) starts
5. CancellationToken fires at 2000ms
6. Task.Delay throws OperationCanceledException ✅
7. Shell stays alive, gets promoted
```

**Bad case (causes problems):**
```
Loop iteration at T=1950ms:
1. Read output (slow due to lock contention - 30ms)
2. Check regex (slow on large output - 40ms)
3. Now at T=2020ms
4. Manual timeout check: 2020ms > 2000ms ❌
5. Throws TimeoutException
6. Never reaches Task.Delay
7. Caught in RunCommandInternalAsync
8. Calls ForceShutdown() - KILLS SHELL ❌
9. RunShellCommand tries to promote dead shell
10. Weird stuff happens!
```

---

## 📊 **Evidence**

We **proved this happens** by adding deliberate delays (lines 513-523) to slow down the loop:

```
Line 988: 🐌 DELIBERATELY SLOWING DOWN - elapsed=1915ms, adding 134ms delay
Line 989: 🐌 Woke from deliberate delay - elapsed=2059ms  
Line 990: ⏱️ Timeout check - elapsed=2059ms > 2000ms threshold
Line 991: 🚨 MANUAL TIMEOUT CHECK FIRED!
Line 992: 🚨 CAUGHT TIMEOUTEXCEPTION - calling ForceShutdown()
Line 993: 🚨 ForceShutdown() completed - SHELL KILLED!
```

**When it happens in real usage:**
- Heavy system load (CPU-bound operations slow everything)
- Large output (regex matching on megabytes of text)
- Lock contention (multiple shells competing)
- Slow operations before the timeout check

---

## 🔧 **The Fix: Option 1 (Recommended)**

### **Remove the Manual Timeout Check**

**What to remove:**
1. Lines 511-533 in `WaitForMarkerAsync` - manual timeout check + test code
2. Lines 268-296 in `RunCommandInternalAsync` - `TimeoutException` catch block
3. Lines 180-196 in `ShellSession.ExecuteCommandAsync` - `TimeoutException` catch block

**What to keep:**
- `CancellationTokenSource(timeout)` mechanism in `RunCommandAsync` line 139
- `OperationCanceledException` catch block in `RunCommandAsync` line 145
- Shell promotion logic when `result.TimedOut == true`

**Result:**
- Only one timeout mechanism (CancellationToken)
- No race condition
- Shell always stays alive for promotion
- Consistent timeout behavior

---

## 🧪 **Testing Done**

1. ✅ Fast commands complete correctly (no timeout)
2. ✅ Commands that timeout are handled via `OperationCanceledException`
3. ✅ Manual timeout check can fire under slow conditions (proved with test)
4. ✅ When manual check fires, shell gets killed (bad behavior confirmed)
5. ✅ Thread-safety with 50ms sleep shows no buffer corruption

---

## 📝 **Code Locations**

- `src/common/ProcessExecution/PersistentShell/PersistentShellProcess.cs`
  - Line 139: CancellationTokenSource creation
  - Line 145: OperationCanceledException handler (KEEP)
  - Line 268: TimeoutException handler (REMOVE)
  - Line 451-533: WaitForMarkerAsync method (REMOVE manual check)

- `src/common/ShellHelpers/ShellSession.cs`
  - Line 180: TimeoutException handler (REMOVE)

- `src/cycod/FunctionCallingTools/ShellAndProcessHelperFunctions.cs`
  - Line 64-122: RunShellCommand function (no changes needed)

---

## 🎓 **What We Learned**

### **Two Different Process Systems:**

1. **RunnableProcess** (one-shot commands like `cycodmd run`)
   - Uses `Task.Delay(timeout)` vs `Process.WaitForExit()` 
   - Throws `TimeoutException` properly
   - Works correctly ✅

2. **PersistentShellProcess** (interactive shells like RunShellCommand)
   - Has BOTH timeout mechanisms (legacy + modern)
   - CancellationToken always wins in normal cases
   - Manual check only fires under slow conditions
   - Manual check kills shell when it fires ❌

### **Thread-Safety Investigation:**

- Originally suspected race condition in buffer reads
- Output reads outside lock looked suspicious
- Testing showed buffers are stable after command completes
- No evidence of buffer corruption found
- "Outside lock" reads are safe because command is finished

---

## ⚠️ **Important Notes**

- The manual timeout check might have been added for a reason (unknown)
- No evidence found that it's needed with modern CancellationToken
- If commands start hanging forever after the fix, we may need to revisit
- All logging added during investigation should be removed after fix is confirmed

---

## 🚀 **Next Steps**

1. ✅ **COMPLETED:** Removed manual timeout check (lines 511-533)
2. ✅ **COMPLETED:** Removed TimeoutException catch in RunCommandInternalAsync (lines 268-300)
3. ✅ **COMPLETED:** Removed TimeoutException catch in ShellSession.ExecuteCommandAsync (lines 180-197)
4. ✅ **COMPLETED:** Removed deliberate slowdown test code
5. ✅ **COMPLETED:** Removed 50ms sleep for thread-safety testing
6. **TODO:** Test with various timeout scenarios after rebuild
7. **TODO:** Clean up excessive logging once confirmed working
8. **TODO:** Monitor for any "hung forever" commands in production

---

## ✅ **Fix Applied - 2025-01-09**

**Files Changed:**
- `src/common/ProcessExecution/PersistentShell/PersistentShellProcess.cs` (-37 lines, +20 lines)
  - Removed manual timeout check and test code
  - Removed TimeoutException handler  
  - Removed deliberate sleep in marker found path
  - **Added**: Capture output before timeout (lines 148-155, 395-402)
  
- `src/common/ShellHelpers/ShellSession.cs` (-18 lines)
  - Removed TimeoutException handler

**Result:**
- Only CancellationToken timeout mechanism remains
- No more ForceShutdown() on timeout
- Shell always stays alive for promotion
- Race condition eliminated
- **Output captured before timeout is now included in result**

**Additional Fix (2025-01-09 20:40):**
- Fixed timeout results to include output captured before timeout
- Previously returned empty strings for stdout/stderr/merged
- Now calls GetCurrentOutput(), GetCurrentError(), GetCurrentMergedOutput()
- This ensures users see what output was produced before the command timed out

**Testing Status:**
- Needs rebuild and verification with timeout scenarios
- Should test: fast commands, slow commands, actual timeouts
- Verify output appears in timeout results
- Monitor for any unexpected behavior

---

## 📚 **Related Files**

- This investigation: `docs/timeout-investigation-findings.md`
- Coding standards: `docs/C#-Coding-Style-Essential.md`
- Test framework: `src/cycodt/TestFramework/README.md`
