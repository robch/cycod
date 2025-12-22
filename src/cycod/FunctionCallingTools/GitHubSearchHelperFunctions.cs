using System;
using System.ComponentModel;
using System.Threading.Tasks;

/// <summary>
/// AI tool wrapper for cycodgr GitHub search CLI
/// </summary>
public class GitHubSearchHelperFunctions
{
    [Description(@"Search GitHub repositories and code using the cycodgr CLI.

This tool provides access to GitHub's search capabilities with powerful filtering options.

QUICK EXAMPLES:
  SearchGitHub(""--contains 'terminal emulator' --max-results 10"")
  SearchGitHub(""microsoft/terminal --file-contains 'ConPTY'"")
  SearchGitHub(""--repo-contains 'jwt' --language csharp --min-stars 500"")
  SearchGitHub(""--file-contains 'OSC 133' --language rust --lines 10"")

MULTI-LANGUAGE SEARCH:
  SearchGitHub(""--file-contains 'async' --language python --max-results 10"")
  SearchGitHub(""--file-contains 'ChatClient' --language csharp --language python"")
  
REPOSITORY FINGERPRINTING (two-step workflow):
  Step 1: SearchGitHub(""--file-contains 'Microsoft.AI' --extension csproj --format repos"")
  Result: List of repo names (one per line)
  Step 2: SearchGitHub(""microsoft/semantic-kernel microsoft/autogen --file-contains 'ChatClient'"")
  
TARGETED SEARCHES:
  SearchGitHub(""microsoft/terminal wezterm/wezterm --file-contains 'OSC 133' --lines 10"")

GET COMPREHENSIVE HELP:
  SearchGitHub(""help"") - Full documentation with all options and examples
  SearchGitHub(""help examples"") - Usage examples  
  SearchGitHub(""help filtering"") - Filtering and sorting options
  SearchGitHub(""help language shortcuts"") - Language-specific shortcuts
  
The help command is automatically expanded to show detailed documentation.")]
    public async Task<string> SearchGitHub(
        [Description("Command-line arguments to pass to cycodgr. Use 'help' to see all available options and examples.")] 
        string arguments)
    {
        // Smart help expansion
        if (arguments == "help" || arguments == "--help" || arguments == "-h")
        {
            arguments = "help topics --expand";
        }
        else if (arguments.StartsWith("help ") && !arguments.Contains("--expand"))
        {
            arguments += " --expand";
        }
        
        Logger.Info($"Executing cycodgr command: cycodgr {arguments}");
        
        if (ConsoleHelpers.IsVerbose())
        {
            ConsoleHelpers.WriteLine($"Executing cycodgr command: cycodgr {arguments}");
        }

        // Execute cycodgr command
        var processBuilder = new RunnableProcessBuilder()
            .WithFileName("cycodgr")
            .WithArguments(arguments)
            .WithTimeout(120000)  // 2 minute timeout for GitHub API calls
            .WithVerboseLogging(ConsoleHelpers.IsVerbose());

        var result = await processBuilder.RunAsync();
        
        // Handle errors
        if (result.CompletionState == ProcessCompletionState.Error)
        {
            var errorOutput = $"<cycodgr command failed>\n{result.StandardError}";
            Logger.Error(errorOutput);
            return errorOutput;
        }
        
        if (result.CompletionState == ProcessCompletionState.TimedOut)
        {
            var timeoutOutput = $"<cycodgr command timed out after 120 seconds>";
            Logger.Warning(timeoutOutput);
            return timeoutOutput;
        }

        return result.StandardOutput;
    }
}
