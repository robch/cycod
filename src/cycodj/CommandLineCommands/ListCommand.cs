using System;
using System.Threading.Tasks;
using CycoDj.CommandLine;

namespace CycoDj.CommandLineCommands;

public class ListCommand : CycoDjCommand
{
    public string? Date { get; set; }
    public int Last { get; set; } = 0;

    public override async Task<int> ExecuteAsync()
    {
        ConsoleHelpers.WriteLine("List Command - To be implemented", ConsoleColor.Yellow);
        
        if (!string.IsNullOrEmpty(Date))
        {
            ConsoleHelpers.WriteLine($"  Filter by date: {Date}", ConsoleColor.Gray);
        }
        
        if (Last > 0)
        {
            ConsoleHelpers.WriteLine($"  Show last: {Last}", ConsoleColor.Gray);
        }
        
        ConsoleHelpers.WriteLine();
        ConsoleHelpers.WriteLine("TODO: Implement JSONL file reading and listing", ConsoleColor.Cyan);
        
        return await Task.FromResult(0);
    }
}
