using Cycod.Debugging.Dap;

namespace Cycod.Debugging.Session;

public class DebugSessionManager : IDebugSessionManager
{
    readonly Dictionary<string, ManagedDebugSession> _sessions = new();
    readonly object _lock = new();

    public string CreateSession(string targetProgram, DapClient client, DebugSession session)
    {
        var id = Guid.NewGuid().ToString("N");
        lock (_lock)
        {
            _sessions[id] = new ManagedDebugSession
            {
                Client = client,
                Session = session,
                CreatedAt = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow,
                AdapterPath = client.AdapterPath
            };
        }
        return id;
    }

    public ManagedDebugSession? Get(string sessionId)
    {
        lock (_lock)
        {
            return _sessions.TryGetValue(sessionId, out var s) ? s : null;
        }
    }

    public IEnumerable<(string sessionId, ManagedDebugSession managed)> List()
    {
        lock (_lock)
        {
            foreach (var kvp in _sessions) yield return (kvp.Key, kvp.Value);
        }
    }

    public bool Remove(string sessionId)
    {
        ManagedDebugSession? toDispose = null;
        lock (_lock)
        {
            if (_sessions.TryGetValue(sessionId, out var s))
            {
                toDispose = s;
                _sessions.Remove(sessionId);
            }
        }
        if (toDispose != null)
        {
            try { toDispose.Client.Dispose(); } catch { /* swallow */ }
            return true;
        }
        return false;
    }
}
