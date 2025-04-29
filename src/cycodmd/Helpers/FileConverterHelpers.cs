using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

class FileConverterHelpers
{
    public static string? ReadAllText(string fileName, out bool isStdin, out bool isMarkdown, out bool isBinary)
    {
        isStdin = fileName == "-";
        isBinary = !isStdin && File.ReadAllBytes(fileName).Any(x => x == 0);
        isMarkdown = isBinary || fileName.EndsWith(".md", StringComparison.OrdinalIgnoreCase);

        return !isBinary
            ? FileHelpers.ReadAllText(fileName)
            : FileConverters.ConvertToMarkdown(fileName);
    }
}
