
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class VersionCommand : Command
{
    public VersionCommand()
    {
    }

    override public string GetCommandName()
    {
        return "version";
    }

    public override bool IsEmpty()
    {
        return false;
    }

    public override Command Validate()
    {
        return this;
    }

    public List<Task<string>> ExecuteAsync()
    {
        var version = VersionInfo.GetVersion();
        Console.WriteLine($"Version: {version}");
        return new List<Task<string>> { Task.FromResult(version) };
    }
}