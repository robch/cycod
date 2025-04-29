using System.Collections.Generic;

public static class FileConverters
{
    private static readonly List<IFileConverter> _converters = new()
    {
        new DocxFileConverter(),
        new PptxFileConverter(),
        new PdfFileConverter(),
        new ImageFileConverter(),
    };

    public static IFileConverter GetConverter(string fileName)
    {
        foreach (var converter in _converters)
        {
            if (converter.CanConvert(fileName))
            {
                return converter;
            }
        }

        return new BinaryFileConverter();
    }

    public static bool CanConvert(string fileName)
    {
        var converter = GetConverter(fileName);
        if (converter is BinaryFileConverter) return false;
        return converter != null && converter.CanConvert(fileName);
    }

    public static string ConvertToMarkdown(string fileName)
    {
        var converter = GetConverter(fileName);        
        return converter.ConvertToMarkdown(fileName);
    }
}
