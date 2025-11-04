using System.Text.Json;
using Cycod.Debugging.Dap;
using Cycod.Debugging.Session;

namespace Cycod.Debugging.Tools;

public class DebugStartTool
{
    readonly IDebugSessionManager _manager;

    public DebugStartTool(IDebugSessionManager manager) => _manager = manager;

    public string Invoke(string jsonPayload)
    {
        DebugStartRequest? req;
        try { req = JsonSerializer.Deserialize<DebugStartRequest>(jsonPayload); }
        catch (Exception ex) { return SerializeError("INVALID_REQUEST", $"Invalid JSON: {ex.Message}"); }
        if (req == null) return SerializeError("INVALID_REQUEST", "Empty request");
        if (string.IsNullOrWhiteSpace(req.ProgramPath)) return SerializeError("PROGRAM_NOT_FOUND", "programPath required");
        var fullPath = Path.GetFullPath(req.ProgramPath);
        if (!File.Exists(fullPath)) return SerializeError("PROGRAM_NOT_FOUND", $"File not found: {fullPath}");

        string adapterPath;
        try { adapterPath = NetcoredbgLocator.FindNetcoredbg(); }
        catch (FileNotFoundException ex) { return SerializeError("ADAPTER_NOT_FOUND", ex.Message.Trim()); }

        var client = new DapClient(adapterPath);
        var session = new DebugSession { TargetProgram = fullPath };
        client.EventReceived += (_, evt) =>
        {
            if (evt.EventType == DapProtocol.StoppedEvent)
            {
                session.IsRunning = false;
                var bodyJson = evt.Body?.ToString();
                if (bodyJson != null)
                {
                    try
                    {
                        var body = JsonSerializer.Deserialize<StoppedEventBody>(bodyJson);
                        session.CurrentThreadId = body?.ThreadId;
                    }
                    catch { }
                }
            }
            else if (evt.EventType == DapProtocol.ContinuedEvent) session.IsRunning = true;
            else if (evt.EventType == DapProtocol.ExitedEvent) session.IsTerminated = true;
        };

        try
        {
            var initResp = client.SendRequestAsync(DapProtocol.InitializeCommand, new InitializeRequestArguments()).Result;
            if (!initResp.Success) return SerializeError("INITIALIZE_FAILED", initResp.Message ?? "Initialize failed");
            if (initResp.Body != null)
            {
                try { session.Capabilities = JsonSerializer.Deserialize<Capabilities>(initResp.Body.ToString()!); } catch { }
            }
            session.IsInitialized = true;
            var launchResp = client.SendRequestAsync(DapProtocol.LaunchCommand, new LaunchRequestArguments { Program = fullPath, Cwd = Path.GetDirectoryName(fullPath), StopAtEntry = req.StopAtEntry }).Result;
            if (!launchResp.Success) return SerializeError("LAUNCH_FAILED", launchResp.Message ?? "Launch failed");
            session.IsLaunched = true;
        }
        catch (TimeoutException tex) { return SerializeError("REQUEST_TIMEOUT", tex.Message); }
        catch (Exception ex) { return SerializeError("INTERNAL_ERROR", ex.Message); }

        var sessionId = _manager.CreateSession(fullPath, client, session);
        var resp = new DebugStartResponse
        {
            SessionId = sessionId,
            Capabilities = new
            {
                session.Capabilities?.SupportsSetVariable,
                session.Capabilities?.SupportsConditionalBreakpoints,
                session.Capabilities?.SupportsEvaluateForHovers
            },
            State = new { session.IsRunning, session.IsLaunched, session.TargetProgram }
        };
        return JsonSerializer.Serialize(resp);
    }

    static string SerializeError(string code, string message) => JsonSerializer.Serialize(new ErrorResponse { Error = new ErrorBody { Code = code, Message = message } });
}
