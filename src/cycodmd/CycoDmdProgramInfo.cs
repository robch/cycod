public class CycoDmdProgramInfo : ProgramInfo
{
    public CycoDmdProgramInfo() : base(
        () => "mdx",
        () => "The AI-Powered Markdown Generator CLI",
        () => ".chatx",
        () => typeof(CycoDmdProgramInfo).Assembly)
    {
    }
}
