public struct YamlTestCaseParseContext
{
    public string Source;
    public FileInfo File;

    public string Area;
    public string Class;
    public Dictionary<string, List<string>> Tags;

    public Dictionary<string, string> Environment;
    public string WorkingDirectory;

    public List<Dictionary<string, string>> Matrix;
}
