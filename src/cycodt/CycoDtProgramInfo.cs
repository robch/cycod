public class CycoDtProgramInfo : ProgramInfo
{
    public CycoDtProgramInfo() : base(
        () => "cycodt",
        () => "AI-powered CLI Test Framework",
        () => ".chatx",
        () => typeof(CycoDtProgramInfo).Assembly)
    {
    }
}
