namespace Cycod.Debugging.Tools;

public class DebugEventBuffer
{
    readonly int _max;
    readonly List<DebugEventItem> _items = new();
    readonly object _lock = new();
    int _nextSeq;

    public DebugEventBuffer(int max = 1000) => _max = max;

    public void Add(string category, string text)
    {
        lock (_lock)
        {
            _items.Add(new DebugEventItem { Seq = _nextSeq++, TimeUtc = DateTime.UtcNow, Category = category, Text = text });
            if (_items.Count > _max) _items.RemoveAt(0);
        }
    }

    public IReadOnlyList<DebugEventItem> GetSince(int sinceSeq)
    {
        lock (_lock)
        {
            return _items.Where(i => i.Seq >= sinceSeq).ToList();
        }
    }
}

public class DebugEventItem
{
    public int Seq { get; set; }
    public DateTime TimeUtc { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}
