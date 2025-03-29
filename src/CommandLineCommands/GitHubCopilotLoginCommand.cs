using System.Collections.Generic;
using System.Threading.Tasks;

class GitHubLoginCommand : Command
{
    public GitHubLoginCommand()
    {
    }

    override public bool IsEmpty()
    {
        return false;
    }

    override public string GetCommandName()
    {
        return "github login";
    }

    public async Task<List<Task<int>>> ExecuteAsync(bool interactive)
    {
        ConsoleHelpers.WriteDebugLine("Initiating GitHub login...");
        
        var helper = new GitHubCopilotHelper();
        var githubToken = await helper.GetGitHubTokenAsync();
        helper.SaveGitHubTokenToConfig(githubToken);
        
        ConsoleHelpers.WriteLine("GitHub login successful! You can now use chatx with GitHub Copilot.", ConsoleColor.Green, overrideQuiet: true);
            
        return new List<Task<int>>() { Task.FromResult(0) };
    }
}