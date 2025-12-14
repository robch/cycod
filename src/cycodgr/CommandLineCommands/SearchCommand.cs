namespace CycoGr.CommandLineCommands;

using CycoGr.CommandLine;

public class SearchCommand : CycoGrCommand
{
    public SearchCommand()
    {
        RepoPatterns = new List<string>();
        Contains = string.Empty;
        FileContains = string.Empty;
        RepoContains = string.Empty;
        MaxResults = 10;
        Language = string.Empty;
        Owner = string.Empty;
        MinStars = 0;
        SortBy = string.Empty;  // Empty = GitHub's default (relevance)
        IncludeForks = false;
        ExcludeForks = false;
        OnlyForks = false;
        Format = "detailed";
        LinesBeforeAndAfter = 5;
        Clone = false;
        MaxClone = 10;
        CloneDirectory = "external";
        AsSubmodules = false;
        FileInstructionsList = new List<Tuple<string, string>>();
        RepoInstructionsList = new List<string>();
        InstructionsList = new List<string>();
    }

    public override string GetCommandName()
    {
        return "";  // Return empty to show usage.txt as welcome message
    }

    public override bool IsEmpty()
    {
        // Empty if no repo patterns and no content search specified
        return !RepoPatterns.Any() && 
               string.IsNullOrEmpty(Contains) && 
               string.IsNullOrEmpty(FileContains) && 
               string.IsNullOrEmpty(RepoContains);
    }

    // Positional repo patterns (like cycodmd's file patterns)
    public List<string> RepoPatterns { get; set; }

    // Content search flags
    public string Contains { get; set; }        // Search both repo metadata AND code
    public string FileContains { get; set; }    // Search code files only
    public string RepoContains { get; set; }    // Search repo metadata only

    // Filtering options (work for both repo and code searches)
    public int MaxResults { get; set; }
    public string Language { get; set; }
    public string Owner { get; set; }
    public int MinStars { get; set; }
    
    // Repo-specific options
    public string SortBy { get; set; }
    public bool IncludeForks { get; set; }
    public bool ExcludeForks { get; set; }
    public bool OnlyForks { get; set; }
    public bool Clone { get; set; }
    public int MaxClone { get; set; }
    public string CloneDirectory { get; set; }
    public bool AsSubmodules { get; set; }

    // Code-specific options
    public int LinesBeforeAndAfter { get; set; }

    // AI instruction options
    public List<Tuple<string, string>> FileInstructionsList { get; set; }
    public List<string> RepoInstructionsList { get; set; }
    public List<string> InstructionsList { get; set; }

    // Output options
    public string Format { get; set; }
}
