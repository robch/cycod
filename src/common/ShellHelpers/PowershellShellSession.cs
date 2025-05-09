public class PowershellShellSession : ShellSession
{
    protected override ShellType GetShellType()
    {
        return ShellType.PowerShell;
    }

    public static PowershellShellSession Instance => _instance ??= new PowershellShellSession();
    private static PowershellShellSession? _instance;
}
