class HelpCommand : Command
{
    public HelpCommand()
    {
    }

    override public string GetCommandName()
    {
        return "help";
    }

    public override bool IsEmpty()
    {
        return false;
    }
}