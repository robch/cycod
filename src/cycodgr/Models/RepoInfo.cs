namespace CycoGr.Models;

public class RepoInfo
{
    public required string Url { get; set; }
    public required string Name { get; set; }
    public required string Owner { get; set; }
    public int Stars { get; set; }
    public string? Language { get; set; }
    public string? Description { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int Forks { get; set; }
    public int OpenIssues { get; set; }
    public bool IsFork { get; set; }
    public List<string>? Topics { get; set; }

    public string FullName 
    { 
        get => _fullName ?? $"{Owner}/{Name}";
        set => _fullName = value;
    }
    private string? _fullName;
    
    public string FormattedStars => Stars switch
    {
        >= 1000000 => $"{Stars / 1000000.0:F1}M",
        >= 1000 => $"{Stars / 1000.0:F1}k",
        _ => Stars.ToString()
    };
}
