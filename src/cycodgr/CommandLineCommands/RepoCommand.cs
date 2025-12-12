using System.Collections.Generic;
using System.Linq;

class RepoCommand : CycoGrCommand
{
    public RepoCommand()
    {
        Keywords = new List<string>();
        MaxResults = 10;
        Clone = false;
        MaxClone = 10;
        CloneDirectory = "external";
        AsSubmodules = false;
        Language = string.Empty;
        Owner = string.Empty;
        SortBy = string.Empty;  // Empty = use GitHub's default (relevance/best-match)
        IncludeForks = false;
        ExcludeForks = false;
        OnlyForks = false;
        MinStars = 0;
        Format = "detailed";  // Changed from "url" to "detailed"
    }

    public List<string> Keywords { get; set; }
    public int MaxResults { get; set; }
    public bool Clone { get; set; }
    public int MaxClone { get; set; }
    public string CloneDirectory { get; set; }
    public bool AsSubmodules { get; set; }
    public string Language { get; set; }
    public string Owner { get; set; }
    public string SortBy { get; set; }
    public bool IncludeForks { get; set; }
    public bool ExcludeForks { get; set; }
    public bool OnlyForks { get; set; }
    public int MinStars { get; set; }
    public string Format { get; set; } // url, table, json, csv, detailed

    override public string GetCommandName()
    {
        return "repo";
    }

    override public bool IsEmpty()
    {
        return !Keywords.Any();
    }

    override public CycoGrCommand Validate()
    {
        return this;
    }
}
