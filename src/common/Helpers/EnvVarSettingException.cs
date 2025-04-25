using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

public class EnvVarSettingException : Exception
{
    public EnvVarSettingException() : base()
    {
    }

    public EnvVarSettingException(string message) : base(message)
    {
    }
}
