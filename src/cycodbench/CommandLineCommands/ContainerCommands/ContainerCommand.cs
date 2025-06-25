using System;
using System.Threading.Tasks;

/// <summary>
/// Base class for container commands
/// </summary>
public abstract class ContainerCommand : CycodBenchCommand
{
    /// <summary>
    /// The container provider to use (docker, aca, aws)
    /// </summary>
    public string ContainerProvider { get; set; } = "docker";

    public override string GetCommandName()
    {
        return "container";
    }
}