public class CmdShellSession : ShellSession
{
    public override PersistentShellType GetShellType()
    {
        return PersistentShellType.Cmd;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CmdShellSession"/> class.
    /// </summary>
    /// <param name="name">Name of the shell.</param>
    public CmdShellSession(string name) : base(name)
    {
    }

    /// <summary>
    /// Gets the singleton instance of the CmdShellSession.
    /// For backward compatibility - use NamedShellManager instead for new code.
    /// </summary>
    [Obsolete("Use NamedShellManager.CreateShell or NamedShellManager.GetShell instead")]
    public static CmdShellSession Instance 
    { 
        get
        {
            if (_instance == null)
            {
                _instance = new CmdShellSession("default-cmd");
                _instance.Initialize();
            }
            return _instance;
        }
    }
    
    private static CmdShellSession? _instance;
}
