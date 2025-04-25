public class CycoDevProgramInfo : ProgramInfo
{
    public CycoDevProgramInfo() : base(
        () => "chatx",
        () => ".chatx",
        () => typeof(CycoDevProgramInfo).Assembly)
    {
    }
}
