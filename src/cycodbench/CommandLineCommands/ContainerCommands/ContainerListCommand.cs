using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.IO;
using CycodBench.Services;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Command to list running containers.
/// </summary>
public class ContainerListCommand : ContainerCommand
{
    /// <summary>
    /// Path to save the container list output
    /// </summary>
    public string? OutputPath { get; set; }

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

            // Get container information
            string result;
            
            if (ConsoleHelpers.IsVerbose())
            {
                // Get detailed container information
                var containerInfos = await containerService.GetContainerDetailsAsync();
                result = FormatContainerDetails(containerInfos);
            }
            else
            {
                // Get simple container IDs list
                var containerIds = await containerService.ListContainersAsync();
                result = string.Join(Environment.NewLine, containerIds);
            }
            
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
                Console.WriteLine("Container list:");
                Console.WriteLine(result);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error listing containers: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Formats the container details into a readable string.
    /// </summary>
    /// <param name="containerInfos">List of container information objects</param>
    /// <returns>Formatted string with container details</returns>
    private string FormatContainerDetails(List<ContainerInfo> containerInfos)
    {
        if (containerInfos == null || containerInfos.Count == 0)
        {
            return "No containers found.";
        }
        
        var sb = new StringBuilder();
        sb.AppendLine("CONTAINER ID\tNAME\t\tSTATUS\t\tIMAGE");
        sb.AppendLine("-------------------------------------------------------------");
        
        foreach (var container in containerInfos)
        {
            sb.AppendLine($"{container.Id}\t{container.Name}\t{container.Status}\t{container.Image}");
        }
        
        return sb.ToString();
    }
}