using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CycodBench.Services;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Command to initialize a container.
/// </summary>
public class ContainerInitCommand : ContainerCommand
{
    /// <summary>
    /// Dataset path or name
    /// </summary>
    public string? DatasetPath { get; set; }
    
    /// <summary>
    /// Problem ID to initialize container for
    /// </summary>
    public string? ProblemId { get; set; }
    
    /// <summary>
    /// Custom name for the container
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Container image to use
    /// </summary>
    public string? Image { get; set; }
    
    /// <summary>
    /// Memory limit for the container
    /// </summary>
    public string? Memory { get; set; }
    
    /// <summary>
    /// CPU limit for the container
    /// </summary>
    public int Cpus { get; set; }
    
    /// <summary>
    /// Existing container ID to use
    /// </summary>
    public string? ContainerId { get; set; }
    
    /// <summary>
    /// Host path to mount as workspace
    /// </summary>
    public string? WorkspacePath { get; set; }
    
    /// <summary>
    /// Whether to set up evaluation tools in the container
    /// </summary>
    public bool SetupTools { get; set; }
    
    /// <summary>
    /// Whether to set up the agent in the container
    /// </summary>
    public string? SetupAgentFromPath { get; set; }

    public override string GetCommandName()
    {
        return "container init";
    }

    public override Command Validate()
    {
        // TODO: Implement validation logic

        if (string.IsNullOrEmpty(ProblemId))
        {
            throw new CommandLineException("ProblemId is required for container initialization.", "container init");
        }

        if (string.IsNullOrEmpty(DatasetPath))
        {
            DatasetPath = "verified";
        }

        return this;
    }

    public override bool IsEmpty()
    {
        // TODO: Implement validation logic
        return false;
    }

    public override async Task<object> ExecuteAsync(bool interactive)
    {
        Console.WriteLine($"Initializing container for dataset: {DatasetPath}");
        Console.WriteLine($"Problem ID: {ProblemId}");

        try
        {
            var serviceProvider = ServiceConfiguration.GetServiceProvider();
            var containerServiceFactory = serviceProvider.GetRequiredService<Func<string, IContainerService>>();

            Console.WriteLine($"Using container provider: {ContainerProvider}");
            var containerService = containerServiceFactory(ContainerProvider);

            var newContainer = string.IsNullOrEmpty(ContainerId);
            var useDefaultAgentPath = newContainer && string.IsNullOrEmpty(SetupAgentFromPath);
            if (useDefaultAgentPath) SetupAgentFromPath = "self-contained";

            var containerId = await containerService.InitContainerAsync(
                problemId: ProblemId,
                name: Name,
                image: Image,
                memoryLimit: Memory,
                cpuLimit: Cpus.ToString(),
                workspacePath: WorkspacePath,
                setupAgentFromPath: SetupAgentFromPath,
                setupTools: SetupTools || newContainer
            );

            Console.WriteLine($"Container initialized successfully: {containerId}");
            return containerId;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error initializing container: {ex.Message}");
            return false;
        }
    }
}