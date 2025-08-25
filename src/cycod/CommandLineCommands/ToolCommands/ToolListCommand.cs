using System;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Command to list available tools.
/// </summary>
class ToolListCommand : ToolBaseCommand
{
    /// <summary>
    /// Constructor initializes the base command.
    /// </summary>
    public ToolListCommand() : base()
    {
        Scope = ConfigFileScope.Any;
    }

    /// <summary>
    /// Gets the command name.
    /// </summary>
    /// <returns>The command name.</returns>
    public override string GetCommandName()
    {
        return "tool list";
    }

    /// <summary>
    /// Execute the list command.
    /// </summary>
    /// <param name="interactive">Whether the command is running in interactive mode.</param>
    /// <returns>Exit code, 0 for success, non-zero for failure.</returns>
    public override async Task<object> ExecuteAsync(bool interactive)
    {
        return await Task.Run(() => ExecuteList(Scope ?? ConfigFileScope.Any));
    }
    
    /// <summary>
    /// Execute the list command for the specified scope.
    /// </summary>
    /// <param name="scope">The scope to list tools for.</param>
    /// <returns>Exit code, 0 for success.</returns>
    private int ExecuteList(ConfigFileScope scope)
    {
        var isAnyScope = scope == ConfigFileScope.Any;
        
        if (isAnyScope || scope == ConfigFileScope.Global)
        {
            ToolDisplayHelpers.DisplayTools(ConfigFileScope.Global);
            if (isAnyScope) ConsoleHelpers.WriteLine(overrideQuiet: true);
        }
        
        if (isAnyScope || scope == ConfigFileScope.User)
        {
            ToolDisplayHelpers.DisplayTools(ConfigFileScope.User);
            if (isAnyScope) ConsoleHelpers.WriteLine(overrideQuiet: true);
        }
        
        if (isAnyScope || scope == ConfigFileScope.Local)
        {
            ToolDisplayHelpers.DisplayTools(ConfigFileScope.Local);
        }
        
        return 0;
    }
}