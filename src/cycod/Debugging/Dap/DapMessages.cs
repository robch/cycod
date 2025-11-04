using System.Text.Json.Serialization;

namespace Cycod.Debugging.Dap;

public abstract class ProtocolMessage
{
    [JsonPropertyName("seq")] public int Seq { get; set; }
    [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
}

public class Request : ProtocolMessage
{
    [JsonPropertyName("command")] public string Command { get; set; } = string.Empty;
    [JsonPropertyName("arguments")] public object? Arguments { get; set; }
    public Request() => Type = "request";
}

public class Response : ProtocolMessage
{
    [JsonPropertyName("request_seq")] public int RequestSeq { get; set; }
    [JsonPropertyName("success")] public bool Success { get; set; }
    [JsonPropertyName("command")] public string Command { get; set; } = string.Empty;
    [JsonPropertyName("message")] public string? Message { get; set; }
    [JsonPropertyName("body")] public object? Body { get; set; }
    public Response() => Type = "response";
}

public class Event : ProtocolMessage
{
    [JsonPropertyName("event")] public string EventType { get; set; } = string.Empty;
    [JsonPropertyName("body")] public object? Body { get; set; }
    public Event() => Type = "event";
}

public class InitializeRequestArguments
{
    [JsonPropertyName("clientID")] public string ClientID { get; set; } = "cycod";
    [JsonPropertyName("clientName")] public string ClientName { get; set; } = "cycod Debugger";
    [JsonPropertyName("adapterID")] public string AdapterID { get; set; } = "coreclr";
    [JsonPropertyName("linesStartAt1")] public bool LinesStartAt1 { get; set; } = true;
    [JsonPropertyName("columnsStartAt1")] public bool ColumnsStartAt1 { get; set; } = true;
    [JsonPropertyName("pathFormat")] public string PathFormat { get; set; } = "path";
}

public class LaunchRequestArguments
{
    [JsonPropertyName("program")] public string Program { get; set; } = string.Empty;
    [JsonPropertyName("cwd")] public string? Cwd { get; set; }
    [JsonPropertyName("stopAtEntry")] public bool? StopAtEntry { get; set; }
}

public class SetBreakpointsArguments
{
    [JsonPropertyName("source")] public Source Source { get; set; } = new();
    [JsonPropertyName("breakpoints")] public SourceBreakpoint[]? Breakpoints { get; set; }
}

public class ContinueArguments { [JsonPropertyName("threadId")] public int ThreadId { get; set; } }
public class NextArguments { [JsonPropertyName("threadId")] public int ThreadId { get; set; } }
public class StepInArguments { [JsonPropertyName("threadId")] public int ThreadId { get; set; } }
public class StepOutArguments { [JsonPropertyName("threadId")] public int ThreadId { get; set; } }

public class StackTraceArguments
{
    [JsonPropertyName("threadId")] public int ThreadId { get; set; }
    [JsonPropertyName("levels")] public int? Levels { get; set; }
}

public class ScopesArguments { [JsonPropertyName("frameId")] public int FrameId { get; set; } }
public class VariablesArguments { [JsonPropertyName("variablesReference")] public int VariablesReference { get; set; } }

public class SetVariableArguments
{
    [JsonPropertyName("variablesReference")] public int VariablesReference { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("value")] public string Value { get; set; } = string.Empty;
}

public class EvaluateArguments
{
    [JsonPropertyName("expression")] public string Expression { get; set; } = string.Empty;
    [JsonPropertyName("frameId")] public int? FrameId { get; set; }
    [JsonPropertyName("context")] public string? Context { get; set; }
}

public class Capabilities
{
    [JsonPropertyName("supportsConfigurationDoneRequest")] public bool SupportsConfigurationDoneRequest { get; set; }
    [JsonPropertyName("supportsSetVariable")] public bool SupportsSetVariable { get; set; }
    [JsonPropertyName("supportsEvaluateForHovers")] public bool SupportsEvaluateForHovers { get; set; }
    [JsonPropertyName("supportsConditionalBreakpoints")] public bool SupportsConditionalBreakpoints { get; set; }
}

public class StackTraceResponseBody { [JsonPropertyName("stackFrames")] public StackFrame[] StackFrames { get; set; } = Array.Empty<StackFrame>(); }
public class ScopesResponseBody { [JsonPropertyName("scopes")] public Scope[] Scopes { get; set; } = Array.Empty<Scope>(); }
public class VariablesResponseBody { [JsonPropertyName("variables")] public Variable[] Variables { get; set; } = Array.Empty<Variable>(); }
public class ThreadsResponseBody { [JsonPropertyName("threads")] public ThreadInfo[] Threads { get; set; } = Array.Empty<ThreadInfo>(); }
public class ThreadInfo { [JsonPropertyName("id")] public int Id { get; set; } [JsonPropertyName("name")] public string? Name { get; set; } }
public class EvaluateResponseBody { [JsonPropertyName("result")] public string Result { get; set; } = string.Empty; [JsonPropertyName("type")] public string? Type { get; set; } }
public class SetBreakpointsResponseBody { [JsonPropertyName("breakpoints")] public Breakpoint[] Breakpoints { get; set; } = Array.Empty<Breakpoint>(); }

public class StoppedEventBody { [JsonPropertyName("reason")] public string Reason { get; set; } = string.Empty; [JsonPropertyName("threadId")] public int? ThreadId { get; set; } [JsonPropertyName("text")] public string? Text { get; set; } }
public class OutputEventBody { [JsonPropertyName("category")] public string? Category { get; set; } [JsonPropertyName("output")] public string Output { get; set; } = string.Empty; }
public class ThreadEventBody { [JsonPropertyName("reason")] public string Reason { get; set; } = string.Empty; [JsonPropertyName("threadId")] public int ThreadId { get; set; } }
public class ExitedEventBody { [JsonPropertyName("exitCode")] public int ExitCode { get; set; } }

public class Source { [JsonPropertyName("path")] public string? Path { get; set; } }
public class SourceBreakpoint { [JsonPropertyName("line")] public int Line { get; set; } [JsonPropertyName("condition")] public string? Condition { get; set; } }
public class Breakpoint { [JsonPropertyName("id")] public int? Id { get; set; } [JsonPropertyName("verified")] public bool Verified { get; set; } [JsonPropertyName("line")] public int? Line { get; set; } [JsonPropertyName("message")] public string? Message { get; set; } }
public class StackFrame { [JsonPropertyName("id")] public int Id { get; set; } [JsonPropertyName("name")] public string Name { get; set; } = string.Empty; [JsonPropertyName("source")] public Source? Source { get; set; } [JsonPropertyName("line")] public int Line { get; set; } [JsonPropertyName("column")] public int Column { get; set; } }
public class Scope { [JsonPropertyName("name")] public string Name { get; set; } = string.Empty; [JsonPropertyName("variablesReference")] public int VariablesReference { get; set; } }
public class Variable { [JsonPropertyName("name")] public string Name { get; set; } = string.Empty; [JsonPropertyName("value")] public string Value { get; set; } = string.Empty; [JsonPropertyName("type")] public string? Type { get; set; } [JsonPropertyName("variablesReference")] public int VariablesReference { get; set; } }
