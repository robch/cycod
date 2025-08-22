using CycoDev.CustomTools;

namespace CycoDev.CommandLine
{
    /// <summary>
    /// Base class for tool commands.
    /// </summary>
    public abstract class ToolBaseCommand : Command
    {
        /// <summary>
        /// Gets or sets the command name.
        /// </summary>
        protected string CommandName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        protected string HelpText { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether to use global scope.
        /// </summary>
        public bool Global { get; set; }

        /// <summary>
        /// Gets or sets whether to use user scope.
        /// </summary>
        public bool User { get; set; }

        /// <summary>
        /// Gets or sets whether to use local scope.
        /// </summary>
        public bool Local { get; set; }

        /// <summary>
        /// Gets or sets whether to use any scope.
        /// </summary>
        public bool Any { get; set; }

        /// <summary>
        /// Gets the command name.
        /// </summary>
        /// <returns>The command name.</returns>
        public override string GetCommandName()
        {
            return CommandName;
        }

        /// <summary>
        /// Checks if the command is empty.
        /// </summary>
        /// <returns>True if the command is empty, false otherwise.</returns>
        public override bool IsEmpty()
        {
            return string.IsNullOrEmpty(CommandName);
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="interactive">Whether the command is interactive.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public override async Task<object> ExecuteAsync(bool interactive)
        {
            return await ExecuteAsync();
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public abstract Task<object> ExecuteAsync();

        /// <summary>
        /// Gets the scope based on command options.
        /// </summary>
        /// <param name="defaultToAny">Whether to default to any scope if no scope is specified.</param>
        /// <returns>The scope to use.</returns>
        protected ToolScope GetScope(bool defaultToAny = false)
        {
            if (Global) return ToolScope.Global;
            if (User) return ToolScope.User;
            if (Local) return ToolScope.Local;
            if (Any) return ToolScope.Any;

            return defaultToAny ? ToolScope.Any : ToolScope.Local;
        }
    }
}