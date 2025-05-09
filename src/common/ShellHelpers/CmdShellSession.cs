public class CmdShellSession : ShellSession
{
    protected override ShellType GetShellType()
    {
        return ShellType.Cmd;
    }

    public static CmdShellSession Instance => _instance ??= new CmdShellSession();
    private static CmdShellSession? _instance;
}
