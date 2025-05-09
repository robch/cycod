public class BashShellSession : ShellSession
{
    protected override ShellType GetShellType()
    {
        return ShellType.Bash;
    }

    public static BashShellSession Instance => _instance ??= new BashShellSession();
    private static BashShellSession? _instance;
}
