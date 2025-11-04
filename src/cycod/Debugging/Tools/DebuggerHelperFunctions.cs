using System.ComponentModel;
using System.Text.Json;
using Cycod.Debugging.Dap;
using Cycod.Debugging.Session;

namespace Cycod.Debugging.Tools;

public class DebuggerHelperFunctions
{
    static readonly DebugSessionManager _manager = new();
    static readonly object _lock = new();

    [Description("Starts a .NET debug session for the specified program.")]
    public string StartDebugSession(
        [Description("Path to the target program DLL")] string programPath,
        [Description("Stop at entry point")] bool stopAtEntry = false)
    {
        var payload = JsonSerializer.Serialize(new DebugStartRequest { ProgramPath = programPath, StopAtEntry = stopAtEntry });
        var tool = new DebugStartTool(_manager);
        return tool.Invoke(payload);
    }

    [Description("Sets a breakpoint at file:line for an existing session.")]
    public string SetBreakpoint(
        [Description("Session Id returned from StartDebugSession")] string sessionId,
        [Description("Source file path")] string filePath,
        [Description("Line number (1-based)")] int line)
    {
        var managed = _manager.Get(sessionId);
        if (managed == null) return ErrorJson("SESSION_NOT_FOUND", $"Session '{sessionId}' not found");
        filePath = Path.GetFullPath(filePath);
        if (!File.Exists(filePath)) return DebugErrorHelpers.Error("FILE_NOT_FOUND", filePath);
        managed.Session.AddBreakpoint(filePath, line);
        try
        {
            SyncBreakpointsForFile(managed.Client, filePath, managed.Session);
            return JsonSerializer.Serialize(new { status = "ok", file = filePath, line });
        }
        catch (Exception ex)
        {
            return ErrorJson("SET_BREAKPOINT_FAILED", ex.Message);
        }
    }

    [Description("Deletes a breakpoint at file:line for an existing session.")]
    public string DeleteBreakpoint(
        [Description("Session Id returned from StartDebugSession")] string sessionId,
        [Description("Source file path")] string filePath,
        [Description("Line number (1-based)")] int line)
    {
        var managed = _manager.Get(sessionId);
        if (managed == null) return ErrorJson("SESSION_NOT_FOUND", $"Session '{sessionId}' not found");
        filePath = Path.GetFullPath(filePath);
        var removed = managed.Session.RemoveBreakpoint(filePath, line);
        if (!removed) return ErrorJson("BREAKPOINT_NOT_FOUND", $"No breakpoint at {filePath}:{line}");
        try
        {
            SyncBreakpointsForFile(managed.Client, filePath, managed.Session);
            return JsonSerializer.Serialize(new { status = "ok", removed = true, file = filePath, line });
        }
        catch (Exception ex)
        {
            return ErrorJson("DELETE_BREAKPOINT_FAILED", ex.Message);
        }
    }

    [Description("Lists breakpoints for a session.")]
    public string ListBreakpoints(
        [Description("Session Id returned from StartDebugSession")] string sessionId)
    {
    [Description("Sets a conditional breakpoint.")]
    public string SetConditionalBreakpoint(string sessionId, string filePath, int line, string condition)
    {
        var managed = _manager.Get(sessionId);
        if (managed == null) return ErrorJson("SESSION_NOT_FOUND", $"Session '{sessionId}' not found");
        filePath = Path.GetFullPath(filePath);
        if (!File.Exists(filePath)) return DebugErrorHelpers.Error("FILE_NOT_FOUND", filePath);
        managed.Session.AddBreakpoint(filePath, line, condition);
        try
        {
            SyncBreakpointsForFile(managed.Client, filePath, managed.Session);
            return JsonSerializer.Serialize(new { status = "ok", file = filePath, line, condition });
        }
        catch (Exception ex) { return ErrorJson("SET_BREAKPOINT_FAILED", ex.Message); }
    }

        var managed = _manager.Get(sessionId);
        if (managed == null) return ErrorJson("SESSION_NOT_FOUND", $"Session '{sessionId}' not found");
        var data = managed.Session.Breakpoints.ToDictionary(k => k.Key, v => v.Value.ToArray());
        return JsonSerializer.Serialize(new { sessionId, breakpoints = data });
    }

    static void SyncBreakpointsForFile(DapClient client, string filePath, DebugSession session)
    {
        if (!session.Breakpoints.TryGetValue(filePath, out var lines)) lines = new List<int>();
        client.SendRequestAsync(DapProtocol.SetBreakpointsCommand, new SetBreakpointsArguments
        {
            Source = new Source { Path = filePath },
            Breakpoints = lines.Select(l => new SourceBreakpoint { Line = l, Condition = session.BreakpointConditions.TryGetValue((filePath,l), out var cond) ? cond : null }).ToArray()
        }).Wait();
    }

    static string ErrorJson(string code, string message) => JsonSerializer.Serialize(new ErrorResponse { Error = new ErrorBody { Code = code, Message = message } });
}

    [Description("Continues execution of the debuggee.")]
    public string Continue(string sessionId)
    {
        var managed = _manager.Get(sessionId);
        if (managed == null) return ErrorJson("SESSION_NOT_FOUND", $"Session '{sessionId}' not found");
        var session = managed.Session;
        var client = managed.Client;
        var threadId = GetOrDetectThreadId(client, session);
        if (threadId == null) return ErrorJson("THREAD_NOT_FOUND", "No thread to continue");
        try
        {
            client.SendRequestAsync(DapProtocol.ContinueCommand, new ContinueArguments { ThreadId = threadId.Value }).Wait();
            session.IsRunning = true;
            return JsonSerializer.Serialize(new { status = "ok", running = true });
        }
        catch (Exception ex) { return ErrorJson("CONTINUE_FAILED", ex.Message); }
    }

    [Description("Steps over one line.")]
    public string StepOver(string sessionId) => StepInternal(sessionId, DapProtocol.NextCommand, "STEP_OVER_FAILED");

    [Description("Steps into the next function call.")]
    public string StepIn(string sessionId) => StepInternal(sessionId, DapProtocol.StepInCommand, "STEP_IN_FAILED");

    [Description("Steps out of the current function.")]
    public string StepOut(string sessionId) => StepInternal(sessionId, DapProtocol.StepOutCommand, "STEP_OUT_FAILED");

    string StepInternal(string sessionId, string command, string errorCode)
    {
        var managed = _manager.Get(sessionId);
        if (managed == null) return ErrorJson("SESSION_NOT_FOUND", $"Session '{sessionId}' not found");
        var session = managed.Session;
        var client = managed.Client;
        var threadId = GetOrDetectThreadId(client, session);
        if (threadId == null) return ErrorJson("THREAD_NOT_FOUND", "No thread for step");
        try
        {
            client.SendRequestAsync(command, new NextArguments { ThreadId = threadId.Value }).Wait();
            return JsonSerializer.Serialize(new { status = "ok" });
        }
        catch (Exception ex) { return ErrorJson(errorCode, ex.Message); }
    }

    int? GetOrDetectThreadId(DapClient client, DebugSession session)
    {
        if (session.CurrentThreadId.HasValue) return session.CurrentThreadId;
        try
        {
            var resp = client.SendRequestAsync(DapProtocol.ThreadsCommand).Result;
            if (resp.Success && resp.Body != null)
            {
                var body = JsonSerializer.Deserialize<ThreadsResponseBody>(resp.Body.ToString()!);
                var first = body?.Threads.FirstOrDefault();
                if (first != null) session.CurrentThreadId = first.Id;
            }
        }
        catch { }
        return session.CurrentThreadId;
    }

    [Description("Gets the current call stack (top frames).")]
    public string StackTrace(string sessionId, int levels = 20)
    {
        var managed = _manager.Get(sessionId);
        if (managed == null) return ErrorJson("SESSION_NOT_FOUND", $"Session '{sessionId}' not found");
        var threadId = GetOrDetectThreadId(managed.Client, managed.Session);
        if (threadId == null) return ErrorJson("THREAD_NOT_FOUND", "No thread for stackTrace");
        try
        {
            var resp = managed.Client.SendRequestAsync(DapProtocol.StackTraceCommand, new StackTraceArguments { ThreadId = threadId.Value, Levels = levels }).Result;
            if (!resp.Success || resp.Body == null) return ErrorJson("STACKTRACE_FAILED", resp.Message ?? "Failed");
            var body = JsonSerializer.Deserialize<StackTraceResponseBody>(resp.Body.ToString()!);
            var frames = body?.StackFrames.Select(f => new { f.Id, f.Name, f.Line, path = f.Source?.Path }).ToArray() ?? Array.Empty<object>();
            return JsonSerializer.Serialize(new { status = "ok", frames });
        }
        catch (Exception ex) { return ErrorJson("STACKTRACE_FAILED", ex.Message); }
    }

    [Description("Gets scopes for the top stack frame.")]
    public string Scopes(string sessionId)
    {
        var managed = _manager.Get(sessionId);
        if (managed == null) return ErrorJson("SESSION_NOT_FOUND", $"Session '{sessionId}' not found");
        var threadId = GetOrDetectThreadId(managed.Client, managed.Session);
        if (threadId == null) return ErrorJson("THREAD_NOT_FOUND", "No thread for scopes");
        try
        {
            var stackResp = managed.Client.SendRequestAsync(DapProtocol.StackTraceCommand, new StackTraceArguments { ThreadId = threadId.Value, Levels = 1 }).Result;
            if (!stackResp.Success || stackResp.Body == null) return ErrorJson("SCOPES_FAILED", stackResp.Message ?? "Stack failed");
            var stackBody = JsonSerializer.Deserialize<StackTraceResponseBody>(stackResp.Body.ToString()!);
            var frameId = stackBody?.StackFrames.FirstOrDefault()?.Id ?? -1;
            if (frameId < 0) return ErrorJson("SCOPES_FAILED", "No frame");
            var scopesResp = managed.Client.SendRequestAsync(DapProtocol.ScopesCommand, new ScopesArguments { FrameId = frameId }).Result;
            if (!scopesResp.Success || scopesResp.Body == null) return ErrorJson("SCOPES_FAILED", scopesResp.Message ?? "Scopes failed");
            var scopesBody = JsonSerializer.Deserialize<ScopesResponseBody>(scopesResp.Body.ToString()!);
            var scopes = scopesBody?.Scopes.Select(s => new { s.Name, s.VariablesReference }).ToArray() ?? Array.Empty<object>();
            return JsonSerializer.Serialize(new { status = "ok", scopes });
        }
        catch (Exception ex) { return ErrorJson("SCOPES_FAILED", ex.Message); }
    }

    [Description("Lists variables for a scope reference.")]
    public string Variables(string sessionId, int variablesReference)
    {
        var managed = _manager.Get(sessionId);
        if (managed == null) return ErrorJson("SESSION_NOT_FOUND", $"Session '{sessionId}' not found");
        try
        {
            var resp = managed.Client.SendRequestAsync(DapProtocol.VariablesCommand, new VariablesArguments { VariablesReference = variablesReference }).Result;
            if (!resp.Success || resp.Body == null) return ErrorJson("VARIABLES_FAILED", resp.Message ?? "Variables failed");
            var body = JsonSerializer.Deserialize<VariablesResponseBody>(resp.Body.ToString()!);
            var vars = body?.Variables.Select(v => new { v.Name, v.Value, v.Type, v.VariablesReference }).ToArray() ?? Array.Empty<object>();
            return JsonSerializer.Serialize(new { status = "ok", variables = vars });
        }
        catch (Exception ex) { return ErrorJson("VARIABLES_FAILED", ex.Message); }
    }

    [Description("Evaluates an expression in the current frame.")]
    public string Evaluate(string sessionId, string expression)
    {
        var managed = _manager.Get(sessionId);
        if (managed == null) return ErrorJson("SESSION_NOT_FOUND", $"Session '{sessionId}' not found");
        var threadId = GetOrDetectThreadId(managed.Client, managed.Session);
        if (threadId == null) return ErrorJson("THREAD_NOT_FOUND", "No thread for evaluate");
        try
        {
            var stackResp = managed.Client.SendRequestAsync(DapProtocol.StackTraceCommand, new StackTraceArguments { ThreadId = threadId.Value, Levels = 1 }).Result;
            if (!stackResp.Success || stackResp.Body == null) return ErrorJson("EVALUATE_FAILED", stackResp.Message ?? "Stack failed");
            var stackBody = JsonSerializer.Deserialize<StackTraceResponseBody>(stackResp.Body.ToString()!);
            var frameId = stackBody?.StackFrames.FirstOrDefault()?.Id ?? -1;
            var resp = managed.Client.SendRequestAsync(DapProtocol.EvaluateCommand, new EvaluateArguments { Expression = expression, FrameId = frameId }).Result;
            if (!resp.Success || resp.Body == null) return ErrorJson("EVALUATE_FAILED", resp.Message ?? "Evaluate failed");
            var evalBody = JsonSerializer.Deserialize<EvaluateResponseBody>(resp.Body.ToString()!);
            return JsonSerializer.Serialize(new { status = "ok", result = evalBody?.Result, evalBody?.Type });
        }
        catch (Exception ex) { return ErrorJson("EVALUATE_FAILED", ex.Message); }
    }

    [Description("Sets a variable's value in the first scope of current frame.")]
    public string SetVariable(string sessionId, string name, string value)
    {
        var managed = _manager.Get(sessionId);
        if (managed == null) return ErrorJson("SESSION_NOT_FOUND", $"Session '{sessionId}' not found");
        var session = managed.Session;
        if (session.Capabilities?.SupportsSetVariable != true) return ErrorJson("FEATURE_UNSUPPORTED", "SetVariable not supported");
        var threadId = GetOrDetectThreadId(managed.Client, session);
        if (threadId == null) return ErrorJson("THREAD_NOT_FOUND", "No thread for setVariable");
        try
        {
            var stackResp = managed.Client.SendRequestAsync(DapProtocol.StackTraceCommand, new StackTraceArguments { ThreadId = threadId.Value, Levels = 1 }).Result;
            if (!stackResp.Success || stackResp.Body == null) return ErrorJson("SET_VARIABLE_FAILED", stackResp.Message ?? "Stack failed");
            var stackBody = JsonSerializer.Deserialize<StackTraceResponseBody>(stackResp.Body.ToString()!);
            var frameId = stackBody?.StackFrames.FirstOrDefault()?.Id ?? -1;
            var scopesResp = managed.Client.SendRequestAsync(DapProtocol.ScopesCommand, new ScopesArguments { FrameId = frameId }).Result;
            if (!scopesResp.Success || scopesResp.Body == null) return ErrorJson("SET_VARIABLE_FAILED", scopesResp.Message ?? "Scopes failed");
            var scopesBody = JsonSerializer.Deserialize<ScopesResponseBody>(scopesResp.Body.ToString()!);
            var firstScope = scopesBody?.Scopes.FirstOrDefault();
            if (firstScope == null) return ErrorJson("SET_VARIABLE_FAILED", "No scope");
            var setResp = managed.Client.SendRequestAsync(DapProtocol.SetVariableCommand, new SetVariableArguments { VariablesReference = firstScope.VariablesReference, Name = name, Value = value }).Result;
            if (!setResp.Success || setResp.Body == null) return ErrorJson("SET_VARIABLE_FAILED", setResp.Message ?? "Failed");
            return JsonSerializer.Serialize(new { status = "ok", name, value });
        }
        catch (Exception ex) { return ErrorJson("SET_VARIABLE_FAILED", ex.Message); }
    }
