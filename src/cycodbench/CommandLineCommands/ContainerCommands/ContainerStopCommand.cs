using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Command to stop and remove a container.
/// </summary>
public class ContainerStopCommand : ContainerCommand
{
    /// <summary>
    /// Container ID to stop
    /// </summary>
    public string? ContainerId { get; set; }
    
    /// <summary>
    /// Keep container after stopping it
    /// </summary>
    public bool Keep { get; set; } = false;
    
    /// <summary>
    /// Timeout for stopping the container in seconds
    /// </summary>
    public int Timeout { get; set; } = 10;

    public override string GetCommandName()
    {
        return "container stop";
    }

    public override async Task<object> ExecuteAsync(bool interactive)
    {
        Console.WriteLine($"Stopping container {ContainerId}");
        Console.WriteLine($"Using container provider: {ContainerProvider}");
        Console.WriteLine($"Keep container: {Keep}");
        Console.WriteLine($"Timeout: {Timeout} seconds");
        
        // TODO: Implement the actual container stop logic
        
        return await Task.FromResult<object>(true);
    }
    
    public override bool IsEmpty()
    {
        return string.IsNullOrEmpty(ContainerId);
    }
    
    public override Command Validate()
    {
        if (string.IsNullOrEmpty(ContainerId))
        {
            throw new CommandLineException("Container ID must be specified for 'container stop'");
        }
        
        return this;
    }
}