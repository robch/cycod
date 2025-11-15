namespace Cycod.Debugging.Dap;

public static class DapProtocol
{
    public const string InitializeCommand = "initialize";
    public const string LaunchCommand = "launch";
    public const string ConfigurationDoneCommand = "configurationDone";
    public const string SetBreakpointsCommand = "setBreakpoints";
    public const string ContinueCommand = "continue";
    public const string NextCommand = "next";
    public const string StepInCommand = "stepIn";
    public const string StepOutCommand = "stepOut";
    public const string StackTraceCommand = "stackTrace";
    public const string ScopesCommand = "scopes";
    public const string VariablesCommand = "variables";
    public const string ThreadsCommand = "threads";
    public const string SetVariableCommand = "setVariable";
    public const string EvaluateCommand = "evaluate";
    public const string DisconnectCommand = "disconnect";
    public const string TerminateCommand = "terminate";

    public const string InitializedEvent = "initialized";
    public const string StoppedEvent = "stopped";
    public const string ContinuedEvent = "continued";
    public const string OutputEvent = "output";
    public const string ThreadEvent = "thread";
    public const string TerminatedEvent = "terminated";
    public const string ExitedEvent = "exited";
}
