public class BinaryFileConverter : IFileConverter
{
    public bool CanConvert(string fileName)
    {
        return true;
    }

    public string ConvertToMarkdown(string fileName)
    {
        return null;
    }
}