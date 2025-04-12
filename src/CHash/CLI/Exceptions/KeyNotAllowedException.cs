namespace CLI.Exceptions;

public class KeyNotAllowedException : ParserLogicException
{
    public KeyNotAllowedException(string message) : base(message)
    {
    }
}