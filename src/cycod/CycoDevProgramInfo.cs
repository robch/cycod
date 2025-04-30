public class CycoDevProgramInfo : ProgramInfo
{
    public CycoDevProgramInfo() : base(
        () => "cycod",
        () => "AI-powered Developer CLI",
        () => ".cycod",
        () => typeof(CycoDevProgramInfo).Assembly)
    {
    }
}
