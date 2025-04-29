class HelpCommand : Command
{
    public HelpCommand()
    {
    }

    override public string GetCommandName()
    {
        return "help";
    }

    override public bool IsEmpty()
    {
        return false;
    }

    override public Command Validate()
    {
        return this;
    }

}