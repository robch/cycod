namespace CycoGr.CommandLine;

public abstract class CycoGrCommand : Command
{
    public CycoGrCommand()
    {
        SaveOutput = string.Empty;
        SaveJson = string.Empty;
        SaveCsv = string.Empty;
        SaveTable = string.Empty;
        SaveUrls = string.Empty;
        SaveRepos = string.Empty;
        SaveFilePaths = string.Empty;
        SaveRepoUrls = string.Empty;
        SaveFileUrls = string.Empty;
        Repos = new List<string>();
        Exclude = new List<string>();
    }

    override public Task<object> ExecuteAsync(bool interactive)
    {
        throw new NotImplementedException("ExecuteAsync is not implemented for CycoGrCommand.");
    }

    public string SaveOutput;
    public string SaveJson;
    public string SaveCsv;
    public string SaveTable;
    public string SaveUrls;
    public string SaveRepos;
    public string SaveFilePaths;
    public string SaveRepoUrls;
    public string SaveFileUrls;
    public List<string> Repos;
    public List<string> Exclude;
}
