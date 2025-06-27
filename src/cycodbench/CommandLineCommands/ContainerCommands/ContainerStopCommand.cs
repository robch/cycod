using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CycodBench.Services;
using Microsoft.Extensions.DependencyInjection;

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
        
        try
        {
            // Get the appropriate container service
            var serviceProvider = ServiceConfiguration.GetServiceProvider();
            var containerServiceFactory = serviceProvider.GetRequiredService<Func<string, IContainerService>>();
            var containerService = containerServiceFactory(ContainerProvider);

            // Stop the container
            bool success = await containerService.StopContainerAsync(
                ContainerId,
                remove: !Keep,
                timeout: Timeout
            );
            
            if (success)
            {
                Console.WriteLine($"Container {ContainerId} {(Keep ? "stopped" : "stopped and removed")} successfully.");
                return true;
            }
            else
            {
                Console.WriteLine($"Container {ContainerId} not found or already stopped.");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error stopping container: {ex.Message}");
            return false;
        }
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