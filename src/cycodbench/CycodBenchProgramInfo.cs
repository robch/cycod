public class CycodBenchProgramInfo : ProgramInfo
{
    public CycodBenchProgramInfo() : base(
        () => "cycodbench",
        () => "Benchmark runner for SWE-bench problems",
        () => ".cycod",
        () => typeof(CycodBenchProgramInfo).Assembly)
    {
    }
}