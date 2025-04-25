public class CycoDevProgramInfo : ProgramInfo
{
    public CycoDevProgramInfo() : base(
        () => "chatx",
        () => "AI-powered Developer CLI",
        () => ".chatx",
        () => typeof(CycoDevProgramInfo).Assembly)
    {
    }
}
