using System.ComponentModel;

public class ThinkingToolHelperFunction
{
    [ReadOnly(true)]
    [Description("Use the tool to think about something. It will not obtain new information or make any changes to the repository, but just log the thought. Use it when complex reasoning or brainstorming is needed. For example, if you explore the repo and discover the source of a bug, call this tool to brainstorm several unique ways of fixing the bug, and assess which change(s) are likely to be simplest and most effective. Alternatively, if you receive some test results, call this tool to brainstorm ways to fix the failing tests.")]
    public string Think(
        [Description("Your thoughts.")] string thought)
    {
        return "Thought logged.";
    }
}
