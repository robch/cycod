using System.Collections.Generic;
using System.Threading.Tasks;

class GitHubCopilotLoginCommand : Command
{
    public GitHubCopilotLoginCommand()
    {
    }

    override public bool IsEmpty()
    {
        return false;
    }

    override public string GetCommandName()
    {
        return "ghcp login";
    }

    public async Task<List<Task<int>>> ExecuteAsync(bool interactive)
    {
        ConsoleHelpers.WriteDebugLine("Initiating GitHub Copilot login...");
        
        var helper = new GitHubCopilotHelper();
        var githubToken = await helper.GetGitHubTokenAsync();
        helper.SaveGitHubTokenToConfig(githubToken);
        
        ConsoleHelpers.WriteLine("GitHub Copilot login successful! You can now use chatx with GitHub Copilot.", ConsoleColor.Green, overrideQuiet: true);
            
        return new List<Task<int>>() { Task.FromResult(0) };
    }
}