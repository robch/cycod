using System.Collections.Generic;
using System.Threading.Tasks;

class GitHubLoginCommand : Command
{
    public string? ConfigFileName { get; internal set; }
    public ConfigFileScope Scope { get; internal set; }

    public GitHubLoginCommand()
    {
        Scope = ConfigFileScope.Local;
        ConfigFileName = null;
    }

    override public bool IsEmpty()
    {
        return false;
    }

    override public string GetCommandName()
    {
        return "github login";
    }

    public async Task<int> ExecuteAsync(bool interactive)
    {
        ConsoleHelpers.WriteDebugLine("Initiating GitHub login...");
        
        var helper = new GitHubCopilotHelper();
        var githubToken = await helper.GetGitHubTokenAsync();
        helper.SaveGitHubTokenToConfig(githubToken, Scope, ConfigFileName);
        
        ConsoleHelpers.WriteLine("GitHub login successful! You can now use chatx with GitHub Copilot.", ConsoleColor.Green, overrideQuiet: true);
            
        return 0;
    }
}