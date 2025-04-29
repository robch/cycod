internal class WebSearchCommandLineException : CommandLineException
{
    public WebSearchCommandLineException() : base()
    {
    }

    public WebSearchCommandLineException(string message) : base(message)
    {
    }

    override public string GetCommand()
    {
        return "web search";
    }
}
