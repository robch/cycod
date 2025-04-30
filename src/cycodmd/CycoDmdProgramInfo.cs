public class CycoDmdProgramInfo : ProgramInfo
{
    public CycoDmdProgramInfo() : base(
        () => "mdx",
        () => "The AI-Powered Markdown Generator CLI",
        () => ".cycod",
        () => typeof(CycoDmdProgramInfo).Assembly)
    {
    }
}
