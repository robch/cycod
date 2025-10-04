using System;

namespace ShellExecution.Results
{
    /// <summary>
    /// Base class for all execution result objects in the shell and process management system.
    /// </summary>
    public abstract class ExecutionResultBase
    {
        /// <summary>
        /// Gets or sets the duration of the operation.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the operation was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the error message if the operation was not successful.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionResultBase"/> class.
        /// </summary>
        protected ExecutionResultBase()
        {
            Success = true;
            ErrorMessage = string.Empty;
            Duration = TimeSpan.Zero;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutionResultBase"/> class with specified values.
        /// </summary>
        /// <param name="success">Whether the operation was successful.</param>
        /// <param name="errorMessage">Error message if not successful.</param>
        /// <param name="duration">Duration of the operation.</param>
        protected ExecutionResultBase(bool success, string errorMessage, TimeSpan duration)
        {
            Success = success;
            ErrorMessage = errorMessage ?? string.Empty;
            Duration = duration;
        }

        /// <summary>
        /// Returns a string representation of the result.
        /// </summary>
        /// <returns>A string representation of the result.</returns>
        public override string ToString()
        {
            if (Success)
            {
                return $"Operation completed successfully in {Duration.TotalMilliseconds}ms";
            }
            else
            {
                return $"Operation failed after {Duration.TotalMilliseconds}ms: {ErrorMessage}";
            }
        }

        /// <summary>
        /// Converts the result to a format suitable for AI presentation.
        /// </summary>
        /// <returns>A string formatted for AI presentation.</returns>
        public virtual string ToAiString()
        {
            return ToString();
        }
    }

    /// <summary>
    /// Represents the result of a shell command execution.
    /// </summary>
    public class ShellCommandResult : ExecutionResultBase
    {
        /// <summary>
        /// Gets or sets the standard output from the command.
        /// </summary>
        public string Stdout { get; set; }

        /// <summary>
        /// Gets or sets the standard error output from the command.
        /// </summary>
        public string Stderr { get; set; }

        /// <summary>
        /// Gets or sets the exit code of the command.
        /// </summary>
        public int ExitCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the command timed out.
        /// </summary>
        public bool TimedOut { get; set; }

        /// <summary>
        /// Gets or sets the name of the shell where the command was executed.
        /// </summary>
        public string ShellName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the command was auto-promoted to a managed shell.
        /// </summary>
        public bool WasPromoted { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellCommandResult"/> class.
        /// </summary>
        public ShellCommandResult()
        {
            Stdout = string.Empty;
            Stderr = string.Empty;
            ShellName = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellCommandResult"/> class with specified values.
        /// </summary>
        public ShellCommandResult(
            string stdout, 
            string stderr, 
            int exitCode, 
            bool timedOut, 
            string shellName, 
            bool wasPromoted,
            bool success,
            string errorMessage,
            TimeSpan duration)
            : base(success, errorMessage, duration)
        {
            Stdout = stdout ?? string.Empty;
            Stderr = stderr ?? string.Empty;
            ExitCode = exitCode;
            TimedOut = timedOut;
            ShellName = shellName ?? string.Empty;
            WasPromoted = wasPromoted;
        }

        /// <summary>
        /// Gets the combined output (stdout and stderr).
        /// </summary>
        public string MergedOutput
        {
            get
            {
                if (string.IsNullOrEmpty(Stderr))
                {
                    return Stdout;
                }
                
                if (string.IsNullOrEmpty(Stdout))
                {
                    return Stderr;
                }
                
                return $"{Stdout}{Environment.NewLine}{Stderr}";
            }
        }

        /// <summary>
        /// Converts the result to a format suitable for AI presentation.
        /// </summary>
        /// <returns>A string formatted for AI presentation.</returns>
        public override string ToAiString()
        {
            var output = MergedOutput;
            
            if (TimedOut)
            {
                output += Environment.NewLine + $"<timed out; still running; use GetShellOrProcessOutput>";
                
                if (WasPromoted)
                {
                    output += Environment.NewLine + $"<in shell: {ShellName}>";
                }
            }
            else if (ExitCode != 0)
            {
                output += Environment.NewLine + $"<exited with code {ExitCode}>";
            }
            
            return output;
        }

        /// <summary>
        /// Creates a ShellCommandResult from an existing PersistentShellCommandResult.
        /// </summary>
        /// <param name="result">The existing result to convert.</param>
        /// <param name="shellName">The name of the shell.</param>
        /// <returns>A new ShellCommandResult.</returns>
        public static ShellCommandResult FromPersistentShellCommandResult(
            PersistentShellCommandResult result, 
            string shellName)
        {
            return new ShellCommandResult(
                result.StandardOutput,
                result.StandardError,
                result.ExitCode,
                result.IsTimeout,
                shellName,
                false,
                result.ExitCode == 0 && !result.IsTimeout,
                result.IsTimeout ? "Command timed out" : result.FriendlyErrorMessage ?? string.Empty,
                result.Duration
            );
        }
    }

    /// <summary>
    /// Represents the result of retrieving output from a shell or process.
    /// </summary>
    public class OutputResult : ExecutionResultBase
    {
        /// <summary>
        /// Gets or sets the standard output.
        /// </summary>
        public string Stdout { get; set; }

        /// <summary>
        /// Gets or sets the standard error output.
        /// </summary>
        public string Stderr { get; set; }

        /// <summary>
        /// Gets or sets the combined output.
        /// </summary>
        public string Combined { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the output buffer was cleared.
        /// </summary>
        public bool WasCleared { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a pattern was matched in the output.
        /// </summary>
        public bool PatternMatched { get; set; }

        /// <summary>
        /// Gets or sets the pattern that was matched, if any.
        /// </summary>
        public string MatchedPattern { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputResult"/> class.
        /// </summary>
        public OutputResult()
        {
            Stdout = string.Empty;
            Stderr = string.Empty;
            Combined = string.Empty;
            MatchedPattern = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputResult"/> class with specified values.
        /// </summary>
        public OutputResult(
            string stdout,
            string stderr,
            bool wasCleared,
            bool patternMatched,
            string matchedPattern,
            bool success,
            string errorMessage,
            TimeSpan duration)
            : base(success, errorMessage, duration)
        {
            Stdout = stdout ?? string.Empty;
            Stderr = stderr ?? string.Empty;
            Combined = string.IsNullOrEmpty(stderr) 
                ? stdout ?? string.Empty 
                : $"{stdout}{Environment.NewLine}{stderr}";
            WasCleared = wasCleared;
            PatternMatched = patternMatched;
            MatchedPattern = matchedPattern ?? string.Empty;
        }

        /// <summary>
        /// Converts the result to a format suitable for AI presentation.
        /// </summary>
        /// <returns>A string formatted for AI presentation.</returns>
        public override string ToAiString()
        {
            var output = Combined;
            
            if (PatternMatched)
            {
                output += Environment.NewLine + $"<pattern matched: \"{MatchedPattern}\">";
            }
            
            if (WasCleared)
            {
                output += Environment.NewLine + "<output buffer has been cleared>";
            }
            
            return output;
        }
    }

    /// <summary>
    /// Represents the result of creating a shell or process.
    /// </summary>
    public class ResourceCreationResult : ExecutionResultBase
    {
        /// <summary>
        /// Gets or sets the name of the created resource.
        /// </summary>
        public string ResourceName { get; set; }

        /// <summary>
        /// Gets or sets the type of resource created.
        /// </summary>
        public string ResourceType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceCreationResult"/> class.
        /// </summary>
        public ResourceCreationResult()
        {
            ResourceName = string.Empty;
            ResourceType = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceCreationResult"/> class with specified values.
        /// </summary>
        public ResourceCreationResult(
            string resourceName,
            string resourceType,
            bool success,
            string errorMessage,
            TimeSpan duration)
            : base(success, errorMessage, duration)
        {
            ResourceName = resourceName ?? string.Empty;
            ResourceType = resourceType ?? string.Empty;
        }

        /// <summary>
        /// Converts the result to a format suitable for AI presentation.
        /// </summary>
        /// <returns>A string formatted for AI presentation.</returns>
        public override string ToAiString()
        {
            if (Success)
            {
                return $"{ResourceType} created with name: {ResourceName}";
            }
            else
            {
                return $"Failed to create {ResourceType}: {ErrorMessage}";
            }
        }
    }

    /// <summary>
    /// Represents the result of terminating a shell or process.
    /// </summary>
    public class TerminationResult : ExecutionResultBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether the resource was running before termination.
        /// </summary>
        public bool WasRunning { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the termination was forced.
        /// </summary>
        public bool WasForced { get; set; }

        /// <summary>
        /// Gets or sets the exit code of the terminated process, if available.
        /// </summary>
        public int? ExitCode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TerminationResult"/> class.
        /// </summary>
        public TerminationResult()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TerminationResult"/> class with specified values.
        /// </summary>
        public TerminationResult(
            bool wasRunning,
            bool wasForced,
            int? exitCode,
            bool success,
            string errorMessage,
            TimeSpan duration)
            : base(success, errorMessage, duration)
        {
            WasRunning = wasRunning;
            WasForced = wasForced;
            ExitCode = exitCode;
        }

        /// <summary>
        /// Converts the result to a format suitable for AI presentation.
        /// </summary>
        /// <returns>A string formatted for AI presentation.</returns>
        public override string ToAiString()
        {
            if (!Success)
            {
                return $"Failed to terminate: {ErrorMessage}. Try using force=true.";
            }
            
            if (!WasRunning)
            {
                return "Process or shell not found or already terminated";
            }
            
            var forcedText = WasForced ? "forcefully " : "";
            var exitCodeText = ExitCode.HasValue ? $" with exit code {ExitCode}" : "";
            
            return $"Process or shell was {forcedText}terminated{exitCodeText}";
        }
    }
    
    /// <summary>
    /// Represents error types for shell and process execution.
    /// </summary>
    public enum ExecutionErrorType
    {
        /// <summary>
        /// The requested resource was not found.
        /// </summary>
        ResourceNotFound,
        
        /// <summary>
        /// A resource with the specified name already exists.
        /// </summary>
        ResourceAlreadyExists,
        
        /// <summary>
        /// The resource is currently busy.
        /// </summary>
        ResourceBusy,
        
        /// <summary>
        /// The command execution failed.
        /// </summary>
        CommandFailed,
        
        /// <summary>
        /// The command execution timed out.
        /// </summary>
        CommandTimeout,
        
        /// <summary>
        /// The command was cancelled by the user.
        /// </summary>
        CommandCancelled,
        
        /// <summary>
        /// The process rejected the input.
        /// </summary>
        InputRejected,
        
        /// <summary>
        /// The output buffer is full.
        /// </summary>
        OutputBufferFull,
        
        /// <summary>
        /// Error in parsing output pattern.
        /// </summary>
        OutputParsingError,
        
        /// <summary>
        /// System resources are insufficient.
        /// </summary>
        InsufficientResources,
        
        /// <summary>
        /// Access was denied.
        /// </summary>
        AccessDenied,
        
        /// <summary>
        /// An unexpected system error occurred.
        /// </summary>
        SystemError,
        
        /// <summary>
        /// An argument was invalid.
        /// </summary>
        InvalidArgument,
        
        /// <summary>
        /// The operation is not valid in the current state.
        /// </summary>
        InvalidOperation
    }

    /// <summary>
    /// Base exception class for shell execution errors.
    /// </summary>
    public class ShellExecutionException : Exception
    {
        /// <summary>
        /// Gets the error type.
        /// </summary>
        public ExecutionErrorType ErrorType { get; }
        
        /// <summary>
        /// Gets the error code.
        /// </summary>
        public string ErrorCode { get; }
        
        /// <summary>
        /// Gets the context information.
        /// </summary>
        public string Context { get; }
        
        /// <summary>
        /// Gets a suggestion for resolving the error.
        /// </summary>
        public string Suggestion { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellExecutionException"/> class.
        /// </summary>
        public ShellExecutionException(
            string message,
            ExecutionErrorType errorType,
            string errorCode,
            string context,
            string suggestion,
            Exception? innerException = null)
            : base(message, innerException)
        {
            ErrorType = errorType;
            ErrorCode = errorCode;
            Context = context;
            Suggestion = suggestion;
        }
    }
    
    /// <summary>
    /// Exception thrown when a requested resource is not found.
    /// </summary>
    public class ResourceNotFoundException : ShellExecutionException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceNotFoundException"/> class.
        /// </summary>
        public ResourceNotFoundException(
            string resourceType,
            string resourceName,
            string? suggestion = null,
            Exception? innerException = null)
            : base(
                $"{resourceType} '{resourceName}' not found", 
                ExecutionErrorType.ResourceNotFound,
                "RESOURCE_NOT_FOUND",
                $"Requested {resourceType}: {resourceName}",
                suggestion ?? $"Check that the {resourceType} name is correct and that it exists",
                innerException)
        {
        }
    }
    
    /// <summary>
    /// Exception thrown when a resource is busy.
    /// </summary>
    public class ResourceBusyException : ShellExecutionException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceBusyException"/> class.
        /// </summary>
        public ResourceBusyException(
            string resourceType,
            string resourceName,
            string currentOperation,
            string? suggestion = null,
            Exception? innerException = null)
            : base(
                $"{resourceType} '{resourceName}' is busy with another operation", 
                ExecutionErrorType.ResourceBusy,
                "RESOURCE_BUSY",
                $"Current operation: {currentOperation}",
                suggestion ?? "Wait for the current operation to complete or use another resource",
                innerException)
        {
        }
    }
}