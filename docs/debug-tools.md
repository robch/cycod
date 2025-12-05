# cycod Debugging Tools

This document describes the .NET debugging feature integrated into the cycod CLI, enabling agents to debug .NET assemblies via the Debug Adapter Protocol (DAP) using `netcoredbg`.

## Overview
The debugging integration provides stateful sessions accessible through function calling tools. A session wraps a DAP adapter process and holds breakpoint, thread, and variable state. Tools are designed to be atomic (stateless inputs, structured JSON outputs) while referencing a `sessionId` for continuity.

## Core Capabilities
- Start a debug session for a compiled .NET DLL
- Set, list, delete breakpoints
- Continue and step (over, in, out)
- Inspect stack frames, scopes, and variables
- Evaluate expressions and set variable values (if adapter supports)
- Fetch buffered events/output incrementally
- Terminate and cleanup idle sessions

## Starting a Session
Tool: `StartDebugSession`
Input:
```
programPath: path/to/assembly.dll
stopAtEntry: false
```
Output example:
```
{
  "sessionId": "<id>",
  "capabilities": {
    "supportsSetVariable": true,
    "supportsConditionalBreakpoints": false,
    "supportsEvaluateForHovers": false
  },
  "state": {
    "isRunning": false,
    "isLaunched": true,
    "targetProgram": "/abs/path/to/assembly.dll"
  }
}
```

## Breakpoints
Tools:
- `SetBreakpoint(sessionId, filePath, line)`
- `DeleteBreakpoint(sessionId, filePath, line)`
- `ListBreakpoints(sessionId)`

Notes:
- File paths are normalized to full paths.
- Conditional breakpoints will be supported later (capability dependent).

## Execution Control
Tools:
- `Continue(sessionId)`
- `StepOver(sessionId)`
- `StepIn(sessionId)`
- `StepOut(sessionId)`

Each returns `{ status: "ok" }` or an error payload.

## Inspection
Tools:
- `StackTrace(sessionId, levels=20)`
- `Scopes(sessionId)`
- `Variables(sessionId, variablesReference)`

Use `Scopes` first to get `variablesReference` IDs for `Variables` calls.

## Evaluation & Mutation
Tools:
- `Evaluate(sessionId, expression)`
- `SetVariable(sessionId, name, value)`

`SetVariable` requires adapter capability `supportsSetVariable`. Expression evaluation runs in the top stack frame.

## Events & Output
Tools:
- `FetchEvents(sessionId, sinceSeq=0)` returns buffered events with sequence numbers.

Event categories (planned): `stdout`, `stderr`, `stopped`, `continued`, `exited`. Current implementation provides a generic buffer; future versions will categorize more explicitly.

## Lifecycle
Tools:
- `ListSessions()` lists active sessions.
- `TerminateSession(sessionId)` stops and disposes the session.
- `CleanupIdleSessions(olderThanMinutes=30)` removes idle or terminated sessions.

## Error Format
All tools return structured errors:
```
{
  "error": {
    "code": "SESSION_NOT_FOUND",
    "message": "Session 'abc' not found"
  }
}
```
Common error codes:
- `SESSION_NOT_FOUND`
- `PROGRAM_NOT_FOUND`
- `ADAPTER_NOT_FOUND`
- `THREAD_NOT_FOUND`
- `FEATURE_UNSUPPORTED`
- `REQUEST_TIMEOUT`
- `EVALUATE_FAILED`
- `SET_VARIABLE_FAILED`

## Test Strategy
YAML tests exercise build + placeholder flows until full command integration:
- `debugging-basic.yaml` builds test program
- Future tests will: start session, set breakpoint, continue, inspect stack, mutate variables, evaluate expressions, terminate session.

## Roadmap
Phase 4 & 5 (future enhancements):
- Conditional breakpoints
- Pause & attach support
- Rich event categorization
- Idle timeout enforcement via configuration
- Expanded variable child navigation

## Usage Guidance for Agents
1. Build target project assembly before starting a session.
2. Start session with `StartDebugSession` capturing `sessionId`.
3. Set one or more breakpoints before `Continue` for deterministic stops.
4. On stop, call `StackTrace` then `Scopes` and `Variables`.
5. Use `Evaluate` to inspect expressions; `SetVariable` to mutate if supported.
6. Periodically fetch events with `FetchEvents` for adapter output.
7. Terminate or cleanup sessions to avoid resource leaks.

## Security Considerations
- Avoid running sessions on untrusted assemblies.
- Expression evaluation executes code within the debuggee—restrict usage if necessary.
- Environment variable injection will be added with whitelisting (future).

## Limitations (Current)
- No direct command line invocation wiring yet (function calling only).
- Conditional breakpoints, pause, attach not implemented.
- Event buffer not yet categorized by source (stdout vs stderr separation planned).

## Contributing
Follow code style guidelines; keep additions minimal and feature-focused. Include/update tests when implementing new capabilities. Document new tool inputs/outputs in this file.
