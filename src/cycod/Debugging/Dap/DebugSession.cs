namespace Cycod.Debugging.Dap;

public class DebugSession
{
    public bool IsInitialized { get; set; }
    public bool IsRunning { get; set; }
    public bool IsLaunched { get; set; }
    public bool IsConfigured { get; set; }
    public bool IsTerminated { get; set; }
    public int? CurrentThreadId { get; set; }
    public string? TargetProgram { get; set; }
    public Dictionary<string, List<int>> Breakpoints { get; } = new();
    public Dictionary<(string file,int line), string> BreakpointConditions { get; } = new();
    public Capabilities? Capabilities { get; set; }

    public void AddBreakpoint(string filePath, int line, string? condition = null)
    {
        var normalized = Path.GetFullPath(filePath);
        if (!Breakpoints.ContainsKey(normalized)) Breakpoints[normalized] = new List<int>();
        if (!Breakpoints[normalized].Contains(line)) Breakpoints[normalized].Add(line);
        if (!string.IsNullOrWhiteSpace(condition)) BreakpointConditions[(normalized,line)] = condition;
    }

    public bool RemoveBreakpoint(string filePath, int line)
    {
        var normalized = Path.GetFullPath(filePath);
        return Breakpoints.TryGetValue(normalized, out var lines) && lines.Remove(line);
    }

    public List<int> GetBreakpoints(string filePath)
    {
        var normalized = Path.GetFullPath(filePath);
        return Breakpoints.TryGetValue(normalized, out var lines) ? new List<int>(lines) : new List<int>();
    }

    public void ClearAllBreakpoints() => Breakpoints.Clear();
}
