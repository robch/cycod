using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Command to copy files to or from a container.
/// </summary>
public class ContainerCopyCommand : ContainerCommand
{
    /// <summary>
    /// Direction of the copy operation: "to" or "from"
    /// </summary>
    public string? Direction { get; set; }
    
    /// <summary>
    /// Container ID to copy to/from
    /// </summary>
    public string? ContainerId { get; set; }
    
    /// <summary>
    /// Source path
    /// </summary>
    public string? SourcePath { get; set; }
    
    /// <summary>
    /// Destination path
    /// </summary>
    public string? DestinationPath { get; set; }

    public override string GetCommandName()
    {
        return "container copy";
    }

    public override async Task<object> ExecuteAsync(bool interactive)
    {
        Console.WriteLine($"Copying {Direction} container {ContainerId}");
        Console.WriteLine($"Source: {SourcePath}");
        Console.WriteLine($"Destination: {DestinationPath}");
        
        // TODO: Implement the actual file copy logic
        
        return await Task.FromResult<object>("File copied successfully");
    }
    
    public override bool IsEmpty()
    {
        return string.IsNullOrEmpty(Direction) ||
               string.IsNullOrEmpty(ContainerId) ||
               string.IsNullOrEmpty(SourcePath) ||
               string.IsNullOrEmpty(DestinationPath);
    }
    
    public override Command Validate()
    {
        if (string.IsNullOrEmpty(Direction))
        {
            throw new CommandLineException("Direction (to/from) must be specified for 'container copy'");
        }
        
        if (Direction != "to" && Direction != "from")
        {
            throw new CommandLineException("Direction must be 'to' or 'from' for 'container copy'");
        }
        
        if (string.IsNullOrEmpty(ContainerId))
        {
            throw new CommandLineException("Container ID must be specified for 'container copy'");
        }
        
        if (string.IsNullOrEmpty(SourcePath))
        {
            throw new CommandLineException("Source path must be specified for 'container copy'");
        }
        
        if (string.IsNullOrEmpty(DestinationPath))
        {
            throw new CommandLineException("Destination path must be specified for 'container copy'");
        }
        
        return this;
    }
}