using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using CycodBench.Services;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Command to execute a command in a container.
/// </summary>
public class ContainerExecCommand : ContainerCommand
{
    /// <summary>
    /// Container ID to execute the command in
    /// </summary>
    public string? ContainerId { get; set; }
    
    /// <summary>
    /// Command to execute in the container
    /// </summary>
    public string? Command { get; set; }
    
    /// <summary>
    /// Timeout for command execution in seconds
    /// </summary>
    public int Timeout { get; set; } = 60;
    
    /// <summary>
    /// Working directory in the container
    /// </summary>
    public string? WorkingDirectory { get; set; }
    
    /// <summary>
    /// Path to save command output
    /// </summary>
    public string? OutputPath { get; set; }

    public override string GetCommandName()
    {
        return "container exec";
    }

    public override async Task<object> ExecuteAsync(bool interactive)
    {
        Console.WriteLine($"Executing command in container {ContainerId}");
        Console.WriteLine($"Command: {Command}");
        Console.WriteLine($"Timeout: {Timeout} seconds");
        Console.WriteLine($"Working directory: {WorkingDirectory}");
        
        if (!string.IsNullOrEmpty(OutputPath))
        {
            Console.WriteLine($"Output path: {OutputPath}");
        }
        
        try
        {
            // Get the appropriate container service
            var serviceProvider = ServiceConfiguration.GetServiceProvider();
            var containerServiceFactory = serviceProvider.GetRequiredService<Func<string, IContainerService>>();
            var containerService = containerServiceFactory(ContainerProvider);

            // Execute the command
            string result = await containerService.ExecuteCommandAsync(
                ContainerId,
                Command,
                WorkingDirectory,
                Timeout
            );
            
            // Save output to file if requested
            if (!string.IsNullOrEmpty(OutputPath))
            {
                // Ensure the directory exists
                var directory = Path.GetDirectoryName(OutputPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                await File.WriteAllTextAsync(OutputPath, result);
            }
            
            // Show output in console
            if (interactive)
            {
                Console.WriteLine("Command output:");
                Console.WriteLine(result);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error executing command: {ex.Message}");
            return false;
        }
    }
    
    public override bool IsEmpty()
    {
        return string.IsNullOrEmpty(ContainerId) || string.IsNullOrEmpty(Command);
    }
    
    public override Command Validate()
    {
        if (string.IsNullOrEmpty(ContainerId))
        {
            throw new CommandLineException("Container ID must be specified for 'container exec'");
        }
        
        if (string.IsNullOrEmpty(Command))
        {
            throw new CommandLineException("Command must be specified for 'container exec'");
        }
        
        return this;
    }
}