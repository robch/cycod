internal class WebGetCommandLineException : CommandLineException
{
    public WebGetCommandLineException() : base()
    {
    }

    public WebGetCommandLineException(string message) : base(message)
    {
    }

    override public string GetCommand()
    {
        return "web get";
    }
}