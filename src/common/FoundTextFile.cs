using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class FoundTextFile
{
    public required string Path { get; set; }
    public string? Content { get; set; }
    public required Func<Task<string>> LoadContent { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
