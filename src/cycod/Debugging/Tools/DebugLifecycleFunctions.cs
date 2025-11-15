using System.ComponentModel;
using System.Text.Json;
using Cycod.Debugging.Dap;
using Cycod.Debugging.Session;

namespace Cycod.Debugging.Tools;

public class DebugLifecycleFunctions
{
    static readonly DebugSessionManager _manager = new();
    static readonly Dictionary<string, DebugEventBuffer> _buffers = new();
    static readonly object _lock = new();

    [Description("List active debug sessions.")]
    public string ListSessions()
    {
        var list = _manager.List().Select(s => new
        {
            s.sessionId,
            program = s.managed.Session.TargetProgram,
            isRunning = s.managed.Session.IsRunning,
            isTerminated = s.managed.Session.IsTerminated,
            createdAt = s.managed.CreatedAt
        }).ToArray();
        return JsonSerializer.Serialize(new { status = "ok", sessions = list });
    }

    [Description("Terminate a debug session.")]
    public string TerminateSession(string sessionId)
    {
        var managed = _manager.Get(sessionId);
        if (managed == null) return Err("SESSION_NOT_FOUND", $"Session '{sessionId}' not found");
        try { managed.Client.SendRequestAsync(DapProtocol.TerminateCommand).Wait(1000); } catch { }
        try { managed.Client.SendRequestAsync(DapProtocol.DisconnectCommand).Wait(1000); } catch { }
        _manager.Remove(sessionId);
        lock (_lock) { _buffers.Remove(sessionId); }
        return JsonSerializer.Serialize(new { status = "ok", sessionId });
    }

    [Description("Cleanup idle or terminated sessions older than minutes.")]
    public string CleanupIdleSessions(int olderThanMinutes = 30)
    {
        var cutoff = DateTime.UtcNow - TimeSpan.FromMinutes(olderThanMinutes);
        var removed = new List<string>();
        foreach (var entry in _manager.List())
        {
            if (entry.managed.Session.IsTerminated || entry.managed.LastActivity < cutoff)
            {
                if (_manager.Remove(entry.sessionId))
                {
                    lock (_lock) { _buffers.Remove(entry.sessionId); }
                    removed.Add(entry.sessionId);
                }
            }
        }
        return JsonSerializer.Serialize(new { status = "ok", removed });
    }

    [Description("Fetch buffered events since sequence.")]
    public string FetchEvents(string sessionId, int sinceSeq = 0)
    {
        var managed = _manager.Get(sessionId);
        if (managed == null) return Err("SESSION_NOT_FOUND", $"Session '{sessionId}' not found");
        DebugEventBuffer buffer;
        lock (_lock)
        {
            if (!_buffers.TryGetValue(sessionId, out buffer!))
            {
                buffer = new DebugEventBuffer(2000);
                _buffers[sessionId] = buffer;
            }
        }
        var events = buffer.GetSince(sinceSeq).Select(e => new { e.Seq, e.TimeUtc, e.Category, e.Text }).ToArray();
        var nextSeq = events.Length > 0 ? events.Max(e => e.Seq) + 1 : sinceSeq;
        return JsonSerializer.Serialize(new { status = "ok", events, nextSeq });
    }

    static string Err(string code, string message) => JsonSerializer.Serialize(new { error = new { code, message } });
}
