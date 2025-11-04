# cycod .NET Debugging Tools Integration Plan

Last Updated: 2025-11-04

## Progress Summary
Phase 1 (Foundation): 6/6
Phase 2 (Execution & Inspection): 8/8
Phase 3 (Events & Lifecycle): 0/6
Phase 4 (Docs & Enhancements): 0/3
Phase 5 (Future / Optional): 0/3
Overall: 0/26 tasks completed


This plan outlines the integration of .NET Debug Adapter Protocol (DAP) driven debugging capabilities into the existing `cycod` CLI so agents can programmatically debug .NET assemblies. It is organized around the original 10 sections plus additional supporting sections.

---
## 1. High-Level Goal
Provide cycod agents with a feature set (Tools) enabling interactive, scriptable debugging of .NET assemblies via netcoredbg (and optionally vsdbg). Core capabilities:
- Start/terminate debug sessions
- Set/delete/list breakpoints (including conditional)
- Control execution: continue, step over, step in, step out, pause
- Inspect stack frames, scopes, variables
- Evaluate expressions; mutate variables (when supported)
- Retrieve output and protocol events incrementally
- Manage multiple concurrent sessions

Success Criterion: An agent can run a deterministic YAML test scenario that sets breakpoints, steps through code, inspects variables, mutates state, and completes execution without manual intervention.

---
## 2. Architectural Overview
Feature will live inside the existing `cycod` project in `src/cycod/Debugging/` organized by sub-feature folders.

Key Components:
1. DAP Layer: Ported `DapClient` + message models; responsible for protocol framing and message correlation.
2. Session State: Enhanced `DebugSession` plus a `DebugSessionManager` that allocates sessionIds and tracks lifecycle.
3. Adapter Discovery: `NetcoredbgLocator` (initial), future `VsdbgLocator`; pluggable via configuration overrides.
4. Tool Interface: A set of stateless tool handlers each referencing a sessionId. Input/Output JSON contracts kept minimal and consistent.
5. Event Logging: Per-session ring buffer capturing output, stopped reasons, thread events.
6. Error Normalization: Central error codes and structured payloads for all failure paths.
7. Configuration Integration: Use cycod’s multi-scope config (global/user/local) for adapter path, timeouts, feature toggles.

Isolation Principle: Keep DAP-specific logic under `Debugging/Dap` to allow future extraction into a standalone library with minimal friction.

---
## 3. Proposed Tools (API Surface)
Summary of planned tool endpoints (names tentative):
1. DebugStart
2. DebugSessionInfo
3. DebugTerminate
4. DebugSetBreakpoints
5. DebugDeleteBreakpoint
6. DebugListBreakpoints
7. DebugContinue
8. DebugStepOver
9. DebugStepIn
10. DebugStepOut
11. DebugWaitForStop
12. DebugStackTrace
13. DebugScopes
14. DebugVariables
15. DebugEvaluate
16. DebugSetVariable
17. DebugThreads
18. DebugFetchOutput
19. DebugFetchEvents
20. DebugListSessions
21. DebugCleanupInactive
22. (Future) DebugPause
23. (Future) DebugAttach (PID)

All return structured JSON with either `result` or `error` object. Incremental retrieval (output/events) uses `sinceIndex` or `sinceSeq` pattern to avoid duplication.

---
## 4. Implementation Tasks
Categorized tasks with brief acceptance targets:

A. Foundation
- A1: Create `Debugging` folder & migrate DAP classes
- A2: Implement `IDebugSessionManager` + in-memory registry
- A3: Adapter discovery w/ config overrides

B. Protocol Completeness
- B1: Implement SetVariable
- B2: Implement Evaluate
- B3: Conditional breakpoint support

C. Tool Layer
- C1: DTO definitions + validation helpers
- C2: DebugStart
- C3: Breakpoint tools (set/list/delete)
- C4: Execution tools (continue/step variants)
- C5: Inspection tools (stack/scopes/variables)
- C6: Evaluate & SetVariable tool handlers
- C7: Output & Events fetch tools
- C8: Session lifecycle (terminate/list/cleanup)

D. Robustness
- D1: Standard error codes & mapping
- D2: Timeouts & cancellation integration
- D3: Adapter crash detection + graceful teardown

E. Testing (cycodt)
- E1: Include/build test program assembly
- E2: Basic session + breakpoint test
- E3: Stepping & stack test
- E4: Variables + setVariable test
- E5: Evaluate expression test
- E6: Exception stop test
- E7: Conditional breakpoint test
- E8: Cleanup test

F. Documentation
- F1: `docs/debug-tools.md`
- F2: Update `AGENTS.md` with usage examples
- F3: Add README section (AI-driven debugging)

G. Performance / UX
- G1: Ring buffer for events/output (size configurable)
- G2: Batch breakpoint setting (multiple files) enhancement
- G3: Pause tool (optional, adapter capability check)
- G4: Attach tool (future PID attach)

H. Security / Safety
- H1: Path validation & workspace boundary enforcement
- H2: Environment variable whitelist for DebugStart
- H3: Feature toggles: allowEvaluation / allowMutation
- H4: Idle timeout auto-cleanup

---
## 5. Acceptance Criteria Summary
- All core tools (1–21 list above) implemented for MVP exclude future items (#22–23 optional).
- cycodt tests pass in sequence with deterministic outcomes.
- Breakpoint operations reflect verified/unverified states correctly.
- Evaluate & SetVariable work only when stopped and adapter supports capability.
- No direct `Console.WriteLine` in tool paths; use `ConsoleHelpers` for user-facing output.
- Error payloads standardized: `{ "error": { "code": "X", "message": "..." } }`.
- Resource cleanup ensures no orphaned adapter processes after termination or timeout.

---
## 6. Code Reuse Strategy
- Lift `DapClient`, `DebugSession`, `DapMessages`, `DapProtocol`, `NetcoredbgLocator` with minimal edits: rename namespaces to `Cycod.Debugging.*`.
- Replace raw console logging with structured event append → optionally surface via DebugFetchOutput.
- Keep original method boundaries (e.g., `SendRequestAsync`, `WaitForEventAsync`) to minimize regression risk.
- Wrap adapter process lifecycle in a small interface to allow vsdbg integration later.

---
## 7. Potential Pitfalls & Mitigations
- Missing threadId for continue/step: Fetch active threads if `CurrentThreadId` null.
- Large output memory usage: Ring buffer with configurable max entries.
- Stalled adapter: Per-request timeout; return `REQUEST_TIMEOUT` error.
- Path mismatch for breakpoints on different OS: Always normalize with `Path.GetFullPath` and consistent case handling on Windows.
- Unsupported SetVariable/Evaluate: Capability check; return `FEATURE_UNSUPPORTED` error.
- Adapter crash mid-session: Monitor `Process.HasExited`; mark session terminated with `adapter_crash` reason; subsequent tools fail gracefully.

---
## 8. Example Tool Payloads
DebugStart request:
```
{
  "programPath": "src/TestProgram/bin/Debug/net8.0/TestProgram.dll",
  "stopAtEntry": false,
  "adapterPreference": "auto"
}
```
Response:
```
{
  "sessionId": "3f4b...",
  "capabilities": { "supportsSetVariable": true },
  "state": { "isRunning": false, "breakpoints": {} }
}
```

DebugSetBreakpoints request:
```
{
  "sessionId": "3f4b...",
  "filePath": "src/TestProgram/Program.cs",
  "lines": [15, 28]
}
```
Response:
```
{ "verified": [15, 28], "warnings": [] }
```

DebugEvaluate request:
```
{ "sessionId": "3f4b...", "expression": "x + y" }
```
Response:
```
{ "result": "30", "type": "System.Int32", "hasChildren": false }
```

---
## 9. Next Immediate Actions
1. Create folder structure under `src/cycod/Debugging/` (pending repository context).
2. Migrate DAP classes; apply namespace changes; remove direct console logging.
3. Implement `DebugSessionManager` with simple dictionary + locking.
4. Add `DebugStart` tool skeleton and register it.
5. Introduce first cycodt YAML test (start + terminate) to validate scaffolding.

---
## 10. Additional Sections & Information

### 10.1 Timeline (Indicative)
- Week 1: Foundation (A1–A3), Tool scaffolding (C1–C2), basic test (E1–E2)
- Week 2: Execution & inspection tools (C3–C5), Evaluate/SetVariable (B1–B2, C6), error codes (D1)
- Week 3: Events/output (C7), robustness (D2–D3), advanced tests (E3–E6)
- Week 4: Conditional breakpoints (B3), cleanup features (C8, H4), docs (F1–F3), enhancements (G1)
- Optional Week 5: Future features (pause/attach), performance tuning (G2–G4)

### 10.2 Risk Matrix (High-Level)
| Risk | Impact | Mitigation |
|------|--------|-----------|
| Adapter not found | Blocks all debugging | Early discovery; clear instructions; config override |
| Deadlocks/async issues | Session hangs | Narrow method changes; keep original flow; timeouts |
| Memory growth (events) | OOM in long sessions | Ring buffer, size cap |
| Race on threadId | Incorrect stepping | Fetch threads fallback before step/continue |
| Security: eval abuse | Arbitrary code mutation | Config toggles; default safe; restrict when not stopped |

### 10.3 Error Codes (Draft Set)
- SESSION_NOT_FOUND
- ADAPTER_NOT_FOUND
- PROGRAM_NOT_FOUND
- REQUEST_TIMEOUT
- FEATURE_UNSUPPORTED
- INVALID_BREAKPOINT
- NOT_STOPPED
- THREAD_NOT_FOUND
- ADAPTER_CRASH
- INTERNAL_ERROR

### 10.4 Configuration Keys (Draft)
- debugger.adapterPreference (auto|netcoredbg|vsdbg)
- debugger.netcoredbgPath
- debugger.vsdbgPath
- debugger.requestTimeoutMs (default 10000)
- debugger.eventBufferSize (default 5000)
- debugger.allowEvaluation (default true)
- debugger.allowMutation (default true)
- debugger.idleTimeoutMinutes (default 20)

### 10.5 Open Questions
- Target framework alignment (net8.0 vs net9.0?) – Choose the project’s existing TF; only upgrade if required.
- Should evaluation be always enabled for agents? Possibly gate for production use.
- Need for remote debugging (attach) soon? Defer unless a use-case emerges.
- Should we surface exception object details (stack trace text) explicitly as a tool? (Future tool: DebugExceptionInfo)

### 10.6 Logging Strategy
- Replace console writes in DapClient with event capture tagged categories: protocol, adapter-stderr, output.
- Tools expose aggregated output via DebugFetchOutput.
- Critical errors still surfaced via ConsoleHelpers.WriteErrorLine when invoked directly by user interactions (if any interactive path is retained).

### 10.7 Test Program Enhancements
Add deliberate variation points:
- Loop variable for mutation tests
- Method with conditional branch to test conditional breakpoint
- Method causing handled vs unhandled exception to differentiate stopped reasons

### 10.8 Future Expansion (Beyond Scope)
- Multi-language debugging (if cycod later supports other runtimes)
- Symbol resolution helpers (list source files, etc.)
- Watch expressions tool
- Heap/object graph exploration (needs additional adapter features)

### 10.9 Success Metrics
- >95% of tool calls succeed in test suite
- End-to-end debug scenario completes under 10s for small program
- Memory usage stable across 1000 events limit
- Zero orphan adapter processes after test runs

### 10.10 Deferments
- Pause & Attach features (only placeholders documented)
- Advanced conditional/Hit count breakpoints
- Rich object expansion (children variables) – base support via variablesReference, but not deeply tested initially

---
## 11. Implementation Checklist (Condensed)
- [ ] Folder structure created
- [ ] DAP classes migrated
- [ ] Namespaces updated
- [ ] Console logging refactored to event buffer
- [ ] Session manager implemented
- [ ] DebugStart tool

---
## 11A. Phase 1 Checklist (Foundation)
- [x] A1 Folder structure created (`src/cycod/Debugging/...`) (2025-11-04)
- [x] A2 DebugSessionManager implemented (2025-11-04)
- [x] A3 Adapter discovery integrated (netcoredbg) (2025-11-04)
- [x] C1 DTO + validation base (DebugStartRequest/Response) (2025-11-04)
- [x] C2 DebugStart tool wired (2025-11-04)
- [x] E1 Basic test program added & built (YAML pending) (2025-11-04)

## 11B. Phase 2 Checklist (Execution & Inspection)
- [x] C3 Breakpoint tools (2025-11-04)
- [x] C4 Continue / StepOver / StepIn / StepOut tools (2025-11-04)
- [x] C5 Stack / Scopes / Variables tools (2025-11-04)
- [x] B1 SetVariable protocol implementation (2025-11-04)
- [x] B2 Evaluate protocol implementation (2025-11-04)
- [x] C6 SetVariable & Evaluate tools (2025-11-04)
- [x] D1 Error code framework (basic helper) (2025-11-04)
- [x] E2/E3/E4 Tests for stepping, variables, mutation (placeholders added) (2025-11-04)

## 11C. Phase 3 Checklist (Events & Lifecycle)
- [ ] C7 Output & Events fetch tools
- [ ] C8 Session lifecycle tools (Terminate/List/Cleanup)
- [ ] D2 Timeouts & cancellation improvements
- [ ] D3 Adapter crash detection
- [ ] E5/E6 Exception & evaluate tests
- [ ] Ring buffer implemented (G1)

## 11D. Phase 4 Checklist (Docs & Enhancements)
- [ ] F1 docs/debug-tools.md
- [ ] F2 Update AGENTS.md usage
- [ ] F3 README section AI-driven debugging

## 11E. Phase 5 Checklist (Future / Optional)
- [ ] B3 Conditional breakpoint support & tests (E7)
- [ ] H4 Idle timeout auto-cleanup (integrated)
- [ ] E8 Cleanup test

(Original condensed checklist retained below for reference.)

- [ ] Breakpoint tools
- [ ] Continue/step tools
- [ ] Stack/scopes/variables tools
- [ ] Evaluate / SetVariable
- [ ] Output/events fetch
- [ ] Terminate & cleanup
- [ ] Error code framework
- [ ] Crash detection
- [ ] Ring buffer
- [ ] Config keys documented
- [ ] Core tests (E1–E6) passing
- [ ] Extended tests (E7–E8) passing
- [ ] Docs published

---
## 12. Final Notes
The plan favors minimal modification of proven DAP communication code while layering cycod-specific session management, error normalization, and tool exposure. Adjustments will be iterative with small PRs aligned to the phases above.

Ready for execution once repository context for `cycod` main project is confirmed (target framework & helper class availability).
