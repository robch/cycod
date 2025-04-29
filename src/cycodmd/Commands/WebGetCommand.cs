using System.Collections.Generic;
using System.Linq;

class WebGetCommand : WebCommand
{
    public WebGetCommand()
    {
        Urls = new List<string>();
    }
    
    public List<string> Urls { get; set; }

    override public string GetCommandName()
    {
        return "web get";
    }

    override public bool IsEmpty()
    {
        
        return !Urls.Any();
    }

    override public Command Validate()
    {
        return this;
    }
}
