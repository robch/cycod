public class CycoDmdProgramInfo : ProgramInfo
{
    public CycoDmdProgramInfo() : base(
        () => "cycodmd",
        () => "AI-Powered Markdown Generator CLI",
        () => ".cycod",
        () => typeof(CycoDmdProgramInfo).Assembly)
    {
    }
}
