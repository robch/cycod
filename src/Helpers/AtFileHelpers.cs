using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class AtFileHelpers
{
    public static string ExpandAtFileValue(string atFileValue, INamedValues? values = null)
    {
        if (atFileValue.StartsWith("@") && FileHelpers.FileExists(atFileValue[1..]))
        {
            return FileHelpers.ReadAllText(atFileValue[1..]);
        }
        else if (atFileValue.StartsWith("@") && ConsoleHelpers.IsStandardInputReference(atFileValue[1..]))
        {
            return string.Join('\n', ConsoleHelpers.GetAllLinesFromStdin());
        }
        else if (atFileValue.StartsWith("@"))
        {
            string key = atFileValue[1..];
            
            // First check the provided values if any
            if (values != null && values.Contains(key))
            {
                string? result = values.Get(key);
                if (result != null)
                {
                    return result;
                }
            }
            
            // Then check ConfigStore
            var configValue = ConfigStore.Instance.GetFromAnyScope(key);
            if (!configValue.IsNotFoundNullOrEmpty())
            {
                return configValue.AsString() ?? string.Empty;
            }
        }
        return atFileValue;
    }
}
