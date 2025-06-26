public class CycodBenchProgramInfo : ProgramInfo
{
    public CycodBenchProgramInfo() : base(
        () => "cycodbench",
        () => "CYCOD SWE-Bench runner",
        () => ".cycod",
        () => typeof(CycodBenchProgramInfo).Assembly)
    {
    }
}