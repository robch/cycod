public class CycoDjProgramInfo : ProgramInfo
{
    public CycoDjProgramInfo() : base(
        () => "cycodj",
        () => "Chat History Journal and Analysis Tool",
        () => ".cycod",
        () => typeof(CycoDjProgramInfo).Assembly)
    {
    }
}
