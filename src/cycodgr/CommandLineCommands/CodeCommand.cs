using System.Collections.Generic;
using System.Linq;

class CodeCommand : CycoGrCommand
{
    public CodeCommand()
    {
        Keywords = new List<string>();
        MaxResults = 10;
        Language = string.Empty;
        Owner = string.Empty;
        FileExtension = string.Empty;
        Format = "detailed";  // Default to detailed format
        LinesBeforeAndAfter = 5;  // Default context lines
        MinStars = 0;
    }

    public List<string> Keywords { get; set; }
    public int MaxResults { get; set; }
    public string Language { get; set; }
    public string Owner { get; set; }
    public string FileExtension { get; set; }
    public string Format { get; set; } // detailed, filenames, files, repos, urls, json, csv
    public int LinesBeforeAndAfter { get; set; }
    public int MinStars { get; set; }

    override public string GetCommandName()
    {
        return "code";
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
