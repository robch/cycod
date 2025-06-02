public class StdioMcpServerConfig
{
    public string Command { get; set; } = string.Empty;
    public List<string> Args { get; set; } = new List<string>();
    public Dictionary<string, string?> Env { get; set; } = new Dictionary<string, string?>();
}
