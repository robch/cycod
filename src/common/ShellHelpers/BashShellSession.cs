public class BashShellSession : ShellSession
{
    public override PersistentShellType GetShellType()
    {
        return PersistentShellType.Bash;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BashShellSession"/> class.
    /// </summary>
    /// <param name="name">Name of the shell.</param>
    public BashShellSession(string name) : base(name)
    {
    }

    /// <summary>
    /// Gets the singleton instance of the BashShellSession.
    /// For backward compatibility - use NamedShellManager instead for new code.
    /// </summary>
    [Obsolete("Use NamedShellManager.CreateShell or NamedShellManager.GetShell instead")]
    public static BashShellSession Instance 
    { 
        get
        {
            if (_instance == null)
            {
                _instance = new BashShellSession("default-bash");
                _instance.Initialize();
            }
            return _instance;
        }
    }
    
    private static BashShellSession? _instance;
}
