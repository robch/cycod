using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

abstract class WebCommand : Command
{
    public WebCommand()
    {
        Interactive = false;

        SearchProvider = WebSearchProvider.Google;
        MaxResults = 10;

        ExcludeURLContainsPatternList = new();

        Browser = BrowserType.Chromium;
        GetContent = false;
        StripHtml = false;

        SaveFolder = null;

        PageInstructionsList = new();
    }

    public bool Interactive { get; set; }

    public WebSearchProvider SearchProvider { get; set; }
    public List<Regex> ExcludeURLContainsPatternList { get; set; }
    public int MaxResults { get; set; }

    public BrowserType Browser { get; set; }
    public bool GetContent { get; set; }
    public bool StripHtml { get; set; }

    public string SaveFolder { get; set; }

    public List<Tuple<string, string>> PageInstructionsList;

    public string SavePageOutput { get; set; }
}
