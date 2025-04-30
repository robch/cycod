public class CycoDmdProgramInfo : ProgramInfo
{
    public CycoDmdProgramInfo() : base(
        () => "cycodmd",
        () => "The AI-Powered Markdown Generator CLI",
        () => ".cycod",
        () => typeof(CycoDmdProgramInfo).Assembly)
    {
    }
}
