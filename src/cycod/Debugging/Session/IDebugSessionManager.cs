using Cycod.Debugging.Dap;

namespace Cycod.Debugging.Session;

public interface IDebugSessionManager
{
    string CreateSession(string targetProgram, DapClient client, DebugSession session);
    ManagedDebugSession? Get(string sessionId);
    IEnumerable<(string sessionId, ManagedDebugSession managed)> List();
    bool Remove(string sessionId);
}

public class ManagedDebugSession
{
    public required DapClient Client { get; init; }
    public required DebugSession Session { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required string AdapterPath { get; init; }
    public DateTime LastActivity { get; set; }
}
