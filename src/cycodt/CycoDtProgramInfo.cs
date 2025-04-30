public class CycoDtProgramInfo : ProgramInfo
{
    public CycoDtProgramInfo() : base(
        () => "cycodt",
        () => "AI-powered CLI Test Framework",
        () => ".cycod",
        () => typeof(CycoDtProgramInfo).Assembly)
    {
    }
}
