using System;
using System.Threading.Tasks;

/// <summary>
/// Command to list available GitHub Copilot models.
/// </summary>
class GitHubModelsCommand : Command
{
    public GitHubModelsCommand()
    {
    }

    public override bool IsEmpty()
    {
        return false;
    }

    public override string GetCommandName()
    {
        return "github models";
    }

    public override Task<object> ExecuteAsync(bool interactive)
    {
        ConsoleHelpers.WriteDebugLine("Fetching GitHub Copilot models...");
        
        try
        {
            // Get GitHub token from configuration or environment
            var githubToken = ConfigStore.Instance.GetFromAnyScope("GitHub.Token").AsString();
            
            if (string.IsNullOrEmpty(githubToken))
            {
                githubToken = EnvironmentHelpers.FindEnvVar("GITHUB_TOKEN");
            }
            
            if (string.IsNullOrEmpty(githubToken))
            {
                ConsoleHelpers.WriteLine("No GitHub token found. Please run 'cycod github login' first.", ConsoleColor.Yellow, overrideQuiet: true);
                return Task.FromResult<object>(1);
            }
            
            // Get a Copilot token using the GitHub token
            var helper = new GitHubCopilotHelper();
            var tokenDetails = helper.GetCopilotTokenDetailsSync(githubToken);
            
            if (string.IsNullOrEmpty(tokenDetails.token))
            {
                ConsoleHelpers.WriteLine("Failed to get a valid Copilot token from GitHub. Please run 'cycod github login' to re-authenticate.", ConsoleColor.Yellow, overrideQuiet: true);
                return Task.FromResult<object>(1);
            }
            
            // Get the editor version from environment or use default
            var editorVersion = EnvironmentHelpers.FindEnvVar("COPILOT_EDITOR_VERSION") ?? "vscode/1.85.0";
            
            // Get models using the Copilot token
            var modelsHelper = new GitHubCopilotModelsHelpers();
            var modelsResponse = modelsHelper.GetModelsSync(tokenDetails.token, editorVersion);
            
            // Format and display the models
            var formattedModels = modelsHelper.FormatModelsForDisplay(modelsResponse);
            ConsoleHelpers.WriteLine(formattedModels, overrideQuiet: true);
            
            return Task.FromResult<object>(0);
        }
        catch (GitHubTokenExpiredException)
        {
            // Message already displayed by the helper
            return Task.FromResult<object>(1);
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteLine($"Error fetching GitHub Copilot models: {ex.Message}", ConsoleColor.Red, overrideQuiet: true);
            ConsoleHelpers.WriteDebugLine(ex.ToString());
            return Task.FromResult<object>(1);
        }
    }
}