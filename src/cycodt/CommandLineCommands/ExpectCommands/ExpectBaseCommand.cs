abstract class ExpectBaseCommand : Command
{
    public ExpectBaseCommand()
    {
        Input = null;
        Output = null;
    }
    
    public string? Input { get; set; }
    public string? Output { get; set; }

    public override bool IsEmpty()
    {
        var noInput = string.IsNullOrEmpty(Input);
        var isRedirected = Console.IsInputRedirected;
        return noInput && !isRedirected;
    }
    
    public override Command Validate()
    {
        var noInput = string.IsNullOrEmpty(Input);
        var implictlyUseStdIn = noInput && Console.IsInputRedirected;
        if (implictlyUseStdIn)
        {
            Input = "-";
        }

        return this;
    }

    protected void WriteOutput(string text)
    {
        if (string.IsNullOrEmpty(Output))
        {
            Console.WriteLine(text);
        }
        else
        {
            FileHelpers.WriteAllText(Output, text);
        }
    }
}