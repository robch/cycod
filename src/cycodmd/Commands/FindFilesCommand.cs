using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

class FindFilesCommand : Command
{
    public FindFilesCommand()
    {
        Globs = new();
        ExcludeGlobs = new();
        ExcludeFileNamePatternList = new();

        IncludeFileContainsPatternList = new();
        ExcludeFileContainsPatternList = new();

        IncludeLineContainsPatternList = new();
        IncludeLineCountBefore = 0;
        IncludeLineCountAfter = 0;
        IncludeLineNumbers = false;

        RemoveAllLineContainsPatternList = new();
        FileInstructionsList = new();
    }

    override public string GetCommandName()
    {
        return "";
    }

    override public bool IsEmpty()
    {
        return !Globs.Any() &&
            !ExcludeGlobs.Any() &&
            !ExcludeFileNamePatternList.Any() &&
            !IncludeFileContainsPatternList.Any() &&
            !ExcludeFileContainsPatternList.Any() &&
            !IncludeLineContainsPatternList.Any() &&
            IncludeLineCountBefore == 0 &&
            IncludeLineCountAfter == 0 &&
            IncludeLineNumbers == false &&
            !RemoveAllLineContainsPatternList.Any() &&
            !FileInstructionsList.Any() &&
            ThreadCount == 0;
    }

    override public Command Validate()
    {
        if (!Globs.Any()) 
        {
            Globs.Add("**");
        }

        var mdxIgnoreFile = FileHelpers.ParentFindFile(".mdxignore");
        if (mdxIgnoreFile != null)
        {
            AddExclusions(mdxIgnoreFile);
        }

        return this;
    }

    public List<string> Globs;
    public List<string> ExcludeGlobs;
    public List<Regex> ExcludeFileNamePatternList;

    public List<Regex> IncludeFileContainsPatternList;
    public List<Regex> ExcludeFileContainsPatternList;

    public List<Regex> IncludeLineContainsPatternList;
    public int IncludeLineCountBefore;
    public int IncludeLineCountAfter;
    public bool IncludeLineNumbers;
    public List<Regex> RemoveAllLineContainsPatternList;

    public List<Tuple<string, string>> FileInstructionsList;

    public string SaveFileOutput;

    private void AddExclusions(string mdxIgnoreFile)
    {
        var lines = File.ReadAllLines(mdxIgnoreFile);
        foreach (var line in lines)
        {
            var assumeIsGlob = line.Contains('/') || line.Contains('\\');
            if (assumeIsGlob)
            {
                ExcludeGlobs.Add(line);
            }
            else
            {
                ExcludeFileNamePatternList.Add(new Regex(line));
            }
        }
    }
}
