public class CycoDevProgramInfo : ProgramInfo
{
    public CycoDevProgramInfo() : base(() => "chatx", () => typeof(CycoDevProgramInfo).Assembly)
    {
    }
}
