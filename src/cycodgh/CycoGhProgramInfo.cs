public class CycoGhProgramInfo : ProgramInfo
{
    public CycoGhProgramInfo() : base(
        () => "cycodgh",
        () => "GitHub Search and Repository Management CLI",
        () => ".cycod",
        () => typeof(CycoGhProgramInfo).Assembly)
    {
    }
}

