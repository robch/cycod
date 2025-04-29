public class ImageFileConverter : IFileConverter
{
    public bool CanConvert(string fileName)
    {
        var configured = OpenAIChatCompletionsClass.IsConfigured();
        var stdin = fileName == "-";
        return configured && !stdin && ImageTypeDetector.GetContentType(fileName) != null;
    }

    public string ConvertToMarkdown(string fileName)
    {
        var chat = new OpenAIChatCompletionsClass();
        var prompt = "What's in this image? First, comment on the image, then, if there is text, extract that text as markdown (but don't wrap it as markdown, except where the text is wrapped as markdown in the image).";
        prompt += "\n\n![image](" + fileName + ")\n\n";
        return chat.GetChatCompletion(prompt, fileName);
    }
}
