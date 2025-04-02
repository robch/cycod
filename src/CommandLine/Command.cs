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
}
