namespace Core.Exceptions;

public class CommandValueRequired : ParserLogicException
{
    public CommandValueRequired(string message) : base(message)
    {
    }
}