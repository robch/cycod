abstract public class CommandWithVariables : Command
{
    public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();
    public List<ForEachVariable> ForEachVariables { get; set; } = new List<ForEachVariable>();

    abstract public CommandWithVariables Clone();
}
