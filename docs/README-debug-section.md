## Debugging (.NET DAP) Preview

cycod now includes a preview feature for debugging .NET assemblies using the Debug Adapter Protocol (DAP) via `netcoredbg`.

### Quick Start
1. Build your target program (DLL) using `dotnet build`.
2. Use the function calling tool `StartDebugSession(programPath)` to begin a session.
3. Set breakpoints with `SetBreakpoint(sessionId, filePath, line)`.
4. Run the program using `Continue(sessionId)`.
5. When a breakpoint hits, inspect with `StackTrace`, `Scopes`, `Variables`.
6. Evaluate expressions via `Evaluate(sessionId, "x + y")`.
7. Mutate state using `SetVariable(sessionId, "x", "42")` (if supported).
8. Fetch events using `FetchEvents(sessionId, sinceSeq)` to stream output.
9. Terminate with `TerminateSession(sessionId)` or clean up stale sessions using `CleanupIdleSessions`.

### Error Handling
All tool responses on failure include a JSON error block:
```
{"error":{"code":"SESSION_NOT_FOUND","message":"Session 'abc' not found"}}
```

### Current Limitations
- No direct CLI command wrappers yet (use function calling interface).
- Conditional breakpoints, pause, attach not implemented.
- Event categories minimal; future versions will separate stdout/stderr and structured stop reasons.

### Roadmap
- Expand lifecycle & attach capabilities
- Conditional breakpoint support
- Enhanced variable object expansion
- Configuration-driven idle session cleanup defaults

See `docs/debug-tools.md` for complete API reference.
