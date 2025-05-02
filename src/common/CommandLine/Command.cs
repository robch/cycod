using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

abstract public class Command
{
    public Command()
    {
    }

    abstract public bool IsEmpty();
    abstract public string GetCommandName();

    virtual public string GetHelpTopic()
    {
        var topic = GetCommandName();
        var ok = !string.IsNullOrEmpty(topic);
        return ok ? topic : "usage";
    }

    virtual public Command Validate()
    {
        return this;
    }

    abstract public Task<object> ExecuteAsync(bool interactive);
}
