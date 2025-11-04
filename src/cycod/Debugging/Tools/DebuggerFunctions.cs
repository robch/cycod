using System.ComponentModel;
using System.Text.Json;
using Cycod.Debugging.Dap;
using Cycod.Debugging.Session;

namespace Cycod.Debugging.Tools;

public class DebuggerFunctions
{
    static readonly DebugSessionManager _manager = new();

    [Description("Starts a .NET debug session for the specified program.")]
    public string StartDebugSession(string programPath, bool stopAtEntry = false)
    {
        if (string.IsNullOrWhiteSpace(programPath)) return Err("PROGRAM_NOT_FOUND", "programPath required");
        var fullPath = Path.GetFullPath(programPath);
        if (!File.Exists(fullPath)) return Err("PROGRAM_NOT_FOUND", $"File not found: {fullPath}");
        string adapterPath;
        try { adapterPath = NetcoredbgLocator.FindNetcoredbg(); }
        catch (FileNotFoundException ex) { return Err("ADAPTER_NOT_FOUND", ex.Message.Trim()); }
        var client = new DapClient(adapterPath);
        var session = new DebugSession { TargetProgram = fullPath };
        client.EventReceived += (_, evt) =>
        {
            if (evt.EventType == DapProtocol.StoppedEvent) session.IsRunning = false;
            else if (evt.EventType == DapProtocol.ContinuedEvent) session.IsRunning = true;
            else if (evt.EventType == DapProtocol.ExitedEvent) session.IsTerminated = true;
        };
        try
        {
            var init = client.SendRequestAsync(DapProtocol.InitializeCommand, new InitializeRequestArguments()).Result;
            if (!init.Success) return Err("INITIALIZE_FAILED", init.Message ?? "init failed");
            if (init.Body != null) { try { session.Capabilities = JsonSerializer.Deserialize<Capabilities>(init.Body.ToString()!); } catch { } }
            session.IsInitialized = true;
            var launch = client.SendRequestAsync(DapProtocol.LaunchCommand, new LaunchRequestArguments { Program = fullPath, Cwd = Path.GetDirectoryName(fullPath), StopAtEntry = stopAtEntry }).Result;
            if (!launch.Success) return Err("LAUNCH_FAILED", launch.Message ?? "launch failed");
            session.IsLaunched = true;
        }
        catch (TimeoutException tex) { return Err("REQUEST_TIMEOUT", tex.Message); }
        catch (Exception ex) { return Err("INTERNAL_ERROR", ex.Message); }
        var id = _manager.CreateSession(fullPath, client, session);
        return JsonSerializer.Serialize(new { sessionId = id, capabilities = new { session.Capabilities?.SupportsSetVariable, session.Capabilities?.SupportsConditionalBreakpoints }, state = new { session.IsRunning, session.IsLaunched, session.TargetProgram } });
    }

    [Description("Sets a breakpoint.")]
    public string SetBreakpoint(string sessionId, string filePath, int line)
    {
        var managed = GetManaged(sessionId); if (managed == null) return managedError;
        filePath = Path.GetFullPath(filePath); if (!File.Exists(filePath)) return Err("FILE_NOT_FOUND", filePath);
        managed.Session.AddBreakpoint(filePath, line);
        return SyncAndOk(managed.Client, filePath, managed.Session, new { file = filePath, line });
    }

    [Description("Sets a conditional breakpoint.")]
    public string SetConditionalBreakpoint(string sessionId, string filePath, int line, string condition)
    {
        var managed = GetManaged(sessionId); if (managed == null) return managedError;
        filePath = Path.GetFullPath(filePath); if (!File.Exists(filePath)) return Err("FILE_NOT_FOUND", filePath);
        managed.Session.AddBreakpoint(filePath, line, condition);
        return SyncAndOk(managed.Client, filePath, managed.Session, new { file = filePath, line, condition });
    }

    [Description("Deletes a breakpoint.")]
    public string DeleteBreakpoint(string sessionId, string filePath, int line)
    {
        var managed = GetManaged(sessionId); if (managed == null) return managedError;
        filePath = Path.GetFullPath(filePath);
        var removed = managed.Session.RemoveBreakpoint(filePath, line);
        if (!removed) return Err("BREAKPOINT_NOT_FOUND", $"No breakpoint at {filePath}:{line}");
        return SyncAndOk(managed.Client, filePath, managed.Session, new { file = filePath, line, removed });
    }

    [Description("Lists breakpoints for session.")]
    public string ListBreakpoints(string sessionId)
    {
        var managed = GetManaged(sessionId); if (managed == null) return managedError;
        var data = managed.Session.Breakpoints.ToDictionary(k => k.Key, v => v.Value.ToArray());
        return JsonSerializer.Serialize(new { status = "ok", sessionId, breakpoints = data });
    }

    [Description("Continue execution.")]
    public string Continue(string sessionId) => StepOrContinue(sessionId, DapProtocol.ContinueCommand, "CONTINUE_FAILED");
    [Description("Step over.")] public string StepOver(string sessionId) => StepOrContinue(sessionId, DapProtocol.NextCommand, "STEP_OVER_FAILED");
    [Description("Step in.")] public string StepIn(string sessionId) => StepOrContinue(sessionId, DapProtocol.StepInCommand, "STEP_IN_FAILED");
    [Description("Step out.")] public string StepOut(string sessionId) => StepOrContinue(sessionId, DapProtocol.StepOutCommand, "STEP_OUT_FAILED");

    [Description("Stack trace.")]
    public string StackTrace(string sessionId, int levels = 20)
    {
        var managed = GetManaged(sessionId); if (managed == null) return managedError;
        var threadId = DetectThread(managed); if (threadId == null) return Err("THREAD_NOT_FOUND", "No thread");
        try
        {
            var resp = managed.Client.SendRequestAsync(DapProtocol.StackTraceCommand, new StackTraceArguments { ThreadId = threadId.Value, Levels = levels }).Result;
            if (!resp.Success || resp.Body == null) return Err("STACKTRACE_FAILED", resp.Message ?? "failed");
            var body = JsonSerializer.Deserialize<StackTraceResponseBody>(resp.Body.ToString()!);
            var frames = body?.StackFrames.Select(f => new { f.Id, f.Name, f.Line, path = f.Source?.Path }).ToArray() ?? Array.Empty<object>();
            return JsonSerializer.Serialize(new { status = "ok", frames });
        }
        catch (Exception ex) { return Err("STACKTRACE_FAILED", ex.Message); }
    }

    [Description("Scopes for top frame.")]
    public string Scopes(string sessionId)
    {
        var managed = GetManaged(sessionId); if (managed == null) return managedError;
        var threadId = DetectThread(managed); if (threadId == null) return Err("THREAD_NOT_FOUND", "No thread");
        try
        {
            var stackResp = managed.Client.SendRequestAsync(DapProtocol.StackTraceCommand, new StackTraceArguments { ThreadId = threadId.Value, Levels = 1 }).Result;
            if (!stackResp.Success || stackResp.Body == null) return Err("SCOPES_FAILED", stackResp.Message ?? "stack failed");
            var stackBody = JsonSerializer.Deserialize<StackTraceResponseBody>(stackResp.Body.ToString()!);
            var frameId = stackBody?.StackFrames.FirstOrDefault()?.Id ?? -1;
            if (frameId < 0) return Err("SCOPES_FAILED", "No frame");
            var scopesResp = managed.Client.SendRequestAsync(DapProtocol.ScopesCommand, new ScopesArguments { FrameId = frameId }).Result;
            if (!scopesResp.Success || scopesResp.Body == null) return Err("SCOPES_FAILED", scopesResp.Message ?? "scopes failed");
            var scopesBody = JsonSerializer.Deserialize<ScopesResponseBody>(scopesResp.Body.ToString()!);
            var scopes = scopesBody?.Scopes.Select(s => new { s.Name, s.VariablesReference }).ToArray() ?? Array.Empty<object>();
            return JsonSerializer.Serialize(new { status = "ok", scopes });
        }
        catch (Exception ex) { return Err("SCOPES_FAILED", ex.Message); }
    }

    [Description("Variables for a scope reference.")]
    public string Variables(string sessionId, int variablesReference)
    {
        var managed = GetManaged(sessionId); if (managed == null) return managedError;
        try
        {
            var resp = managed.Client.SendRequestAsync(DapProtocol.VariablesCommand, new VariablesArguments { VariablesReference = variablesReference }).Result;
            if (!resp.Success || resp.Body == null) return Err("VARIABLES_FAILED", resp.Message ?? "vars failed");
            var body = JsonSerializer.Deserialize<VariablesResponseBody>(resp.Body.ToString()!);
            var vars = body?.Variables.Select(v => new { v.Name, v.Value, v.Type, v.VariablesReference }).ToArray() ?? Array.Empty<object>();
            return JsonSerializer.Serialize(new { status = "ok", variables = vars });
        }
        catch (Exception ex) { return Err("VARIABLES_FAILED", ex.Message); }
    }

    [Description("Evaluate expression.")]
    public string Evaluate(string sessionId, string expression)
    {
        var managed = GetManaged(sessionId); if (managed == null) return managedError;
        var threadId = DetectThread(managed); if (threadId == null) return Err("THREAD_NOT_FOUND", "No thread");
        try
        {
            var stackResp = managed.Client.SendRequestAsync(DapProtocol.StackTraceCommand, new StackTraceArguments { ThreadId = threadId.Value, Levels = 1 }).Result;
            if (!stackResp.Success || stackResp.Body == null) return Err("EVALUATE_FAILED", stackResp.Message ?? "stack failed");
            var stackBody = JsonSerializer.Deserialize<StackTraceResponseBody>(stackResp.Body.ToString()!);
            var frameId = stackBody?.StackFrames.FirstOrDefault()?.Id ?? -1;
            var resp = managed.Client.SendRequestAsync(DapProtocol.EvaluateCommand, new EvaluateArguments { Expression = expression, FrameId = frameId }).Result;
            if (!resp.Success || resp.Body == null) return Err("EVALUATE_FAILED", resp.Message ?? "eval failed");
            var evalBody = JsonSerializer.Deserialize<EvaluateResponseBody>(resp.Body.ToString()!);
            return JsonSerializer.Serialize(new { status = "ok", result = evalBody?.Result, evalBody?.Type });
        }
        catch (Exception ex) { return Err("EVALUATE_FAILED", ex.Message); }
    }

    [Description("Set variable value (first scope).")]
    public string SetVariable(string sessionId, string name, string value)
    {
        var managed = GetManaged(sessionId); if (managed == null) return managedError;
        if (managed.Session.Capabilities?.SupportsSetVariable != true) return Err("FEATURE_UNSUPPORTED", "SetVariable not supported");
        var threadId = DetectThread(managed); if (threadId == null) return Err("THREAD_NOT_FOUND", "No thread");
        try
        {
            var stackResp = managed.Client.SendRequestAsync(DapProtocol.StackTraceCommand, new StackTraceArguments { ThreadId = threadId.Value, Levels = 1 }).Result;
            if (!stackResp.Success || stackResp.Body == null) return Err("SET_VARIABLE_FAILED", stackResp.Message ?? "stack failed");
            var stackBody = JsonSerializer.Deserialize<StackTraceResponseBody>(stackResp.Body.ToString()!);
            var frameId = stackBody?.StackFrames.FirstOrDefault()?.Id ?? -1;
            var scopesResp = managed.Client.SendRequestAsync(DapProtocol.ScopesCommand, new ScopesArguments { FrameId = frameId }).Result;
            if (!scopesResp.Success || scopesResp.Body == null) return Err("SET_VARIABLE_FAILED", scopesResp.Message ?? "scopes failed");
            var scopesBody = JsonSerializer.Deserialize<ScopesResponseBody>(scopesResp.Body.ToString()!);
            var firstScope = scopesBody?.Scopes.FirstOrDefault();
            if (firstScope == null) return Err("SET_VARIABLE_FAILED", "No scope");
            var setResp = managed.Client.SendRequestAsync(DapProtocol.SetVariableCommand, new SetVariableArguments { VariablesReference = firstScope.VariablesReference, Name = name, Value = value }).Result;
            if (!setResp.Success || setResp.Body == null) return Err("SET_VARIABLE_FAILED", setResp.Message ?? "failed");
            return JsonSerializer.Serialize(new { status = "ok", name, value });
        }
        catch (Exception ex) { return Err("SET_VARIABLE_FAILED", ex.Message); }
    }

    ManagedDebugSession? GetManaged(string sessionId)
    {
        var m = _manager.Get(sessionId); if (m == null) managedError = Err("SESSION_NOT_FOUND", $"Session '{sessionId}' not found"); return m;
    }
    string managedError = string.Empty;

    string StepOrContinue(string sessionId, string command, string errorCode)
    {
        var managed = GetManaged(sessionId); if (managed == null) return managedError;
        var threadId = DetectThread(managed); if (threadId == null) return Err("THREAD_NOT_FOUND", "No thread");
        try
        {
            managed.Client.SendRequestAsync(command, new ContinueArguments { ThreadId = threadId.Value }).Wait();
            return JsonSerializer.Serialize(new { status = "ok" });
        }
        catch (Exception ex) { return Err(errorCode, ex.Message); }
    }

    int? DetectThread(ManagedDebugSession managed)
    {
        if (managed.Session.CurrentThreadId.HasValue) return managed.Session.CurrentThreadId;
        try
        {
            var resp = managed.Client.SendRequestAsync(DapProtocol.ThreadsCommand).Result;
            if (resp.Success && resp.Body != null)
            {
                var body = JsonSerializer.Deserialize<ThreadsResponseBody>(resp.Body.ToString()!);
                var first = body?.Threads.FirstOrDefault();
                if (first != null) managed.Session.CurrentThreadId = first.Id;
            }
        }
        catch { }
        return managed.Session.CurrentThreadId;
    }

    string SyncAndOk(DapClient client, string filePath, DebugSession session, object payload)
    {
        if (!session.Breakpoints.TryGetValue(filePath, out var lines)) lines = new List<int>();
        client.SendRequestAsync(DapProtocol.SetBreakpointsCommand, new SetBreakpointsArguments
        {
            Source = new Source { Path = filePath },
            Breakpoints = lines.Select(l => new SourceBreakpoint { Line = l, Condition = session.BreakpointConditions.TryGetValue((filePath, l), out var cond) ? cond : null }).ToArray()
        }).Wait();
        return JsonSerializer.Serialize(new { status = "ok", payload });
    }

    static string Err(string code, string message) => JsonSerializer.Serialize(new { error = new { code, message } });
}
