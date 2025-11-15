#if false // Disabled legacy broken implementation; replaced by DebugLifecycleFunctions

using System.ComponentModel;
using System.Text.Json;
using Cycod.Debugging.Session;
using Cycod.Debugging.Dap;

namespace Cycod.Debugging.Tools;

public class DebugSessionLifecycleFunctions
{
    static readonly DebugSessionManager _manager = new();
    static readonly Dictionary<string, DebugEventBuffer> _buffers = new();
    static readonly object _lock = new();

    [Description("Lists active debug sessions.")]
    public string ListSessions()
    {
        var list = _manager.List().Select(s => new
        {
            s.sessionId,
            program = s.managed.Session.TargetProgram,
            createdAt = s.managed.CreatedAt,
            isRunning = s.managed.Session.IsRunning,
            isTerminated = s.managed.Session.IsTerminated
        }).ToArray();
        return JsonSerializer.Serialize(new { status = "ok", sessions = list });
    }

    [Description("Terminates a debug session and disposes resources.")]
    public string TerminateSession(string sessionId)
    {
        var managed = _manager.Get(sessionId);
        if (managed == null) return DebugErrorHelpers.Error("SESSION_NOT_FOUND", $"Session '{sessionId}' not found");
        try
        {
            // Attempt graceful terminate
            try { managed.Client.SendRequestAsync(DapProtocol.TerminateCommand).Wait(2000); } catch { }
            try { managed.Client.SendRequestAsync(DapProtocol.DisconnectCommand).Wait(2000); } catch { }
        }
        catch { }
        var removed = _manager.Remove(sessionId);
        lock (_lock) { _buffers.Remove(sessionId); }
        return JsonSerializer.Serialize(new { status = removed ? "ok" : "not_found", sessionId });
    }

    [Description("Cleans up idle sessions older than the specified minutes.")]
    public string CleanupIdleSessions(int olderThanMinutes = 30)
    {
        var cutoff = DateTime.UtcNow - TimeSpan.FromMinutes(olderThanMinutes);
        var removed = new List<string>();
        foreach (var entry in _manager.List())
        {
            if (entry.managed.LastActivity < cutoff || entry.managed.Session.IsTerminated)
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

        catch { }
        var removed = _manager.Remove(sessionId);
        lock (_lock) { _buffers.Remove(sessionId); }
        return JsonSerializer.Serialize(new { status = removed ? "ok" : "not_found", sessionId });
    }

    [Description("Fetches buffered debug events (adapter output etc.) since a sequence number.")]
    public string FetchEvents(string sessionId, int sinceSeq = 0)
    {
        var managed = _manager.Get(sessionId);
        if (managed == null) return DebugErrorHelpers.Error("SESSION_NOT_FOUND", $"Session '{sessionId}' not found");
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
        return JsonSerializer.Serialize(new { status = "ok", events, nextSeq = events.Length > 0 ? events.Max(e => e.Seq) + 1 : sinceSeq });
    }
}

#endif
