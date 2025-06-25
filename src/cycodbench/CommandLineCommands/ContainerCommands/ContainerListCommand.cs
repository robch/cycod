using System;
using System.Threading.Tasks;

/// <summary>
/// Command to list running containers.
/// </summary>
public class ContainerListCommand : ContainerCommand
{
    /// <summary>
    /// Path to save the container list output
    /// </summary>
    public string? OutputPath { get; set; }

    /// <summary>
    /// Whether to show verbose output
    /// </summary>
    public bool Verbose { get; set; }

    public ContainerListCommand()
    {
        // Default to docker as the container provider
        ContainerProvider = "docker";
    }

    public override string GetCommandName()
    {
        return "container list";
    }

    public override Command Validate()
    {
        // No additional validation needed
        return this;
    }

    public override bool IsEmpty()
    {
        // This command doesn't require any arguments
        return false;
    }

    public override async Task<object> ExecuteAsync(bool interactive)
    {
        Console.WriteLine($"Listing containers");
        Console.WriteLine($"Using container provider: {ContainerProvider}");
        Console.WriteLine($"Verbose: {Verbose}");
        
        if (!string.IsNullOrEmpty(OutputPath))
        {
            Console.WriteLine($"Output path: {OutputPath}");
        }
        
        // TODO: Implement the actual container listing logic
        
        return await Task.FromResult<object>("Container list would appear here");
    }
}