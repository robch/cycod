public class CycoGrProgramInfo : ProgramInfo
{
    public CycoGrProgramInfo() : base(
        () => "cycodgr",
        () => "GitHub Search and Repository Management CLI",
        () => ".cycod",
        () => typeof(CycoGrProgramInfo).Assembly)
    {
    }
}

