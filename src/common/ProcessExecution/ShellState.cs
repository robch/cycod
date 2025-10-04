namespace ProcessExecution
{
    /// <summary>
    /// Represents the state of a shell.
    /// </summary>
    public enum ShellState
    {
        /// <summary>
        /// The shell is initializing.
        /// </summary>
        Initializing,
        
        /// <summary>
        /// The shell is idle and ready to execute commands.
        /// </summary>
        Idle,
        
        /// <summary>
        /// The shell is executing a command.
        /// </summary>
        Busy,
        
        /// <summary>
        /// The shell has been terminated.
        /// </summary>
        Terminated
    }
}