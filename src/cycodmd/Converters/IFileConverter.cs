public interface IFileConverter
{
    /// <summary>
    /// Determines if this converter can handle the given file (often by extension).
    /// </summary>
    bool CanConvert(string fileName);

    /// <summary>
    /// Performs the actual conversion to Markdown.
    /// </summary>
    string ConvertToMarkdown(string fileName);
}
