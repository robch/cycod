[TestClass]
public static class TestAssemblyInitializer
{
    /// <summary>
    /// Initializes the test assembly by setting up the program information.
    /// This runs once before any tests in the assembly.
    /// </summary>
    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext testContext)
    {
        ProgramInfo = new CycoDevTestsProgramInfo();
        testContext.WriteLine("ProgramInfo initialized for test assembly");
    }
    
    /// <summary>
    /// Performs cleanup after all tests in the assembly have run.
    /// </summary>
    [AssemblyCleanup]
    public static void AssemblyCleanup()
    {
        ProgramInfo = null;
    }

    /// <summary>
    /// Gets the program info instance used for testing.
    /// </summary>
    public static ProgramInfo? ProgramInfo { get; private set; }

    private class CycoDevTestsProgramInfo : ProgramInfo
    {
        public CycoDevTestsProgramInfo() : base(
            () => "chatx",
            () => "CYCOD 'dotnet test' tests",
            () => ".chatx-tests",
            () => typeof(CycoDevTestsProgramInfo).Assembly)
        {
        }
    }
}