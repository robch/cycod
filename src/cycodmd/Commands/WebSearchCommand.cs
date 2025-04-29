using System.Collections.Generic;
using System.Linq;

class WebSearchCommand : WebCommand
{
    public WebSearchCommand()
    {
        Terms = new List<string>();
    }

    public List<string> Terms { get; set; }

    override public string GetCommandName()
    {
        return "web search";
    }

    override public bool IsEmpty()
    {
        return !Terms.Any();
    }

    override public Command Validate()
    {
        var noContent = !GetContent;
        var hasInstructions = PageInstructionsList.Any() || InstructionsList.Any();

        var assumeGetAndStrip = noContent && hasInstructions;
        if (assumeGetAndStrip)
        {
            GetContent = true;
            StripHtml = true;
        }

        return this;
    }
}
