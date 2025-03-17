using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

abstract class Command
{
    public Command()
    {
        ThreadCount = 0;
    }

    abstract public bool IsEmpty();
    abstract public string GetCommandName();

    public int ThreadCount;
}
