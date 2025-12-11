using System.Collections.Generic;
using System.Linq;

class SearchCommand : CycoGhCommand
{
    public SearchCommand()
    {
        Keywords = new List<string>();
        MaxResults = 10;
        Clone = false;
        MaxClone = 10;
        CloneDirectory = "external";
        AsSubmodules = false;
        Language = string.Empty;
        FileExtension = string.Empty;
        SortBy = "stars";
        IncludeForks = false;
    }

    public List<string> Keywords { get; set; }
    public int MaxResults { get; set; }
    public bool Clone { get; set; }
    public int MaxClone { get; set; }
    public string CloneDirectory { get; set; }
    public bool AsSubmodules { get; set; }
    public string Language { get; set; }
    public string FileExtension { get; set; }
    public string SortBy { get; set; }
    public bool IncludeForks { get; set; }

    override public string GetCommandName()
    {
        return "search";
    }

    override public bool IsEmpty()
    {
        return !Keywords.Any();
    }

    override public CycoGhCommand Validate()
    {
        return this;
    }
}
