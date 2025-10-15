public class PowershellShellSession : ShellSession
{
    public override PersistentShellType GetShellType()
    {
        return PersistentShellType.PowerShell;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PowershellShellSession"/> class.
    /// </summary>
    /// <param name="name">Name of the shell.</param>
    public PowershellShellSession(string name) : base(name)
    {
    }

    /// <summary>
    /// Gets the singleton instance of the PowershellShellSession.
    /// For backward compatibility - use NamedShellProcessManager instead for new code.
    /// </summary>
    [Obsolete("Use NamedShellProcessManager.CreateShell or NamedShellProcessManager.GetShell instead")]
    public static PowershellShellSession Instance 
    { 
        get
        {
            if (_instance == null)
            {
                _instance = new PowershellShellSession("default-powershell");
                _instance.Initialize();
            }
            return _instance;
        }
    }
    
    private static PowershellShellSession? _instance;
}
