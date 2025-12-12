namespace CycoGr.Models;

public class CodeMatch
{
    public required string Path { get; set; }
    public required RepoInfo Repository { get; set; }
    public required string Sha { get; set; }
    public string Url { get; set; } = string.Empty;
    public List<TextMatch> TextMatches { get; set; } = new();
}

public class TextMatch
{
    public required string Fragment { get; set; }
    public List<MatchIndices> Matches { get; set; } = new();
    public string Property { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

public class MatchIndices
{
    public required int[] Indices { get; set; }
    public required string Text { get; set; }
}
