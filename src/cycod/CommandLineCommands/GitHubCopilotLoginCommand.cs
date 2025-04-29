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

    public override bool IsEmpty()
    {
        return false;
    }

    public override string GetCommandName()
    {
        return "github login";
    }

    public override async Task<object> ExecuteAsync(bool interactive)
    {
        ConsoleHelpers.WriteDebugLine("Initiating GitHub login...");
        
        var helper = new GitHubCopilotHelper();
        var githubToken = await helper.GetGitHubTokenAsync();
        helper.SaveGitHubTokenToConfig(githubToken, Scope, ConfigFileName);
        
        ConsoleHelpers.WriteLine("GitHub login successful! You can now use chatx with GitHub Copilot.", ConsoleColor.Green, overrideQuiet: true);
            
        return 0;
    }
}