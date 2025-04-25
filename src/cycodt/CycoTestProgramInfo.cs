public class CycoTestProgramInfo : ProgramInfo
{
    public CycoTestProgramInfo() : base(
        () => "cycodt",
        () => "AI-powered CLI Test Framework",
        () => ".chatx",
        () => typeof(CycoTestProgramInfo).Assembly)
    {
    }
}
